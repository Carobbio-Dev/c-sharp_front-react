using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Read IP and Port configuration from appsettings.json
var serverIp = builder.Configuration.GetValue<string>("ServerOptions:Ip") ?? "0.0.0.0";
var serverPort = builder.Configuration.GetValue<int?>("ServerOptions:Port") ?? 5000;
builder.WebHost.UseUrls($"http://{serverIp}:{serverPort}");

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

// Configure CORS for local development (e.g. Vite dev server on port 5173)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

// Serve static frontend files from ../frontend/dist or wwwroot if present
var frontendDistPath = Path.Combine(app.Environment.ContentRootPath, "..", "frontend", "dist");
var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");

string? staticPath = null;
if (Directory.Exists(frontendDistPath))
{
    staticPath = frontendDistPath;
}
else if (Directory.Exists(wwwrootPath))
{
    staticPath = wwwrootPath;
}

if (staticPath != null)
{
    var fileProvider = new PhysicalFileProvider(Path.GetFullPath(staticPath));

    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = fileProvider
    });

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = fileProvider
    });
}

// API Routes
app.MapGet("/api/ping", () =>
{
    return Results.Ok(new
    {
        message = "pong",
        timestamp = DateTime.UtcNow
    });
})
.WithName("Ping");

// SPA Fallback: route unmatched requests to frontend index.html
if (staticPath != null)
{
    app.MapFallback(async context =>
    {
        var indexPath = Path.Combine(staticPath, "index.html");
        if (File.Exists(indexPath))
        {
            context.Response.ContentType = "text/html";
            await context.Response.SendFileAsync(indexPath);
        }
        else
        {
            context.Response.StatusCode = 404;
        }
    });
}

app.Run();
