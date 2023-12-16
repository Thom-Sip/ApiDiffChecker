using RefactorHelper.Models.RequestHandler;

namespace RefactorHelper.Models
{
    public class RequestHandlerOutput
    {
        public required List<RefactorTestResult> Results { get; init; }
    }
}
