using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using UP.Api.Features.AppErrorFeature;
using UP.Api.Features.AuthFeature.Constants;
using UP.Api.Features.AuthFeature.Repositories;
using UP.IntegrationTests.Infrastructure;

namespace UP.IntegrationTests.Tests.AuthFeatureTests;

public class RefreshPositiveTests(Fixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task Should_Refresh_Tokens_When_Refresh_Token_Is_Valid()
    {
        using var scope = Fixture.Factory.Services.CreateScope();
        var ar = scope.ServiceProvider.GetRequiredService<IAuthRepository>();

        var credantials = await RegisterRootAuthUser();

        var loginResponse = await Fixture.Client.PostAsJsonAsync(AuthRouts.Login, credantials);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResponse = await Fixture.Client.PostAsync(AuthRouts.Refresh, null);

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var authUser = await ar.FindAuthUserByEmailAsync(credantials.Email);

        authUser.Should().NotBeNull();
        authUser.RefreshTokens.Should().HaveCount(2);

        var tokens = authUser.RefreshTokens.
            OrderByDescending(rt => rt.CreatedAt)
                .ThenByDescending(rt => rt.Id).ToList();

        var newToken = tokens.First();
        var previousToken = tokens.Last();

        newToken.RevokedAt.Should().BeNull();
        previousToken.RevokedAt.Should().NotBeNull();

        previousToken.ReplacedByToken.Should().NotBeNull();
        previousToken.ReplacedByToken!.Id.Should().Be(newToken.Id);

        newToken.FamilyId.Should().Be(previousToken.FamilyId);
    }

    [Fact]
    public async Task Should_Set_New_Access_And_Refresh_Token_Cookies()
    {
        var cradantials = await RegisterRootAuthUser();

        var loginResponse = await Fixture.Client.PostAsJsonAsync(AuthRouts.Login, cradantials);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResponse = await Fixture.Client.PostAsync(AuthRouts.Refresh, null);

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cookies = refreshResponse.Headers
            .GetValues("Set-Cookie")
            .Select(c => SetCookieHeaderValue.Parse(c))
            .ToList();

        cookies.Should().HaveCount(2);

        cookies.Should().Contain(c => c.Name == TokenNames.AccessToken);
        cookies.Should().Contain(c => c.Name == TokenNames.RefreshToken);

        var accessTokenCookie = cookies.First(c => c.Name == TokenNames.AccessToken);
        var refreshTokenCookie = cookies.First(c => c.Name == TokenNames.RefreshToken);

        accessTokenCookie.Value.ToString().Should().NotBeNullOrWhiteSpace();
        refreshTokenCookie.Value.ToString().Should().NotBeNullOrWhiteSpace();
    }
}

public class RefreshNegativeTests(Fixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task Should_Return_Unauthorized_When_Refresh_Token_Is_Invalid()
    {
        var response = await Fixture.Client.PostAsync(AuthRouts.Refresh, null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var problem = await response.Content.ReadFromJsonAsync<AuthError>();

        problem.Should().NotBeNull();
        problem!.Message.Should().Be("Invalid refresh token");
        problem.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Should_Return_Unauthorized_When_Refresh_Token_Is_Reused()
    {
        using var scope = Fixture.Factory.Services.CreateScope();
        var ar = scope.ServiceProvider.GetRequiredService<IAuthRepository>();

        var credantials = await RegisterRootAuthUser();

        var response = await Fixture.Client.PostAsJsonAsync(AuthRouts.Login, credantials);

        var refreshTokenCookie = response.Headers
            .GetValues(HeaderNames.SetCookie)
            .Single(c => c.StartsWith($"{TokenNames.RefreshToken}="));

        var revokedRefreshTokenValue = SetCookieHeaderValue.Parse(refreshTokenCookie).Value.ToString();

        await Fixture.Client.PostAsync(AuthRouts.Refresh, null);

        var client = Fixture.Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        var request = new HttpRequestMessage(HttpMethod.Post, AuthRouts.Refresh);
        request.Headers.Add("Cookie", $"{TokenNames.RefreshToken}={revokedRefreshTokenValue}");

        var reuseResponse = await client.SendAsync(request);

        reuseResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var problem = await reuseResponse.Content.ReadFromJsonAsync<AuthError>();

        problem.Should().NotBeNull();
        problem!.Message.Should().Be("Refresh token reuse detected");
        problem.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

}

