using ApiDiffChecker.Models.Config;
using ApiDiffChecker.Models;
using ApiDiffChecker.Models.RequestHandler;

namespace ApiDiffChecker.UIGenerator
{
    public class BaseContentGenerator(RefactorHelperSettings settings, RefactorHelperState state)
    {
        protected RefactorHelperState State { get; } = state;

        protected RefactorHelperSettings Settings { get; } = settings;

        protected static string GetResultCodeHeaderText(RequestWrapper? wrapper)
        {
            if (wrapper?.TestResult?.Result1 != null)
                return $"{GetResultCode(wrapper?.TestResult?.Result1)} {GetResultCodeString(wrapper?.TestResult?.Result1)}";

            return "Pending";
        }

        protected static string GetResultCode(RefactorTestResult? result)
        {
            var statusCode = result?.ResponseObject?.StatusCode;
            return statusCode != null
                ? ((int)statusCode).ToString()
                : "_";
        }

        protected static string GetResultCodeString(RefactorTestResult? result)
        {
            var statusCode = result?.ResponseObject?.StatusCode;
            return statusCode != null
                ? $"{statusCode}"
                : "N/A";
        }
    }
}
