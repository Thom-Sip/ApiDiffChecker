namespace ApiDiffChecker.Features.RequestHandler
{
    public class RequestHandlerResult
    {
        public required HttpResponseMessage ResponseObject { get; init; }

        public required string Response { get; init; }
    }
}
