using RefactorHelper.App;
using RefactorHelper.Models.Config;
using System.Diagnostics;

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

            // ==================== Setup Dependency Injection ====================
            builder.Services.AddRefactorHelper(new RefactorHelperSettings
            {
                SwaggerUrl = "https://localhost:44371/swagger/v1/swagger.json",
                BaseUrl1 = "https://localhost:44371",
                BaseUrl2 = "https://localhost:44371",
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
            });
            // ===================== End Dependency Injection =====================

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.MapGet("/run-refactor-helper", async context => {
                context.Response.Headers["content-type"] = "text/html";
                var service = app.Services.GetRequiredService<RefactorHelperApp>();
                var result = await service.Run();

                if (result.Any())
                {
                    var p = new Process
                    {
                        StartInfo = new ProcessStartInfo(result.First())
                        {
                            UseShellExecute = true
                        }
                    };
                    p.Start();
                }

                await context.Response.WriteAsync("Thank you for using RefactorHelper");
            });

            app.Run();
        }
    }
}
