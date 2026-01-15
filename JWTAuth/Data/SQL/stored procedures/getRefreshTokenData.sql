SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		James
-- Create date: 14/01/2026
-- Description:	Get refresh token and its expiration time for user with given id
-- =============================================
CREATE PROCEDURE dbo.getRefreshTokenData
	-- Add the parameters for the stored procedure here
	@userId INT = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT refreshtoken, refreshtokenexpirationtime FROM dbo.users WHERE id = @userId;
END
GO
