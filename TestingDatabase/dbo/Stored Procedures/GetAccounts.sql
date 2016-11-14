
-- =============================================
-- Author:		Liaan Booysen
-- Description:	Test sproc
-- =============================================
CREATE PROCEDURE GetAccounts @CompanyName VARCHAR(255) = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT *
	FROM account act
	WHERE act.CompanyName = @CompanyName
		OR @CompanyName IS NULL
END