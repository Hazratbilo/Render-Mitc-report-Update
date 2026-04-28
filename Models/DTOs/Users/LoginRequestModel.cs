using MITCRMS.Models.DTOs.Role;
using MITCRMS.Models.Entities;

namespace MITCRMS.Models.DTOs.Users
{
    public class LoginRequestModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponseModel : BaseResponse
    {
        public string FirstName { get; set; }
        public string FullName { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public IEnumerable<RoleDto> Roles { get; set; } = new List<RoleDto>();

        public Guid Id { get; set; }
        public Guid DepartmentId { get; set; }
        public string PhoneNumber { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }

        public User User { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}
