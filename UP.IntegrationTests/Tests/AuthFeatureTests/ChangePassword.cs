using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity.Data;
using UP.Api.Features.AuthFeature.Constants;
using UP.Api.Features.AuthFeature.Requests;
using UP.IntegrationTests.Infrastructure;

namespace UP.IntegrationTests.Tests.AuthFeatureTests;

public class ChangePassword(Fixture fixture) : TestBase(fixture)
{
    [Fact]
    public async Task Should_Change_Password()
    {
        var credentials = await RegisterRootAuthUser();

        var changePasswordRequest = new ChangePasswordRequest
        {
            CurrentPassword = credentials.Password,
            NewPassword = $"{credentials.Password}_new"
        };

        var newPasswordLoginRequest = new LoginRequest
        {
            Email = credentials.Email,
            Password = changePasswordRequest.NewPassword
        };

        await Fixture.Client.PostAsJsonAsync(AuthRouts.Login, credentials);

        var changePasswordResponse = await Fixture.Client.PostAsJsonAsync(AuthRouts.ChangePassword, changePasswordRequest);
        changePasswordResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var badLoginResponse = await Fixture.Client.PostAsJsonAsync(AuthRouts.Login, credentials);
        badLoginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var okLoginResponse = await Fixture.Client.PostAsJsonAsync(AuthRouts.Login, newPasswordLoginRequest);
        okLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
