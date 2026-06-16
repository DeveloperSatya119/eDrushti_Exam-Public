USE [InterviewAppDb];
GO

SET XACT_ABORT ON;
BEGIN TRAN;

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
        N'.NET Core, C#, Angular/React, AWS, SQL, REST APIs, and Microservices MCQ assessment.',
        1,
        SYSUTCDATETIME()
    );

    SET @TrackId = SCOPE_IDENTITY();
END;

DECLARE @Topics TABLE
(
    [Name] nvarchar(150) NOT NULL,
    [SortOrder] int NOT NULL
);

INSERT INTO @Topics ([Name], [SortOrder])
VALUES
    (N'.NET Core and C#', 1),
    (N'Angular / React', 2),
    (N'AWS', 3),
    (N'SQL', 4),
    (N'APIs / REST APIs / Microservices', 5);

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

DELETE cq
FROM [dbo].[CandidateQuestions] cq
JOIN [dbo].[Questions] q ON q.[Id] = cq.[QuestionId]
JOIN [dbo].[Topics] topic ON topic.[Id] = q.[TopicId]
WHERE topic.[TrackId] = @TrackId;

DELETE answer
FROM [dbo].[CandidateAnswers] answer
JOIN [dbo].[Questions] q ON q.[Id] = answer.[QuestionId]
JOIN [dbo].[Topics] topic ON topic.[Id] = q.[TopicId]
WHERE topic.[TrackId] = @TrackId;

DELETE q
FROM [dbo].[Questions] q
JOIN [dbo].[Topics] topic ON topic.[Id] = q.[TopicId]
WHERE topic.[TrackId] = @TrackId;

DELETE topic
FROM [dbo].[Topics] topic
WHERE topic.[TrackId] = @TrackId
  AND NOT EXISTS (SELECT 1 FROM @Topics keep WHERE keep.[Name] = topic.[Name]);

UPDATE c
SET c.[ScorePercent] = NULL,
    c.[ResultStatus] = NULL,
    c.[SubmittedAt] = NULL
FROM [dbo].[Candidates] c
WHERE c.[TrackId] = @TrackId
  AND c.[IsAdmin] = 0;

DECLARE @Questions TABLE
(
    [TopicName] nvarchar(150) NOT NULL,
    [QuestionText] nvarchar(2000) NOT NULL,
    [OptionA] nvarchar(500) NOT NULL,
    [OptionB] nvarchar(500) NOT NULL,
    [OptionC] nvarchar(500) NOT NULL,
    [OptionD] nvarchar(500) NOT NULL,
    [CorrectAnswer] nvarchar(10) NOT NULL,
    [OrderIndex] int NOT NULL
);

