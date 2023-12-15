using Microsoft.AspNetCore.Rewrite;
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
                OutputFolder = $"{GetBinPath()}/wwwroot/RefactorHelper/Output/",
                ContentFolder = $"{Environment.CurrentDirectory}/wwwroot/RefactorHelper/Content/",
                swaggerUrl = "https://localhost:44308/swagger/v1/swagger.json",
                BaseUrl1 = "https://localhost:44308",
                BaseUrl2 = "https://localhost:44308"
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

            //"D:/git/RefactorHelper/src/Demos/Sample_Api_Demo/bin/Debug/net8.0/Output/2023-12-14_21.12.22/_AAARefactor.html"

            //RewriteOptions rewriteOptions = new RewriteOptions()
                //.AddRewrite("test", "index.html", true);

            //_ = app.UseRewriter(rewriteOptions);

            //app.Map()


            //app.MapFallbackToFile("/2023-12-14_21.12.22/_AAARefactor.html");

            //app.MapGet()

            app.Run();
        }

        private static string GetBinPath() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "");
    }
}
