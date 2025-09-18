using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace QueueTriggerDemo
{
    public class MyQueueTrigger
    {
        private readonly ILogger<MyQueueTrigger> _logger;

        public MyQueueTrigger(ILogger<MyQueueTrigger> logger)
        {
            _logger = logger;
        }

        [Function(nameof(MyQueueTrigger))]
        public void Run([QueueTrigger("myqueue-items", Connection = "AzureWebJobsStorage")] string message)
        {
            _logger.LogInformation("Queue trigger received message: {message}", message);

            if (string.IsNullOrWhiteSpace(message)) return;

            var employee = JsonSerializer.Deserialize<Employee>(message);
            if (employee == null)
            {
                _logger.LogWarning("Failed to deserialize message.");
                return;
            }

            var service = new EmployeeService(_logger);
            service.InsertEmployee(employee);
        }
    }
}
