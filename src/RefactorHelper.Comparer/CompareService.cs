using RefactorHelper.Models;
using RefactorHelper.Models.Comparer;
using RefactorHelper.Models.External;

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
            var diffs2 = _dmp.diff_main(fileTwo, fileOne);

            // Only show relevant differences
            _dmp.diff_cleanupSemantic(diffs1);
            _dmp.diff_cleanupSemantic(diffs2);

            return new CompareResultPair
            {
                Result1 = GetCompareResult(fileOne, diffs1, response1 ?? new()),
                Result2 = GetCompareResult(fileTwo, diffs2, response2 ?? new())
            };
        }

        private CompareResult GetCompareResult(string result, List<Diff> diffs, HttpResponseMessage? response)
        {
            return new CompareResult
            {
                Diffs = diffs,
                Result = result,
                Response = new HttpResponseMessage()
            };
        }
    }
}
