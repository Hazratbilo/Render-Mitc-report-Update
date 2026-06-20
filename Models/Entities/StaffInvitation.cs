using MITCRMS.Contract.Entity;

namespace MITCRMS.Models.Entities
{
    public class StaffInvitation : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public Guid DepartmentId { get; set; }
        public Department Department { get; set; }
        public string RoleIds { get; set; }
        public string TokenHash { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}
