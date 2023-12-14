using RefactorHelper.App;
using RefactorHelper.Models;

namespace Sample_Api_Demo
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
                OutputFolder = $"{GetBinPath()}/RefactorHelper/Output/",
                ContentFolder = $"{Environment.CurrentDirectory}/RefactorHelper/Content/",
                swaggerUrl = "https://localhost:7138/swagger/v1/swagger.json",
                BaseUrl1 = "https://localhost:7138",
                BaseUrl2 = "https://localhost:7138"
            };

            // Setup Dependency Injection
            builder.Services.AddRefactorHelper(settings);

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        private static string GetBinPath() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "");
    }
}
