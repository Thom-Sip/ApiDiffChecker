using System.Net;

namespace RefactorHelper.Models.RequestHandler
{
    public class RefactorTestResult
    {
        public required HttpResponseMessage ResponseObject { get; init; }

        public required string Response { get; init; }
    }
}
