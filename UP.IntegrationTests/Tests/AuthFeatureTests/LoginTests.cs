using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using UP.Api.Bootstrap;
using UP.Api.Features.AppErrorFeature;
using UP.Api.Features.AuthFeature.Constants;
using UP.Api.Features.AuthFeature.Models.RefreshToken;
using UP.Api.Features.AuthFeature.Options;
using UP.Api.Features.AuthFeature.Repositories;
using UP.Api.Features.AuthFeature.Services;
using UP.IntegrationTests.Infrastructure;

namespace UP.IntegrationTests.Tests.AuthFeatureTests;

public class LoginTests(Fixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task Should_Set_Access_And_Refresh_Tokens_To_Cookies()
    {
        using var scope = Fixture.Factory.Services.CreateScope();

        var loginRequest = await RegisterRootAuthUser();
        var jwtOptions = scope.ServiceProvider.GetRequiredService<IOptions<JwtOptions>>().Value;

        var response = await Fixture.Client.PostAsJsonAsync(AuthRouts.Login, loginRequest);

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
        accessTokenCookie.SameSite.Should().Be(Microsoft.Net.Http.Headers.SameSiteMode.Lax);
        accessTokenCookie.HttpOnly.Should().BeTrue();

        refreshTokenCookie.Path.ToString().Should().Be($"/{AuthRouts.Base}");
        refreshTokenCookie.SameSite.Should().Be(Microsoft.Net.Http.Headers.SameSiteMode.Lax);
        refreshTokenCookie.HttpOnly.Should().BeTrue();

        accessTokenCookie.Value.ToString().Should().NotBeNullOrWhiteSpace();
        refreshTokenCookie.Value.ToString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Revoke_Previous_Refresh_Tokens_When_Logging_In_Multiple_Times()
    {
        using var scope = Fixture.Factory.Services.CreateScope();

        var ar = scope.ServiceProvider.GetRequiredService<IAuthRepository>();

        var tries = 5;
        var credantials = await RegisterRootAuthUser();

        for (var i = 0; i < tries; i++)
        {
            var response = await Fixture.Client.PostAsJsonAsync(AuthRouts.Login, credantials);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        var authUser = await ar.FindAuthUserByEmailAsync(credantials.Email);

        authUser.Should().NotBeNull();
        authUser!.RefreshTokens.Should().HaveCount(tries);

        var refreshTokens = authUser.RefreshTokens;

        refreshTokens.First().RevokedAt.Should().BeNull();
        refreshTokens.Skip(1).Should().OnlyContain(rt => rt.RevokedAt != null);
    }

    [Fact]
    public async Task Should_Refresh_Tokens_When_Refresh_Token_Is_Valid()
    {
        using var scope = Fixture.Factory.Services.CreateScope();
        var ar = scope.ServiceProvider.GetRequiredService<IAuthRepository>();

        var credantials = await RegisterRootAuthUser();

        var loginResponse1 = await Fixture.Client.PostAsJsonAsync(AuthRouts.Login, credantials);
        var loginResponse2 = await Fixture.Client.PostAsJsonAsync(AuthRouts.Login, credantials);

        loginResponse1.StatusCode.Should().Be(HttpStatusCode.OK);
        loginResponse2.StatusCode.Should().Be(HttpStatusCode.OK);

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
    public async Task Should_Provoke_Excessive_Refresh_Tokens()
    {
        using var scope = Fixture.Factory.Services.CreateScope();

        var credantials = await RegisterRootAuthUser();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ts = scope.ServiceProvider.GetRequiredService<ITokenService>();
        var ar = scope.ServiceProvider.GetRequiredService<IAuthRepository>();
        var ao = scope.ServiceProvider.GetRequiredService<IOptions<AuthOptions>>();
        var authUser = await ar.FindAuthUserByEmailAsync(credantials.Email);
        ICollection<RefreshTokenModel> refreshTokens = [];

        for (int i = 0; i < 10; i++)
        {
            var (_, refreshToken) = ts.GenerateRefreshToken(authUser!.Id);
            refreshTokens.Add(refreshToken);
        }

        await context.RefreshTokens.AddRangeAsync(refreshTokens);
        await context.SaveChangesAsync();

        await Fixture.Client.PostAsJsonAsync(AuthRouts.Login, credantials);

        var existingRefreshTokens = await context.RefreshTokens
            .Where(rt => rt.RevokedAt == null && rt.ExpiresAt > DateTimeOffset.UtcNow)
            .ToListAsync();

        existingRefreshTokens.Should().HaveCount(ao.Value.MaxConcurrentFamilies);
    }

    [Fact]
    public async Task Should_Login_Multiply_Users()
    {
        using var scope = Fixture.Factory.Services.CreateScope();

        var acs = scope.ServiceProvider.GetRequiredService<IAuthControllerService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var numberOfAuthUsers = 100;

        for (int i = 0; i < numberOfAuthUsers; i++)
        {
            var authUser = new RegisterRequest()
            {
                Email = $"test{i}@maail.com",
                Password = $"Password123@{i}"
            };

            var loginRequest = new LoginRequest
            {
                Email = authUser.Email,
                Password = authUser.Password,
            };

            await acs.RegisterAsync(authUser);
            await Fixture.Client.PostAsJsonAsync(AuthRouts.Login, loginRequest);
        }

        var authUsers = await context.Users.ToListAsync();
        authUsers.Should().HaveCount(numberOfAuthUsers);
    }
}

public class LoginNegativeTests(Fixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task Should_Return_Unauthorized_When_Wrong_Password_Provided()
    {
        var credantials = await RegisterRootAuthUser();

        var invalidLoginRequest = new LoginRequest { Email = credantials.Email, Password = "wrong-password" };
        var negativeResponse = await Fixture.Client.PostAsJsonAsync(AuthRouts.Login, invalidLoginRequest);
        var problem = await negativeResponse.Content.ReadFromJsonAsync<AuthError>();

        negativeResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        problem.Should().NotBeNull();
        problem.Message.Should().NotBeNull();
        problem.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Should_Return_Unauthorized_When_Wrong_Email_Provided()
    {
        var credantials = await RegisterRootAuthUser();

        var invalidLoginRequest = new LoginRequest { Email = "wrong-email", Password = credantials.Password };
        var negativeResponse = await Fixture.Client.PostAsJsonAsync(AuthRouts.Login, invalidLoginRequest);
        var problem = await negativeResponse.Content.ReadFromJsonAsync<AuthError>();

        negativeResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        problem.Should().NotBeNull();
        problem.Message.Should().NotBeNull();
        problem.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }
}
