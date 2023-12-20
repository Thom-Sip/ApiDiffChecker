﻿using Microsoft.AspNetCore.Http;
using RefactorHelper.Models;
using RefactorHelper.Models.Comparer;
using RefactorHelper.Models.External;
using System.Text;

namespace RefactorHelper.UIGenerator
{
    public class UIGeneratorService
    {
        protected string _outputFolder { get; set; }
        protected string _runfolder { get; set; }
        protected string _template { get; set; }
        protected string _contentTemplate { get; set; }
        protected string _diffBoxTemplate { get; set; }
        protected string _requestListTemplate { get; set; }

        protected string _requestListHtml { get; set; } = string.Empty;

        public UIGeneratorService(string contentFolder, string outputFolder)
        {
            _template = File.ReadAllText($"{contentFolder}/Template.html");
            _contentTemplate = File.ReadAllText($"{contentFolder}/ContentTemplate.html");
            _diffBoxTemplate = File.ReadAllText($"{contentFolder}/DiffBoxTemplate.html");
            _requestListTemplate = File.ReadAllText($"{contentFolder}/RequestListTemplate.html");
            _outputFolder = outputFolder;
            _runfolder = outputFolder;

            if (!Directory.Exists(_outputFolder))
                Directory.CreateDirectory(_outputFolder);
        }

        public void GenerateUI(RefactorHelperState state, HttpContext httpContext)
        {
            GenerateRequestListHtml(state.Data, httpContext);

            foreach (var wrapper in state.Data)
            {
                var original = diff_prettyHtml_custom(wrapper.CompareResultPair.Result1, wrapper);
                var changed = diff_prettyHtml_custom(wrapper.CompareResultPair.Result2, wrapper);

                var content = _contentTemplate
                    .Replace("[CONTENT_ORIGINAL]", original)
                    .Replace("[CONTENT_CHANGED]", changed);

                wrapper.ResultHtml = _template
                    .Replace("[REFRESH_SIDEBAR_URL]", GetRefreshUrl(httpContext, wrapper.Id))
                    .Replace("[REQUEST_LIST_URL]", $"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/request-list")
                    .Replace("[CONTENT_BLOCK]", content);
            }
        }

        public string GetSinglePageContent(RequestWrapper wrapper, RefactorHelperState state, HttpContext httpContext)
        {
            var original = diff_prettyHtml_custom(wrapper.CompareResultPair.Result1, wrapper);
            var changed = diff_prettyHtml_custom(wrapper.CompareResultPair.Result2, wrapper);

            var content = _contentTemplate
                .Replace("[CONTENT_ORIGINAL]", original)
                .Replace("[CONTENT_CHANGED]", changed);

            // TODO: Only run this when the result is different then before
            GenerateRequestListHtml(state.Data, httpContext);

            return content;
        }

        public string GetRequestListHtml() => _requestListHtml;

        private string GetRefreshUrl(HttpContext httpContext, int index) => $"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/{index}";

        private void GenerateRequestListHtml(List<RequestWrapper> wrappers, HttpContext httpContext)
        {
            var requestsFailedListHtml = GetSidebarContent(wrappers.Where(x => x.Changed).ToList(), httpContext);
            var requestsSuccessListHtml = GetSidebarContent(wrappers.Where(x => !x.Changed).ToList(), httpContext);

            _requestListHtml = _requestListTemplate
                .Replace("[REQUESTS_FAILED]", requestsFailedListHtml)
                .Replace("[REQUESTS_SUCCESS]", requestsSuccessListHtml)
                .Replace("[REFRESH_SIDEBAR_URL]", $"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/request-list")
                .Replace("[RETRY_REQUEST_URL]", $"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/retry");
        }

        private string GetBaseUrl(HttpRequest request)
        {
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            return baseUrl;
        }

        private string GetSidebarContent(List<RequestWrapper> resultPairs, HttpContext httpContext)
        {
            var sb = new StringBuilder();
            sb.Append("<ul>");

            foreach(var item in resultPairs)
            {
                sb.Append($"<li>" +
                    $"<span class=\"request-item\" hx-get=\"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/{item.Id}\" " +
                          $"hx-swap=\"innerHTML\" hx-target=\"#result-container\">{item.TestResult.Path}</span>" +
                    $"</li>");
            }

            sb.Append("</ul>");
            return sb.ToString();
        }

        private string diff_prettyHtml_custom(CompareResult result, RequestWrapper wrapper)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Diff aDiff in result.Diffs)
            {
                string text = aDiff.text
                    .Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");

                switch (aDiff.operation)
                {
                    case Operation.INSERT:
                        sb.Append("<ins style=\"background:#0f8009;\">").Append(text)
                            .Append("</ins>");
                        break;
                    case Operation.DELETE:
                        sb.Append("<del style=\"background:#ab1b11;\">").Append(text)
                            .Append("</del>");
                        break;
                    case Operation.EQUAL:
                        sb.Append("<span>").Append(text).Append("</span>");
                        break;
                }
            }

            var html = _diffBoxTemplate
                  .Replace("[TITLE]", wrapper.CompareResultPair.Path)
                  .Replace("[URL]", $"{result.Response?.RequestMessage?.RequestUri}")
                  .Replace("[RESULTCODE]", $"{result.Response?.StatusCode.ToString() ?? "N/A"}")
                  .Replace("[CONTENT]", sb.ToString());

            return html;
        }
    }
}
