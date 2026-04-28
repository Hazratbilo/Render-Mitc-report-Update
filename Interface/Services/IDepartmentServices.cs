using Microsoft.AspNetCore.Mvc.Rendering;
using MITCRMS.Models.DTOs;
using MITCRMS.Models.DTOs.Department;

namespace MITCRMS.Interface.Services
{
    public interface IDepartmentServices
    {
        Task<BaseResponse<bool>> AddDepartment(CreateDepartmentRequestModel request);
        Task<BaseResponse<bool>> DeleteDepartment(Guid id);
        Task<BaseResponse<DepartmentDto>> GetDepartmentById(Guid id);
        Task<BaseResponse<IEnumerable<DepartmentDto>>> GetAllDepartmentsAsync();

        Task<IEnumerable<SelectListItem>> GetDepartmentsSelectList();
    }
}
