using System;
using System.Collections.Generic;
using System.Text;

namespace UP.IntegrationTests.Features.AppErrorFeature
{
    public class AppErrorTests(HttpClient client)
    {
        private readonly HttpClient _client = client;

        [Fact]
        public void ShouldSendCorrectResponseWithErrorInfo()
        {

        }
    }
}
