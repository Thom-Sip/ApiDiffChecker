using Microsoft.AspNetCore.Mvc;
using RefactorHelper.App;
using System.Diagnostics;

namespace TestApi.Controllers
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
        public async Task<IEnumerable<string>> Get()
        {
            var outputfilenames = await App.Run();

            // open result in browser
            foreach (var path in outputfilenames)
            {
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo(path)
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
