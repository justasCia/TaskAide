namespace TaskAide.API.DTOs.Auth
{
    public class TokenDto
    {
        public string AccessToken { get;}
        public string RefreshToken { get; }
        public DateTime RefreshTokenExpiryDate { get; }

        public TokenDto(string accessToken, string refreshToken, DateTime refreshTokenExpiryDate)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            RefreshTokenExpiryDate = refreshTokenExpiryDate;
        }
    }
}
