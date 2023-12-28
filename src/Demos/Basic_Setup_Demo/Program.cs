using RefactorHelper.App;
using RefactorHelper.Models.Config;

namespace Basic_Setup_Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            if (builder.Environment.IsDevelopment())
            {
                // Get Settings from json
                //var settingsFromJson = RefactorHelperSettings.GetSettingsFromJson(
                //    jsonPath: GetSettingsJsonPath(),
                //    baseUrl1: "https://localhost:44371",
                //    baseUrl2: "https://localhost:44371");

                // Static settings
                var settings = new RefactorHelperSettings(
                    baseUrl1: "https://localhost:44371",
                    baseUrl2: "https://localhost:44371");

                // RefactorHelper Dependency Injection
                builder.Services.AddRefactorHelper(settings);
            } 

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                // RefactorHelper Endpoints
                app.AddRefactorHelperEndpoints();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

        private static string GetSettingsJsonPath()
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                AppDomain.CurrentDomain.RelativeSearchPath ?? "",
                "refactorHelperSettings.json");
        }
    }
}
