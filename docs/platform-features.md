# CodeVerdict Platform Features

## Purpose

CodeVerdict is an online coding judge for practicing programming problems, authoring and reviewing tasks, and running contests. This document describes the product capabilities the platform should provide. The system boundaries and technical responsibilities are defined in [project-structure.md](project-structure.md).

The feature set is grouped by the people who use the platform and by the services that make judging reliable.

## Product Goals

CodeVerdict should:

- Let users discover problems, submit solutions, and understand verdicts.
- Give problem authors the tools to create correct, well-tested, maintainable problems.
- Provide fair and reproducible judging for untrusted code.
- Support both individual practice and timed contests.
- Keep the web application responsive by processing submissions asynchronously.
- Preserve a clear separation between the website, application use cases, infrastructure, worker, and judge engine.

## Users and Roles

### Participant

A participant solves problems, submits code, reviews results, and participates in contests.

### Problem Author

An author creates and maintains problems, test cases, constraints, examples, explanations, and checker configuration.

### Contest Administrator

An administrator creates contests, selects problems, configures rules, monitors submissions, and publishes results.

### Platform Administrator

A platform administrator manages users and roles, moderates content, configures supported languages, and monitors the judging system.

## Core Participant Features

### Account and Profile

- Register, sign in, sign out, and recover an account.
- Manage profile information and preferred settings.
- View solved problems, submission history, scores, and contest participation.
- Use roles and permissions for participant, author, contest administrator, and platform administrator capabilities.
- Optionally make profile and statistics public or private.

### Problem Discovery

- Browse all published problems.
- Search by title, identifier, and keywords.
- Filter by difficulty, topic, supported language, status, and contest.
- Sort by popularity, difficulty, newest, acceptance rate, or personal progress.
- Show whether a problem is unsolved, attempted, or solved by the current user.
- Show problem metadata such as time limit, memory limit, score, topics, and supported languages.

### Problem Statement

A problem page should contain:

- Title and stable problem identifier.
- Problem description.
- Input format.
- Output format.
- Constraints.
- Examples with input and output.
- Explanation or editorial content when published.
- Time and memory limits.
- Supported programming languages.
- Submission entry point.
- Links to related problems and contests.

The statement should support formatted text, code blocks, mathematical notation, and images where appropriate. Statements should be versioned so that changes do not make historical submissions difficult to interpret.

### Code Submission

- Select a problem and programming language.
- Enter or paste source code in an editor.
- Submit code for judging.
- Validate basic request rules before creating a submission, including problem availability, language support, and source size limits.
- Receive a submission identifier immediately after the submission is accepted into the queue.
- Continue browsing while the submission is queued or running.
- Optionally save drafts without submitting them.

The web application should create the submission and enqueue the work. It should not compile or execute user code inside the HTTP request.

### Submission Results

Each submission should display:

- Submission identifier, problem, user, language, and creation time.
- Current processing state.
- Final verdict when judging is complete.
- Score, execution time, and memory usage when available.
- The first failed test or a permitted failure summary.
- Compilation output for compilation errors.
- Runtime diagnostics appropriate for the platform policy.
- Source code visibility according to the problem and contest rules.

Supported states and verdicts should include the states described in the architecture, including `Queued`, `Compiling`, `Running`, `Accepted`, `WrongAnswer`, `CompilationError`, `RuntimeError`, `TimeLimitExceeded`, `MemoryLimitExceeded`, and `SystemError`.

### Submission History

- View a user's submissions for a problem.
- Filter by verdict, language, and date.
- Compare submission metadata and performance.
- Reopen a previous source submission for editing or resubmission.
- Show the best accepted or highest-scoring submission for a problem.
- Prevent private test data from being exposed through history or error messages.

### Progress and Statistics

- Track solved and attempted problems.
- Track results by topic, difficulty, language, and date.
- Show acceptance rate and submission counts.
- Provide streaks or activity history if the product chooses to gamify practice.
- Show rankings only where the relevant contest or platform policy allows them.

## Problem Authoring Features

### Problem Lifecycle

Problems should move through explicit states such as:

```text
Draft -> Review -> Published -> Archived
```

- Draft problems are visible only to authorized users.
- Review problems can be checked by reviewers before publication.
- Published problems are available to participants.
- Archived problems are retained for historical submissions but are not normally available for new attempts.

### Problem Management

Authors and administrators should be able to:

- Create, edit, duplicate, publish, unpublish, and archive problems.
- Assign a stable identifier, title, difficulty, topics, and author.
- Set time limits, memory limits, score rules, and supported languages.
- Write the statement and examples.
- Add an explanation or editorial.
- Preview the participant-facing problem page.
- Review a change history and restore a previous version.
- Restrict access to private, group, or contest-only problems.

### Test Case Management

