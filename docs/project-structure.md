\# CodeVerdict Architecture



\## Overview



CodeVerdict is an online programming judge platform similar to Codeforces, e-olymp, and other competitive programming platforms.



The system is split into several projects, each with a specific responsibility.



The main architecture is:



```text

&#x20;                   ┌─────────────────────┐

&#x20;                   │   CodeVerdict.Web   │

&#x20;                   │   ASP.NET Core MVC   │

&#x20;                   └──────────┬──────────┘

&#x20;                              │

&#x20;                              ▼

&#x20;                   ┌─────────────────────┐

&#x20;                   │ CodeVerdict.Application │

&#x20;                   │    Use Cases / Logic │

&#x20;                   └──────────┬──────────┘

&#x20;                              │

&#x20;                ┌─────────────┴─────────────┐

&#x20;                ▼                           ▼

&#x20;      ┌───────────────────┐       ┌────────────────────┐

&#x20;      │ CodeVerdict.Domain│       │CodeVerdict.Infrastructure│

&#x20;      │ Business Models   │       │ EF Core / Identity  │

&#x20;      └───────────────────┘       └────────────────────┘

&#x20;                                          

&#x20;                             

&#x20;                   Submission / Judge Request

&#x20;                              │

&#x20;                              ▼

&#x20;                   ┌─────────────────────┐

&#x20;                   │ CodeVerdict.Worker  │

&#x20;                   │ Background Worker   │

&#x20;                   └──────────┬──────────┘

&#x20;                              │

&#x20;                              ▼

&#x20;                   ┌─────────────────────┐

&#x20;                   │ CodeVerdict.Judge   │

&#x20;                   │ Judge Engine        │

&#x20;                   └──────────┬──────────┘

&#x20;                              │

&#x20;                              ▼

&#x20;                   ┌─────────────────────┐

&#x20;                   │ Docker Sandbox      │

&#x20;                   │ Untrusted Code      │

&#x20;                   └─────────────────────┘

```



\---



\# 1. CodeVerdict.Web



\*\*Project type:\*\* ASP.NET Core MVC



```text

CodeVerdict.Web/

```



\## Responsibility



This is the user-facing website.



It handles:



\* HTTP requests

\* Controllers

\* Razor Views

\* Forms

\* Authentication UI

\* Problem pages

\* Submission pages

\* Contest pages

\* User profiles

\* Admin pages

\* Displaying submission results



It should \*\*not\*\* contain the actual judging engine.



\---



\## Main structure



```text

CodeVerdict.Web/

├── Controllers/

├── Views/

├── ViewModels/

├── Services/

├── Filters/

├── wwwroot/

├── Program.cs

└── appsettings.json

```



\### Controllers



Controllers receive HTTP requests and call the Application layer.



Example:



```text

POST /submissions/create

&#x20;       ↓

SubmissionsController

&#x20;       ↓

SubmitCode use case

```



Controllers should remain relatively thin.



They should not contain logic such as:



```text

Compile C# code

Start Docker container

Compare output

Calculate verdict

```



That belongs elsewhere.



\---



\## Views



Razor Views are responsible for displaying information.



Examples:



```text

Views/

├── Problems/

├── Submissions/

├── Contests/

├── Account/

└── Shared/

```



\---



\## ViewModels



ViewModels represent the data needed by a specific page.



For example:



```text

SubmitCodeViewModel

ProblemDetailsViewModel

SubmissionDetailsViewModel

ContestDetailsViewModel

```



They should not be used as database entities.



\---



\# 2. CodeVerdict.Application



\*\*Project type:\*\* .NET Class Library



```text

CodeVerdict.Application/

```



\## Responsibility



This layer contains the application's \*\*use cases\*\*.



It answers:



> "What does the application need to do?"



Examples:



\* Create a problem

\* Update a problem

\* Submit code

\* Retrieve a submission

\* Retrieve problems

\* Register for a contest

\* Start judging

\* Get contest standings



