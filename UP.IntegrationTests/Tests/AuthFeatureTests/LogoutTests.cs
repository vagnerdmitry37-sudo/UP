using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using UP.Api.Features.AuthFeature.Constants;
using UP.Api.Features.AuthFeature.Repositories;
using UP.IntegrationTests.Infrastructure;

namespace UP.IntegrationTests.Tests.AuthFeatureTests;

public class LogoutTests(Fixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task Should_Revoke_Refresh_Token_And_Clear_Auth_Cookies_When_Logging_Out()
    {
        using var scope = Fixture.Factory.Services.CreateScope();

        var ar = scope.ServiceProvider.GetRequiredService<IAuthRepository>();
        var credantials = await RegisterRootAuthUser();

        var loginResponse = await Fixture.Client.PostAsJsonAsync(AuthRouts.Login, credantials);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var meResponse = await Fixture.Client.PostAsync(AuthRouts.Me, null);

        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var logoutResponse = await Fixture.Client.PostAsync(AuthRouts.Logout, null);

        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cookies = logoutResponse.Headers
            .GetValues("Set-Cookie")
            .Select(c => SetCookieHeaderValue.Parse(c))
            .ToList();

        cookies.Should().Contain(c => c.Name == TokenNames.AccessToken);
        cookies.Should().Contain(c => c.Name == TokenNames.RefreshToken);

        var accessTokenCookie = cookies.First(c => c.Name == TokenNames.AccessToken);
        var refreshTokenCookie = cookies.First(c => c.Name == TokenNames.RefreshToken);

        accessTokenCookie.Expires.Should().NotBeNull();
        refreshTokenCookie.Expires.Should().NotBeNull();

        var updatedAuthUser = await ar.FindAuthUserByEmailAsync(credantials.Email);

        updatedAuthUser.Should().NotBeNull();
        updatedAuthUser!.RefreshTokens.Should().ContainSingle();
        updatedAuthUser.RefreshTokens.Single().RevokedAt.Should().NotBeNull();
    }
}
