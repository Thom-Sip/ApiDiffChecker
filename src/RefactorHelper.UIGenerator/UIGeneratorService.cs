using Microsoft.AspNetCore.Http;
using RefactorHelper.Models;
using RefactorHelper.Models.Comparer;
using RefactorHelper.Models.Config;
using RefactorHelper.Models.External;
using RefactorHelper.Models.RequestHandler;
using RefactorHelper.Models.SwaggerProcessor;
using RefactorHelper.Models.Uigenerator;
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
        protected string _settingsFragmentTemplate { get; set; }

        protected string _requestListHtml { get; set; } = string.Empty;

        protected Formbuilder Formbuilder { get; set; }

        public UIGeneratorService(string contentFolder, string outputFolder)
        {
            _template = File.ReadAllText($"{contentFolder}/Template.html");
            _contentTemplate = File.ReadAllText($"{contentFolder}/ContentTemplate.html");
            _diffBoxTemplate = File.ReadAllText($"{contentFolder}/DiffBoxTemplate.html");
            _requestListTemplate = File.ReadAllText($"{contentFolder}/RequestListTemplate.html");
            _settingsFragmentTemplate = File.ReadAllText($"{contentFolder}/SettingsFragment.html");
            _outputFolder = outputFolder;
            _runfolder = outputFolder;

            Formbuilder = new Formbuilder(contentFolder);

            if (!Directory.Exists(_outputFolder))
                Directory.CreateDirectory(_outputFolder);
        }

        public void GenerateBaseUI(RefactorHelperState state, HttpContext httpContext)
        {
            GenerateRequestListHtml(state.Data, httpContext);

            state.BaseHtmlTemplate = new HtmlTemplate
            {
                Html = _template
                    .Replace("[RETRY_REQUEST_URL]", $"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/fragment/retry")
                    .Replace("[RETRY_ALL_URL]", $"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/fragment/run-all")
                    .Replace("[RESET_URL]", $"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/reset")
                    .Replace("[REQUEST_LIST_URL]", $"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/fragment/request-list")
                    .Replace("[SETTINGS_URL]", $"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/settings")
                    .Replace("[SETTINGS_FRAGMENT_URL]", $"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/fragment/settings")
            };
        }

        public string GetHtmlPage(CompareResultPair resultPair)
        {
            return _template.Replace("[CONTENT_BLOCK]", GetContent(resultPair, resultPair.Diffs, null));
        }

        public string GetTestResultFragment(RequestWrapper wrapper, RefactorHelperState state, HttpContext httpContext)
        {
            var content = GetContent(wrapper);
            GenerateRequestListHtml(state.Data, httpContext);
            return content;
        }

        public string GetSettingsFragment(SwaggerProcessorOutput swaggerOutput)
        {
            var result = _settingsFragmentTemplate
                .Replace("[URL_PARAMETERS]", Formbuilder.GetForm(swaggerOutput.UrlParameters))
                .Replace("[QUERY_PARAMETERS]", Formbuilder.GetForm(swaggerOutput.QueryParameters));

            return result;
        }

        public string GetRequestListFragment() => _requestListHtml;

        private string GetContent(RequestWrapper wrapper) =>
            GetContent(wrapper.CompareResultPair, wrapper.CompareResultPair?.Diffs ?? [], wrapper);

        private string GetContent(CompareResultPair? compareResultPair, List<Diff> diffs, RequestWrapper? wrapper)
        {
            var original = diff_prettyHtml_custom(compareResultPair?.Result1, diffs, wrapper, [Operation.EQUAL, Operation.DELETE]);
            var changed = diff_prettyHtml_custom(compareResultPair?.Result1, diffs, wrapper, [Operation.EQUAL, Operation.INSERT]);

            return _contentTemplate
                .Replace("[CONTENT_ORIGINAL]", original)
                .Replace("[CONTENT_CHANGED]", changed);
        }

        private void GenerateRequestListHtml(List<RequestWrapper> wrappers, HttpContext httpContext)
        {
            var pendingRequests = wrappers.Where(x => !x.Executed).ToList();
            var failedRequests = wrappers.Where(x => x.Changed && x.Executed).ToList();
            var successfulRequest = wrappers.Where(x => !x.Changed && x.Executed).ToList();

            var requestsPendingListHtml = GetSidebarContent(pendingRequests, httpContext);
            var requestsFailedListHtml = GetSidebarContent(failedRequests, httpContext);
            var requestsSuccessListHtml = GetSidebarContent(successfulRequest, httpContext);

            _requestListHtml = _requestListTemplate
                .Replace("[REQUESTS_PENDING]", requestsPendingListHtml)
                .Replace("[REQUESTS_PENDING_COUNT]", $"{pendingRequests.Count}")
                .Replace("[REQUESTS_FAILED]", requestsFailedListHtml)
                .Replace("[REQUESTS_FAILED_COUNT]", $"{failedRequests.Count}")
                .Replace("[REQUESTS_SUCCESS]", requestsSuccessListHtml)
                .Replace("[REQUESTS_SUCCESS_COUNT]", $"{successfulRequest.Count}");
        }

        private string GetBaseUrl(HttpRequest request)
        {
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            return baseUrl;
        }

        private string GetSidebarContent(List<RequestWrapper> resultPairs, HttpContext httpContext)
        {
            var sb = new StringBuilder();

            foreach(var item in resultPairs)
            {
                sb.Append(
                    $"<li>" +
                        $"<span class=\"request-item\" " +
                              $"hx-get=\"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/fragment/{item.Id}\"" +
                              $"hx-swap=\"innerHTML\" " +
                              $"hx-replace-url=\"/run-refactor-helper/{item.Id}\"" +
                              $"hx-target=\"#result-container\">" +
                              $"{GetResultCode(item.TestResult?.Result1)} {item.Request.Path}" +
                        $"</span>" +
                    $"</li>");
            }

            return sb.ToString();
        }

        private string diff_prettyHtml_custom(CompareResult? result, List<Diff> diffs, RequestWrapper? wrapper, List<Operation> operations)
        {
            StringBuilder sb = new();

            foreach (Diff aDiff in diffs ?? [])
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
                  .Replace("[URL]", $"Url: {result?.Response?.RequestMessage?.RequestUri?.ToString() ?? "Pending"}")
                  .Replace("[RESULTCODE]", GetResultCodeHeaderText(wrapper))
                  .Replace("[CONTENT]", sb.ToString());

            return html;
        }

        private string GetResultCodeHeaderText(RequestWrapper? wrapper)
        {
            if (wrapper?.TestResult?.Result1 != null)
                return $"{GetResultCode(wrapper?.TestResult?.Result1)} {GetResultCodeString(wrapper?.TestResult?.Result1)}";

            return "Pending";
        }

        private string GetResultCode(RefactorTestResult? result)
        {
            var statusCode = result?.ResponseObject?.StatusCode;
            return statusCode != null
                ? ((int)statusCode).ToString()
                : "_";
        }

        private string GetResultCodeString(RefactorTestResult? result)
        {
            var statusCode = result?.ResponseObject?.StatusCode;
            return statusCode != null
                ? $"{statusCode}"
                : "N/A";
        }
    }
}
