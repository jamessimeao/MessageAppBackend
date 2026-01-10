namespace JWTAuth.Dtos
{
    public class TokenDto
    {
        // The access token is used to identify the user.
        // It is short lived, for this reason we have also a refresh token,
        // to request for new access tokens without having to login.
        public required string AccessToken { get; set; }

        // The refresh token is used to request a new access token once it has expired.
        // The refresh token is valid for more time than the access token.
        // Once the refresh token is invalid, the user will have to login again to recreate
        // both the access token and refresh token.
        public required string RefreshToken { get; set; }
    }
}
