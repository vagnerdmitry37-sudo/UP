using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using UP.Api.Features.AuthFeature;

namespace UP.IntegrationTests.Utils
{
    public class AuthTestFixture : IAsyncLifetime
    {
        public readonly CustomWebApplicationFactory _factory;
        public CustomWebApplicationFactory Factory => _factory;
        public HttpClient Client { get; private set; } = null!;
        private readonly string authUserEmail = "admin@test.com";
        private readonly string authUserPassword = "Password123!";

        public AuthTestFixture()
        {
            _factory = new CustomWebApplicationFactory();
        }

        public async Task InitializeAsync()
        {
            await SeedAuthUser();
            Client = _factory.CreateClient();
            await LoginAuthUser();
        }

        public Task DisposeAsync()
        {
            Client.Dispose();
            Factory.Dispose();
            return Task.CompletedTask;
        }

        private async Task SeedAuthUser()
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

        private async Task LoginAuthUser()
        {
            var response = await Client.PostAsJsonAsync($"{AuthRouts.Base}/{AuthRouts.Login}", new
            {
                Email = authUserEmail,
                Password = authUserPassword,
            });

            response.EnsureSuccessStatusCode();

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse!.AccessToken);
        }
    }
}
