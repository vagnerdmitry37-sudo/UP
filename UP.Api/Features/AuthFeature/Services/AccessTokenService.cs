using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UP.Api.Features.AuthFeature.Models;

namespace UP.Api.Features.AuthFeature.Services
{
    public interface IAccessTokenService
    {
        string GenerateToken(AuthUser authUser);
    }

    public class AccessTokenService(IConfiguration config) : IAccessTokenService
    {
        private readonly IConfiguration _config = config;
        private readonly int accessTokeExpiresSeconds = 600;

        public string GenerateToken(AuthUser authUser)
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
