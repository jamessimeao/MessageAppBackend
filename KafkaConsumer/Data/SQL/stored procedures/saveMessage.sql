SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		James
-- Create date: 11/02/2026
-- Description:	save message sent to room
-- =============================================
CREATE PROCEDURE dbo.saveMessage
	-- Add the parameters for the stored procedure here
	@roomid INT = NULL,
	@senderid INT = NULL,
	@message NVARCHAR(100) = NULL,
	@time DATETIME = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO dbo.chat (roomid, senderid, message, time)
		VALUES(@roomid, @senderid, @message, @time);
END
GO
