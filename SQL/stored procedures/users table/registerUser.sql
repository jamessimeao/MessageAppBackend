SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		James
-- Create date: 13/01/2026
-- Description:	register a new user
-- =============================================
CREATE PROCEDURE dbo.registerUser
	-- Add the parameters for the stored procedure here
	@email NVARCHAR(320) = NULL, -- 320 is the maximum length an email can have
	@passwordhash NVARCHAR(500) = NULL,
	@username NVARCHAR(100) = NULL,
	@userrole NVARCHAR(50) = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO dbo.users (email, passwordhash, username, userrole)
		VALUES(@email, @passwordhash, @username, @userrole);
END
GO
