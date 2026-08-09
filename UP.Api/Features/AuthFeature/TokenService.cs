using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace UP.Api.Features.AuthFeature
{
    public interface ITokenService
    {
        string GenerateAccessToken(AuthUser authUser);
        RefreshToken GenerateRefreshToken();
        bool ValidateAccessToken(string token);
    }

    public class TokenService(IConfiguration config) : ITokenService
    {
        private readonly IConfiguration _config = config;
        private readonly int accessTokeExpiresSeconds = 600;

        public string GenerateAccessToken(AuthUser authUser)
        {
            var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, authUser.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, authUser.Email!)
                };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(accessTokeExpiresSeconds),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public RefreshToken GenerateRefreshToken()
        {
            return new RefreshToken
            {
                Value = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };
        }

        public bool ValidateAccessToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(token, GetTokenValidationParameters(_config), out var validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static TokenValidationParameters GetTokenValidationParameters(IConfiguration config)
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = config["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
                ValidateLifetime = true
            };
        }
    }
}
