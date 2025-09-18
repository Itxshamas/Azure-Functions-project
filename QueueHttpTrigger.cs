using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Queues;

namespace QueueTriggerDemo
{
    public class QueueHttpTrigger
    {
        private readonly ILogger<QueueHttpTrigger> _logger;
        private readonly string _queueConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        public QueueHttpTrigger(ILogger<QueueHttpTrigger> logger)
        {
            _logger = logger;
        }
[Function("QueueHttpTrigger")]
public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
{
    var response = req.CreateResponse();

    try
    {
        var content = await req.ReadAsStringAsync();
        _logger.LogInformation("HTTP request received: " + content);

        if (string.IsNullOrWhiteSpace(content))
        {
            response.StatusCode = System.Net.HttpStatusCode.BadRequest;
            await response.WriteStringAsync("Request body is empty.");
            return response;
        }

        var queueClient = new QueueClient(_queueConnectionString, "myqueue-items");
        await queueClient.CreateIfNotExistsAsync();
        await queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(content)));

        response.StatusCode = System.Net.HttpStatusCode.OK;
        await response.WriteStringAsync("Message sent to queue successfully!");
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error sending message to queue: {ex.Message}");
        response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
        await response.WriteStringAsync("Error sending message to queue: " + ex.Message);
    }

    return response;
}

    }
}
