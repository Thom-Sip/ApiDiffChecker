namespace ApiDiffChecker.Models.RequestHandler
{
    public class RefactorTestResultPair
    {
        public required RequestHandlerResult Result1 { get; init; }

        public required RequestHandlerResult Result2 { get; init; }
    }
}
