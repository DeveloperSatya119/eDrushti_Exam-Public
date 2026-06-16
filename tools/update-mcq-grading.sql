USE [InterviewAppDb];
GO

SET XACT_ABORT ON;

IF COL_LENGTH('dbo.Questions', 'QuestionType') IS NULL
    ALTER TABLE [dbo].[Questions] ADD [QuestionType] nvarchar(20) NOT NULL CONSTRAINT [DF_Questions_QuestionType] DEFAULT (N'Text');

IF COL_LENGTH('dbo.Questions', 'OptionA') IS NULL
    ALTER TABLE [dbo].[Questions] ADD [OptionA] nvarchar(500) NULL;

IF COL_LENGTH('dbo.Questions', 'OptionB') IS NULL
    ALTER TABLE [dbo].[Questions] ADD [OptionB] nvarchar(500) NULL;

IF COL_LENGTH('dbo.Questions', 'OptionC') IS NULL
    ALTER TABLE [dbo].[Questions] ADD [OptionC] nvarchar(500) NULL;

IF COL_LENGTH('dbo.Questions', 'OptionD') IS NULL
    ALTER TABLE [dbo].[Questions] ADD [OptionD] nvarchar(500) NULL;

IF COL_LENGTH('dbo.Questions', 'CorrectAnswer') IS NULL
    ALTER TABLE [dbo].[Questions] ADD [CorrectAnswer] nvarchar(10) NULL;

IF COL_LENGTH('dbo.CandidateAnswers', 'IsCorrect') IS NULL
    ALTER TABLE [dbo].[CandidateAnswers] ADD [IsCorrect] bit NULL;

IF COL_LENGTH('dbo.CandidateAnswers', 'Score') IS NULL
    ALTER TABLE [dbo].[CandidateAnswers] ADD [Score] int NOT NULL CONSTRAINT [DF_CandidateAnswers_Score] DEFAULT (0);

IF COL_LENGTH('dbo.Candidates', 'ScorePercent') IS NULL
    ALTER TABLE [dbo].[Candidates] ADD [ScorePercent] decimal(5,2) NULL;

IF COL_LENGTH('dbo.Candidates', 'ResultStatus') IS NULL
    ALTER TABLE [dbo].[Candidates] ADD [ResultStatus] nvarchar(20) NULL;

IF COL_LENGTH('dbo.Candidates', 'SubmittedAt') IS NULL
    ALTER TABLE [dbo].[Candidates] ADD [SubmittedAt] datetime2 NULL;

GO

SET XACT_ABORT ON;
BEGIN TRAN;

;WITH DotNetQuestions AS
(
    SELECT q.Id,
           q.QuestionText,
           topic.Name AS TopicName,
           ROW_NUMBER() OVER (PARTITION BY topic.Id ORDER BY q.OrderIndex, q.Id) AS TopicQuestionNumber,
           ((ROW_NUMBER() OVER (ORDER BY topic.SortOrder, q.OrderIndex, q.Id) - 1) % 4) + 1 AS CorrectSlot
    FROM dbo.Questions q
    JOIN dbo.Topics topic ON topic.Id = q.TopicId
    JOIN dbo.Tracks track ON track.Id = topic.TrackId
    WHERE track.Slug = N'dotnet-full-stack'
)
UPDATE q
SET q.QuestionType = N'MCQ',
    q.QuestionText = CONCAT(N'Which option is most appropriate for this senior .NET full-stack scenario: ', d.QuestionText),
    q.CorrectAnswer = CASE d.CorrectSlot WHEN 1 THEN N'A' WHEN 2 THEN N'B' WHEN 3 THEN N'C' ELSE N'D' END,
    q.OptionA = CASE d.CorrectSlot
        WHEN 1 THEN v.CorrectOption
        WHEN 2 THEN v.DistractorOne
        WHEN 3 THEN v.DistractorTwo
        ELSE v.DistractorThree
    END,
    q.OptionB = CASE d.CorrectSlot
        WHEN 1 THEN v.DistractorOne
        WHEN 2 THEN v.CorrectOption
        WHEN 3 THEN v.DistractorThree
        ELSE v.DistractorTwo
    END,
    q.OptionC = CASE d.CorrectSlot
        WHEN 1 THEN v.DistractorTwo
        WHEN 2 THEN v.DistractorThree
        WHEN 3 THEN v.CorrectOption
        ELSE v.DistractorOne
    END,
    q.OptionD = CASE d.CorrectSlot
        WHEN 1 THEN v.DistractorThree
        WHEN 2 THEN v.DistractorTwo
        WHEN 3 THEN v.DistractorOne
        ELSE v.CorrectOption
    END
FROM dbo.Questions q
JOIN DotNetQuestions d ON d.Id = q.Id
CROSS APPLY
(
    SELECT
        CorrectOption = CASE d.TopicName
            WHEN N'.NET Core and C#' THEN N'Use idiomatic C# and ASP.NET Core patterns with dependency injection, async APIs, configuration, tests, and clear separation of concerns.'
            WHEN N'REST APIs and Web APIs' THEN N'Design a consistent REST API with proper status codes, validation, authentication, observability, resilience, and backward-compatible contracts.'
            WHEN N'Frontend with React or Angular' THEN N'Build a maintainable SPA with component boundaries, typed API integration, route protection, state management, validation, and performance-aware rendering.'
            WHEN N'AWS, Microservices, and Cloud Architecture' THEN N'Use cloud-native design with appropriate AWS managed services, stateless services, secure configuration, observability, CI/CD, and resilient service communication.'
            ELSE N'Use normalized schema design, correct indexes, query plan analysis, EF Core projections, safe migrations, caching, and end-to-end performance measurement.'
        END,
        DistractorOne = N'Put all business logic directly inside UI event handlers and controllers so the feature can be completed quickly without service boundaries.',
        DistractorTwo = N'Ignore validation, logging, metrics, and automated tests until after production issues are reported by users.',
        DistractorThree = N'Prefer tightly coupled synchronous calls and unindexed database queries because they are simpler to write initially.'
) v;

UPDATE existing
SET existing.ScorePercent = scored.ScorePercent,
    existing.ResultStatus = CASE WHEN scored.ScorePercent >= 80 THEN N'Pass' ELSE N'Fail' END,
    existing.SubmittedAt = scored.SubmittedAt
FROM dbo.Candidates existing
JOIN
(
    SELECT c.Id,
           CAST(ROUND(100.0 * SUM(CASE WHEN a.IsCorrect = 1 THEN 1 ELSE 0 END) / NULLIF(COUNT(q.Id), 0), 2) AS decimal(5,2)) AS ScorePercent,
           MAX(a.SubmittedAt) AS SubmittedAt
    FROM dbo.Candidates c
    JOIN dbo.CandidateAnswers a ON a.CandidateId = c.Id
    JOIN dbo.Questions q ON q.Id = a.QuestionId AND q.QuestionType = N'MCQ'
    GROUP BY c.Id
) scored ON scored.Id = existing.Id;

COMMIT;

SELECT track.Name AS TrackName,
       COUNT(question.Id) AS McqCount
FROM dbo.Tracks track
JOIN dbo.Topics topic ON topic.TrackId = track.Id
JOIN dbo.Questions question ON question.TopicId = topic.Id
WHERE track.Slug = N'dotnet-full-stack'
  AND question.QuestionType = N'MCQ'
GROUP BY track.Name;
GO
