CREATE TABLE [dbo].[Account] (
    [AccountID]        INT           IDENTITY (1, 1) NOT NULL,
    [CompanyName]      VARCHAR (255) NOT NULL,
    [Address]          VARCHAR (255) NOT NULL,
    [LastModifiedBy]   VARCHAR (255) NOT NULL,
    [LastModified]     DATETIME      NOT NULL,
    [AccountReference] VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_dbo.Account] PRIMARY KEY CLUSTERED ([AccountID] ASC)
);

