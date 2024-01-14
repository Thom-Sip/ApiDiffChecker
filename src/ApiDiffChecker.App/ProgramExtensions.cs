using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ApiDiffChecker.Models;
using ApiDiffChecker.Models.Settings;
using ApiDiffChecker.Features.Comparer;
using ApiDiffChecker.Features.RequestHandler;
using ApiDiffChecker.Features.SwaggerProcessor;
using ApiDiffChecker.Features.UIGenerator;
using ApiDiffChecker.Models.State;
using ApiDiffChecker.Models.Enums;

namespace ApiDiffChecker
{
    public static class ProgramExtensions
    {
        public static IServiceCollection AddApiDiffChecker(this IServiceCollection services, ApiDiffCheckerSettings settings)
        {
            services.AddSingleton(settings);
            services.AddSingleton<ApiDiffCheckerState>();
            services.AddSingleton<RefactorHelperApp>();
            services.AddSingleton<SwaggerProcessorService>();
            services.AddSingleton<RequestHandlerService>();
            services.AddSingleton<CompareService>();
            services.AddSingleton<FormGeneratorService>();
            services.AddSingleton<ContentGeneratorService>();
            services.AddSingleton<SidebarGeneratorService>();

            return services;
        }   

        public static void AddApiDiffCheckerEndpoints(this WebApplication app)
        {
            var myApp = app.Services.GetRequiredService<RefactorHelperApp>();

            app.DashboardPage(myApp);
            app.ResultPage(myApp);
            app.ResetPage(myApp);
            app.SettingsPage(myApp);

            app.SidebarFragment(myApp);

            app.RunAllFragment(myApp);
            app.ResultFragment(myApp);
            app.RetryRequestFragment(myApp);
            app.SettingsFragment(myApp);
            app.AddNewRunFragment(myApp);
            app.DuplicateRunFragment(myApp);
            app.ApplySettingsFragment(myApp);
            app.RemoveRunSettingsSideBarFragment(myApp);
            app.FormFragment(myApp);
            app.AddRowToFormFragment(myApp);
            app.DeleteRowFromFormFragment(myApp);
            app.SaveFormFragment(myApp);

            app.DownloadSettings(myApp);
            app.SaveSettingsToDisk(myApp);

            app.AddStaticFileEndpoint(myApp, "styles.css");
            app.AddStaticFileEndpoint(myApp, "htmx.min.js");
        }

