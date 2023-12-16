using RefactorHelper.App;
using RefactorHelper.Models.Config;

namespace Basic_Setup_Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Settings
            var settings = new RefactorHelperSettings
            {
                SwaggerUrl = "https://localhost:44371/swagger/v1/swagger.json",
                BaseUrl1 = "https://localhost:44371",
                BaseUrl2 = "https://localhost:44371",
                DefaultParameters = [new("customerId", "4007")],
                Runs =
                [
                    [new("message", "Foo")],
                    [new("message", "Bar")],
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

                // Setup Trigger endpoint
                app.AddRefactorHelperEndpoint();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