\---



\## Structure



```text

CodeVerdict.Application/

├── Problems/

├── Submissions/

├── Contests/

└── Users/

```



For example:



```text

Submissions/

├── SubmitCode/

├── GetSubmission/

├── GetSubmissions/

└── JudgeSubmission/

```



\---



\## Example submission flow



A user submits:



```text

POST /submissions

```



The Web project calls:



```text

SubmitCode

```



The Application layer then:



1\. Validates the submission.

2\. Checks the problem.

3\. Checks the selected language.

4\. Creates a `Submission`.

5\. Stores it.

6\. Places the submission into the judging queue.

7\. Returns the submission ID.



The Application layer does \*\*not\*\* directly execute the user's code.



\---



\# 3. CodeVerdict.Domain



\*\*Project type:\*\* .NET Class Library



```text

CodeVerdict.Domain/

```



\## Responsibility



This is the core business model of CodeVerdict.



It contains things that are fundamentally part of the CodeVerdict system regardless of technology.



It should not depend on:



\* ASP.NET Core

\* EF Core

\* Docker

\* SQL Server

\* HTTP

\* filesystem APIs



\---



\## Entities



```text

Domain/

├── Entities/

│   ├── Problem.cs

│   ├── TestCase.cs

│   ├── Submission.cs

│   ├── Contest.cs

│   └── ContestProblem.cs

│

├── Enums/

│   ├── SubmissionStatus.cs

│   ├── ProgrammingLanguage.cs

│   └── ContestStatus.cs

│

└── ValueObjects/

```



\---



\## Example



A submission can have:



```text

Submission

├── Id

├── ProblemId

├── UserId

├── Language

├── SourceCode

├── Status

├── Score

├── ExecutionTime

└── MemoryUsed

```



The Domain layer defines what a `Submission` means.



It does not care whether the submission is stored in SQL Server or PostgreSQL.



\---



\# 4. CodeVerdict.Infrastructure



\*\*Project type:\*\* .NET Class Library



```text

CodeVerdict.Infrastructure/

```



\## Responsibility



Infrastructure contains implementations that depend on external technologies.



Examples:



\* EF Core

\* Database

\* ASP.NET Identity

\* Repositories

\* Queue implementations

\* File storage

\* External services



\---



\## Structure



```text

CodeVerdict.Infrastructure/

├── Data/

│   ├── CodeVerdictDbContext.cs

│   ├── Configurations/

│   └── Migrations/

│

├── Identity/

│   ├── ApplicationUser.cs

│   └── IdentityConfiguration.cs

│

├── Repositories/

├── Queue/

└── Storage/

```



\---



\# Database



`CodeVerdictDbContext` is the EF Core database context.



It can inherit from:



```text

IdentityDbContext<ApplicationUser>

```



This allows CodeVerdict to use ASP.NET Identity together with its application entities.



For example:



```text

Database

│

├── AspNetUsers

├── AspNetRoles

├── Problems

├── TestCases

├── Submissions

├── Contests

└── ContestProblems

```



\---



\# Identity



Authentication is handled by ASP.NET Identity.



The user account is represented by:



```text

ApplicationUser

```



There should not be a separate:



```text

Domain/Entities/User.cs

```



just to duplicate Identity.



Identity belongs in Infrastructure because it is an implementation detail of the application's authentication system.



\---



\# 5. CodeVerdict.Judge



\*\*Project type:\*\* .NET Class Library



```text

CodeVerdict.Judge/

```



\## Responsibility



This project contains the actual \*\*judging engine\*\*.



It answers:



> "Given source code, a problem, and test cases, what verdict should this submission receive?"



It is independent from ASP.NET MVC.



\---



\## Structure



