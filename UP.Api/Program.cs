using UP.Api.Features.AppErrorFeature;
using UP.Api.Features.BootstrapFeatuer;
using UP.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var bootstap = new Bootstrap(builder);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHostedService<AppBackgroundService>();

var corsMode = bootstap.Init();

builder.Services.AddControllers(options => options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter()));

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.UseCors(corsMode);

app.UseHttpsRedirection();

app.UseMiddleware<AppErrorMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

await bootstap.RunMigrateAsync(app.Services);

app.Run();
