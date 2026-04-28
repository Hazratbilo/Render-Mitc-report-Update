using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using MITCRMS.Models.Entities;

namespace MITCRMS.Models.DTOs.Users
{
    public class CreateUserRequestModel
    {
#pragma warning disable CS8618
        public string FirstName { get; set; }
#pragma warning restore CS8618
#pragma warning disable CS8618 
        public string LastName { get; set; }
#pragma warning restore CS8618
#pragma warning disable CS8618 
        public string Address { get; set; }
#pragma warning restore CS8618
#pragma warning disable CS8618
        public string Email { get; set; }
#pragma warning restore CS8618
#pragma warning disable CS8618
        public List<SelectListItem> Departments { get; set; }
#pragma warning restore CS8618 
        public string PasswordHash { get; set; }
        public string ConfirmPassword { get; set; }
        public string PhoneNumber { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = [];
      
        public Guid RoleId {  get; set; }
        public Guid DepartmentId { get; set; }
        public List<Guid> RoleIds { get; set; } = [];
    }
}
