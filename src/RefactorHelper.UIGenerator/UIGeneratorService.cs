using DiffMatchPatch;
using RefactorHelper.Models;
using System.Text;

namespace RefactorHelper.UIGenerator
{
    public class UIGeneratorService
    {
        protected string _outputFolder { get; set; }
        protected string _runfolder { get; set; }
        protected string _template { get; set; }

        public UIGeneratorService(string contentFolder, string outputFolder)
        {
            _template = File.ReadAllText($"{contentFolder}/Template.html");
            _outputFolder = outputFolder;

            if (!Directory.Exists(_outputFolder))
                Directory.CreateDirectory(_outputFolder);
        }

        private void SetupRunfolder()
        {
            _runfolder = $"{_outputFolder}/{DateTime.Now:yyyy-MM-dd_HH.mm.ss}";

            if (!Directory.Exists(_runfolder))
                Directory.CreateDirectory(_runfolder);
        }

        public List<string> GenerateUI(List<CompareResult> results)
        {
            SetupRunfolder();
            var requestsFailedListHtml = GetSidebarContent(results.Where(x => x.Changed), _runfolder);
            var requestsSuccessListHtml = GetSidebarContent(results.Where(x => !x.Changed), _runfolder);
            var urls = new List<string>();

            foreach (var result in results)
            {
                var original = diff_prettyHtml_thom(result.Diffs1);
                var changed = diff_prettyHtml_thom(result.Diffs2);

                var html = _template.Replace("[CONTENT_ORIGINAL]", original);
                html = html.Replace("[CONTENT_CHANGED]", changed);
                html = html.Replace("[REQUESTS_FAILED]", requestsFailedListHtml);
                html = html.Replace("[REQUESTS_SUCCESS]", requestsSuccessListHtml);

                var filename = $"{result.Path.Replace("/", "_")}.html";
                var outputFileName = $"{_runfolder}/{filename}";

                // save to Disk
                File.WriteAllText(outputFileName, html);

                urls.Add(outputFileName);
            }

            return urls;
        }

        private string GetSidebarContent(IEnumerable<CompareResult> results, string runFolder)
        {
            var sb = new StringBuilder();
            sb.Append("<ul>");

            foreach (var item in results)
            {
                sb.Append($"<li>" +
                    $"<a href=\"{runFolder}/{item.Path.Replace("/", "_")}.html\">{item.Path}</a>" +
                    $"</li>");
            }

            sb.Append("</ul>");
            return sb.ToString();
        }

        private string diff_prettyHtml_thom(List<Diff> diffs)
        {
            StringBuilder html = new StringBuilder();
            foreach (Diff aDiff in diffs)
            {
                string text = aDiff.text
                    .Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");

                switch (aDiff.operation)
                {
                    case Operation.INSERT:
                        html.Append("<ins style=\"background:#0f8009;\">").Append(text)
                            .Append("</ins>");
                        break;
                    case Operation.DELETE:
                        html.Append("<del style=\"background:#ab1b11;\">").Append(text)
                            .Append("</del>");
                        break;
                    case Operation.EQUAL:
                        html.Append("<span>").Append(text).Append("</span>");
                        break;
                }
            }
            return html.ToString();
        }
    }
}
