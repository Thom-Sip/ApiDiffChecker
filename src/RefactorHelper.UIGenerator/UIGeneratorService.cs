using Microsoft.AspNetCore.Http;
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
                wrapper.ResultHtml = _template
                    .Replace("[REFRESH_SIDEBAR_URL]", GetRefreshUrl(httpContext, wrapper.Id))
                    .Replace("[REQUEST_LIST_URL]", $"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/request-list")
                    .Replace("[CONTENT_BLOCK]", GetContent(wrapper));
            }
        }

        public string GenerateHtmlPage(CompareResultPair resultPair)
        {
            return _template
                .Replace("[REFRESH_SIDEBAR_URL]", "/Does-not-exist")
                .Replace("[REQUEST_LIST_URL]", "/Does-not-exist")
                .Replace("[CONTENT_BLOCK]", GetContent(resultPair));
        }

        public string GetSinglePageContent(RequestWrapper wrapper, RefactorHelperState state, HttpContext httpContext)
        {
            var content = GetContent(wrapper);

            // TODO: Only run this when the result is different then before
            GenerateRequestListHtml(state.Data, httpContext);

            return content;
        }

        private string GetContent(RequestWrapper wrapper)
        {
            var original = diff_prettyHtml_custom(wrapper.CompareResultPair?.Result1, wrapper, [Operation.EQUAL, Operation.INSERT]);
            var changed = diff_prettyHtml_custom(wrapper.CompareResultPair?.Result1, wrapper, [Operation.EQUAL, Operation.DELETE]);

            return _contentTemplate
                .Replace("[CONTENT_ORIGINAL]", original)
                .Replace("[CONTENT_CHANGED]", changed);
        }

        private string GetContent(CompareResultPair compareResultPair)
        {
            var original = diff_prettyHtml_custom(compareResultPair?.Result1, null, [Operation.EQUAL, Operation.DELETE]);
            var changed = diff_prettyHtml_custom(compareResultPair?.Result1, null, [Operation.EQUAL, Operation.INSERT]);

            return _contentTemplate
                .Replace("[CONTENT_ORIGINAL]", original)
                .Replace("[CONTENT_CHANGED]", changed);
        }

        public string GetRequestListHtml() => _requestListHtml;

        private string GetRefreshUrl(HttpContext httpContext, int index) => $"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/{index}";

        private void GenerateRequestListHtml(List<RequestWrapper> wrappers, HttpContext httpContext)
        {
            var requestsPendingListHtml = GetSidebarContent(wrappers.Where(x => !x.Executed).ToList(), httpContext);
            var requestsFailedListHtml = GetSidebarContent(wrappers.Where(x => x.Changed && x.Executed).ToList(), httpContext);
            var requestsSuccessListHtml = GetSidebarContent(wrappers.Where(x => !x.Changed && x.Executed).ToList(), httpContext);

            _requestListHtml = _requestListTemplate
                .Replace("[REQUESTS_PENDING]", requestsPendingListHtml)
                .Replace("[REQUESTS_FAILED]", requestsFailedListHtml)
                .Replace("[REQUESTS_SUCCESS]", requestsSuccessListHtml)
                .Replace("[REFRESH_SIDEBAR_URL]", $"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/request-list")
                .Replace("[RETRY_REQUEST_URL]", $"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/retry")
                .Replace("[RETRY_ALL_URL]", $"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/run-all");
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
                          $"hx-swap=\"innerHTML\" hx-target=\"#result-container\">{item.Request.Path}</span>" +
                    $"</li>");
            }

            sb.Append("</ul>");
            return sb.ToString();
        }

        private string diff_prettyHtml_custom(CompareResult? result, RequestWrapper? wrapper, List<Operation> operations)
        {
            StringBuilder sb = new();

            foreach (Diff aDiff in result?.Diffs ?? [])
            {
                string text = aDiff.text
                    .Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");

                switch (aDiff.operation)
                {
                    case Operation.INSERT:
                        if(operations.Contains(Operation.INSERT))
                            sb.Append("<span class=\"addition\">").Append(text).Append("</span>");
                        break;
                    case Operation.DELETE:
                        if (operations.Contains(Operation.DELETE))
                            sb.Append("<span class=\"removal\">").Append(text).Append("</span>");
                        break;
                    case Operation.EQUAL:
                        if (operations.Contains(Operation.EQUAL))
                            sb.Append("<span>").Append(text).Append("</span>");
                        break;
                }
            }

            var html = _diffBoxTemplate
                  .Replace("[TITLE]", wrapper?.Request.Path)
                  .Replace("[URL]", $"{result?.Response?.RequestMessage?.RequestUri}")
                  .Replace("[RESULTCODE]", $"{result?.Response?.StatusCode.ToString() ?? "N/A"}")
                  .Replace("[CONTENT]", sb.ToString());

            return html;
        }
    }
}
