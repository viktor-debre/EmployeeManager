using EmployeeManager.Data;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace EmployeeManager.Controllers;

[ApiController]
[Route("[controller]")]
public class EmployeeController : ControllerBase
{
    private const string connectionString = "connection string";

    [HttpGet]
    public Employee GetEmployeeById([FromQuery] int employeeId)
    {
        Employee rootEmployee = null;
        var employeeDictionary = new Dictionary<int, Employee>();

        string query = @"
               WITH RECURSIVE EmployeeHierarchy AS (
                   SELECT Id, Name, ManagerId, Enabled
                   FROM Employee
                   WHERE Id = @EmployeeId
                   UNION ALL
                   SELECT e.Id, e.Name, e.ManagerId, e.Enabled
                   FROM Employee e
                   INNER JOIN EmployeeHierarchy eh ON e.ManagerId = eh.Id
               )
               SELECT Id, Name, ManagerId, Enabled
               FROM EmployeeHierarchy;";

        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@EmployeeId", employeeId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var employee = new Employee
            {
                Id = (int)reader["Id"],
                Name = (string)reader["Name"],
                ManagerId = reader["ManagerId"] as int?,
                Enabled = (bool)reader["Enabled"]
            };

            if (rootEmployee == null)
            {
                rootEmployee = employee;
            }

            employeeDictionary.Add(employee.Id, employee);
        }

        reader.Close();

        foreach (var employee in employeeDictionary.Values)
        {
            if (employee.ManagerId.HasValue && employeeDictionary.ContainsKey(employee.ManagerId.Value))
            {
                Employee manager = employeeDictionary[employee.ManagerId.Value];
                manager.Employees.Add(employee);
            }
        }

        return rootEmployee;
    }

    [HttpPut]
    public void EnableEmployee(
        [FromQuery] int employeeId,
        [FromQuery] bool enable)
    {
        string updateQuery =
            @"UPDATE Employee 
              SET Enabled = @Enable
              WHERE Id = @EmployeeId";

        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        using var command = new NpgsqlCommand(updateQuery, connection);
        command.Parameters.AddWithValue("@EmployeeId", employeeId);
        command.Parameters.AddWithValue("@Enable", enable);

        command.ExecuteNonQuery();
    }
}
