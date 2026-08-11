using Docker.DotNet.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using UP.Api.Db;
using UP.Api.Features.AuthFeature;
using UP.Api.Features.AuthFeature.Models;
using UP.Api.Features.AuthFeature.Requests;
using UP.Api.Features.AuthFeature.Responses;
using UP.Api.Features.UserFeature;
using UP.IntegrationTests.Fixtures;

namespace UP.IntegrationTests.Tests.Features.AuthFeature
{
    public class AuthControllerTests(ShareFixture sf) : IClassFixture<ShareFixture>
    {
        [Fact]
        public async Task Should_Login_Auth_User()
        {
            var unauthorizedResult = await sf.Client.GetAsync(UserRoutes.Base);
            unauthorizedResult.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            await sf.Auth.RegisterAsync();
            await sf.Auth.LoginAsync();

            var authorizedResult = await sf.Client.GetAsync(UserRoutes.Base);
            authorizedResult.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Should_Refresh_Access_Token()
        {
            var authUser = await sf.Auth.RegisterAsync();
            var loginResponse = await sf.Auth.LoginAsync();

            await using var scope = sf.Factory.Services.CreateAsyncScope();
            var Db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
            var requset = new RefreshTokenRequest
            {
                Email = authUser!.Email!,
                RefreshToken = loginResponse.RefreshToken
            };
            var result = await sf.Client.PostAsJsonAsync($"{AuthRouts.Base}/{AuthRouts.Refresh}", requset);
            var content = await result.Content.ReadFromJsonAsync<LoginResponse>();

            content!.AccessToken.Should().NotBeNullOrEmpty();
            content!.RefreshToken.Should().NotBeNullOrEmpty();


        }
    }
}
