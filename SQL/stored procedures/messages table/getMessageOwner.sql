SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		James
-- Create date: 17/02/2026
-- Description:	get the user id of the message
-- =============================================
CREATE PROCEDURE dbo.getMessageOwner
	-- Add the parameters for the stored procedure here
	@messageid INT = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT senderid
	FROM dbo.messages
	WHERE id = @messageid;
END
GO
