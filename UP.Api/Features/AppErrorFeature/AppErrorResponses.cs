using System.Net;

namespace UP.Api.Features.AppErrorFeature
{
    public class AppErrorResponses
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
    }
}
