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
            return services.AddSingleton(new RefactorHelperApp(settings));
        }   

        public static void AddRefactorHelperEndpoints(this WebApplication app)
        {
            var myApp = app.Services.GetRequiredService<RefactorHelperApp>();

            app.DashboardPage(myApp);
            app.ResultPage(myApp);
            app.ResetPage(myApp);
            app.SettingsPage(myApp);

            app.RunAllFragment(myApp);
            app.ResultFragment(myApp);
            app.RetryRequestFragment(myApp);
            app.RequestListFragment(myApp);
            app.SettingsFragment(myApp);
            app.UrlParamsFragment(myApp);
            app.SaveUrlParamsFragment(myApp);

            app.AddStaticFileEndpoint(myApp, "styles.css");
            app.AddStaticFileEndpoint(myApp, "htmx.min.js");
        }

        #region Pages
        private static void DashboardPage(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper", async (HttpContext context) =>
            {
                await myApp.Initialize(context);
                var result = myApp.GetDashboard();

                await context.Response
                    .SetHtmlHeader()
                    .WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static void ResultPage(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper/{requestId}", async (int requestId, HttpContext context) =>
            {
                await myApp.Initialize(context);
                var result = myApp.GetResultPage(requestId, context);

                await context.Response
                    .SetHtmlHeader()
                    .WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static void ResetPage(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper/reset", async (HttpContext context) =>
            {
                await myApp.Reset(context);
                var result = myApp.GetDashboard();

                await context.Response
                    .SetHtmlHeader()
                    .WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static void SettingsPage(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper/settings", async (HttpContext context) =>
            {
                await myApp.Initialize(context);
                var result = myApp.GetSettingsPage(context);

                await context.Response
                    .SetHtmlHeader()
                    .WriteAsync(result);

            }).ExcludeFromDescription();
        }
        #endregion

        #region Fragments
        private static void RunAllFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper/fragment/run-all", async (HttpContext context) =>
            {
                var result = await myApp.RunAll(context);

                await context.Response
                    .SetHtmlHeader()
                    .SetHxTriggerHeader("refresh-request-list")
                    .WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static void ResultFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Navigate to the results of a request based on its index
            app.MapGet("/run-refactor-helper/fragment/{requestId}", async (int requestId, HttpContext context) =>
            {
                var result = myApp.GetResultFragment(context, requestId);

                await context.Response
                    .SetHtmlHeader()
                    .WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static void RetryRequestFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run single requst and return html to replace result in page
            app.MapGet("/run-refactor-helper/fragment/retry", async (HttpContext context) =>
            {
                var result = await myApp.RetryCurrentRequestFragment(context);

                await context.Response
                    .SetHtmlHeader()
                    .SetHxTriggerHeader("refresh-request-list")
                    .WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static void SettingsFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper/fragment/settings", async (HttpContext context) =>
            {
                await myApp.Initialize(context);
                var result = myApp.GetSettingsFragment(context);

                await context.Response
                    .SetHtmlHeader()
                    .WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static void UrlParamsFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper/fragment/settings/urlparams", async (bool allowEdit, HttpContext context) =>
            {
                await myApp.Initialize(context);
                var result = myApp.GetUrlParamsFragment(context, allowEdit);

                await context.Response
                    .SetHtmlHeader()
                    .WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static void RequestListFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run single request and return html to replace result in page
            app.MapGet("/run-refactor-helper/fragment/request-list", async (HttpContext context) =>
            {
                var result = myApp.GetRequestListFragment();

                await context.Response
                    .SetHtmlHeader()
                    .WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static void SaveUrlParamsFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run single request and return html to replace result in page
            app.MapPut("/run-refactor-helper/fragment/save/urlparams", async (HttpContext context, IFormCollection form) =>
            {
                myApp.SaveUrlParams(form);
                myApp.ProcessSettings(context);
                var result = myApp.GetUrlParamsFragment(context, true);

                await context.Response
                    .SetHtmlHeader()
                    .SetHxTriggerHeader("refresh-request-list")
                    .WriteAsync(result);

            }).ExcludeFromDescription().DisableAntiforgery();
        }

        #endregion

        #region Misc

        private static void AddStaticFileEndpoint(this WebApplication app, RefactorHelperApp myApp, string fileName)
        {
            // Get css so we don't need to service static files
            app.MapGet($"/run-refactor-helper/{fileName}", async (HttpContext context) =>
            {
                var result = myApp.GetContentFile(fileName);
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

        #endregion
    }
}
