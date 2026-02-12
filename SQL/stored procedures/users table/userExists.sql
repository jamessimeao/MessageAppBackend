SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		James
-- Create date: 13/01/2026
-- Description:	Check if user exists by counting rows with given email
-- =============================================
CREATE PROCEDURE dbo.userExists
	-- Add the parameters for the stored procedure here
	@email NVARCHAR(320) = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT CASE WHEN EXISTS(
		SELECT email FROM dbo.users WHERE email = @email
	)
	THEN CAST(1 AS BIT)
	ELSE CAST(0 AS BIT)
	END
END
GO