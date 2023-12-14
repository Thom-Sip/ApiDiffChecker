using Newtonsoft.Json;
using RefactorHelper.Models;
using Swashbuckle.Swagger;

namespace RefactorHelper.SwaggerProcessor
{
    public class SwaggerProcessorService
    {
        public List<RequestDetails> ProcessSwagger(string swaggerJson)
        {
            var doc = JsonConvert.DeserializeObject<Swashbuckle.Swagger.SwaggerDocument>(swaggerJson);
            return doc.paths.Select(p => GetRequestDetails(p)).ToList();
        }

        private RequestDetails GetRequestDetails(KeyValuePair<string, PathItem> path)
        {
            if(path.Value.get != null)
            {
                return new RequestDetails 
                { 
                    Path = path.Key,
                    UrlVariables = GetUrlVariables(path.Value.get)
                };
            }

            return new RequestDetails { Path = path.Key };
        }

        private List<string> GetUrlVariables(Operation operation)
        {
            return operation.parameters?.Select(x => x.name).ToList() ?? new List<string>();
        }
    }
}
