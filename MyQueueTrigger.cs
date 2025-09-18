using System;
using System.Text.Json;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace QueueTriggerDemo
{
    public class MyQueueTrigger
    {
        private readonly ILogger<MyQueueTrigger> _logger;
        private readonly string _connectionString;

        public MyQueueTrigger(ILogger<MyQueueTrigger> logger)
        {
            _logger = logger;
            _connectionString = Environment.GetEnvironmentVariable("SqlConnectionString") 
                                ?? throw new InvalidOperationException("SQL connection string is missing in environment variables.");
        }

        [Function(nameof(MyQueueTrigger))]
        public void Run([QueueTrigger("myqueue-items", Connection = "AzureWebJobsStorage")] string message)
        {
            _logger.LogInformation("Queue trigger processed message: {messageText}", message);

            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    _logger.LogWarning("Received empty message from queue.");
                    return;
                }

                var employee = JsonSerializer.Deserialize<Employee>(message);
                if (employee == null)
                {
                    _logger.LogWarning("Failed to deserialize queue message.");
                    return;
                }

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = "INSERT INTO Employees (FirstName, LastName, Department, Sallary) VALUES (@FirstName, @LastName, @Department, @Salary)";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", employee.FirstName ?? string.Empty);
                        command.Parameters.AddWithValue("@LastName", employee.LastName ?? string.Empty);
                        command.Parameters.AddWithValue("@Department", employee.Department ?? string.Empty);
                        command.Parameters.AddWithValue("@Salary", employee.Sallary);

                        command.ExecuteNonQuery();
                    }

                    _logger.LogInformation("New Employee inserted successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting employee: {ex.Message}");
            }
        }

        public class Employee
        {
            public required string FirstName { get; set; }
            public required string LastName { get; set; }
            public required string Department { get; set; }
            public int Sallary { get; set; }
        }
    }
}
