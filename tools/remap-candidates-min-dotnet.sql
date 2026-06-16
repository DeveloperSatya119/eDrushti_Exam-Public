USE [InterviewAppDb];
GO

SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @TrackId int;
SELECT @TrackId = Id FROM dbo.Tracks WHERE Slug = N'dotnet-full-stack';

IF @TrackId IS NOT NULL
BEGIN
    DELETE cq
    FROM dbo.CandidateQuestions cq
    JOIN dbo.Candidates c ON c.Id = cq.CandidateId
    WHERE c.TrackId = @TrackId
      AND c.IsAdmin = 0
      AND NOT EXISTS (SELECT 1 FROM dbo.CandidateAnswers a WHERE a.CandidateId = c.Id);

    ;WITH DotNetPick AS
    (
        SELECT c.Id AS CandidateId,
               q.Id AS QuestionId,
               ROW_NUMBER() OVER (PARTITION BY c.Id ORDER BY NEWID()) AS rn
        FROM dbo.Candidates c
        JOIN dbo.Questions q ON q.IsActive = 1 AND q.QuestionType = N'MCQ'
        JOIN dbo.Topics t ON t.Id = q.TopicId AND t.TrackId = c.TrackId
        WHERE c.TrackId = @TrackId
          AND c.IsAdmin = 0
          AND t.Name = N'.NET Core and C#'
          AND NOT EXISTS (SELECT 1 FROM dbo.CandidateAnswers a WHERE a.CandidateId = c.Id)
    ),
    ChosenDotNet AS
    (
        SELECT CandidateId, QuestionId
        FROM DotNetPick
        WHERE rn <= 6
    ),
    RemainingPick AS
    (
        SELECT c.Id AS CandidateId,
               q.Id AS QuestionId,
               ROW_NUMBER() OVER (PARTITION BY c.Id ORDER BY NEWID()) AS rn
        FROM dbo.Candidates c
        JOIN dbo.Questions q ON q.IsActive = 1 AND q.QuestionType = N'MCQ'
        JOIN dbo.Topics t ON t.Id = q.TopicId AND t.TrackId = c.TrackId
        WHERE c.TrackId = @TrackId
          AND c.IsAdmin = 0
          AND NOT EXISTS (SELECT 1 FROM dbo.CandidateAnswers a WHERE a.CandidateId = c.Id)
          AND NOT EXISTS
          (
              SELECT 1
              FROM ChosenDotNet chosen
              WHERE chosen.CandidateId = c.Id
                AND chosen.QuestionId = q.Id
          )
    ),
    FinalQuestions AS
    (
        SELECT CandidateId, QuestionId FROM ChosenDotNet
        UNION ALL
        SELECT CandidateId, QuestionId FROM RemainingPick WHERE rn <= 14
    ),
    Ordered AS
    (
        SELECT CandidateId,
               QuestionId,
               ROW_NUMBER() OVER (PARTITION BY CandidateId ORDER BY NEWID()) AS OrderIndex
        FROM FinalQuestions
    )
    INSERT INTO dbo.CandidateQuestions (CandidateId, QuestionId, OrderIndex, AssignedAt)
    SELECT CandidateId, QuestionId, OrderIndex, SYSUTCDATETIME()
    FROM Ordered;
END;

COMMIT;

SELECT c.Id,
       c.Email,
       COUNT(cq.QuestionId) AS TotalMapped,
       SUM(CASE WHEN t.Name = N'.NET Core and C#' THEN 1 ELSE 0 END) AS DotNetMapped
FROM dbo.Candidates c
LEFT JOIN dbo.CandidateQuestions cq ON cq.CandidateId = c.Id
LEFT JOIN dbo.Questions q ON q.Id = cq.QuestionId
LEFT JOIN dbo.Topics t ON t.Id = q.TopicId
WHERE c.IsAdmin = 0
GROUP BY c.Id, c.Email;
GO
