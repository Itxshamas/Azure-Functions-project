using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace QueueTriggerDemo
{
    public class MyTimerTrigger
    {
        private readonly ILogger<MyTimerTrigger> _logger;
        private readonly string _connectionString;

        public MyTimerTrigger(ILogger<MyTimerTrigger> logger)
        {
            _logger = logger;
            _connectionString = Environment.GetEnvironmentVariable("SqlConnectionString") 
                                ?? throw new InvalidOperationException("SQL connection string is missing in environment variables.");
        }

        [Function("MyTimerTrigger")]
        public void Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"[Timer] Function executed at: {DateTime.UtcNow}");

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                var query = @"UPDATE Employees 
                              SET Sallary = Sallary + (Sallary * 0.1) 
                              WHERE Department = 'HR'";

                using var command = new SqlCommand(query, connection);
                int rowsAffected = command.ExecuteNonQuery();

                _logger.LogInformation($"{rowsAffected} employees updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating employees: {ex.Message}");
            }
        }
    }
}
