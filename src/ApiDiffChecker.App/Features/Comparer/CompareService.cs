using ApiDiffChecker.Models.State;

namespace ApiDiffChecker.Features.Comparer
{
    public class CompareService
    {
        private ApiDiffCheckerState State { get; }
        protected diff_match_patch _dmp { get; }

        public CompareService(ApiDiffCheckerState state)
        {
            State = state;
            _dmp = new diff_match_patch();
        }

        public void CompareResponses()
        {
            foreach (var testresultPair in State.Data)
                CompareResponse(testresultPair);
        }

        public void CompareResponse(RequestWrapper wrapper)
        {
            wrapper.CompareResultPair = GetCompareResultPair(
                wrapper.TestResult.Result1.Response,
                wrapper.TestResult.Result2.Response,
                wrapper.TestResult.Result1.ResponseObject,
                wrapper.TestResult.Result2.ResponseObject);
        }

        public CompareResultPair GetCompareResultPair(string fileOne, string fileTwo, HttpResponseMessage? response1 = null, HttpResponseMessage? response2 = null)
        {
            // Get diffs
            var diffs1 = _dmp.diff_main(fileOne, fileTwo);

            // Only show relevant differences
            _dmp.diff_cleanupSemantic(diffs1);

            return new CompareResultPair
            {
                Diffs = diffs1,
                Result1 = GetCompareResult(fileOne, response1 ?? new()),
                Result2 = GetCompareResult(fileTwo, response2 ?? new())
            };
        }

        private CompareResult GetCompareResult(string result, HttpResponseMessage response)
        {
            return new CompareResult
            {
                Result = result,
                Response = response
            };
        }
    }
}
