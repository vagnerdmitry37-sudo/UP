using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using UP.Api.Features.AuthFeature;

namespace UP.IntegrationTests.Features.Fixtures
{
    public class AuthHelper(HttpClient client, CustomWebApplicationFactory factory)
    {
        private readonly HttpClient _client = client;
        private readonly CustomWebApplicationFactory _factory = factory;

        private readonly string authUserEmail = "admin@test.com";
        private readonly string authUserPassword = "Password123!";

        public async Task Register()
        {
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthUser>>();

            var user = new AuthUser
            {
                Email = authUserEmail,
                UserName = authUserEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(user, authUserPassword);
        }

        public async Task Login()
        {
            var response = await _client.PostAsJsonAsync($"{AuthRouts.Base}/{AuthRouts.Login}", new
            {
                Email = authUserEmail,
                Password = authUserPassword,
            });

            response.EnsureSuccessStatusCode();

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);
        }
    }
}
