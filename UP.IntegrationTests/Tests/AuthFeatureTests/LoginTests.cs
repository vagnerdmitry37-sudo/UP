using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using UP.Api.Features.AuthFeature.Constants;
using UP.Api.Features.AuthFeature.Options;
using UP.Api.Features.AuthFeature.Repositories;
using UP.IntegrationTests.Infrastructure;

namespace UP.IntegrationTests.Tests.AuthFeatureTests;

public class LoginTests(ShareFixture sf) : IClassFixture<ShareFixture>
{
    [Fact]
    public async Task Should_Set_Access_And_Refresh_Tokens_To_Cookies()
    {
        using var scope = sf.Factory.Services.CreateScope();

        var loginRequest = await sf.RegisterRootAuthUser();
        var jwtOptions = scope.ServiceProvider.GetRequiredService<IOptions<JwtOptions>>().Value;

        var response = await sf.Client.PostAsJsonAsync(AuthRouts.Login, loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cookies = response.Headers.GetValues("Set-Cookie").Select(c => SetCookieHeaderValue.Parse(c)).ToList();

        cookies.Should().HaveCount(2);

        var accessTokenCookie = cookies.First(c => c.Name == TokenNames.AccessToken);
        var refreshTokenCookie = cookies.First(c => c.Name == TokenNames.RefreshToken);

        var jwt = new JwtSecurityTokenHandler()
            .ReadJwtToken(accessTokenCookie.Value.ToString());

        jwt.Claims.Should().Contain(c =>
            c.Type == JwtRegisteredClaimNames.Email &&
            c.Value == loginRequest.Email &&
            c.Issuer == jwtOptions.Issuer);

        jwt.Audiences.Should().ContainSingle(jwtOptions.Audience);

        accessTokenCookie.Path.ToString().Should().Be("/");
        accessTokenCookie.SameSite.Should().Be(SameSiteMode.Lax);
        accessTokenCookie.HttpOnly.Should().BeTrue();

        refreshTokenCookie.Path.ToString().Should().Be($"/{AuthRouts.Base}");
        refreshTokenCookie.SameSite.Should().Be(SameSiteMode.Lax);
        refreshTokenCookie.HttpOnly.Should().BeTrue();

        accessTokenCookie.Value.ToString().Should().NotBeNullOrWhiteSpace();
        refreshTokenCookie.Value.ToString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Revoke_Previous_Refresh_Tokens_When_Logging_In_Multiple_Times()
    {
        using var scope = sf.Factory.Services.CreateScope();

        var ar = scope.ServiceProvider.GetRequiredService<IAuthRepository>();

        var tries = 5;
        var loginRequest = await sf.RegisterRootAuthUser();

        for (var i = 0; i < tries; i++)
        {
            var response = await sf.Client.PostAsJsonAsync(AuthRouts.Login, loginRequest);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        var authUser = await ar.FindAuthUserByEmailAsync(loginRequest.Email);

        authUser.Should().NotBeNull();
        authUser!.RefreshTokens.Should().HaveCount(tries);

        var refreshTokens = authUser.RefreshTokens;

        refreshTokens.First().RevokedAt.Should().BeNull();
        refreshTokens.Skip(1).Should().OnlyContain(rt => rt.RevokedAt != null);
    }
}
