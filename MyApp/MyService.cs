using Microsoft.Extensions.Logging;

namespace MyApp
{
    public interface IMyService
    {
        void Bar();
    }

    public class MyService : IMyService
    {
        private readonly ILogger<MyService> _logger;

        public MyService(ILogger<MyService> logger)
        {
            _logger = logger;
        }

        public void Bar()
        {
            _logger.LogInformation("bar");
        }
    }
}
