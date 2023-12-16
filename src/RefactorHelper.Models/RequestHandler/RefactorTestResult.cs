using System.Net;

namespace RefactorHelper.Models.RequestHandler
{
    public class RefactorTestResult
    {
        public required HttpResponseMessage Response1Object { get; init; }

        public required string Response1 { get; init; }

        public HttpStatusCode ResultCode1 { get; init; }

        public required HttpResponseMessage Response2Object { get; init; }

        public required string Response2 { get; init; }

        public HttpStatusCode ResultCode2 { get; init; }

        public required string Path { get; init; }
    }
}
