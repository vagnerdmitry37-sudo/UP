using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using UP.Api.Db;
using UP.Api.Features.AppErrorFeature;
using UP.Api.Features.AuthFeature.Models;
using UP.Api.Features.AuthFeature.Requests;
using UP.Api.Features.AuthFeature.Responses;

namespace UP.Api.Features.AuthFeature.Services
{
    public interface IAuthService
    {
        Task<AuthUser> Register(RegisterRequest request);
        Task<LoginResponse> Login(LoginRequest request);
        Task<LoginResponse> Refresh(RefreshTokenRequest request);
    }

    public class AuthService(
        IConfiguration config,
        IAccessTokenService ats,
        IRefreshTokenService rts,
        AppDbContext context,
        UserManager<AuthUser> manager) : IAuthService
    {
        private readonly IAccessTokenService _ats = ats;
        private readonly IRefreshTokenService _rts = rts;
        private readonly AppDbContext _context = context;
        private readonly UserManager<AuthUser> _manager = manager;

        public async Task<AuthUser> Register(RegisterRequest request)
        {
            var existingAuthUser = await _manager.FindByEmailAsync(request.Email);
            if (existingAuthUser != null) throw new AuthError("An account with this email already exists.");

            var newAuthUser = new AuthUser()
            {
                Email = request.Email,
                UserName = request.Email,
            };

            var identityResult = await _manager.CreateAsync(newAuthUser, request.Password);
            if (!identityResult.Succeeded)
            {
                var errorMessage = string.Join(", ", identityResult.Errors.Select(x => x.Description));
                throw new AuthError(errorMessage);
            }

            return newAuthUser;
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            var validAuthUser = await ValidateAuthUserAsync(request);
            await _rts.RevokeOldTokensAsync(validAuthUser.Id);
            var refreshToken = _rts.GenerateToken();
            validAuthUser.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            await _manager.UpdateAsync(validAuthUser);

            return new LoginResponse
            {
                RefreshToken = refreshToken.Value,
                AccessToken = _ats.GenerateToken(validAuthUser)
            };
        }

        public async Task<LoginResponse> Refresh(RefreshTokenRequest request)
        {
            var errorMessage = "Refresh token validation failed.";

            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Value == request.RefreshToken);
            if (refreshToken == null || !refreshToken.IsActive) throw new AuthError(errorMessage);
            var authUther = await _manager.FindByEmailAsync(request.Email) ?? throw new AuthError(errorMessage);

            refreshToken.RevokedAt = DateTimeOffset.UtcNow;
            var newRefreshToken = _rts.GenerateToken();
            authUther.RefreshTokens.Add(newRefreshToken);
            await _manager.UpdateAsync(authUther);

            return new LoginResponse
            {
                AccessToken = _ats.GenerateToken(refreshToken.AuthUser),
                RefreshToken = newRefreshToken.Value
            };
        }

        private async Task<AuthUser> ValidateAuthUserAsync(LoginRequest request)
        {
            var errorMessage = "Not valid email or password";

            var authUser = await _manager.FindByEmailAsync(request.Email)
                ?? throw new AuthError(errorMessage);

            var isPasswordValid = await _manager.CheckPasswordAsync(authUser, request.Password);
            if (!isPasswordValid) throw new AuthError(errorMessage);

            return authUser;
        }
    }
}