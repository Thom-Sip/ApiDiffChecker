using ApiDiffChecker;

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
                builder.Services.AddApiDiffChecker();
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
                app.ApiDiffCheckerInitialize("./ApiDiffChecker/settings.json");
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
