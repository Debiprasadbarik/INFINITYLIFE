using InfinityLife.Models;

namespace InfinityLife.DataAccess.Interfaces
{
    public interface IEmployeeRoleRepository
    {
        Task<IEnumerable<EmployeeRole>> GetAllRoles();
        Task<EmployeeRole> GetRoleById(int id);
    }
}