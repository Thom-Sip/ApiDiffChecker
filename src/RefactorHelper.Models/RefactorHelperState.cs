using RefactorHelper.Models.Comparer;
using RefactorHelper.Models.SwaggerProcessor;

namespace RefactorHelper.Models
{
    public class RefactorHelperState
    {
        public string SwaggerJson { get; set; } = string.Empty;

        public SwaggerProcessorOutput SwaggerProcessorOutput { get; set; } = new();

        public RequestHandlerOutput RequestHandlerOutput { get; set; } = new();

        public ComparerOutput ComparerOutput { get; set; } = new();

        public List<string> HtmlPages { get; set; } = [];

        public int CurrentRequest { get; set; }

        public List<RequestWrapper> Data { get; set; } = [];

        public RequestWrapper GetCurrentRequest() => Data[CurrentRequest];
    }
}
