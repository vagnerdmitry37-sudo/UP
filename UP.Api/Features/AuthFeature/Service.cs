using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;

namespace UP.Api.Features.AuthFeature
{
    public interface IAuthService
    {
        Task Register(RegisterRequest request);
        Task<LoginResponse?> Login(LoginRequest request);
    }

    public class AuthService(UserManager<AuthUser> userManager, ITokenService ts) : IAuthService
    {
        private readonly UserManager<AuthUser> _userManager = userManager;
        private readonly ITokenService _ts = ts;

        public async Task Register(RegisterRequest request)
        {
            var existing = await _userManager.FindByEmailAsync(request.Email);


            if (existing != null)
            {
                throw new Exception("User already exists");
            }


            var authUser = new AuthUser()
            {
                Email = request.Email,
                UserName = request.Email,
            };


            var result = await _userManager.CreateAsync(authUser, request.Password);


            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description);
                throw new Exception(string.Join(", ", errors));
            }
        }

        public async Task<LoginResponse?> Login(LoginRequest request)
        {
            var authUser = await _userManager.FindByEmailAsync(request.Email);
            if (authUser == null) return null;

            var valid = await _userManager.CheckPasswordAsync(authUser, request.Password);
            if (!valid) return null;

            var refreshToken = _ts.GenerateRefreshToken();

            authUser.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            });

            await _userManager.UpdateAsync(authUser);

            return new LoginResponse
            {
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
                RefreshToken = refreshToken,
                AccessToken = _ts.GenerateAccessToken(authUser),
            };
        }
    }
}