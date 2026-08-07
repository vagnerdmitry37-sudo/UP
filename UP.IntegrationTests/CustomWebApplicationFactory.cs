using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UP.Api.Features.AuthFeature;

namespace UP.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public async Task CreateUserAsync()
        {
            using var scope = Services.CreateScope();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<AuthUser>>();

            var user = new AuthUser
            {
                UserName = "admin@test.com",
                Email = "admin@test.com",
            };

            await userManager.CreateAsync(user,"Password123!");
        }
    }
}
