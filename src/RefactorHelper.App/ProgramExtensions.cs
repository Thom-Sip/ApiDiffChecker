using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RefactorHelper.Models.Config;

namespace RefactorHelper.App
{
    public static class ProgramExtensions
    {
        public static IServiceCollection AddRefactorHelper(this IServiceCollection services, RefactorHelperSettings settings) =>
            services.AddSingleton(new RefactorHelperApp(settings));

        private static RefactorHelperApp App(this WebApplication app) => 
            app.Services.GetRequiredService<RefactorHelperApp>();

        public static void AddRefactorHelperEndpoints(this WebApplication app)
        {
            app.AddInitliazeEndpoint();
            app.AddOpenResultPageEndpoint();
            app.AddRunAllFragmentEndpoint();
            app.AddOpenRequestFragmentEndpoint();
            app.AddRetrySingleRequestFragmentEndpoint();
            app.AddGetRequestListFragmentEndpoint();
            app.AddStaticFileEndpoint("styles.css");
            app.AddStaticFileEndpoint("htmx.min.js");
        }

        private static void AddInitliazeEndpoint(this WebApplication app)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper", async (HttpContext context) =>
            {
                await app.App().Initialize(context);
                var result = app.App().GetDashboard();

                await context.Response
                    .SetHtmlHeader()
                    .WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static void AddOpenResultPageEndpoint(this WebApplication app)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper/{requestId}", async (int requestId, HttpContext context) =>
            {
                await app.App().Initialize(context);
                var result = app.App().GetResultPage(requestId, context);

                await context.Response
                    .SetHtmlHeader()
                    .WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static void AddRunAllFragmentEndpoint(this WebApplication app)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper/fragment/run-all", async (HttpContext context) =>
            {
                var result = await app.App()
                    .RunAll(context);

                await context.Response
                    .SetHtmlHeader()
                    .SetHxTriggerHeader("refresh-request-list")
                    .WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static void AddOpenRequestFragmentEndpoint(this WebApplication app)
        {
            // Navigate to the results of a request based on its index
            app.MapGet("/run-refactor-helper/fragment/{requestId}", async (int requestId, HttpContext context) =>
            {
                var result = app.App()
                    .GetResultFragment(context, requestId);

                await context.Response
                    .SetHtmlHeader()
                    .WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static void AddRetrySingleRequestFragmentEndpoint(this WebApplication app)
        {
            // Run single requst and return html to replace result in page
            app.MapGet("/run-refactor-helper/fragment/retry", async (HttpContext context) =>
            {
                var result = await app.App()
                    .RetryCurrentRequestFragment(context);

                await context.Response
                    .SetHtmlHeader()
                    .SetHxTriggerHeader("refresh-request-list")
                    .WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static void AddGetRequestListFragmentEndpoint(this WebApplication app)
        {
            // Run single request and return html to replace result in page
            app.MapGet("/run-refactor-helper/fragment/request-list", async (HttpContext context) =>
            {
                var result = app.App()
                    .GetRequestListFragment();

                await context.Response
                    .SetHtmlHeader()
                    .WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static void AddStaticFileEndpoint(this WebApplication app, string fileName)
        {
            // Get css so we don't need to service static files
            app.MapGet($"/run-refactor-helper/{fileName}", async (HttpContext context) =>
            {
                var result = app.App().GetContentFile(fileName);
                await context.Response.WriteAsync(result);

            }).ExcludeFromDescription();
        }

        public static HttpResponse SetHtmlHeader(this HttpResponse response) => 
            response.SetResponseHeader("ContentType", "text/html");

        public static HttpResponse SetHxTriggerHeader(this HttpResponse response, string trigger) =>
            response.SetResponseHeader("HX-Trigger", trigger);

        public static HttpResponse SetResponseHeader(this HttpResponse response, string key, string value)
        {
            response.Headers[key] = value;
            return response;
        }
    }
}
