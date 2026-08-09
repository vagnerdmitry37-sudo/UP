using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using UP.Api.Features.AppErrorFeature;

namespace UP.Api.Features.AuthFeature
{
    public interface IAuthService
    {
        Task Register(RegisterRequest request);
        Task<LoginResponse> Login(LoginRequest request);
    }

    public class AuthService(UserManager<AuthUser> userManager, ITokenService ts) : IAuthService
    {
        private readonly UserManager<AuthUser> _userManager = userManager;
        private readonly ITokenService _ts = ts;

        public async Task Register(RegisterRequest request)
        {
            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing != null) throw new AuthError("An account with this email already exists.");

            var authUser = new AuthUser()
            {
                Email = request.Email,
                UserName = request.Email,
            };

            var result = await _userManager.CreateAsync(authUser, request.Password);
            if (!result.Succeeded) throw new AuthError(string.Join(", ", result.Errors.Select(x => x.Description)));
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            var errorMessage = "Not valid email or password";

            var authUser = await _userManager.FindByEmailAsync(request.Email);
            if (authUser == null) throw new AuthError(errorMessage);

            var valid = await _userManager.CheckPasswordAsync(authUser, request.Password);
            if (!valid) throw new AuthError(errorMessage);

            var refreshTolen = _ts.GenerateRefreshToken();
            authUser.RefreshTokens.Add(refreshTolen);

            await _userManager.UpdateAsync(authUser);

            return new LoginResponse
            {
                RefreshToken = refreshTolen.Value,
                AccessToken = _ts.GenerateAccessToken(authUser),
            };
        }
    }
}