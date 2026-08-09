namespace UP.IntegrationTests.Features.Fixtures
{
    public class IntegrationFixture : IAsyncLifetime
    {
        public CustomWebApplicationFactory Factory { get; }
        public HttpClient Client { get; }
        public AuthHelper Auth { get; }

        public IntegrationFixture()
        {
            Factory = new CustomWebApplicationFactory();
            Client = Factory.CreateClient();

            Auth = new AuthHelper(Client, Factory);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            Client.Dispose();
            Factory.Dispose();

            return Task.CompletedTask;
        }
    }
}
