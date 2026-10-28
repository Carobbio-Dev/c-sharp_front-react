using System;
using System.IO;
using System.Net.Http.Formatting;
using System.Web.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace Backend
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var serverIp = config["ServerOptions:Ip"] ?? "0.0.0.0";
            var portStr = config["ServerOptions:Port"];
            int serverPort = 5000;
            if (!string.IsNullOrEmpty(portStr) && int.TryParse(portStr, out int parsedPort))
            {
                serverPort = parsedPort;
            }

            string bindIp = (serverIp == "0.0.0.0") ? "*" : serverIp;
            string url = $"http://{bindIp}:{serverPort}/";

            Console.WriteLine($"Starting server on {url}...");

            using (WebApp.Start(url, app => Configuration(app, Directory.GetCurrentDirectory())))
            {
                Console.WriteLine($"Server listening on {url}");
                var resetEvent = new System.Threading.ManualResetEvent(false);
                AppDomain.CurrentDomain.ProcessExit += (sender, e) => resetEvent.Set();
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    resetEvent.Set();
                };
                resetEvent.WaitOne();
            }
        }

        public static void Configuration(IAppBuilder app, string baseDirectory)
        {
            app.UseCors(CorsOptions.AllowAll);

            var httpConfig = new HttpConfiguration();
            httpConfig.MapHttpAttributeRoutes();
            httpConfig.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            httpConfig.Formatters.Clear();
            httpConfig.Formatters.Add(new JsonMediaTypeFormatter());

            app.UseWebApi(httpConfig);

            string frontendDistPath = Path.GetFullPath(Path.Combine(baseDirectory, "..", "frontend", "dist"));
            string wwwrootPath = Path.GetFullPath(Path.Combine(baseDirectory, "wwwroot"));

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
                var fileSystem = new PhysicalFileSystem(staticPath);

                var fileServerOptions = new FileServerOptions
                {
                    FileSystem = fileSystem,
                    EnableDefaultFiles = true,
                    EnableDirectoryBrowsing = false
                };
                fileServerOptions.DefaultFilesOptions.DefaultFileNames = new[] { "index.html" };

                app.UseFileServer(fileServerOptions);

                app.Use(async (context, next) =>
                {
                    await next();

                    if (context.Response.StatusCode == 404 && !context.Request.Path.Value.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
                    {
                        string indexPath = Path.Combine(staticPath, "index.html");
                        if (File.Exists(indexPath))
                        {
                            context.Response.StatusCode = 200;
                            context.Response.ContentType = "text/html";
                            byte[] bytes = File.ReadAllBytes(indexPath);
                            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                        }
                    }
                });
            }
        }
    }

    [RoutePrefix("api")]
    public class PingController : ApiController
    {
        [HttpGet]
        [Route("ping")]
        public IHttpActionResult Ping()
        {
            return Ok(new
            {
                message = "pong",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
