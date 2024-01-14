using ApiDiffChecker;
using ApiDiffChecker.Models.Settings;

namespace Basic_Setup_Demo
{
    public class Large_Project
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            if (builder.Environment.IsDevelopment())
            {
                // ApiDiffChecker Dependency Injection
                builder.Services.AddApiDiffChecker(
                    ApiDiffCheckerSettings.GetSettingsFromJson(
                    jsonPath: $"{Environment.CurrentDirectory}/ApiDiffCheckerSettings.json",
                    baseUrl1: "https://localhost:44366",
                    baseUrl2: "https://localhost:44366"));
            }

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                // ApiDiffChecker Endpoints
                app.AddApiDiffCheckerEndpoints();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
