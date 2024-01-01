using RefactorHelper.App;
using RefactorHelper.Models.Config;

namespace Basic_Setup_Demo
{
    public class Large_Project
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            if (builder.Environment.IsDevelopment())
            {
                // RefactorHelper Dependency Injection
                builder.Services.AddRefactorHelper(
                    RefactorHelperSettings.GetSettingsFromJson(
                    jsonPath: $"{Environment.CurrentDirectory}/refactorHelperSettings.json",
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

                // RefactorHelper Endpoints
                app.AddRefactorHelperEndpoints();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
