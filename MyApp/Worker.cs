using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Context;

namespace MyApp
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMyService _myService;

        public Worker(ILogger<Worker> logger, IMyService myService)
        {
            _logger = logger;
            _myService = myService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (LogContext.PushProperty("foobar", 1))
            {
                Foo();
                await Task.CompletedTask;
            }
        }

        private void Foo()
        {
            _logger.LogInformation("foo");
            _myService.Bar();
        }
    }
}
