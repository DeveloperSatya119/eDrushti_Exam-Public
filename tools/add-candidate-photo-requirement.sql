USE [InterviewAppDb];
GO

IF COL_LENGTH('dbo.Candidates', 'IsPhotoRequired') IS NULL
    ALTER TABLE [dbo].[Candidates] ADD [IsPhotoRequired] bit NOT NULL CONSTRAINT [DF_Candidates_IsPhotoRequired] DEFAULT (0);

IF COL_LENGTH('dbo.Candidates', 'PhotoConsentAccepted') IS NULL
    ALTER TABLE [dbo].[Candidates] ADD [PhotoConsentAccepted] bit NOT NULL CONSTRAINT [DF_Candidates_PhotoConsentAccepted] DEFAULT (0);

IF COL_LENGTH('dbo.Candidates', 'PhotoPath') IS NULL
    ALTER TABLE [dbo].[Candidates] ADD [PhotoPath] nvarchar(500) NULL;

IF COL_LENGTH('dbo.Candidates', 'PhotoCapturedAt') IS NULL
    ALTER TABLE [dbo].[Candidates] ADD [PhotoCapturedAt] datetime2 NULL;

SELECT
    COL_LENGTH('dbo.Candidates', 'IsPhotoRequired') AS IsPhotoRequiredCol,
    COL_LENGTH('dbo.Candidates', 'PhotoConsentAccepted') AS PhotoConsentAcceptedCol,
    COL_LENGTH('dbo.Candidates', 'PhotoPath') AS PhotoPathCol,
    COL_LENGTH('dbo.Candidates', 'PhotoCapturedAt') AS PhotoCapturedAtCol;
GO
