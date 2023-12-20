using RefactorHelper.Models;
using RefactorHelper.Models.Comparer;
using RefactorHelper.Models.External;
using RefactorHelper.Models.RequestHandler;

namespace RefactorHelper.Comparer
{
    public class CompareService
    {
        protected diff_match_patch _dmp { get; }

        public CompareService()
        {
            _dmp = new diff_match_patch();
        }

        public void CompareResponses(RefactorHelperState state)
        {
            foreach(var testresultPair in state.Data)
                CompareResponse(testresultPair);
        }

        public void CompareResponse(RequestWrapper wrapper)
        {
            // Get diffs
            var diffs1 = _dmp.diff_main(wrapper.TestResult.Result1.Response, wrapper.TestResult.Result2.Response);
            var diffs2 = _dmp.diff_main(wrapper.TestResult.Result2.Response, wrapper.TestResult.Result1.Response);

            // Only show relevant differences
            _dmp.diff_cleanupSemantic(diffs1);
            _dmp.diff_cleanupSemantic(diffs2);

            wrapper.CompareResultPair = new CompareResultPair
            {
                Result1 = GetCompareResult(wrapper.TestResult.Result1, diffs1),
                Result2 = GetCompareResult(wrapper.TestResult.Result1, diffs2)
            };
        }

        private CompareResult GetCompareResult(RefactorTestResult result, List<Diff> diffs)
        {
            return new CompareResult
            {
                Diffs = diffs,
                Result = result.Response,
                Response = result.ResponseObject
            };
        }

        private string MakePathSafe(string url)
        {
            return url.Replace(" ", "").Replace("&", "_").Replace("?", "_").Replace("/", "_");
        }
    }
}
