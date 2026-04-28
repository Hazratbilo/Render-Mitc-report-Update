
using MITCRMS.Models.DTOs.Department;
using MITCRMS.Models.DTOs.Users;
using MITCRMS.Models.Enum;


namespace MITCRMS.Models.DTOs.Report
{
    public class ReportDto
    {
        public Guid Id { get; set; }
        public DateTime DateCreated { get; set; }

        public Guid? UserId { get; set; }
#pragma warning disable CS8618 
        public UserDto User { get; set; }
#pragma warning restore CS8618 
#pragma warning disable CS8618
        public string FullName { get; set; }
#pragma warning restore CS8618
#pragma warning disable CS8618 
        public string DepartmentName { get; set; }
#pragma warning restore CS8618

#pragma warning disable CS8618
        public string Tittle { get; set; }
#pragma warning restore CS8618
#pragma warning disable CS8618 
        public string Content { get; set; }
#pragma warning restore CS8618
        public string? FileUrl { get; set; }
        public ReportStatus Status { get; set; }
        public DateTime? ReportDate { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByAdminId { get; set; }

    }

}
