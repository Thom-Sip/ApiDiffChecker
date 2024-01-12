using ApiDiffChecker;
using ApiDiffChecker.Models.Settings;

namespace Test_Diff_UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Settings
            var settings = new ApiDiffCheckerSettings(
                baseUrl1: "https://localhost:44371", 
                baseUrl2: "https://localhost:44371")
            {
                RunOnStart = false,
            };

            var builder = WebApplication.CreateBuilder(args);

            // Setup DI
            builder.Services.AddRefactorHelper(settings);
            WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                // Setup all endpoints required for RefactorHelper to work
                app.AddRefactorHelperEndpoints();

                // Run all request and open static html in browser
                app.MapGet("/static-compare", async (HttpContext context) =>
                {
                    var result = await app.Services
                        .GetRequiredService<RefactorHelperApp>()
                        .StaticCompare("example1.json", "example2.json");

                    await context.Response
                        .SetHtmlHeader()
                        .WriteAsync(result);
                });
            }

            app.UseHttpsRedirection();
            app.Run();
        }
    }
}
