using RefactorHelper.Models;
using Newtonsoft.Json;
using RefactorHelper.Models.RequestHandler;
using RefactorHelper.Models.SwaggerProcessor;
using RefactorHelper.Models.Config;
using Newtonsoft.Json.Linq;

namespace RefactorHelper.RequestHandler
{
    public class RequestHandlerService
    {
        private HttpClient _client1 { get; }

        private HttpClient _client2 { get; }

        private RefactorHelperSettings _settings { get; }

        public RequestHandlerService(HttpClient client1, HttpClient client2, RefactorHelperSettings settings)
        {
            _client1 = client1;
            _client2 = client2;
            _settings = settings;
        }

        public async Task<RequestHandlerOutput> QueryApis(SwaggerProcessorOutput requests)
        {
            var tasks = requests.Requests.Select(x => GetResponses(x.Path)).ToList();

            await Task.WhenAll(tasks);

            return new RequestHandlerOutput
            {
                Results = [.. tasks.Select(x => x.Result).OrderBy(x => x.Path)]
            };
        }

        public async Task<RefactorTestResultPair> QueryEndpoint(RequestDetails request)
        {
            return await GetResponses(request.Path);
        }

        public async Task<RefactorTestResultPair> GetResponses(string path)
        {
            var request1 = _client1.GetAsync(path);
            var request2 = _client2.GetAsync(path);

            await Task.WhenAll(request1, request2);

            var response1 = await request1.Result.Content.ReadAsStringAsync();
            var response2 = await request2.Result.Content.ReadAsStringAsync();

            response1 = TryFormatResponse(response1);
            response2 = TryFormatResponse(response2);

            return new RefactorTestResultPair
            {
                Path = path,
                Result1 = GetRefactorTestResult(response1, request1.Result),
                Result2 = GetRefactorTestResult(response2, request2.Result),
            };
        }

        private RefactorTestResult GetRefactorTestResult(string result, HttpResponseMessage response)
        {
            return new RefactorTestResult
            {
                Response = result,
                ResponseObject = response
            };
        }

        public string TryFormatResponse(string response)
        {
            try
            {
                var responseObj1 = JsonConvert.DeserializeObject<object>(response);

                if (_settings.PropertiesToReplace.Count > 0)
                {
                    if(responseObj1 is JArray arr)
                    {
                        foreach (var item in arr)
                        {
                            foreach (JProperty attributeProperty in item.Cast<JProperty>())
                            {
                                var replaceProp = _settings.PropertiesToReplace.FirstOrDefault(x => 
                                    x.Key.Equals(attributeProperty.Name, StringComparison.OrdinalIgnoreCase));

                                if (replaceProp != null)
                                {
                                    var attribute = item[attributeProperty.Name];
                                    attributeProperty.Value = replaceProp.Value;
                                }
                            }
                        }
                    }
                }

                response = JsonConvert.SerializeObject(responseObj1, Formatting.Indented);
            }
            catch
            {

            }
            return response;
        }
    }
}
