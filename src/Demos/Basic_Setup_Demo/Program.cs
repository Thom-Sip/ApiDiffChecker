using RefactorHelper.App;
using RefactorHelper.Models.Config;

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
                DefaultParameters = [new("customerId", "4007")],
                Runs =
                [
                    [ new("message", "Foo") ],
                    [ new("message", "Bar") ],
                ]
            });
            // ===================== End Dependency Injection =====================

            WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                // ==================== Setup Trigger Endpoint ====================
                app.AddRefactorHelperEndpoint();
                // ===================== End Trigger Endpoint =====================
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
