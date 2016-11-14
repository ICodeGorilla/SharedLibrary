CREATE TABLE [dbo].[Contact] (
    [ContactID]      INT           IDENTITY (1, 1) NOT NULL,
    [Name]           VARCHAR (255) NOT NULL,
    [EmailAddress]   VARCHAR (255) NOT NULL,
    [LastModified]   DATE          NOT NULL,
    [LastModifiedBy] VARCHAR (255) NOT NULL,
    [AccountID]      INT           NOT NULL,
    CONSTRAINT [PK_dbo.Contact] PRIMARY KEY CLUSTERED ([ContactID] ASC),
    CONSTRAINT [FK_dbo.Contact_dbo.Account_AccountID] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Account] ([AccountID])
);


GO
CREATE NONCLUSTERED INDEX [IX_AccountID]
    ON [dbo].[Contact]([AccountID] ASC);

