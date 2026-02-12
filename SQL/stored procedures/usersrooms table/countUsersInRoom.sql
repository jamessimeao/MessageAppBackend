SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		James
-- Create date: 12/02/2026
-- Description:	count how many users are in room
-- =============================================
CREATE PROCEDURE dbo.countUsersInRoom
	-- Add the parameters for the stored procedure here
	@roomid INT = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT COUNT(userid)
	FROM dbo.usersrooms
	WHERE roomid = @roomid;
END
GO
