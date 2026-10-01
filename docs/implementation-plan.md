# CodeVerdict Implementation Plan

## Purpose

This document turns the platform feature list into an ordered implementation plan for the current CodeVerdict solution. It assumes the solution begins with the existing .NET projects and initial ASP.NET Core MVC/Identity setup.

The architecture and ownership rules are documented in [project-structure.md](project-structure.md). The intended product scope is documented in [platform-features.md](platform-features.md).

## Starting Point

The solution currently contains:

- `CodeVerdict.Web`: ASP.NET Core MVC application with Identity pages, controllers, views, and EF Core migrations.
- `CodeVerdict.Domain`: class library ready for business entities and rules.
- `CodeVerdict.Application`: class library ready for use cases and application contracts.
- `CodeVerdict.Infrastructure`: class library ready for persistence, Identity, queues, and storage implementations.
- `CodeVerdict.Judge`: class library ready for language runners, sandboxing, checkers, and the judge engine.

The Worker project and automated test projects are future additions.

## Delivery Strategy

Build one thin, usable path before expanding the platform:

```text
Create problem -> Publish problem -> Submit code -> Queue submission
    -> Judge in sandbox -> Store verdict -> Display result
```

Each milestone should leave the solution buildable and should add tests for the behavior introduced in that milestone. Avoid implementing contest features or multiple languages before the basic submission path is reliable.

## Milestone 0: Repository Foundation

**Status: Complete**

### Goal

Make the solution easy to build, test, configure, and extend.

### Tasks

- [x] Confirm solution project references and dependency direction.
- [x] Add a consistent folder structure inside each project.
- [x] Add test projects for Domain, Application, Judge, and Web behavior.
- [x] Add shared build and test instructions to the README.
- [x] Add local configuration documentation for the database and judge runtime.
- [x] Add environment-specific settings without committing secrets.
- [x] Define a common approach for validation errors and application failures.
- [x] Add CI validation for build, tests, and formatting where available.

### Completion Criteria

- The full solution builds from a clean checkout.
- Tests can run without requiring a production service.
- Project references follow the dependency direction in the architecture document.
- Local setup steps are written down and reproducible.

## Milestone 1: Domain Model

### Goal

Define the business concepts that do not depend on ASP.NET Core, EF Core, Docker, or a specific database.

### Tasks

- [ ] Add `Problem` with identity, statement content, lifecycle state, limits, and metadata.
- [ ] Add `TestCase` with sample/hidden status, input, expected output, and optional points.
- [ ] Add `Submission` with problem, user, language, source, status, score, time, memory, and timestamps.
- [ ] Add `Contest` and `ContestProblem` foundations without implementing contest workflows yet.
- [ ] Add `ProgrammingLanguage`, `SubmissionStatus`, and `ContestStatus` enums.
- [ ] Add domain rules for valid status transitions.
- [ ] Add domain rules for problem publication and submission eligibility.
- [ ] Define value objects for resource limits and verdict details where they reduce ambiguity.
- [ ] Keep Identity's application user out of the Domain project.

### Completion Criteria

- Domain tests cover valid and invalid state transitions.
- Entities can be created and changed without infrastructure dependencies.
- Business rules do not require an HTTP request, database context, container, or filesystem.

## Milestone 2: Persistence and Identity Integration

### Goal

Persist problems, test cases, submissions, and results using the existing EF Core and Identity foundation.

### Tasks

- [ ] Configure entity relationships and indexes.
- [ ] Add migrations for platform entities.
- [ ] Add repositories or persistence abstractions where the Application layer needs them.
- [ ] Configure `ApplicationUser` relationships without duplicating the user in Domain.
- [ ] Add database seed data for a development administrator and sample problem only when safe and documented.
- [ ] Store source code and judge metadata with explicit retention expectations.
- [ ] Add concurrency protection for submission and job state updates.
- [ ] Add persistence tests using a supported test database strategy.

### Completion Criteria

- A fresh development database can be created from migrations.
- Problems, test cases, submissions, and results can be persisted and retrieved.
- Concurrent updates cannot silently overwrite a newer submission state.
- Authentication and authorization roles work in the web application.

## Milestone 3: Problem Management

### Goal

Allow authorized users to create, validate, publish, and browse problems.

### Tasks

