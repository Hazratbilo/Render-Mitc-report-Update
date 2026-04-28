using MITCRMS.Models.DTOs;
using MITCRMS.Models.DTOs.Role;

namespace MITCRMS.Interface.Services
{
    public interface IRoleServices
    {
        Task<BaseResponse<IEnumerable<RoleDto>>> GetRolesAsync();
    }
}
