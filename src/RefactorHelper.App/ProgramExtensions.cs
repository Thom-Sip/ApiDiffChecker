using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RefactorHelper.Comparer;
using RefactorHelper.Models;
using RefactorHelper.Models.Config;
using RefactorHelper.Models.Uigenerator;
using RefactorHelper.RequestHandler;
using RefactorHelper.SwaggerProcessor;
using RefactorHelper.UIGenerator;

namespace RefactorHelper.App
{
    public static class ProgramExtensions
    {
        public static IServiceCollection AddRefactorHelper(this IServiceCollection services, RefactorHelperSettings settings)
        {
            services.AddSingleton(settings);
            services.AddSingleton<RefactorHelperState>();
            services.AddSingleton<RefactorHelperApp>();
            services.AddSingleton<SwaggerProcessorService>();
            services.AddSingleton<RequestHandlerService>();
            services.AddSingleton<CompareService>();
            services.AddSingleton<UIGeneratorService>();

            return services;
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
            app.SettingsRunByIdFragment(myApp);
            app.SettingsSideBarFragment(myApp);
            app.AddRunSettingsSideBarFragment(myApp);
            app.UrlParamsFragment(myApp);
            app.SaveUrlParamsFragment(myApp);

            app.DownloadSettings(myApp);

            app.AddStaticFileEndpoint(myApp, "styles.css");
            app.AddStaticFileEndpoint(myApp, "htmx.min.js");
        }

        #region Pages
        private static void DashboardPage(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper", async (HttpContext context) =>
            {
                await myApp.Initialize();
                var result = myApp.UIGeneratorService.GetEmptyRequestPage();
                await context.Response.WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void ResultPage(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper/{requestId}", async (int requestId, HttpContext context) =>
            {
                await myApp.Initialize();
                var result = myApp.UIGeneratorService.GetTestResultPage(requestId);
                await context.Response.WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void ResetPage(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper/reset", async (HttpContext context) =>
            {
                await myApp.Reset();
                var result = myApp.UIGeneratorService.GetEmptyRequestPage();
                await context.Response.WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void SettingsPage(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper/settings", async (HttpContext context) =>
            {
                await myApp.Initialize();
                var result = myApp.UIGeneratorService.GetSettingsPage();
                await context.Response.WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }
        #endregion

        #region Fragments
        private static void RunAllFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper/fragment/run-all", async (HttpContext context) =>
            {
                var result = await myApp.RunAll();
                await context.Response
                    .SetHxTriggerHeader("refresh-request-list")
                    .WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void ResultFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Navigate to the results of a request based on its index
            app.MapGet("/run-refactor-helper/fragment/{requestId}", async (int requestId, HttpContext context) =>
            {
                var result = myApp.UIGeneratorService.GetTestResultFragment(requestId);
                await context.Response
                    .SetHxTriggerHeader("refresh-request-list")
                    .WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void RetryRequestFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run single requst and return html to replace result in page
            app.MapGet("/run-refactor-helper/fragment/retry", async (HttpContext context) =>
            {
                var result = await myApp.RetryCurrentRequestFragment();
                await context.Response
                    .SetHxTriggerHeader("refresh-request-list")
                    .WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void SettingsFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper/fragment/settings", async (HttpContext context) =>
            {
                await myApp.Initialize();
                var result = myApp.UIGeneratorService.GetSettingsFragment();

                await context.Response
                    .SetHxTriggerHeader("refresh-settings-list")
                    .WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void SettingsRunByIdFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper/fragment/settings/runs/{runId}", async (HttpContext context, int runId) =>
            {
                await myApp.Initialize();
                var result = myApp.UIGeneratorService.GetSettingsFragment(runId);

                await context.Response
                    .SetHxTriggerHeader("refresh-settings-list")
                    .WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void UrlParamsFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper/fragment/settings/forms/{formType}", async (bool allowEdit, FormType formType, HttpContext context) =>
            {
                await myApp.Initialize();
                var result = myApp.UIGeneratorService.GetFormFragment(formType, allowEdit);
                await context.Response.WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void SaveUrlParamsFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run single request and return html to replace result in page
            app.MapPut("/run-refactor-helper/fragment/save/{formType}", async (HttpContext context, FormType formType, IFormCollection formData) =>
            {
                myApp.SaveUrlParams(formType, formData);
                myApp.ProcessSettings();
                var result = myApp.UIGeneratorService.GetFormFragment(formType, false);

                await context.Response
                    .SetHxTriggerHeader("refresh-settings-list")
                    .WriteHtmlResponse(result);

            }).ExcludeFromDescription().DisableAntiforgery();
        }

        private static void RequestListFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run single request and return html to replace result in page
            app.MapGet("/run-refactor-helper/fragment/request-list", async (HttpContext context) =>
            {
                var result = myApp.UIGeneratorService.GetRequestListFragment();
                await context.Response.WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void SettingsSideBarFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run single request and return html to replace result in page
            app.MapGet("/run-refactor-helper/fragment/sidebar/settings", async (HttpContext context) =>
            {
                var result = myApp.UIGeneratorService.GetSettingsSideBarFragment();
                await context.Response.WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void AddRunSettingsSideBarFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run single request and return html to replace result in page
            app.MapGet("/run-refactor-helper/fragment/sidebar/settings/add", async (HttpContext context) =>
            {
                myApp.Settings.Runs.Add(new());
                var result = myApp.UIGeneratorService.GenerateSettingsSideBarFragment();
                await context.Response.WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        #endregion

        #region Download
        private static void DownloadSettings(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run single request and return html to replace result in page
            app.MapGet("/run-refactor-helper/download/settings", async (HttpContext context) =>
            {
                var result = JsonConvert.SerializeObject(myApp.Settings, Formatting.Indented);
                await context.Response.SetResponseHeader("ContentType", "application/json").WriteAsync(result);

            }).ExcludeFromDescription();
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

        private static async Task WriteHtmlResponse(this HttpResponse response, string result)
            => await response.SetHtmlHeader().WriteAsync(result);

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
