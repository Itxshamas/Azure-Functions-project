using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace QueueTriggerDemo
{
    public class MyTimerTrigger
    {
        private readonly ILogger<MyTimerTrigger> _logger;

        public MyTimerTrigger(ILogger<MyTimerTrigger> logger)
        {
            _logger = logger;
        }

        [Function("MyTimerTrigger")]
        public void Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"[Timer] Trigger executed at: {System.DateTime.UtcNow}");

            var service = new EmployeeService(_logger);

            service.UpdateHRSalary();
        }
    }
}
