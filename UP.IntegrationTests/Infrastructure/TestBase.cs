using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.DependencyInjection;
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

        await scope.ServiceProvider
            .GetRequiredService<IAuthRepository>()
            .CreateAuthUserAsync(authUser, loginRequest.Password);

        return loginRequest;
    }
}
