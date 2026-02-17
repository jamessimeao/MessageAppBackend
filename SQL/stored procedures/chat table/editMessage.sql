SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		James
-- Create date: 17/02/2026
-- Description:	replace message content by a new one
-- =============================================
CREATE PROCEDURE dbo.editMessage
	-- Add the parameters for the stored procedure here
	@messageid INT = NULL,
	@newcontent NVARCHAR(100) = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	UPDATE dbo.messages
	SET content = @newcontent
	WHERE id = @messageid;
END
GO
