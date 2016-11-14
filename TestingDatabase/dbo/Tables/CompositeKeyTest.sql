CREATE TABLE [dbo].[CompositeKeyTest] (
    [FirstKey]       INT           NOT NULL,
    [SecondKey]      NVARCHAR (50) NOT NULL,
    [Value]          VARCHAR (MAX) NOT NULL,
    [LastModified]   DATE          NOT NULL,
    [LastModifiedBy] VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_dbo.CompositeKeyTest] PRIMARY KEY CLUSTERED ([FirstKey] ASC, [SecondKey] ASC)
);

