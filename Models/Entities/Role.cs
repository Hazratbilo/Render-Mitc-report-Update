using MITCRMS.Contract.Entity;

namespace MITCRMS.Models.Entities
{
    public class Role:BaseEntity
    {
        public string RoleName { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = [];

    }
}