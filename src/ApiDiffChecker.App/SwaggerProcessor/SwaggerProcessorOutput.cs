using ApiDiffChecker.Models.Config;

namespace ApiDiffChecker.SwaggerProcessor
{
    public class SwaggerProcessorOutput
    {
        public List<Parameter> UrlParameters { get; init; } = [];
        public List<Parameter> QueryParameters { get; init; } = [];
        public List<RequestDetails> Requests { get; init; } = [];
    }
}
