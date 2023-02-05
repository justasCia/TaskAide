namespace TaskAide.API.DTOs.Auth
{
    public class TokenDto
    {
        public string AccessToken { get;}
        public string RefreshToken { get; }

        public TokenDto(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }
}