INSERT INTO @Questions ([TopicName], [QuestionText], [OptionA], [OptionB], [OptionC], [OptionD], [CorrectAnswer], [OrderIndex])
VALUES
    (N'.NET Core and C#', N'Which of the following is a feature of .NET Core?', N'Cross-platform support', N'Windows-only support', N'No cloud support', N'No API support', N'A', 1),
    (N'.NET Core and C#', N'What is Dependency Injection in .NET Core?', N'Database connection pooling', N'A design pattern for providing dependencies to a class', N'Authentication mechanism', N'Logging framework', N'B', 2),
    (N'.NET Core and C#', N'Which keyword is used to inherit a class in C#?', N'implements', N'inherits', N':', N'extends', N'C', 3),
    (N'.NET Core and C#', N'What is the purpose of the async keyword?', N'Makes code run faster', N'Enables asynchronous programming', N'Creates threads', N'Prevents exceptions', N'B', 4),
    (N'.NET Core and C#', N'Which collection provides key-value pair storage?', N'List<T>', N'Array', N'Dictionary<TKey, TValue>', N'Queue', N'C', 5),
    (N'.NET Core and C#', N'What is Middleware in ASP.NET Core?', N'Database Layer', N'UI Component', N'Software component that handles requests/responses', N'Authentication Token', N'C', 6),
    (N'.NET Core and C#', N'Which HTTP method is generally used to update a resource?', N'GET', N'POST', N'PUT', N'OPTIONS', N'C', 7),
    (N'.NET Core and C#', N'What is the purpose of Entity Framework Core?', N'Frontend Development', N'ORM Framework', N'API Gateway', N'Logging Tool', N'B', 8),
    (N'.NET Core and C#', N'What is the default lifetime of services registered using AddScoped()?', N'Singleton', N'Application Lifetime', N'Per Request', N'Per Thread', N'C', 9),
    (N'.NET Core and C#', N'Which principle does SOLID''s "S" represent?', N'Secure Principle', N'Single Responsibility Principle', N'Service Principle', N'Simple Principle', N'B', 10),

    (N'Angular / React', N'React is primarily a:', N'Database', N'Backend Framework', N'UI Library', N'Operating System', N'C', 1),
    (N'Angular / React', N'Which hook is used for managing state in React?', N'useEffect', N'useState', N'useMemo', N'useRef', N'B', 2),
    (N'Angular / React', N'What is JSX?', N'Java XML', N'JavaScript Extension Syntax', N'JSON Extension', N'Java Syntax XML', N'B', 3),
    (N'Angular / React', N'Which Angular decorator defines a component?', N'@Injectable', N'@Module', N'@Component', N'@Controller', N'C', 4),
    (N'Angular / React', N'What is Virtual DOM in React?', N'Browser Memory', N'Lightweight copy of Real DOM', N'Database Cache', N'API Layer', N'B', 5),
    (N'Angular / React', N'Which directive is used for looping in Angular?', N'*ngIf', N'*ngLoop', N'*ngFor', N'*ngRepeat', N'C', 6),
    (N'Angular / React', N'What is the purpose of useEffect()?', N'Manage State', N'Handle Side Effects', N'Create Components', N'Manage Routing', N'B', 7),
    (N'Angular / React', N'Which Angular feature is used for Dependency Injection?', N'Services', N'Pipes', N'Directives', N'Guards', N'A', 8),
    (N'Angular / React', N'React components should ideally be:', N'Stateful only', N'Reusable', N'Database-driven', N'Server-side only', N'B', 9),
    (N'Angular / React', N'Which routing library is commonly used with React?', N'React-Routes', N'Angular Router', N'React Router', N'RouteJS', N'C', 10),

    (N'AWS', N'What is Amazon EC2?', N'Database Service', N'Virtual Server Service', N'Storage Service', N'Networking Service', N'B', 1),
    (N'AWS', N'Which AWS service provides object storage?', N'RDS', N'Lambda', N'S3', N'EC2', N'C', 2),
    (N'AWS', N'What is AWS Lambda?', N'Container Service', N'Serverless Compute Service', N'Database Service', N'Monitoring Service', N'B', 3),
    (N'AWS', N'Which AWS service is used for relational databases?', N'DynamoDB', N'RDS', N'S3', N'CloudFront', N'B', 4),
    (N'AWS', N'What is an Auto Scaling Group?', N'Storage Expansion', N'Automatic Scaling of Compute Resources', N'Database Replication', N'Security Service', N'B', 5),
    (N'AWS', N'Which AWS service is used for monitoring?', N'CloudWatch', N'Route53', N'IAM', N'ECS', N'A', 6),
    (N'AWS', N'IAM stands for:', N'Internet Access Management', N'Identity and Access Management', N'Internal Access Module', N'Identity Authorization Model', N'B', 7),
    (N'AWS', N'What is the purpose of Route 53?', N'Monitoring', N'DNS Management', N'Storage', N'Compute', N'B', 8),
    (N'AWS', N'Which service helps distribute content globally?', N'S3', N'CloudFront', N'Lambda', N'IAM', N'B', 9),
    (N'AWS', N'DynamoDB is:', N'Relational Database', N'NoSQL Database', N'Cache Service', N'File Storage', N'B', 10),

    (N'SQL', N'Which SQL command retrieves data?', N'INSERT', N'UPDATE', N'SELECT', N'DELETE', N'C', 1),
    (N'SQL', N'Which clause filters rows?', N'ORDER BY', N'GROUP BY', N'WHERE', N'JOIN', N'C', 2),
    (N'SQL', N'Which JOIN returns matching records from both tables?', N'LEFT JOIN', N'RIGHT JOIN', N'INNER JOIN', N'FULL JOIN', N'C', 3),
    (N'SQL', N'What does a Primary Key ensure?', N'Duplicate values', N'Unique records', N'Null values', N'Foreign relationships', N'B', 4),
    (N'SQL', N'Which function returns the number of rows?', N'SUM()', N'COUNT()', N'AVG()', N'MAX()', N'B', 5),
    (N'SQL', N'Which SQL statement modifies existing data?', N'INSERT', N'ALTER', N'UPDATE', N'CREATE', N'C', 6),
    (N'SQL', N'What is an Index used for?', N'Security', N'Faster Query Performance', N'Backup', N'Replication', N'B', 7),
    (N'SQL', N'Which clause is used to group records?', N'GROUP BY', N'ORDER BY', N'WHERE', N'HAVING', N'A', 8),
    (N'SQL', N'What is a Foreign Key?', N'Unique Key', N'Key linking tables', N'Index Key', N'Cluster Key', N'B', 9),
    (N'SQL', N'Which operator checks for NULL values?', N'=', N'NULL', N'IS NULL', N'LIKE', N'C', 10),

    (N'APIs / REST APIs / Microservices', N'What does REST stand for?', N'Remote State Transfer', N'Representational State Transfer', N'Resource State Transfer', N'Runtime Service Transfer', N'B', 1),
    (N'APIs / REST APIs / Microservices', N'Which HTTP method is typically used to create a resource?', N'GET', N'PUT', N'POST', N'DELETE', N'C', 2),
    (N'APIs / REST APIs / Microservices', N'Which status code indicates success?', N'200', N'404', N'500', N'401', N'A', 3),
    (N'APIs / REST APIs / Microservices', N'Which status code means "Not Found"?', N'200', N'201', N'404', N'500', N'C', 4),
    (N'APIs / REST APIs / Microservices', N'JSON stands for:', N'Java Standard Object Notation', N'JavaScript Object Notation', N'Java Serialized Object Network', N'JavaScript Open Notation', N'B', 5),
    (N'APIs / REST APIs / Microservices', N'What is an API Endpoint?', N'Database Table', N'URL where API can be accessed', N'Authentication Key', N'Server Instance', N'B', 6),
    (N'APIs / REST APIs / Microservices', N'Which authentication method is commonly used in modern APIs?', N'FTP', N'JWT', N'SMTP', N'Telnet', N'B', 7),
    (N'APIs / REST APIs / Microservices', N'What is a Microservice?', N'Small Database', N'Independent deployable service', N'API Documentation', N'Frontend Component', N'B', 8),
    (N'APIs / REST APIs / Microservices', N'Which HTTP method is idempotent?', N'POST', N'PUT', N'PATCH', N'CONNECT', N'B', 9),
    (N'APIs / REST APIs / Microservices', N'Swagger/OpenAPI is used for:', N'Database Design', N'API Documentation and Testing', N'Authentication', N'Logging', N'B', 10);

