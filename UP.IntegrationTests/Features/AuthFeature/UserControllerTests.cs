using FluentAssertions;
using Microsoft.AspNetCore.Identity.Data;
using System.Net;
using System.Net.Http.Json;
using UP.Api.Features.AuthFeature;
using UP.Api.Features.UserFeature;
using UP.IntegrationTests.Utils;

namespace UP.IntegrationTests.Features.AuthFeature
{
    public class UserControllerTests(AuthTestFixture fixture) : IClassFixture<AuthTestFixture>
    {
        private readonly HttpClient _client = fixture.Client;

        [Fact]
        public async Task ShouldReturnHello()
        {
            var response = await _client.GetAsync(UserRoutes.Base);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
