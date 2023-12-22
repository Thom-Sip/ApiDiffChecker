using RefactorHelper.App;
using RefactorHelper.Models.Config;

namespace Test_Diff_UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Settings
            var settings = new RefactorHelperSettings
            {
                RunOnStart = false,
                BaseUrl1 = "https://localhost:44371",
                BaseUrl2 = "https://localhost:44371"
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
                        .StaticCompare(context, "example1.json", "example2.json");

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
