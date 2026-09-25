using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UP.Api.Bootstrap;
using UP.Api.Features.AppUserFeature.Models.AppUser;
using UP.Api.Features.AuthFeature.Models.AuthUser;
using UP.Api.Features.AuthFeature.Repositories;

namespace UP.IntegrationTests.Infrastructure;

public abstract class TestBase(Fixture fixture) : IClassFixture<Fixture>, IAsyncLifetime
{
    protected Fixture Fixture { get; } = fixture;

    public async Task InitializeAsync()
    {
        await Fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task<LoginRequest> RegisterRootAuthUser()
    {
        using var scope = Fixture.Factory.Services.CreateScope();

        var authUser = new AuthUserModel
        {
            Email = "testRoot@mail.com",
            UserName = "TestRoot",
        };

        var loginRequest = new LoginRequest
        {
            Email = authUser.Email,
            Password = "Password123@"
        };

        var authRepository = scope.ServiceProvider
            .GetRequiredService<IAuthRepository>();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        // 1. Create the Identity/Auth user
        await authRepository.CreateAuthUserAsync(authUser, loginRequest.Password);

        var createdAuthuser = await authRepository.FindAuthUserByEmailAsync(authUser.Email);

        // 2. Create the application user referencing the existing Auth user
        var appUser = new AppUserModel
        {
            Name = createdAuthuser!.UserName!,
            Email = createdAuthuser.Email!,
            AuthUser = createdAuthuser
        };

        await context.AppUsers.AddAsync(appUser);
        await context.SaveChangesAsync();

        var test = await context.AppUsers.ToListAsync();

        return loginRequest;
    }
}
