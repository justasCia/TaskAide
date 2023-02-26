namespace TaskAide.API.Common
{
    public static class Constants
    {
        public static class Configuration
        {
            public static class Jwt
            {
                public const string ValidIssuer = "JWT:ValidIssuer";
                public const string ValidAudience = "JWT:ValidAudience";
                public const string Secret = "JWT:Secret";
                public const string AccessTokenValidityInMinutes = "JWT:AccessTokenValidityInMinutes";
                public const string RefreshTokenValidityInDays = "JWT:RefreshTokenValidityInDays";
                public const string RefreshToken = "refreshToken";
            }
            public static class Authentication
            {
                public static class Google
                {
                    public const string ClientId = "Authentication:Google:ClientId";
                    public const string ClientSecret = "Authentication:Google:ClientSecret";
                }
            }
            public static class Encryption
            {
                public const string Key = "Encryption:Key";
            }
        }
    }
}
