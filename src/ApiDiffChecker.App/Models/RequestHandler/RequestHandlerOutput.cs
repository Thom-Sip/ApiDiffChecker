using ApiDiffChecker.Models.RequestHandler;

namespace ApiDiffChecker.Models
{
    public class RequestHandlerOutput
    {
        public List<RefactorTestResultPair> Results { get; init; } = [];
    }
}
