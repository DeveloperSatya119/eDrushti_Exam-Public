USE [InterviewAppDb];
GO

IF OBJECT_ID(N'[dbo].[CandidateDraftAnswers]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CandidateDraftAnswers]
    (
        [CandidateId] int NOT NULL,
        [QuestionId] int NOT NULL,
        [AnswerText] nvarchar(3000) NOT NULL,
        [UpdatedAt] datetime2 NOT NULL CONSTRAINT [DF_CandidateDraftAnswers_UpdatedAt] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_CandidateDraftAnswers] PRIMARY KEY ([CandidateId], [QuestionId]),
        CONSTRAINT [FK_CandidateDraftAnswers_Candidates_CandidateId]
            FOREIGN KEY ([CandidateId]) REFERENCES [dbo].[Candidates]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CandidateDraftAnswers_Questions_QuestionId]
            FOREIGN KEY ([QuestionId]) REFERENCES [dbo].[Questions]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_CandidateDraftAnswers_QuestionId]
        ON [dbo].[CandidateDraftAnswers]([QuestionId]);
END;

SELECT CASE WHEN OBJECT_ID(N'[dbo].[CandidateDraftAnswers]', N'U') IS NULL THEN 'Missing' ELSE 'Exists' END AS CandidateDraftAnswersTable;
GO
