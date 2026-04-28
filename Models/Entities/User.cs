using Microsoft.AspNetCore.Mvc.Rendering;
using MITCRMS.Contract.Entity;
using MITCRMS.Models.DTOs.Users;
using MITCRMS.Models.Enum;
using System.Numerics;

namespace MITCRMS.Models.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName()
        {
            return $"{FirstName} {LastName}";
        }
        public Department Department { get; set; }
        public Guid DepartmentId { get; set; }

        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = [];

        public string ChangePassword(string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new ArgumentException("Password cannot be empty", nameof(newPassword));
            }
            PasswordHash = newPassword;
            return PasswordHash;
        }
    }
}
