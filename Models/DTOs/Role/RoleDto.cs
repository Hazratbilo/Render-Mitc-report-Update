using MITCRMS.Models.Entities;

namespace MITCRMS.Models.DTOs.Role
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public DateTime DateCreated { get; set; }
#pragma warning disable CS8618
        public string Name { get; set; }
#pragma warning restore CS8618 
#pragma warning disable CS8618 
        public string Description { get; set; }
#pragma warning restore CS8618 
        public ICollection<UserRole> UserRoles { get; set; } = [];
    }
}
