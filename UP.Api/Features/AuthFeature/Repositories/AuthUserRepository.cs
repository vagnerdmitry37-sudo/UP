using Microsoft.AspNetCore.Identity;
using UP.Api.Features.AuthFeature.Models;

namespace UP.Api.Features.AuthFeature.Repositories;

public interface IAuthUserRepository
{
    Task<IdentityResult> CreateAsync(AuthUser authUser, string password);
    Task<IdentityResult> UpdateAsync(AuthUser authUser);
    Task<AuthUser?> FindByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(AuthUser authUser, string password);
    Task<AuthUser?> FindByIdAsync(string id);
}

public class AuthUserRepository(UserManager<AuthUser> manager) : IAuthUserRepository
{
    private readonly UserManager<AuthUser> _manager = manager;

    public async Task<IdentityResult> CreateAsync(AuthUser authUser, string password) => await _manager.CreateAsync(authUser, password);
    public async Task<IdentityResult> UpdateAsync(AuthUser authUser) => await _manager.UpdateAsync(authUser);
    public async Task<AuthUser?> FindByEmailAsync(string email) => await _manager.FindByEmailAsync(email);
    public async Task<AuthUser?> FindByIdAsync(string id) => await _manager.FindByIdAsync(id);
    public async Task<bool> CheckPasswordAsync(AuthUser authUser, string password) => await _manager.CheckPasswordAsync(authUser, password);
}
