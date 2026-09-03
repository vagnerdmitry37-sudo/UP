using Microsoft.EntityFrameworkCore;

namespace UP.Api.Features.AppErrorFeature;

public class AppErrorMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DbUpdateConcurrencyException)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsJsonAsync(new AppErrorResponses
            {
                Message = "Refresh token is no longer valid",
                StatusCode = StatusCodes.Status401Unauthorized
            });
        }
        catch (AppError ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsJsonAsync(new AppErrorResponses
            {
                Message = ex.Message,
                StatusCode = ex.StatusCode
            });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new AppErrorResponses
            {
                Message = ex.Message,
                StatusCode = StatusCodes.Status500InternalServerError
            });
        }
    }
}
