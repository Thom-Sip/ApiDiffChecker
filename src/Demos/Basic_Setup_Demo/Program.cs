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
                RunOnStart = false,
                BaseUrl1 = "https://localhost:44371",
                BaseUrl2 = "https://localhost:44371",
                DefaultParameters = [new("customerId", "4007"), new("message", "Foo")],
                Runs =
                [
                    [new("message", "Yoo")],
                    [new("message", "Bar")],
                ],
                PropertiesToReplace = 
                [
                    new("Timestamp", "[REPLACED_TIMESTAMP]00000000-0000-0000-0000-00000000000000000000-0000-0000-0000-00000000000000000000-0000-0000-0000-00000000000000000000-0000-0000-0000-00000000000000000000-0000-0000-0000-000000000000"),
                    new("requestId", $"{Guid.Empty}"),
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

                // Setup all endpoints required for RefactorHelper to work
                app.AddRefactorHelperEndpoints();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
