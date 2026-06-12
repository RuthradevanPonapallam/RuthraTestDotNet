using MySqlConnector;
using RuthraTestDotnet.Models;

namespace RuthraTestDotnet.Data;

public sealed class EmployeeRepository
{
    private readonly string _connectionString;

    public EmployeeRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("EmployeeDb")
            ?? throw new InvalidOperationException("Connection string 'EmployeeDb' was not found.");

        EnsureDatabase();
    }

    public IReadOnlyList<EmployeeBiodata> GetFilteredEmployees()
    {
        var employees = new List<EmployeeBiodata>();

        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT employee_no, employee_name, birth_date
            FROM employee_biodata
            WHERE (UPPER(employee_name) LIKE 'A%' OR UPPER(employee_name) LIKE 'G%' OR UPPER(employee_name) LIKE 'V%')
              AND MONTH(birth_date) BETWEEN 1 AND 3
            ORDER BY employee_no;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            employees.Add(new EmployeeBiodata
            {
                EmployeeNo = reader.GetString(0),
                EmployeeName = reader.GetString(1),
                BirthDate = reader.GetDateTime(2)
            });
        }

        return employees;
    }

    private void EnsureDatabase()
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Open();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS employee_biodata (
                    employee_no VARCHAR(10) NOT NULL PRIMARY KEY,
                    employee_name VARCHAR(60) NOT NULL,
                    birth_date DATETIME NOT NULL
                );";
            command.ExecuteNonQuery();
        }

        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(1) FROM employee_biodata;";
        var rowCount = Convert.ToInt32(countCommand.ExecuteScalar());

        if (rowCount > 0)
        {
            return;
        }

        var seedEmployees = new[]
        {
            new EmployeeBiodata { EmployeeNo = "00001", EmployeeName = "Alex Song", BirthDate = new DateTime(1979, 12, 31) },
            new EmployeeBiodata { EmployeeNo = "00002", EmployeeName = "Johnson Ong", BirthDate = new DateTime(1985, 1, 27) },
            new EmployeeBiodata { EmployeeNo = "00003", EmployeeName = "Henry Lim", BirthDate = new DateTime(1985, 12, 26) },
            new EmployeeBiodata { EmployeeNo = "00004", EmployeeName = "Anders Ngo", BirthDate = new DateTime(1986, 2, 5) },
            new EmployeeBiodata { EmployeeNo = "00005", EmployeeName = "Summer Leow", BirthDate = new DateTime(1980, 8, 12) }
        };

        using var transaction = connection.BeginTransaction();
        using var insertCommand = connection.CreateCommand();
        insertCommand.Transaction = transaction;
        insertCommand.CommandText = "INSERT INTO employee_biodata (employee_no, employee_name, birth_date) VALUES (@employee_no, @employee_name, @birth_date);";

        var employeeNoParameter = insertCommand.CreateParameter();
        employeeNoParameter.ParameterName = "@employee_no";
        insertCommand.Parameters.Add(employeeNoParameter);

        var employeeNameParameter = insertCommand.CreateParameter();
        employeeNameParameter.ParameterName = "@employee_name";
        insertCommand.Parameters.Add(employeeNameParameter);

        var birthDateParameter = insertCommand.CreateParameter();
        birthDateParameter.ParameterName = "@birth_date";
        insertCommand.Parameters.Add(birthDateParameter);

        foreach (var employee in seedEmployees)
        {
            employeeNoParameter.Value = employee.EmployeeNo;
            employeeNameParameter.Value = employee.EmployeeName;
            birthDateParameter.Value = employee.BirthDate;
            insertCommand.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}