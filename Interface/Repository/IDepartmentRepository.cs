using MITCRMS.Models.Entities;

namespace MITCRMS.Interface.Repository
{
    public interface IDepartmentRepository:IBaseRepository
    {
        public Task<bool> DeleteDepartment(Department department);
        public Task<Department> GetDepartmentById(Guid id);
        Task<List<Department>> GetAllDepartments();
        Task<bool> ExistsByName(string departmentName);
        Task<List<Department>> GetAllDepartmentsAndReports();
        Task<Department> UpdateDepartment(Department department);
    }
}
