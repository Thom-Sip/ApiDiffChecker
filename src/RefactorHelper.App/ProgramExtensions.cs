using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RefactorHelper.Models.Config;
using System.Diagnostics;

namespace RefactorHelper.App
{
    public static class ProgramExtensions
    {
        public static IServiceCollection AddRefactorHelper(this IServiceCollection services, RefactorHelperSettings settings)
        {
            services.AddSingleton<RefactorHelperApp>(new RefactorHelperApp(settings));

            return services;
        }

        public static void AddRefactorHelperEndpoint(this WebApplication app)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper", async (HttpContext context) =>
            {
                context.Response.Headers.ContentType = "text/html";
                var service = app.Services.GetRequiredService<RefactorHelperApp>();
                var result = await service.Run(context);

                if (result.Count > 0)
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
            }).ExcludeFromDescription();

            // Run single requst and return html to replace result in page
            app.MapGet("/run-refactor-helper/{runId}", async (int runId, HttpContext context) => 
            {
                var service = app.Services.GetRequiredService<RefactorHelperApp>();
                var result = await service.PerformSingleCall(runId);

                context.Response.Headers.ContentType = "text/html";
                await context.Response.WriteAsync(result);
            }).ExcludeFromDescription();

            // Run single requst and return html to replace result in page
            app.MapGet("/run-refactor-helper/request-list", async (HttpContext context) =>
            {
                var service = app.Services.GetRequiredService<RefactorHelperApp>();
                var result = service.GetRequestListHtml();

                context.Response.Headers.ContentType = "text/html";
                await context.Response.WriteAsync(result);
            }).ExcludeFromDescription();
        }
    }
}
