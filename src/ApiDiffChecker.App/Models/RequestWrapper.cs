using ApiDiffChecker.Models.Comparer;
using ApiDiffChecker.Models.RequestHandler;
using ApiDiffChecker.Models.SwaggerProcessor;

namespace ApiDiffChecker.Models
{
    public class RequestWrapper
    {
        public required int Id { get; set; }

        public required RequestState State { get; set; }

        public bool Changed { get; set; }

        public bool Executed => TestResult != null;

        public required RequestDetails Request { get; set; }

        public RequestHandlerResultPair? TestResult { get; set; }

        public CompareResultPair? CompareResultPair { get; set; }

        public void Clear()
        {
            TestResult = null;
            CompareResultPair = null;
            State = RequestState.Pending;
        }
    }
}
