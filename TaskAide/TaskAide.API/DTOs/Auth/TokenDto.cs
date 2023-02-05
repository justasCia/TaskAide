namespace TaskAide.API.DTOs.Auth
{
    public class TokenDto
    {
        public string AccessToken { get;}
        public string RefreshToken { get; }
        public DateTime ValidTo { get; }

        public TokenDto(string accessToken, string refreshToken, DateTime validTo)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ValidTo = validTo;
        }
    }
}
