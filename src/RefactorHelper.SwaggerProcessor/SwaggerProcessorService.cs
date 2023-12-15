using Newtonsoft.Json;
using RefactorHelper.Models;
using Swashbuckle.Swagger;

namespace RefactorHelper.SwaggerProcessor
{
    public class SwaggerProcessorService
    {
        public List<RequestDetails> ProcessSwagger(string swaggerJson)
        {
            var doc = JsonConvert.DeserializeObject<SwaggerDocument>(swaggerJson);
            return doc.paths.Select(p => GetRequestDetails(p)).ToList();
        }

        private RequestDetails GetRequestDetails(KeyValuePair<string, PathItem> path)
        {
            if(path.Value.get != null)
            {
                return new RequestDetails 
                { 
                    Path = path.Key,
                    Operation = path.Value.get,
                };
            }

            throw new NotImplementedException();
        }
    }
}
