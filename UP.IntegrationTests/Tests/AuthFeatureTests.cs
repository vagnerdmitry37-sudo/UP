using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity.Data;
using UP.Api.Features.AuthFeature;
using UP.Api.Features.AuthFeature.Requests;
using UP.Api.Features.AuthFeature.Responses;
using UP.IntegrationTests.Fixtures;

namespace UP.IntegrationTests.Tests;

public class AuthFeatureTests(ShareFixture sf) : IClassFixture<ShareFixture>
{
    [Fact]
    public async Task Should_Login_Auth_User()
    {
        var registerRequest = new RegisterRequest
        {
            Email = $"test-{Guid.NewGuid()}@test.com",
            Password = "Password@test123"
        };

        var loginRequest = new LoginRequest
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        var unauthorizedHttpResponse = await sf.Client.PostAsJsonAsync(AuthRouts.Register, registerRequest);
        unauthorizedHttpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var (newAuthUser, identityResult) = await sf.Auth.CreateAuthUserAsync(registerRequest);
        identityResult.Succeeded.Should().BeTrue();

        var exitedAuthUser = await sf.Auth.FindAuthUserByEmailAsync(newAuthUser.Email!);
        exitedAuthUser!.Email.Should().Be(registerRequest.Email);
        exitedAuthUser!.PasswordHash.Should().NotBeNullOrEmpty();

        var loginHttpResponse = await sf.Client.PostAsJsonAsync(AuthRouts.Login, loginRequest);
        loginHttpResponse.EnsureSuccessStatusCode();

        var loginResponse = await loginHttpResponse.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse!.AccessToken.Should().NotBeNullOrEmpty();
        loginResponse!.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_Refresh_Access_Token()
    {
        var registerRequest = new RegisterRequest
        {
            Email = $"test-{Guid.NewGuid()}@test.com",
            Password = "Password@test123"
        };

        var loginRequest = new LoginRequest
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        await sf.Auth.CreateAuthUserAsync(registerRequest);
        var loginHttpResponse = await sf.Client.PostAsJsonAsync(AuthRouts.Login, loginRequest);
        var loginResponse = await loginHttpResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var refreshTokenRequest = new RefreshTokenRequest
        {
            Email = registerRequest.Email,
            RefreshToken = loginResponse!.RefreshToken
        };

        var refreshHttpResponse = await sf.Client.PostAsJsonAsync(AuthRouts.Refresh, refreshTokenRequest);
        var refreshResponse = await refreshHttpResponse.Content.ReadFromJsonAsync<LoginResponse>();
        var existedRefreshToken = await sf.Auth.FindRefreshTokenByValueAsync(loginResponse!.RefreshToken);
        refreshResponse!.AccessToken.Should().NotBeNullOrEmpty();
        refreshResponse!.RefreshToken.Should().NotBeNullOrEmpty();
        existedRefreshToken!.IsActive.Should().BeFalse();
        existedRefreshToken.RevokedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        var newRrfreshToknen = await sf.Auth.FindRefreshTokenByValueAsync(refreshResponse!.RefreshToken);
        newRrfreshToknen!.IsActive.Should().BeTrue();
        newRrfreshToknen.RevokedAt.Should().BeNull();
    }
}