- Add, edit, remove, and reorder test cases.
- Mark tests as sample, public, or hidden.
- Store input and expected output securely.
- Assign per-test points for partial scoring when enabled.
- Validate that test cases follow the problem constraints.
- Run an author's reference solution against the test set.
- Run a candidate solution against selected tests before publishing.
- Detect duplicate or ineffective tests where practical.
- Keep hidden tests inaccessible to participants.

### Checker Configuration

The platform should support the checker types described in the architecture:

- Exact output comparison.
- Token-based comparison that ignores permitted whitespace differences.
- Floating-point comparison with configured tolerance.
- Special checkers for problems with multiple valid outputs.

Checker configuration must be validated and tested before a problem can be published. Custom checker code must be treated as untrusted code and executed with the same isolation standards as other judge workloads.

### Review Workflow

- Assign reviewers to draft problems.
- Record review comments and requested changes.
- Require a successful validation run before publication.
- Confirm that examples, constraints, limits, and expected outputs are consistent.
- Confirm that the problem can be judged using its configured language and checker.

## Contest Features

### Contest Creation

Contest administrators should be able to:

- Create a contest with a title, description, rules, and timezone.
- Configure registration, start, end, and scoreboard-freeze times.
- Select published or contest-only problems.
- Define problem order, points, penalties, and tie-break rules.
- Configure allowed languages and submission limits.
- Preview the contest before publishing it.

### Contest Participation

- Register for an upcoming contest.
- View the contest problem set and rules.
- Submit solutions during the allowed period.
- See contest-specific submission status and score.
- View the live or frozen scoreboard according to contest settings.
- Review final standings and accepted solutions after the contest.

### Contest Standings

Standings should support the configured scoring model, including:

- Total score.
- Time or penalty.
- Problems solved.
- Per-problem score and status.
- Tie-breaking rules.
- Scoreboard freeze and post-contest reveal.

Contest result calculations should be deterministic and should use the recorded submission and contest timestamps rather than mutable current state.

## Judge and Execution Features

### Language Support

The judge should provide a language runner abstraction so languages can be added without changing the application workflow. Each language definition should specify:

- Accepted source file and entry-point conventions.
- Compilation command, if compilation is required.
- Runtime command.
- Supported language version.
- Resource requirements and default limits.
- Compiler and runtime diagnostic handling.

The initial release can begin with one language, then add C#, C++, Java, Python, and other languages as the runner and sandbox contracts mature.

### Judging Pipeline

The judging engine should:

1. Validate the judging request.
2. Resolve the language runner and checker.
3. Compile source code once when compilation is required.
4. Return a compilation verdict without running tests when compilation fails.
5. Execute the compiled program against the test cases in a sandbox.
6. Enforce time, memory, process, filesystem, and network limits.
7. Check output using the configured checker.
8. Stop early or continue according to the problem scoring policy.
9. Aggregate test results into a final verdict and score.
10. Return structured execution metadata for persistence and display.

### Sandbox Security

Every submission must be treated as untrusted. The sandbox should provide:

- No network access.
- No access to the host filesystem, database, Docker socket, or internal services.
- Non-root execution.
- CPU, memory, process, and wall-clock limits.
- Restricted filesystem permissions.
- Container cleanup after every run.
- Maximum source, input, output, and log sizes.
- Protection against fork bombs, runaway output, and repeated process creation.
- A clear distinction between user-code failures and judge infrastructure failures.

Sandbox hardening is a release requirement, not an optional convenience feature.

### Queue and Worker Processing

- Place accepted judging requests on a queue.
- Return a submission ID without waiting for the full judge run.
- Allow one or more workers to consume jobs.
- Update submission state as the worker progresses.
- Retry transient infrastructure failures.
- Detect abandoned jobs and requeue them safely.
- Prevent the same submission from being finalized twice.
- Record worker and judge diagnostics for administrators without exposing sensitive details to participants.
- Support graceful shutdown so active jobs are either completed or returned to the queue.

The queue technology should remain an Infrastructure concern. The Worker coordinates jobs, while CodeVerdict.Judge owns compilation, execution, checking, and verdict production.

### Reliability and Fairness

- Use consistent resource limits across workers for the same problem and language.
- Record judge version, language version, checker version, and limit configuration with each result.
- Make judging deterministic where the problem permits it.
- Isolate worker failures from the web application.
- Provide administrators with a way to rejudge submissions after a checker, test set, or judge version changes.
- Mark results affected by an infrastructure incident and preserve an audit trail.

## Administration and Moderation

- Manage users, roles, and account status.
- Grant and revoke author and administrator permissions.
- Review reported problems or inappropriate content.
- Disable a problem or contest without deleting historical data.
- Configure supported languages and judge availability.
- Inspect queue depth, worker health, failed jobs, and average judging time.
- View audit events for publication, permission, contest, and rejudge actions.
- Configure retention rules for source code, logs, and execution artifacts.

## Notifications and User Feedback

