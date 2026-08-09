namespace UP.Api.Features.AppErrorFeature
{
    public  abstract class AppError(string message) : Exception(message)
    {
        public abstract int StatusCode { get; set; }
    }


    public class AuthError(string message) : AppError(message)
    {
        override public int StatusCode { get; set; } = StatusCodes.Status401Unauthorized;
    }
}
