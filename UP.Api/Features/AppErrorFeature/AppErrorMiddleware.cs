namespace UP.Api.Features.AppErrorFeature
{
    public class AppErrorMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppError ex)
            {
                context.Response.StatusCode = ex.StatusCode;
                await context.Response.WriteAsJsonAsync(new
                {
                    ex.Message,
                    ex.StatusCode
                });
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new
                {
                    ex.Message,
                    StatusCode = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