```text

CodeVerdict.Judge/

├── Engine/

│   ├── JudgeEngine.cs

│   ├── TestRunner.cs

│   └── OutputComparer.cs

│

├── Sandbox/

│   ├── ISandbox.cs

│   ├── DockerSandbox.cs

│   ├── SandboxLimits.cs

│   └── SandboxResult.cs

│

├── Languages/

│   ├── ILanguageRunner.cs

│   ├── CSharp/

│   ├── Cpp/

│   ├── Java/

│   └── Python/

│

├── Checkers/

│   ├── IChecker.cs

│   ├── ExactChecker.cs

│   ├── TokenChecker.cs

│   ├── FloatingPointChecker.cs

│   └── SpecialChecker.cs

│

└── Workers/

&#x20;   └── JudgeWorker.cs

```



\---



\# Judge Pipeline



A submission goes through roughly this process:



```text

Source Code

&#x20;    │

&#x20;    ▼

Language Runner

&#x20;    │

&#x20;    ▼

Compilation

&#x20;    │

&#x20;    ▼

Compiled Program

&#x20;    │

&#x20;    ▼

Sandbox

&#x20;    │

&#x20;    ▼

Test Case

&#x20;    │

&#x20;    ▼

Program Output

&#x20;    │

&#x20;    ▼

Checker

&#x20;    │

&#x20;    ▼

Verdict

```



For example:



```text

Source Code

&#x20;   ↓

C# compiler

&#x20;   ↓

Executable

&#x20;   ↓

Docker container

&#x20;   ↓

Input:

5

&#x20;   ↓

Program

&#x20;   ↓

Output:

120

&#x20;   ↓

Expected:

120

&#x20;   ↓

Accepted

```



\---



\# Compilation



The judge should eventually compile a submission \*\*once\*\*.



It should not do:



```text

Compile

Run Test 1



Compile

Run Test 2



Compile

Run Test 3

```



Instead:



```text

Compile

&#x20;  ↓

Executable

&#x20;  ├── Test 1

&#x20;  ├── Test 2

&#x20;  ├── Test 3

&#x20;  └── Test 4

```



This is significantly more efficient.



\---



\# Sandbox



User-submitted code is untrusted.



It must never simply be executed directly by the ASP.NET server.



The initial sandbox implementation will use Docker.



The sandbox should enforce limits such as:



```text

CPU limit

Memory limit

Process limit

Execution timeout

Network disabled

Filesystem restrictions

Non-root execution

Container cleanup

```



Example conceptual execution:



```text

Docker Container

│

├── User program

├── Input

└── Restricted environment

```



The container should not have access to:



```text

Host filesystem

Database

Docker socket

Internal services

Internet

```



\---



\# Checkers



The checker determines whether the program output is correct.



Initially:



```text

ExactChecker

```



can compare output directly.



Later:



```text

TokenChecker

FloatingPointChecker

SpecialChecker

```



can support more advanced problems.



For example:



```text

Exact:



42

```



versus:



```text

Token:



42  17  91

```



where whitespace differences may not matter.



Eventually CodeVerdict can support custom checkers for problems where multiple valid outputs exist.



\---



\# 6. CodeVerdict.Worker



\*\*Future project type:\*\* .NET Worker Service



```text

CodeVerdict.Worker/

```



This project will be added when CodeVerdict moves from a simple application into a real judge infrastructure.



Its responsibility is:



> Continuously receive submissions that need judging and execute the judging process.



\---



\# Why the Worker Exists



The web server should not sit there waiting for a submission to finish judging.



For example, suppose a user submits a program that takes 5 seconds.



The ideal architecture is:



```text

Web Request

&#x20;   ↓

Create Submission

&#x20;   ↓

Queue Submission

&#x20;   ↓

Return immediately

```



Then:



```text

Worker

&#x20;   ↓

Pick Submission

&#x20;   ↓

Judge

&#x20;   ↓

Save Result

```



The user can then refresh the submission page and see:



```text

Queued

&#x20;  ↓

Compiling

&#x20;  ↓

Running

&#x20;  ↓

Accepted

```



\---



\# Future Worker Architecture



The eventual architecture will look approximately like:



