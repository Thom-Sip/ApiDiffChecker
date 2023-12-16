using Microsoft.AspNetCore.Mvc;
using RefactorHelper.App;
using System.Diagnostics;

namespace Basic_Setup_Demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AAARefactorController : ControllerBase
    {
        private ILogger<AAARefactorController> _logger { get; }

        private RefactorHelperApp App { get; }

        public AAARefactorController(ILogger<AAARefactorController> logger, RefactorHelperApp app)
        {
            _logger = logger;
            App = app;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IEnumerable<string>> Get()
        {
            var outputfilenames = await App.Run();

            if (outputfilenames.Any())
            {
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo(outputfilenames.First())
                    {
                        UseShellExecute = true
                    }
                };
                p.Start();
            }

            return new List<string> { "Thank you", "for using", "RefactorHelper" };
        }
    }
}
