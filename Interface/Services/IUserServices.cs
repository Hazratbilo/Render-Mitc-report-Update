using MITCRMS.Models.DTOs;
using MITCRMS.Models.DTOs.Report;
using MITCRMS.Models.DTOs.Users;

namespace MITCRMS.Interface.Services
{
    public interface IUserServices
    {
        Task<BaseResponse<bool>> CreateUserAsync(CreateUserRequestModel request);
   
        public Task<BaseResponse<LoginResponseModel>> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken);
        public Task<BaseResponse<UserDto>> GetUserProfileByUserId(Guid userId, CancellationToken cancellationToken);
        Task<List<UserDto>> GetAllUsersWithRolesAsync();
        public Task<BaseResponse<bool>> DeleteUser(Guid id);
        public Task<BaseResponse<UserDto>> GetUserById(Guid id);
    }
}