- [ ] Add Application commands and queries for create, update, publish, archive, and retrieve problem.
- [ ] Add authorization rules for authors and administrators.
- [ ] Add problem lifecycle validation: draft, review, published, and archived.
- [ ] Add test-case management use cases.
- [ ] Add validation for required statement fields, limits, examples, and test data.
- [ ] Add Web controllers and view models for authoring and browsing.
- [ ] Add participant-facing problem details and sample tests.
- [ ] Hide unpublished problems from unauthorized participants.
- [ ] Add a preview path for the participant-facing problem page.

### Completion Criteria

- An authorized author can create a draft problem and test cases.
- A published problem can be found and viewed by a participant.
- An unpublished problem cannot be submitted against by a participant.
- Web controllers remain thin and call Application use cases.

## Milestone 4: First Judge Engine

### Goal

Judge one supported language against exact-output test cases in an isolated execution environment.

### Tasks

- [ ] Define judge request and result contracts in the Judge project.
- [ ] Define `ILanguageRunner` for compilation and execution.
- [ ] Implement the first language runner selected by the project team.
- [ ] Define `ISandbox`, `SandboxLimits`, and structured sandbox results.
- [ ] Implement Docker execution with no network and restricted filesystem access.
- [ ] Implement compile-once behavior.
- [ ] Implement `ExactChecker`.
- [ ] Implement the judge pipeline from source code to per-test result to final verdict.
- [ ] Capture execution time, memory, exit status, and bounded diagnostics.
- [ ] Add unit tests for checkers and judge decision logic.
- [ ] Add integration tests that run a small set of safe sample programs in Docker.

### Completion Criteria

- Accepted, wrong-answer, compilation-error, runtime-error, time-limit, and memory-limit cases are distinguishable.
- The source program never runs directly inside the Web process.
- The compiled program is reused across tests when the language requires compilation.
- Containers are cleaned up after successful and failed runs.
- Hidden test input and expected output are not exposed in participant responses.

## Milestone 5: Submission Application Flow

### Goal

Connect the Web, Application, persistence, and Judge boundaries without blocking an HTTP request on judging.

### Tasks

- [ ] Add `SubmitCode` use case.
- [ ] Validate problem availability, language support, source size, and user permissions.
- [ ] Create a submission with `Queued` status.
- [ ] Define a queue abstraction in Application or an appropriate shared contract.
- [ ] Add an Infrastructure queue implementation for development.
- [ ] Add submission details and submission history queries.
- [ ] Add Web submission form and result pages.
- [ ] Show state changes from `Queued` through final verdict.
- [ ] Add tests for invalid submissions and duplicate/failed queue operations.

### Completion Criteria

- A participant can submit source code and immediately receive a submission ID.
- The HTTP request does not wait for compilation or test execution.
- A queued submission remains durable across a web process restart.
- Participants can view a submission while it is being processed.

## Milestone 6: Worker and Job Reliability

### Goal

Move queue consumption and judge orchestration into a dedicated Worker process.

### Tasks

- [ ] Add `CodeVerdict.Worker` as a Worker Service project.
- [ ] Consume submission jobs from the configured queue.
- [ ] Load submission, problem, tests, and language configuration.
- [ ] Update status to `Compiling`, then `Running`, then the final verdict.
- [ ] Save structured judge results and bounded diagnostics.
- [ ] Add retry handling for transient infrastructure failures.
- [ ] Add job leases, heartbeat, or equivalent abandoned-job detection.
- [ ] Make finalization idempotent so a job cannot produce two final results.
- [ ] Support graceful shutdown and job recovery.
- [ ] Add Worker integration tests with a fake queue and fake Judge implementation.

### Completion Criteria

- Worker failure does not permanently strand a queued submission.
- Transient failures are retried without duplicating final results.
- The Worker coordinates jobs but does not contain compilation, sandbox, or checker logic.
- The Web and Judge projects remain independently testable.

## Milestone 7: Production-Ready Judge Capabilities

### Goal

Expand judge correctness, security, and maintainability before adding broad product scope.

### Tasks