INSERT INTO [dbo].[Questions]
(
    [TopicId],
    [QuestionText],
    [HintText],
    [QuestionType],
    [OptionA],
    [OptionB],
    [OptionC],
    [OptionD],
    [CorrectAnswer],
    [OrderIndex],
    [IsActive],
    [CreatedAt]
)
SELECT topic.[Id],
       q.[QuestionText],
       NULL,
       N'MCQ',
       q.[OptionA],
       q.[OptionB],
       q.[OptionC],
       q.[OptionD],
       q.[CorrectAnswer],
       q.[OrderIndex],
       1,
       SYSUTCDATETIME()
FROM @Questions q
JOIN [dbo].[Topics] topic
  ON topic.[TrackId] = @TrackId
 AND topic.[Name] = q.[TopicName];

INSERT INTO [dbo].[CandidateQuestions] ([CandidateId], [QuestionId], [OrderIndex], [AssignedAt])
SELECT picked.[CandidateId], picked.[QuestionId], picked.[rn], SYSUTCDATETIME()
FROM
(
    SELECT c.[Id] AS CandidateId,
           q.[Id] AS QuestionId,
           ROW_NUMBER() OVER (PARTITION BY c.[Id] ORDER BY NEWID()) AS rn
    FROM [dbo].[Candidates] c
    JOIN [dbo].[Questions] q ON q.[QuestionType] = N'MCQ' AND q.[IsActive] = 1
    JOIN [dbo].[Topics] topic ON topic.[Id] = q.[TopicId] AND topic.[TrackId] = c.[TrackId]
    WHERE c.[TrackId] = @TrackId
      AND c.[IsAdmin] = 0
      AND NOT EXISTS (SELECT 1 FROM [dbo].[CandidateAnswers] answer WHERE answer.[CandidateId] = c.[Id])
) picked
WHERE picked.[rn] <= 20;

COMMIT;

SELECT track.[Name] AS TrackName,
       COUNT(DISTINCT topic.[Id]) AS TopicCount,
       COUNT(q.[Id]) AS QuestionCount,
       SUM(CASE WHEN q.[QuestionType] = N'MCQ' THEN 1 ELSE 0 END) AS McqCount
FROM [dbo].[Tracks] track
JOIN [dbo].[Topics] topic ON topic.[TrackId] = track.[Id]
JOIN [dbo].[Questions] q ON q.[TopicId] = topic.[Id]
WHERE track.[Id] = @TrackId
GROUP BY track.[Name];
GO
