namespace EmployeeManager.Data;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? ManagerId { get; set; }
    public bool Enabled { get; set; }
    public List<Employee>? Employees { get; set; } = new List<Employee>();
}
