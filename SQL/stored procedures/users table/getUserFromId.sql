SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		James
-- Create date: 14/01/2026
-- Description:	Get user from its id
-- =============================================
CREATE PROCEDURE dbo.getUserFromId
	-- Add the parameters for the stored procedure here
	@id INT = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT id, email, passwordhash, username, userrole FROM dbo.users WHERE id = @id;
END
GO
