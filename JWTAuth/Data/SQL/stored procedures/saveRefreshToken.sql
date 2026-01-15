SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		James
-- Create date: 14/01/2026
-- Description:	Save refresh token data for given user
-- =============================================
CREATE PROCEDURE dbo.saveRefreshToken
	-- Add the parameters for the stored procedure here
	@id INT = NULL,
	@refreshtoken NVARCHAR(500) = NULL,
	@refreshtokenexpirationtime DATETIME = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	UPDATE dbo.users
		SET refreshtoken = @refreshtoken, refreshtokenexpirationtime = @refreshtokenexpirationtime
		WHERE id = @id;
END
GO
