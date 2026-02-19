SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		James
-- Create date: 19/02/2026
-- Description:	get the name of a room with given id
-- =============================================
CREATE PROCEDURE dbo.getRoomName
	-- Add the parameters for the stored procedure here
	@roomid INT = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT name
	FROM dbo.rooms
	WHERE id = @roomid;
END
GO