- [ ] Add token-based output checking.
- [ ] Add floating-point checking with explicit tolerance configuration.
- [ ] Add configurable partial scoring where required.
- [ ] Add more language runners one at a time.
- [ ] Version language images, compiler versions, and checker implementations.
- [ ] Add sandbox hardening tests for network, filesystem, process, timeout, and output limits.
- [ ] Add judge version and configuration metadata to every result.
- [ ] Add rejudge workflows for changed tests, checkers, or judge versions.
- [ ] Add operational metrics for queue depth, throughput, latency, and failure rates.
- [ ] Review source code and artifact retention policies.

### Completion Criteria

- Each supported language has documented compile and runtime behavior.
- A problem's checker configuration is validated before publication.
- Security limits are tested rather than assumed.
- Administrators can identify and rejudge results affected by a judge change.

## Milestone 8: Contests

### Goal

Add timed competitions after individual problem solving and judging are dependable.

### Tasks

- [ ] Add contest create, update, publish, start, end, and archive use cases.
- [ ] Add contest registration and eligibility rules.
- [ ] Associate problems with contest-specific order and scoring.
- [ ] Record contest-relative submission timing.
- [ ] Implement standings and deterministic tie-breaking.
- [ ] Add scoreboard freeze and post-contest reveal.
- [ ] Add contest-specific language, source, and submission limits.
- [ ] Add participant contest pages and administrator monitoring.
- [ ] Add end-to-end tests for contest timing and scoring.

### Completion Criteria

- A complete contest can be configured, run, scored, and reviewed.
- Standings are reproducible from recorded submissions.
- Contest rules do not leak hidden tests or frozen scoreboard information.
- Contest behavior is isolated from ordinary practice submissions.

## Milestone 9: Scale and Advanced Platform Features

### Goal

Support larger workloads and richer learning workflows after the core platform is stable.

### Candidates

- [ ] Multiple worker instances consuming a shared queue.
- [ ] Queue backpressure and per-user or per-contest fairness policies.
- [ ] Caching for published problem metadata.
- [ ] Live submission updates.
- [ ] Profiles, statistics, topics, recommendations, and learning paths.
- [ ] Groups, classrooms, and private contests.
- [ ] Special checkers and custom checker review workflows.
- [ ] Similarity detection with an explicit privacy policy.
- [ ] Backup, restore, disaster recovery, and capacity testing.

These features should be selected based on measured user needs and operational evidence rather than implemented all at once.

## Cross-Cutting Rules

### Dependency Direction

- Web depends on Application.
- Application depends on Domain and abstractions.
- Infrastructure implements persistence and external-service abstractions.
- Judge owns compilation, execution, checking, and verdict production.
- Worker consumes jobs and invokes Judge.
- Domain does not depend on Web, EF Core, Docker, SQL Server, or HTTP.

### Security Rules

- Never execute participant code in the Web process.
- Never give sandboxed code network access by default.
- Never expose hidden tests, host paths, service credentials, or internal diagnostics.
- Validate authorization in the Application layer, not only in views.
- Bound source, input, output, log, and diagnostic sizes.
- Treat custom checkers and uploaded artifacts as untrusted code.

### Testing Rules

Every milestone should include the narrowest useful tests:

- Domain tests for business rules and state transitions.
- Application tests for use cases, validation, and authorization decisions.
- Infrastructure tests for persistence and queue behavior.
- Judge tests for runners, checkers, limits, and verdict aggregation.
- Worker tests for retries, recovery, and idempotency.
- Web tests for authorization and important user workflows.
- End-to-end tests for the complete submit-to-verdict path.

## First Recommended Work Package

The first implementation slice should be:

```text
Problem entity
  -> EF Core persistence
  -> Create/list/view problem use cases
  -> Publish a problem
  -> Display problem details
```

After that slice is tested, implement the first language runner and exact checker, then connect submissions to the queue and Worker. This keeps the first feedback loop small while establishing the contracts needed by the full platform.

## Release Checklist

Before calling the first usable release complete:

- [ ] Users can authenticate and receive the correct role permissions.
- [ ] Authors can create and publish a problem with hidden tests.
- [ ] Participants can submit a supported language.
- [ ] Submissions are durable and processed asynchronously.
- [ ] The judge compiles and runs code only inside the sandbox.
- [ ] Accepted and major failure verdicts are correct and explainable.
- [ ] Submission results include time and memory when available.
- [ ] Hidden tests and infrastructure details remain protected.
- [ ] Worker failure and retry behavior have been tested.
- [ ] Build, tests, database setup, and local judge setup are documented.
