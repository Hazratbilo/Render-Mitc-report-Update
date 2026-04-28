
using Microsoft.AspNetCore.Mvc.Rendering;
using MITCRMS.Models.DTOs.Role;
using MITCRMS.Models.Entities;

namespace MITCRMS.Models.DTOs.Users
{
    public class UserDto
    {

        public Guid Id { get; set; }
        public string PhoneNumber { get; set; }

        public string Email { get; set; }
        public ICollection<RoleDto> Roles { get; set; } = [];
        public string FirstName { get; set; }
        public string FullName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public Guid SuperAdminId { get; set; }
        public List<SelectListItem> Departments { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}
