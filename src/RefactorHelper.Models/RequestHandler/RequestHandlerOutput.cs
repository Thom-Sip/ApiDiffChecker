using RefactorHelper.Models.RequestHandler;

namespace RefactorHelper.Models
{
    public class RequestHandlerOutput
    {
        public required List<RefactorTestResultPair> Results { get; init; }
    }
}
