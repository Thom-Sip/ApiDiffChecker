using Microsoft.AspNetCore.Http;
using RefactorHelper.Models.Comparer;
using RefactorHelper.Models.External;
using System.Net.Http;
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

        public UIGeneratorService(string contentFolder, string outputFolder)
        {
            _template = File.ReadAllText($"{contentFolder}/Template.html");
            _contentTemplate = File.ReadAllText($"{contentFolder}/ContentTemplate.html");
            _diffBoxTemplate = File.ReadAllText($"{contentFolder}/DiffBoxTemplate.html");
            _outputFolder = outputFolder;
            _runfolder = outputFolder;

            if (!Directory.Exists(_outputFolder))
                Directory.CreateDirectory(_outputFolder);
        }

        public List<string> GenerateUI(ComparerOutput results, HttpContext httpContext)
        {
            SetupRunfolder();
            var requestsFailedListHtml = GetSidebarContent(results.Results.Where(x => x.Changed), _runfolder);
            var requestsSuccessListHtml = GetSidebarContent(results.Results.Where(x => !x.Changed), _runfolder);
            var urls = new List<string>();

            foreach (var result in results.Results)
            {
                var original = diff_prettyHtml_custom(result.Result1, result);
                var changed = diff_prettyHtml_custom(result.Result2, result);

                var content = _contentTemplate
                    .Replace("[CONTENT_ORIGINAL]", original)
                    .Replace("[CONTENT_CHANGED]", changed);



                var html = _template
                    .Replace("[REFRESH_URL]", $"{GetBaseUrl(httpContext.Request)}/run-refactor-helper/{results.Results.IndexOf(result)}")
                    .Replace("[CONTENT_BLOCK]", content)
                    .Replace("[REQUESTS_FAILED]", requestsFailedListHtml)
                    .Replace("[REQUESTS_SUCCESS]", requestsSuccessListHtml);

                var outputFileName = $"{_runfolder}/{result.FilePath}";

                // save to Disk
                File.WriteAllText(outputFileName, html);

                urls.Add(outputFileName);
            }

            return urls;
        }

        private string GetBaseUrl(HttpRequest request)
        {
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            return baseUrl;
        }

        public string GetSinglePageContent(CompareResultPair result)
        {
            var original = diff_prettyHtml_custom(result.Result1, result);
            var changed = diff_prettyHtml_custom(result.Result2, result);

            var content = _contentTemplate
                .Replace("[CONTENT_ORIGINAL]", original)
                .Replace("[CONTENT_CHANGED]", changed);

            return content;
        }

        private void SetupRunfolder()
        {
            _runfolder = $"{_outputFolder}/{DateTime.Now:yyyy-MM-dd_HH.mm.ss}";

            if (!Directory.Exists(_runfolder))
                Directory.CreateDirectory(_runfolder);
        }

        private string GetSidebarContent(IEnumerable<CompareResultPair> resultPairs, string runFolder)
        {
            var sb = new StringBuilder();
            sb.Append("<ul>");

            foreach (var item in resultPairs)
            {
                sb.Append($"<li>" +
                    $"<a href=\"{runFolder}/{item.FilePath}\">{item.Path}</a>" +
                    $"</li>");
            }

            sb.Append("</ul>");
            return sb.ToString();
        }

        private string diff_prettyHtml_custom(CompareResult result, CompareResultPair compare)
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
                  .Replace("[TITLE]", compare.Path)
                  .Replace("[URL]", $"{result.Response?.RequestMessage?.RequestUri}")
                  .Replace("[RESULTCODE]", $"{result.Response?.StatusCode.ToString() ?? "N/A"}")
                  .Replace("[CONTENT]", sb.ToString());

            return html;
        }
    }
}
