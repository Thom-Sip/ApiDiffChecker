using ApiDiffChecker.Models.Config;
using ApiDiffChecker.Features.RequestHandler;
using ApiDiffChecker.Models.State;

namespace ApiDiffChecker.Features.UIGenerator
{
    public class BaseContentGenerator(ApiDiffCheckerSettings settings, ApiDiffCheckerState state)
    {
        protected ApiDiffCheckerState State { get; } = state;

        protected ApiDiffCheckerSettings Settings { get; } = settings;

        protected static string GetResultCodeHeaderText(RequestWrapper? wrapper)
        {
            if (wrapper?.TestResult?.Result1 != null)
                return $"{GetResultCode(wrapper?.TestResult?.Result1)} {GetResultCodeString(wrapper?.TestResult?.Result1)}";

            return "Pending";
        }

        protected static string GetResultCode(RequestHandlerResult? result)
        {
            var statusCode = result?.ResponseObject?.StatusCode;
            return statusCode != null
                ? ((int)statusCode).ToString()
                : "_";
        }

        protected static string GetResultCodeString(RequestHandlerResult? result)
        {
            var statusCode = result?.ResponseObject?.StatusCode;
            return statusCode != null
                ? $"{statusCode}"
                : "N/A";
        }
    }
}
