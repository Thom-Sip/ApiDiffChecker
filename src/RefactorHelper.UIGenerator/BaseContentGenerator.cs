using RefactorHelper.Models.Config;
using RefactorHelper.Models;
using RefactorHelper.Models.RequestHandler;

namespace RefactorHelper.UIGenerator
{
    public class BaseContentGenerator(RefactorHelperSettings settings, RefactorHelperState state)
    {
        protected RefactorHelperState State { get; set; } = state;

        protected RefactorHelperSettings Settings { get; set; } = settings;

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
