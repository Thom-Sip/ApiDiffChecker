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
                // RefactorHelper Settings
                var settings = new RefactorHelperSettings(
                    baseUrl1: "https://localhost:44371",
                    baseUrl2: "https://localhost:44371")
                {
                    RunOnStart = false,
                    DefaultParameters = 
                    [
                        new("customerId", "400721")
                    ],
                    Runs =
                    [
                        [new("accountId", "1")],
                        [new("accountId", "2")],
                    ],
                    PropertiesToReplace =
                    [
                        new("Timestamp", "[REPLACED_TIMESTAMP]"),
                        new("requestId", $"[{Guid.Empty}]"),
                    ]
                };

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
    }
}