        #region Pages
        private static void DashboardPage(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet(Url.Page.Root, async (HttpContext context) =>
            {
                await myApp.Initialize();
                var result = myApp.UIGeneratorService.GetEmptyRequestPage();
                await context.Response.WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void ResultPage(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet($"{Url.Page.TestResult}/{{requestId}}", async (int requestId, HttpContext context) =>
            {
                await myApp.Initialize();
                var result = myApp.UIGeneratorService.GetTestResultPage(requestId);
                await context.Response.WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void ResetPage(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet(Url.Page.Reset, async (HttpContext context) =>
            {
                await myApp.Reset();
                var result = myApp.UIGeneratorService.GetEmptyRequestPage();
                await context.Response.WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void SettingsPage(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet(Url.Page.Settings, async (HttpContext context, int? runId = null) =>
            {
                await myApp.Initialize();
                var result = myApp.UIGeneratorService.GetSettingsPage(runId);
                await context.Response.WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }
        #endregion

        #region Sidebar
        private static void SidebarFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            app.MapGet($"{Url.Fragment.Sidebar}/{{sidebarType}}", async (HttpContext context, SidebarType sidebarType) =>
            {
                var result = myApp.SidebarGeneratorService.GetSideBarFragment(sidebarType);
                await context.Response.WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }
        #endregion

        #region Fragments
        private static void RunAllFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet(Url.Fragment.RunAll, async (HttpContext context) =>
            {
                var result = myApp.RunAll();
                await context.Response
                    .SetSidebar(SidebarType.RequestsPolling)
                    .WriteHtmlResponse("");

            }).ExcludeFromDescription();
        }

        private static void ResultFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Navigate to the results of a request based on its index
            app.MapGet($"{Url.Fragment.TestResult}/{{requestId}}", async (int requestId, HttpContext context) =>
            {
                var result = myApp.UIGeneratorService.GetTestResultFragment(requestId);
                await context.Response
                    .SetSidebar(SidebarType.Requests)
                    .WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void RetryRequestFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run single requst and return html to replace result in page
            app.MapGet(Url.Fragment.RetryCurrentRequest, async (HttpContext context) =>
            {
                var result = myApp.RetryCurrentRequestFragment();
                await context.Response
                    .SetSidebar(SidebarType.RequestsPolling)
                    .WriteHtmlResponse("");

            }).ExcludeFromDescription();
        }

        private static void SettingsFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet(Url.Fragment.Settings, async (HttpContext context, int? runId = null) =>
            {
                await myApp.Initialize();
                var result = myApp.UIGeneratorService.GetSettingsFragment(runId);

                await context.Response
                    .SetSidebar(SidebarType.Settings)
                    .WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void AddNewRunFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run single request and return html to replace result in page
            app.MapGet(Url.Fragment.AddNewRun, async (HttpContext context) =>
            {
                myApp.AddRun();
                var result = myApp.UIGeneratorService.GetSettingsFragment(myApp.Settings.Runs.Count - 1);

                await context.Response
                    .SetSidebar(SidebarType.Settings)
                    .WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void DuplicateRunFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run single request and return html to replace result in page
            app.MapGet(Url.Fragment.CopyRun, async (HttpContext context, int? runId) =>
            {
                myApp.DuplicateRun(runId);
                var result = myApp.UIGeneratorService.GetSettingsFragment(myApp.Settings.Runs.Count - 1);

                await context.Response
                    .SetSidebar(SidebarType.Settings)
                    .WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void ApplySettingsFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet(Url.Fragment.ApplySettings, async (HttpContext context, int? runId = null) =>
            {
                await myApp.Reset();

                await context.Response
                    .SetSidebar(SidebarType.Requests)
                    .WriteHtmlResponse("");

            }).ExcludeFromDescription();
        }

        private static void FormFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet($"{Url.Fragment.FormGet}/{{formType}}", async (bool allowEdit, FormType formType, int? runId, HttpContext context) =>
            {
                await myApp.Initialize();
                myApp.ClearEmptyReplaceValues(runId);
                var result = myApp.FormGeneratorService.GetFormFragment(formType, allowEdit, runId);
                await context.Response.WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }

        private static void AddRowToFormFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run single request and return html to replace result in page
            app.MapPut($"{Url.Fragment.FormPut}/{{formType}}/add", async (HttpContext context, FormType formType, int? runId, IFormCollection formData) =>
            {
                myApp.FormGeneratorService.SaveForm(formType, formData, runId);
                myApp.FormGeneratorService.AddRow(formType, runId);
                myApp.ProcessSettings();
                var result = myApp.FormGeneratorService.GetFormFragment(formType, true, runId);

                await context.Response
                    .SetSidebar(SidebarType.Settings)
                    .WriteHtmlResponse(result);

            }).ExcludeFromDescription().DisableAntiforgery();
        }

        private static void DeleteRowFromFormFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run single request and return html to replace result in page
            app.MapDelete($"{Url.Fragment.FormDeleteRow}/{{formType}}", async (HttpContext context, FormType formType, int? runId, int rowId, IFormCollection formData) =>
            {
                myApp.FormGeneratorService.SaveForm(formType, formData, runId);
                myApp.FormGeneratorService.DeleteRow(formType, runId, rowId);
                myApp.ProcessSettings();
                var result = myApp.FormGeneratorService.GetFormFragment(formType, true, runId);

                await context.Response
                    .SetSidebar(SidebarType.Settings)
                    .WriteHtmlResponse(result);

            }).ExcludeFromDescription().DisableAntiforgery();
        }

        private static void SaveFormFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run single request and return html to replace result in page
            app.MapPut($"{Url.Fragment.FormPut}/{{formType}}", async (HttpContext context, FormType formType, int? runId, IFormCollection formData) =>
            {
                myApp.FormGeneratorService.SaveForm(formType, formData, runId);
                myApp.ProcessSettings();
                var result = myApp.FormGeneratorService.GetFormFragment(formType, false, runId);

                await context.Response
                    .SetSidebar(SidebarType.Settings)
                    .WriteHtmlResponse(result);

            }).ExcludeFromDescription().DisableAntiforgery();
        }

        private static void RemoveRunSettingsSideBarFragment(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run single request and return html to replace result in page
            app.MapDelete($"{Url.Fragment.SideBarSettingsRemoveRun}/{{runId}}", async (HttpContext context, int runId) =>
            {
                myApp.Settings.Runs.RemoveAt(runId);
                var result = myApp.SidebarGeneratorService.GenerateSettingsSideBarFragment(runId);
                await context.Response.WriteHtmlResponse(result);

            }).ExcludeFromDescription();
        }
        #endregion

        #region Download and Save
        private static void DownloadSettings(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run single request and return html to replace result in page
            app.MapGet(Url.Download.Settings, async (HttpContext context) =>
            {
                var result = JsonConvert.SerializeObject(myApp.Settings, Formatting.Indented);
                await context.Response.SetResponseHeader("ContentType", "application/json").WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static void SaveSettingsToDisk(this WebApplication app, RefactorHelperApp myApp)
        {
            // Run all request and open static html in browser
            app.MapGet(Url.Fragment.SaveSettingsToDisk, async (HttpContext context) =>
            {
                myApp.Settings.SaveSettingsToDisk();

                await context.Response
                    .SetSidebar(SidebarType.Requests)
                    .WriteHtmlResponse("");

            }).ExcludeFromDescription();
        }
        #endregion

        #region Misc
        private static void AddStaticFileEndpoint(this WebApplication app, RefactorHelperApp myApp, string fileName)
        {
            // Get css so we don't need to service static files
            app.MapGet($"{Url.Page.Root}/{fileName}", async (HttpContext context) =>
            {
                var result = myApp.GetContentFile(fileName);
                await context.Response.WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static async Task WriteHtmlResponse(this HttpResponse response, string result)
            => await response.SetHtmlHeader().WriteAsync(result);

        public static HttpResponse SetHtmlHeader(this HttpResponse response) => 
            response.SetResponseHeader("ContentType", "text/html");

        public static HttpResponse SetSidebar(this HttpResponse response, SidebarType sidebarType) =>
            response.SetHxTriggerHeader($"{{\"loadSidebar\":\"{Url.Fragment.Sidebar}/{sidebarType}\"}}");

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
