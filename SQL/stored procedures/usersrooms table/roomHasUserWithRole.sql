SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		James
-- Create date: 19/02/2026
-- Description:	check if the room has an user with the specified role
-- =============================================
CREATE PROCEDURE dbo.roomHasUserWithRole
	-- Add the parameters for the stored procedure here
	@roomid INT = NULL,
	@roleinroom NVARCHAR(50) = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT CASE WHEN EXISTS (
		SELECT 1
			FROM dbo.usersrooms
			WHERE roomid = @roomid AND roleinroom = @roleinroom
	)
	THEN CAST(0 AS BIT)
	ELSE CAST(1 AS BIT)
	END
END
GO
