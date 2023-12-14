using Newtonsoft.Json;
using RefactorHelper.Models;

namespace RefactorHelper.SwaggerProcessor
{
    public class SwaggerProcessorService
    {
        public List<RequestDetails> ProcessSwagger(string swaggerJson)
        {
            var doc = JsonConvert.DeserializeObject<Swashbuckle.Swagger.SwaggerDocument>(swaggerJson);
            return doc.paths.Select(p => new RequestDetails { Path = p.Key?.ToString() }).ToList();
        }
    }
}
