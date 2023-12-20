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

        public ComparerOutput CompareResponses(RequestHandlerOutput responseData)
        {
            var result = new ComparerOutput();

            foreach(var testresultPair in responseData.Results)
            {
                result.Results.Add(CompareResponse(testresultPair));
            }

            return result;
        }

        public CompareResultPair CompareResponse(RefactorTestResultPair testresultPair)
        {
            // Get diffs
            var diffs1 = _dmp.diff_main(testresultPair.Result1.Response, testresultPair.Result2.Response);
            var diffs2 = _dmp.diff_main(testresultPair.Result2.Response, testresultPair.Result1.Response);

            // Only show relevant differences
            _dmp.diff_cleanupSemantic(diffs1);
            _dmp.diff_cleanupSemantic(diffs2);

            return new CompareResultPair
            {
                Id = testresultPair.Id,
                Changed = testresultPair.Result1.Response != testresultPair.Result2.Response,
                Path = testresultPair.Path,
                FilePath = $"{MakePathSafe(testresultPair.Path)}.html",
                Result1 = GetCompareResult(testresultPair.Result1, diffs1),
                Result2 = GetCompareResult(testresultPair.Result1, diffs2)
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
