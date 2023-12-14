using RefactorHelper.App;
using RefactorHelper.Models;

namespace TestApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var settings = new RefactorHelperSettings
            {
                OutputFolder = $"{GetBinPath()}/Output/",
                ContentFolder = $"{Environment.CurrentDirectory}/Content/",
                swaggerUrl = "https://localhost:7138/swagger/v1/swagger.json",
                BaseUrl1 = "https://localhost:7138",
                BaseUrl2 = "https://localhost:7138"
            };

            builder.Services.AddRefactorHelper(settings);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
