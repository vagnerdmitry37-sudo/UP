using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using UP.Api.Features.AuthFeature;
using UP.Api.Features.AuthFeature.Models;
using UP.Api.Features.AuthFeature.Responses;
using UP.IntegrationTests.Infrastructure;
using UP.Api.Features.AppErrorFeature;

namespace UP.IntegrationTests.Helpers
{
    public class AuthHelper(CustomWebApplicationFactory factory, HttpClient client)
    {
        private readonly string authUserEmail = "admin@test.com";
        private readonly string authUserPassword = "Password123!";

        public async Task<AuthUser> FindByEmailAsync()
        {
            using var scope = factory.Services.CreateScope();
            var userMeneger = scope.ServiceProvider.GetRequiredService<UserManager<AuthUser>>();

            return await userMeneger.FindByEmailAsync(authUserEmail) 
                ?? throw new NotFoundError("Not found auth test user");
        }

        public async Task<AuthUser> RegisterAsync()
        {
            using var scope = factory.Services.CreateScope();
            var userMeneger = scope.ServiceProvider.GetRequiredService<UserManager<AuthUser>>();

            var user = new AuthUser
            {
                Email = authUserEmail,
                UserName = authUserEmail,
            };

            await userMeneger.CreateAsync(user, authUserPassword);
            return await FindByEmailAsync();
        }

        public async Task<LoginResponse> LoginAsync()
        {
            var response = await client.PostAsJsonAsync($"{AuthRouts.Base}/{AuthRouts.Login}", new
            {
                Email = authUserEmail,
                Password = authUserPassword,
            });

            response.EnsureSuccessStatusCode();

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);

            return loginResponse;
        }
    }
}
