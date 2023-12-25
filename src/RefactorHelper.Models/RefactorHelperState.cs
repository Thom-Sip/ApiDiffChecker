using RefactorHelper.Models.Uigenerator;

namespace RefactorHelper.Models
{
    public class RefactorHelperState
    {
        public string SwaggerJson { get; set; } = string.Empty;

        public int CurrentRequest { get; set; }

        public string BaseHtml { get; set; } = string.Empty;

        public HtmlTemplate BaseHtmlTemplate { get; set; } = new();

        public List<RequestWrapper> Data { get; set; } = [];

        public RequestWrapper GetCurrentRequest() => Data[CurrentRequest];
    }
}
