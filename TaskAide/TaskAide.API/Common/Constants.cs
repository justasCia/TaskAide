﻿namespace TaskAide.API.Common
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
            public static class Encryption
            {
                public const string Key = "Encryption:Key";
            }

            public static class Stripe
            {
                public const string SecretKey = "Stripe:SecretKey";
                public const string PriceId = "Stripe:PriceId";
            }

            public const string DatabaseConnectionString = nameof(DatabaseConnectionString);
            public const string TaskAideAppUrl = nameof(TaskAideAppUrl);
        }
    }
}
