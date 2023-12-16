using RefactorHelper.App;
using RefactorHelper.Models;

namespace Basic_Setup_Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Refactor Helper Settings
            var settings = new RefactorHelperSettings
            {
                OutputFolder = $"{GetBinPath()}/wwwroot/RefactorHelper/Output/",
                ContentFolder = $"{Environment.CurrentDirectory}/wwwroot/RefactorHelper/Content/",
                swaggerUrl = "https://localhost:44308/swagger/v1/swagger.json",
                BaseUrl1 = "https://localhost:44308",
                BaseUrl2 = "https://localhost:44308",
                DefaultParameters =
                [
                    new("orderId", "AA072"),
                    new("key", "Qwerty1234"),
                ],
                Runs =
                [
                    [
                        new("customerId", "12"),
                        new("message", "Hello_world"),
                    ],
                    [
                        new("customerId", "69"),
                        new("message", "Bye_world"),
                    ],
                ]
            };

            // Setup Dependency Injection
            builder.Services.AddRefactorHelper(settings);

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static string GetBinPath() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "");
    }
}
