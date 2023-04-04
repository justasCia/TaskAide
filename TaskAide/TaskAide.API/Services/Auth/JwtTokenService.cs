using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskAide.API.Common;
using TaskAide.Domain.Entities.Users;

namespace TaskAide.API.Services.Auth
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly SymmetricSecurityKey _authSigningKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenValidityInMinutes;

        public JwtTokenService(IConfiguration configuration)
        {
            _authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration[Constants.Configuration.Jwt.Secret] ?? ""));
            _issuer = configuration[Constants.Configuration.Jwt.ValidIssuer] ?? "";
            _audience = configuration[Constants.Configuration.Jwt.ValidAudience] ?? "";
            _accessTokenValidityInMinutes = int.Parse(configuration[Constants.Configuration.Jwt.AccessTokenValidityInMinutes] ?? "1");
        }

        public string GenerateAccessToken(User user, IEnumerable<string> userRoles)
        {
            var authClaims = new List<Claim>
            {
                new("email", user.Email),
                new("firstName", string.IsNullOrEmpty(user.FirstName) ? "" : user.FirstName),
                new("lastName", string.IsNullOrEmpty(user.LastName) ? "" : user.LastName),
                new("companyName", string.IsNullOrEmpty(user.CompanyName) ? "" : user.CompanyName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, user.Id)
            };

            authClaims.AddRange(userRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));

            var accessSecurityToken = new JwtSecurityToken
            (
                issuer: _issuer,
                audience: _audience,
                expires: DateTime.Now.AddMinutes(_accessTokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(_authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(accessSecurityToken);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _authSigningKey,
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;

        }
    }
}