```text

&#x20;                 ┌──────────────────┐

&#x20;                 │  CodeVerdict.Web │

&#x20;                 └────────┬─────────┘

&#x20;                          │

&#x20;                          ▼

&#x20;                 ┌──────────────────┐

&#x20;                 │   Application    │

&#x20;                 └────────┬─────────┘

&#x20;                          │

&#x20;                          ▼

&#x20;                 ┌──────────────────┐

&#x20;                 │   Message Queue  │

&#x20;                 └────────┬─────────┘

&#x20;                          │

&#x20;            ┌─────────────┼─────────────┐

&#x20;            │             │             │

&#x20;            ▼             ▼             ▼

&#x20;      ┌──────────┐  ┌──────────┐  ┌──────────┐

&#x20;      │ Worker 1 │  │ Worker 2 │  │ Worker 3 │

&#x20;      └────┬─────┘  └────┬─────┘  └────┬─────┘

&#x20;           │             │             │

&#x20;           ▼             ▼             ▼

&#x20;      ┌────────────────────────────────────┐

&#x20;      │          CodeVerdict.Judge         │

&#x20;      └────────────────────────────────────┘

&#x20;                      │

&#x20;                      ▼

&#x20;               Docker Sandbox

```



This allows multiple submissions to be judged simultaneously.



\---



\# Worker Responsibilities



A Worker should:



1\. Receive a submission ID.

2\. Retrieve the submission.

3\. Retrieve the problem and test cases.

4\. Mark the submission as `Compiling`.

5\. Compile the source code.

6\. Mark it as `Running`.

7\. Execute the program in the sandbox.

8\. Run the appropriate checker.

9\. Determine the final verdict.

10\. Store the result.

11\. Mark the job as completed.



\---



\# Worker vs Judge



These two projects have different responsibilities.



\## CodeVerdict.Worker



Responsible for:



```text

WHEN should a submission be judged?

WHERE should it be processed?

HOW should jobs be consumed?

```



\## CodeVerdict.Judge



Responsible for:



```text

HOW is code compiled?

HOW is code executed?

HOW are test cases run?

HOW is output checked?

WHAT verdict does the submission receive?

```



Therefore:



```text

Worker

&#x20;  │

&#x20;  │ calls

&#x20;  ▼

Judge

&#x20;  │

&#x20;  │ executes

&#x20;  ▼

Sandbox

```



The Worker should not contain the actual compilation/checking implementation.



\---



\# Multiple Workers



Eventually CodeVerdict should be able to run:



```text

Worker 1

Worker 2

Worker 3

Worker 4

...

Worker N

```



All workers consume from the same queue.



For example:



```text

Queue



Submission #101

Submission #102

Submission #103

Submission #104

Submission #105

```



Workers could process:



```text

Worker 1 → #101

Worker 2 → #102

Worker 3 → #103

Worker 1 → #104

Worker 2 → #105

```



This allows the judging system to scale horizontally.



\---



\# Queue



The initial implementation can use a simple database-backed or in-process queue.



Later, CodeVerdict can use a dedicated message broker.



Possible future technologies include:



```text

RabbitMQ

Redis

Kafka

Azure Service Bus

```



The exact queue technology should remain an Infrastructure concern.



The Application and Judge layers should not be tightly coupled to a specific queue implementation.



\---



\# Worker Failure



The Worker architecture should eventually account for failures.



For example:



```text

Worker starts submission #123

&#x20;       ↓

Worker crashes

&#x20;       ↓

Submission remains "Running"

&#x20;       ↓

Queue detects abandoned job

&#x20;       ↓

Submission is requeued

```



This prevents submissions from becoming permanently stuck.



\---



\# Submission States



A submission will eventually use states such as:



```text

Queued

Compiling

Running

Accepted

WrongAnswer

CompilationError

RuntimeError

TimeLimitExceeded

MemoryLimitExceeded

SystemError

```



The Worker is responsible for updating these states as the submission progresses.



\---



\# Project Dependencies



