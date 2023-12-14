using System.Net;

namespace RefactorHelper.Models
{
    public class RefactorTestResponse
    {
        public required string Response1 { get; init; }

        public HttpStatusCode ResultCode1 { get; init; }

        public required string Response2 { get; init; }

        public HttpStatusCode ResultCode2 { get; init; }

        public required string Path { get; init; }
    }
}
