USE [InterviewAppDb];
GO

SET XACT_ABORT ON;
BEGIN TRAN;

IF OBJECT_ID(N'[dbo].[CandidateQuestions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CandidateQuestions]
    (
        [CandidateId] int NOT NULL,
        [QuestionId] int NOT NULL,
        [OrderIndex] int NOT NULL,
        [AssignedAt] datetime2 NOT NULL CONSTRAINT [DF_CandidateQuestions_AssignedAt] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_CandidateQuestions] PRIMARY KEY ([CandidateId], [QuestionId]),
        CONSTRAINT [FK_CandidateQuestions_Candidates_CandidateId]
            FOREIGN KEY ([CandidateId]) REFERENCES [dbo].[Candidates]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CandidateQuestions_Questions_QuestionId]
            FOREIGN KEY ([QuestionId]) REFERENCES [dbo].[Questions]([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [IX_CandidateQuestions_CandidateId_OrderIndex]
        ON [dbo].[CandidateQuestions]([CandidateId], [OrderIndex]);

    CREATE INDEX [IX_CandidateQuestions_QuestionId]
        ON [dbo].[CandidateQuestions]([QuestionId]);
END;

DECLARE @TrackId int;

SELECT @TrackId = [Id]
FROM [dbo].[Tracks]
WHERE [Slug] = N'dotnet-full-stack';

IF @TrackId IS NULL
BEGIN
    INSERT INTO [dbo].[Tracks] ([Name], [Slug], [Description], [IsActive], [CreatedAt])
    VALUES
    (
        N'.NET Full-Stack Developer',
        N'dotnet-full-stack',
        N'.NET Core, C#, REST APIs, React or Angular, AWS, microservices, database design, and performance tuning.',
        1,
        SYSUTCDATETIME()
    );

    SET @TrackId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    UPDATE [dbo].[Tracks]
    SET [Name] = N'.NET Full-Stack Developer',
        [Description] = N'.NET Core, C#, REST APIs, React or Angular, AWS, microservices, database design, and performance tuning.',
        [IsActive] = 1
    WHERE [Id] = @TrackId;
END;

DECLARE @Topics TABLE
(
    [Name] nvarchar(150) NOT NULL,
    [SortOrder] int NOT NULL
);

INSERT INTO @Topics ([Name], [SortOrder])
VALUES
    (N'.NET Core and C#', 1),
    (N'REST APIs and Web APIs', 2),
    (N'Frontend with React or Angular', 3),
    (N'AWS, Microservices, and Cloud Architecture', 4),
    (N'Database Design and Performance', 5);

INSERT INTO [dbo].[Topics] ([TrackId], [Name], [SortOrder])
SELECT @TrackId, t.[Name], t.[SortOrder]
FROM @Topics t
WHERE NOT EXISTS
(
    SELECT 1
    FROM [dbo].[Topics] existing
    WHERE existing.[TrackId] = @TrackId
      AND existing.[Name] = t.[Name]
);

UPDATE existing
SET existing.[SortOrder] = t.[SortOrder]
FROM [dbo].[Topics] existing
JOIN @Topics t ON t.[Name] = existing.[Name]
WHERE existing.[TrackId] = @TrackId;

DECLARE @Questions TABLE
(
    [TopicName] nvarchar(150) NOT NULL,
    [QuestionText] nvarchar(2000) NOT NULL,
    [HintText] nvarchar(500) NULL,
    [OrderIndex] int NOT NULL
);

INSERT INTO @Questions ([TopicName], [QuestionText], [HintText], [OrderIndex])
VALUES
    (N'.NET Core and C#', N'Explain the request processing pipeline in ASP.NET Core and where middleware fits in.', N'Mention order, short-circuiting, and common middleware.', 1),
    (N'.NET Core and C#', N'How do you choose between scoped, transient, and singleton service lifetimes in dependency injection?', N'Include examples and common mistakes.', 2),
    (N'.NET Core and C#', N'What are async and await in C#, and how would you avoid deadlocks or thread pool starvation?', N'Cover Task, ConfigureAwait where relevant, and blocking calls.', 3),
    (N'.NET Core and C#', N'Explain records, classes, structs, and when you would use each in modern C#.', N'Compare equality, mutability, and allocation behavior.', 4),
    (N'.NET Core and C#', N'How do you implement centralized exception handling and consistent error responses in ASP.NET Core?', N'Mention middleware, filters, ProblemDetails, and logging.', 5),
    (N'.NET Core and C#', N'What is the Options pattern in .NET Core and how would you validate configuration at startup?', N'Include IOptions, IOptionsSnapshot, and IOptionsMonitor.', 6),
    (N'.NET Core and C#', N'How do you structure a large .NET solution for maintainability across API, domain, infrastructure, and tests?', N'Discuss boundaries and dependency direction.', 7),
    (N'.NET Core and C#', N'Explain cancellation tokens and how you would propagate cancellation through a Web API call chain.', N'Include database calls and external HTTP calls.', 8),
    (N'.NET Core and C#', N'How would you secure secrets and environment-specific configuration in a .NET application?', N'Mention local development, cloud, and production practices.', 9),
    (N'.NET Core and C#', N'What testing strategy would you use for a senior .NET full-stack project?', N'Cover unit, integration, contract, and end-to-end tests.', 10),

    (N'REST APIs and Web APIs', N'How do you design RESTful endpoints for a resource that supports search, filtering, sorting, and pagination?', N'Include URI design and query parameters.', 1),
    (N'REST APIs and Web APIs', N'What status codes and response shapes would you use for validation errors, not found, conflicts, and server errors?', N'Mention consistency and client usability.', 2),
    (N'REST APIs and Web APIs', N'Explain idempotency in REST APIs and where it matters in enterprise applications.', N'Compare GET, PUT, PATCH, POST, and DELETE.', 3),
    (N'REST APIs and Web APIs', N'How would you version a public Web API without breaking existing clients?', N'Cover URL, header, media type, and compatibility tradeoffs.', 4),
    (N'REST APIs and Web APIs', N'How do you implement authentication and authorization for APIs used by a React or Angular frontend?', N'Mention JWT, cookies, claims, roles, and CORS.', 5),
    (N'REST APIs and Web APIs', N'What is CORS, why does it happen, and how should it be configured safely?', N'Include allowed origins, methods, headers, and credentials.', 6),
    (N'REST APIs and Web APIs', N'How would you make an API resilient when calling slow or unreliable downstream services?', N'Mention timeout, retry, circuit breaker, and bulkhead patterns.', 7),
    (N'REST APIs and Web APIs', N'How do you document and test Web APIs in a team environment?', N'Include OpenAPI, Swagger, contract testing, and examples.', 8),
    (N'REST APIs and Web APIs', N'Explain model binding and validation in ASP.NET Core Web API.', N'Mention DTOs, data annotations, FluentValidation, and ModelState.', 9),
    (N'REST APIs and Web APIs', N'How would you troubleshoot a production API that has high latency and intermittent 500 errors?', N'Cover logs, traces, metrics, dependency checks, and rollback.', 10),

    (N'Frontend with React or Angular', N'How do you decide whether to use React or Angular for an enterprise full-stack application?', N'Compare ecosystem, team skills, structure, and maintainability.', 1),
    (N'Frontend with React or Angular', N'Explain component state, server state, and global state in React. How do you choose where data belongs?', N'Mention hooks, context, Redux or query libraries.', 2),
    (N'Frontend with React or Angular', N'How would you structure a React application that consumes multiple REST APIs?', N'Cover services, hooks, error handling, and reusable components.', 3),
    (N'Frontend with React or Angular', N'Explain Angular services, dependency injection, and RxJS usage in API-driven applications.', N'Mention observables, subscriptions, and async pipe.', 4),
    (N'Frontend with React or Angular', N'How do you handle authentication state and route protection in a SPA?', N'Include token storage tradeoffs and refresh flow.', 5),
    (N'Frontend with React or Angular', N'What frontend performance issues have you seen, and how did you fix them?', N'Mention rendering, bundle size, memoization, lazy loading, and network calls.', 6),
    (N'Frontend with React or Angular', N'How do you design reusable UI components for forms, validation, and API errors?', N'Cover composition, validation messages, and accessibility.', 7),
    (N'Frontend with React or Angular', N'How would you prevent duplicate form submissions and handle retry behavior in a SPA?', N'Mention loading states, idempotency, and API error mapping.', 8),
    (N'Frontend with React or Angular', N'Explain how you would test React or Angular components that depend on API data.', N'Cover mocks, integration tests, and user-focused assertions.', 9),
    (N'Frontend with React or Angular', N'How do you keep frontend and backend contracts aligned across teams?', N'Mention OpenAPI, generated clients, contract tests, and versioning.', 10),

    (N'AWS, Microservices, and Cloud Architecture', N'How would you deploy a .NET Core Web API to AWS for a scalable production workload?', N'Compare EC2, ECS, EKS, Lambda, and Elastic Beanstalk where relevant.', 1),
    (N'AWS, Microservices, and Cloud Architecture', N'Which AWS services have you used for logs, metrics, secrets, storage, queues, and databases?', N'Explain concrete use cases, not only names.', 2),
    (N'AWS, Microservices, and Cloud Architecture', N'Explain how you would design a microservice boundary for an order or interview platform.', N'Cover ownership, data boundaries, and communication.', 3),
    (N'AWS, Microservices, and Cloud Architecture', N'What are the tradeoffs between synchronous REST communication and asynchronous messaging between services?', N'Mention latency, coupling, reliability, and consistency.', 4),
    (N'AWS, Microservices, and Cloud Architecture', N'How do you handle distributed transactions and eventual consistency in microservices?', N'Include saga, outbox, retries, and compensating actions.', 5),
    (N'AWS, Microservices, and Cloud Architecture', N'How would you implement observability across multiple .NET microservices?', N'Mention correlation IDs, structured logs, metrics, traces, and dashboards.', 6),
    (N'AWS, Microservices, and Cloud Architecture', N'What is cloud-native architecture, and how does it change application design?', N'Cover statelessness, autoscaling, health checks, and managed services.', 7),
    (N'AWS, Microservices, and Cloud Architecture', N'How do you secure service-to-service communication and external API access in a cloud environment?', N'Mention IAM, network controls, TLS, and token-based auth.', 8),
    (N'AWS, Microservices, and Cloud Architecture', N'How would you design CI/CD for a .NET full-stack application deployed on AWS?', N'Include build, tests, artifacts, approvals, deployments, and rollback.', 9),
    (N'AWS, Microservices, and Cloud Architecture', N'How do you handle configuration, feature flags, and zero-downtime deployment for microservices?', N'Mention config stores, blue-green or canary, and backward compatibility.', 10),

    (N'Database Design and Performance', N'How would you design a relational database schema for candidates, questions, answers, and interview results?', N'Cover normalization, keys, constraints, and indexes.', 1),
    (N'Database Design and Performance', N'Explain how you identify and fix a slow SQL query in production.', N'Mention execution plans, indexes, statistics, locking, and query shape.', 2),
    (N'Database Design and Performance', N'What indexing strategy would you use for search and reporting screens in an admin dashboard?', N'Include composite indexes and selectivity.', 3),
    (N'Database Design and Performance', N'How do you choose between eager loading, lazy loading, explicit loading, and projections in Entity Framework Core?', N'Mention N+1 queries and DTO projection.', 4),
    (N'Database Design and Performance', N'What are common Entity Framework Core performance pitfalls and how do you avoid them?', N'Include tracking, batching, includes, pagination, and compiled queries.', 5),
    (N'Database Design and Performance', N'How would you design pagination for large datasets in SQL Server and APIs?', N'Compare offset and keyset pagination.', 6),
    (N'Database Design and Performance', N'How do you handle database migrations safely in production?', N'Cover backward-compatible changes, backups, rollback, and deployment order.', 7),
    (N'Database Design and Performance', N'Explain transaction isolation levels and how they affect concurrency and locking.', N'Mention read committed, snapshot, repeatable read, and serializable.', 8),
    (N'Database Design and Performance', N'How would you cache data in a .NET application without serving stale or incorrect results?', N'Mention cache keys, invalidation, TTL, and distributed cache.', 9),
    (N'Database Design and Performance', N'Describe an end-to-end performance tuning approach for a .NET full-stack application.', N'Cover frontend, API, database, network, and cloud infrastructure.', 10);

INSERT INTO [dbo].[Questions] ([TopicId], [QuestionText], [HintText], [OrderIndex], [IsActive], [CreatedAt])
SELECT topic.[Id], q.[QuestionText], q.[HintText], q.[OrderIndex], 1, SYSUTCDATETIME()
FROM @Questions q
JOIN [dbo].[Topics] topic
  ON topic.[TrackId] = @TrackId
 AND topic.[Name] = q.[TopicName]
WHERE NOT EXISTS
(
    SELECT 1
    FROM [dbo].[Questions] existing
    WHERE existing.[TopicId] = topic.[Id]
      AND existing.[QuestionText] = q.[QuestionText]
);

UPDATE existing
SET existing.[HintText] = q.[HintText],
    existing.[OrderIndex] = q.[OrderIndex],
    existing.[IsActive] = 1
FROM [dbo].[Questions] existing
JOIN [dbo].[Topics] topic ON topic.[Id] = existing.[TopicId]
JOIN @Questions q ON q.[TopicName] = topic.[Name]
                 AND q.[QuestionText] = existing.[QuestionText]
WHERE topic.[TrackId] = @TrackId;

COMMIT;

SELECT track.[Id] AS TrackId,
       track.[Name] AS TrackName,
       COUNT(DISTINCT topic.[Id]) AS TopicCount,
       COUNT(question.[Id]) AS QuestionCount
FROM [dbo].[Tracks] track
LEFT JOIN [dbo].[Topics] topic ON topic.[TrackId] = track.[Id]
LEFT JOIN [dbo].[Questions] question ON question.[TopicId] = topic.[Id]
WHERE track.[Id] = @TrackId
GROUP BY track.[Id], track.[Name];
GO
