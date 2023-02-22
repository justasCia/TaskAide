namespace TaskAide.API.DTOs.Auth
{
    public class RefreshTokenDto
    {
        public string AccessToken { get; }

        public RefreshTokenDto(string accessToken)
        {
            AccessToken = accessToken;
        }
    }
}
