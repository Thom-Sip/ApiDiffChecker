using RefactorHelper.Models.RequestHandler;

namespace RefactorHelper.Models
{
    public class RequestHandlerResponse
    {
        public required List<RefactorTestResult> Results { get; init; }
    }
}
