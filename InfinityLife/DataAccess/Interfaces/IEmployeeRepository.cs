using InfinityLife.Models;

namespace InfinityLife.DataAccess.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllEmployees();
        Task<Employee> GetEmployeeById(string id);
        Task<Employee> GetEmployeeByEmail(string email);
        Task<Employee> SearchEmployee(string searchTerm);
        Task<Employee> CreateEmployee(Employee employee);
        Task<bool> UpdateEmployee(Employee employee);
        //Task<PaySlip> GeneratePaySlip(string employeeId, DateTime payPeriod);
        Task<bool> DeleteEmployee(string id);
        //Task<List<EmployeeRole>> GetAllRoles();
        Task UpdatePassword(string email, string newPassword);
        Task UpdateEmployeeProfilePic(Employee employee, byte[] profilePicture);
    }
}
