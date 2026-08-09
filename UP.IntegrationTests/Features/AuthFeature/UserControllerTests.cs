using FluentAssertions;
using System.Net;
using UP.Api.Features.UserFeature;
using UP.IntegrationTests.Features.Fixtures;

namespace UP.IntegrationTests.Features.AuthFeature
{
    public class UserControllerTests(IntegrationFixture fixture) : IClassFixture<IntegrationFixture>
    {
        [Fact]
        public async Task ShouldReturnHello()
        {
            await fixture.Auth.Register();
            await fixture.Auth.Login();
            var response = await fixture.Client.GetAsync(UserRoutes.Base);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