The intended dependency direction is:



```text

CodeVerdict.Web

&#x20;       │

&#x20;       ▼

CodeVerdict.Application

&#x20;       │

&#x20;       ▼

CodeVerdict.Domain

```



Infrastructure provides implementations:



```text

CodeVerdict.Infrastructure

&#x20;       │

&#x20;       ├── EF Core

&#x20;       ├── Identity

&#x20;       ├── Queue

&#x20;       └── Storage

```



The Judge is separate:



```text

CodeVerdict.Judge

&#x20;       │

&#x20;       ├── Sandbox

&#x20;       ├── Languages

&#x20;       ├── Checkers

&#x20;       └── Judge Engine

```



The future Worker connects the queue to the Judge:



```text

CodeVerdict.Worker

&#x20;       │

&#x20;       ├── Queue

&#x20;       │

&#x20;       └── CodeVerdict.Judge

```



\---



\# Complete Project Structure



The final solution will approximately look like:



```text

CodeVerdict/

│

├── src/

│   │

│   ├── CodeVerdict.Web/

│   │

│   ├── CodeVerdict.Application/

│   │

│   ├── CodeVerdict.Domain/

│   │

│   ├── CodeVerdict.Infrastructure/

│   │

│   ├── CodeVerdict.Judge/

│   │

│   └── CodeVerdict.Worker/

│

├── tests/

│   ├── CodeVerdict.Domain.Tests/

│   ├── CodeVerdict.Application.Tests/

│   ├── CodeVerdict.Judge.Tests/

│   └── CodeVerdict.Web.Tests/

│

├── docker/

│   ├── csharp/

│   ├── cpp/

│   ├── java/

│   └── python/

│

├── docs/

│   ├── architecture.md

│   ├── judging.md

│   └── security.md

│

├── docker-compose.yml

├── .gitignore

└── README.md

```



\---



\# Development Order



CodeVerdict should not be built in its final distributed form immediately.



A practical implementation order is:



```text

1\. Domain

&#x20;     ↓

2\. Application

&#x20;     ↓

3\. Infrastructure + EF Core

&#x20;     ↓

4\. Web

&#x20;     ↓

5\. Judge

&#x20;     ↓

6\. Docker Sandbox

&#x20;     ↓

7\. Submission System

&#x20;     ↓

8\. Worker

&#x20;     ↓

9\. Queue

&#x20;     ↓

10\. Multiple Workers

&#x20;     ↓

11\. Multiple Languages

&#x20;     ↓

12\. Advanced Judge Features

&#x20;     ↓

13\. Contests

&#x20;     ↓

14\. Production Security / Scaling

```



Initially, the system can even execute judging directly from an application/background process.



Once the core judge works correctly, introduce:



```text

Queue

&#x20; ↓

Worker

&#x20; ↓

Judge

&#x20; ↓

Sandbox

```



Then multiple workers can be added without redesigning the actual judging engine.



\---



\# Core Principle



Each project should have one clear responsibility:



| Project                      | Responsibility                                |

| ---------------------------- | --------------------------------------------- |

| `CodeVerdict.Web`            | Website and HTTP/MVC                          |

| `CodeVerdict.Application`    | Application use cases                         |

| `CodeVerdict.Domain`         | Core business model                           |

| `CodeVerdict.Infrastructure` | Database, Identity, queues, storage           |

| `CodeVerdict.Judge`          | Compile, execute, check, and produce verdicts |

| `CodeVerdict.Worker`         | Consume judging jobs and invoke the Judge     |

| `CodeVerdict.\*.Tests`        | Automated testing                             |



The most important separation is:



```text

WEB

&#x20;↓

APPLICATION

&#x20;↓

QUEUE

&#x20;↓

WORKER

&#x20;↓

JUDGE

&#x20;↓

SANDBOX

```



The Web application should never become the judge itself.



The Judge should never become the website.



The Worker should never become the judging engine.



Each component should remain independently replaceable and scalable.



