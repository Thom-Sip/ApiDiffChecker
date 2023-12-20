﻿using Microsoft.AspNetCore.Builder;
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
            app.AddOpenRequestEndpoint();
            app.AddRetrySingleRequestEndpoint();
            app.AddGetRequestListEndpoint();
            app.AddStaticFileEndpoint("styles.css");
            app.AddStaticFileEndpoint("htmx.min.js");
        }

        private static void AddPrimaryEndpoint(this WebApplication app)
        {
            // Run all request and open static html in browser
            app.MapGet("/run-refactor-helper", async (HttpContext context) =>
            {
                var result = await app.Services
                    .GetRequiredService<RefactorHelperApp>()
                    .Run(context);

                await context.Response
                    .SetHtmlHeader()
                    .WriteAsync(result[0]);

            }).ExcludeFromDescription();
        }

        private static void AddOpenRequestEndpoint(this WebApplication app)
        {
            // Navigate to the results of a request based on its index
            app.MapGet("/run-refactor-helper/{requestId}", async (int requestId, HttpContext context) =>
            {
                var result = app.Services
                    .GetRequiredService<RefactorHelperApp>()
                    .GetResultPage(context, requestId);

                await context.Response
                    .SetHtmlHeader()
                    .WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static void AddRetrySingleRequestEndpoint(this WebApplication app)
        {
            // Run single requst and return html to replace result in page
            app.MapGet("/run-refactor-helper/retry", async (HttpContext context) =>
            {
                var result = await app.Services
                    .GetRequiredService<RefactorHelperApp>()
                    .RetryCurrentRequest(context);

                await context.Response
                    .SetHtmlHeader()
                    .SetHxTriggerHeader("refresh-request-list")
                    .WriteAsync(result);

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
                var result = app.Services
                    .GetRequiredService<RefactorHelperApp>()
                    .GetContentFile(fileName);

                await context.Response.WriteAsync(result);

            }).ExcludeFromDescription();
        }

        private static HttpResponse SetHtmlHeader(this HttpResponse response) => 
            response.SetResponseHeader("ContentType", "text/html");

        private static HttpResponse SetHxTriggerHeader(this HttpResponse response, string trigger) =>
            response.SetResponseHeader("HX-Trigger", trigger);

        private static HttpResponse SetResponseHeader(this HttpResponse response, string key, string value)
        {
            response.Headers[key] = value;
            return response;
        }
    }
}
