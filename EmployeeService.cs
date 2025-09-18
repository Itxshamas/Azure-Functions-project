using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace QueueTriggerDemo
{
    public class EmployeeService
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;

        public EmployeeService(ILogger logger)
        {
            _logger = logger;
            _connectionString = Environment.GetEnvironmentVariable("SqlConnectionString")
                                ?? throw new InvalidOperationException("SQL connection string missing.");
        }

        public void InsertEmployee(Employee employee)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                var query = "INSERT INTO Employees (FirstName, LastName, Department, Sallary) VALUES (@FirstName, @LastName, @Department, @Salary)";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FirstName", employee.FirstName ?? string.Empty);
                command.Parameters.AddWithValue("@LastName", employee.LastName ?? string.Empty);
                command.Parameters.AddWithValue("@Department", employee.Department ?? string.Empty);
                command.Parameters.AddWithValue("@Salary", employee.Sallary);
                command.ExecuteNonQuery();

                _logger.LogInformation($"Employee {employee.FirstName} {employee.LastName} inserted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting employee: {ex.Message}");
            }
        }

        public void UpdateHRSalary()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                var query = @"UPDATE Employees 
                              SET Sallary = Sallary + (Sallary * 0.1) 
                              WHERE Department = 'HR'";

                using var command = new SqlCommand(query, connection);
                int rowsAffected = command.ExecuteNonQuery();

                _logger.LogInformation($"{rowsAffected} HR employees updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating employees: {ex.Message}");
            }
        }
    }
}
