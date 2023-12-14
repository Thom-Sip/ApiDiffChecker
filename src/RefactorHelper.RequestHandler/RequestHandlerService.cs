using RefactorHelper.Models;
using Newtonsoft.Json;

namespace RefactorHelper.RequestHandler
{
    public class RequestHandlerService
    {
        private HttpClient _client1 { get; }

        private HttpClient _client2 { get; }

        public RequestHandlerService(HttpClient client)
        {
            _client1 = client;
            _client2 = client;
        }

        public RequestHandlerService(HttpClient client1, HttpClient client2)
        {
            _client1 = client1;
            _client2 = client2;
        }

        public async Task<List<RefactorTestResponse>> QueryApis(List<RequestDetails> requests)
        {
            var tasks = requests.Select(x => GetResponses(x.Path)).ToList();

            await Task.WhenAll(tasks);

            return tasks.Select(x => x.Result).ToList();
        }

        public async Task<RefactorTestResponse> GetResponses(string path)
        {
            var request1 = _client1.GetAsync(path);
            var request2 = _client2.GetAsync(path);

            await Task.WhenAll(request1, request2);

            var response1 = await request1.Result.Content.ReadAsStringAsync();
            var response2 = await request2.Result.Content.ReadAsStringAsync();

            var responseObj1 = JsonConvert.DeserializeObject<object>(response1);
            response1 = JsonConvert.SerializeObject(responseObj1, Formatting.Indented);

            var responseObj2 = JsonConvert.DeserializeObject<object>(response2);
            response2 = JsonConvert.SerializeObject(responseObj2, Formatting.Indented);

            return new RefactorTestResponse
            {
                Response1 = response1,
                ResultCode1 = request1.Result.StatusCode,
                Response2 = response2,
                ResultCode2 = request2.Result.StatusCode,
                Path = path
            };
        }
    }
}
