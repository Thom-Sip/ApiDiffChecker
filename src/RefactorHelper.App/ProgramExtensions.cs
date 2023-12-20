using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RefactorHelper.Models.Config;

namespace RefactorHelper.App
{
    public static class ProgramExtensions
    {
        public static IServiceCollection AddRefactorHelper(this IServiceCollection services, RefactorHelperSettings settings)
        {
            services.AddSingleton(new RefactorHelperApp(settings));
            return services;
        }

        public static void AddRefactorHelperEndpoints(this WebApplication app)
        {
            app.AddPrimaryEndpoint();
            app.AddRetrySingleRequestEndpoint();
            app.AddGetRequestListEndpoint();
        }

        private static void AddPrimaryEndpoint(this WebApplication app)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper", async (HttpContext context) =>
            {
                var result = await app.Services
                    .GetRequiredService<RefactorHelperApp>()
                    .Run(context);

                context.Response.Headers.ContentType = "text/html";
                await context.Response.WriteAsync(result[0]);
            }).ExcludeFromDescription();
        }

        private static void AddRetrySingleRequestEndpoint(this WebApplication app)
        {
            // Run single requst and return html to replace result in page
            app.MapGet("/run-refactor-helper/{requestId}", async (int requestId, HttpContext context) =>
            {
                var result = await app.Services
                    .GetRequiredService<RefactorHelperApp>()
                    .PerformSingleCall(context, requestId);

                context.Response.Headers.ContentType = "text/html";
                await context.Response.WriteAsync(result);
            }).ExcludeFromDescription();
        }

        private static void AddGetRequestListEndpoint(this WebApplication app)
        {
            // Run single request and return html to replace result in page
            app.MapGet("/run-refactor-helper/request-list", async (HttpContext context) =>
            {
                var result = app.Services
                    .GetRequiredService<RefactorHelperApp>()
                    .GetRequestListHtml();

                context.Response.Headers.ContentType = "text/html";
                await context.Response.WriteAsync(result);
            }).ExcludeFromDescription();
        }
    }
}
