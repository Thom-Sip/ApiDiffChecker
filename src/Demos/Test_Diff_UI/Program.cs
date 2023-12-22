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
                BaseUrl2 = "https://localhost:44371",
                DefaultParameters = [new("customerId", "4007")],
                Runs =
                [
                    [new("message", "Foo")],
                    [new("message", "Bar")],
                ],
                PropertiesToReplace =
                [
                    new("Timestamp", "[REPLACED_TIMESTAMP]"),
                    new("requestId", $"{Guid.Empty}"),
                ]
            };

            var builder = WebApplication.CreateBuilder(args);

            // Setup DI
            builder.Services.AddRefactorHelper(settings);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                // Setup all endpoints required for RefactorHelper to work
                app.AddRefactorHelperEndpoints();

                string result = null;

                // Run all request and open static html in browser
                app.MapGet("/static-compare", async (HttpContext context) =>
                {
                    result ??= await app.Services
                        .GetRequiredService<RefactorHelperApp>()
                        .StaticCompare(context, "example1.json", "example2.json");

                    await context.Response
                        .SetHtmlHeader()
                        .WriteAsync(result);

                }).ExcludeFromDescription();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