The platform should provide clear feedback for long-running operations:

- Submission state changes on the submission page.
- Optional email or in-app notification when a submission finishes.
- Contest start, end, and result notifications where enabled.
- Clear error messages for invalid submissions and unavailable services.
- No leakage of hidden test data, host details, secrets, or sandbox internals.

Live updates can initially use polling. A push mechanism can be added later if the user experience and scale justify it.

## Reporting and Observability

### Participant-Facing Reporting

- Submission verdict and score.
- Execution time and memory usage.
- Problem and contest progress.
- Public standings and profile statistics according to policy.

### Administrator-Facing Reporting

- Queue depth and job age.
- Worker availability and throughput.
- Verdict distribution.
- Compilation and runtime failure rates.
- Problem acceptance rates.
- Judge latency by language and problem.
- Sandbox and infrastructure error counts.
- Rejudge history and affected submissions.

Logs and metrics should use correlation identifiers such as submission ID and job ID. Sensitive source code and test data should not be written to ordinary logs.

## Non-Functional Requirements

### Security

- Treat source code, custom checkers, and uploaded problem assets as untrusted input.
- Apply authorization checks in the Application layer for every protected use case.
- Protect authentication and session data with ASP.NET Identity and standard security practices.
- Validate input sizes and formats at service boundaries.
- Keep secrets out of source control and participant-visible responses.
- Separate public web functionality from judge execution infrastructure.

### Performance

- Submission creation should be fast and independent of judge duration.
- Problem pages should remain usable while workers are busy.
- Compile once and reuse the compiled result across tests.
- Support multiple workers without changing the judging contract.
- Cache safe, read-heavy data such as published problem metadata where useful.

### Availability and Recovery

- A web restart must not lose accepted submissions.
- A worker restart must not permanently strand a submission.
- Failed jobs must be retryable without corrupting final results.
- Database backups must include problems, tests, submissions, contests, and results.
- Published problem and contest data must be recoverable with its version history.

### Accessibility and Usability

- Use keyboard-accessible forms and code submission controls.
- Provide readable contrast, labels, validation messages, and responsive layouts.
- Make verdicts understandable without requiring knowledge of internal services.
- Keep important actions and status information visible on small screens.

## Recommended Delivery Phases

### Phase 1: Usable Practice Judge

- Identity and roles.
- Problem creation and publication.
- Problem browsing and details.
- One supported language.
- Sample and hidden test cases.
- Exact checker.
- Submission creation and history.
- Basic background judging.
- Core verdicts and result display.
- Docker sandbox with strict resource limits.

### Phase 2: Reliable Platform Foundation

- Dedicated Worker project.
- Persistent queue and retry handling.
- Multiple workers.
- Token and floating-point checkers.
- Author review workflow.
- Problem versioning.
- Administrative monitoring and audit events.
- Rejudge support.
- Additional language runners.

### Phase 3: Contests and Growth

- Contest creation, registration, timing, scoring, and standings.
- Contest-specific visibility and limits.
- Profiles, progress, and statistics.
- Notifications and live status updates.
- Performance reporting and capacity monitoring.
- Horizontal scaling and production recovery procedures.

### Phase 4: Advanced Capabilities

- Special checkers and partial scoring.
- Custom checker validation workflows.
- Groups, private competitions, and classroom features.
- Editorials, tags, recommendations, and learning paths.
- Plagiarism or similarity detection with an explicit privacy policy.
- Reproducible judge environments and historical rejudge tooling.

## Feature Ownership by Project

| Capability | Primary project or boundary |
| --- | --- |
| Pages, forms, authentication UI, and participant workflows | `CodeVerdict.Web` |
| Use cases, validation, authorization decisions, and orchestration | `CodeVerdict.Application` |
| Problem, submission, contest, and verdict concepts | `CodeVerdict.Domain` |
| Database, Identity, queue, storage, notifications, and external services | `CodeVerdict.Infrastructure` |
| Compilation, sandbox execution, checkers, and judge results | `CodeVerdict.Judge` |
| Queue consumption, retries, job lifecycle, and worker coordination | `CodeVerdict.Worker` |

The main runtime path should remain:

```text
Web -> Application -> Queue -> Worker -> Judge -> Sandbox
```

No participant-facing feature should require placing compilation or execution logic in the Web project. No judge feature should require the Judge project to know about Razor views, HTTP requests, or database-specific details.

## Definition of Done for a Feature

A feature is ready for release when:

- Its user-visible behavior and authorization rules are defined.
- Its owning project and dependencies are clear.
- Success, validation failure, and infrastructure failure paths are handled.
- Sensitive data exposure has been considered.
- Relevant domain, application, judge, worker, or web tests exist.
- Metrics and logs are sufficient to diagnose failures.
- The feature has a documented migration or rollout path when it changes persisted data.
- The participant experience remains understandable when judging is delayed or unavailable.
