
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers(options => options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter()));

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
