namespace JWTAuth.Models
{
    public class RefreshTokenData
    {
        public required string RefreshToken { get; set; }
        public required DateTime RefreshTokenExpirationTime { get; set; }
    }
}
