using WhatsAppPlatform.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Platform")
    ?? throw new InvalidOperationException("Configure ConnectionStrings__Platform before starting the API.");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/health/live", () => Results.Ok(new { status = "alive" }));
app.MapFallback("/api/{**path}", () => Results.NotFound());
app.MapFallbackToFile("index.html");
app.Run();
