using MITCRMS.Models.DTOs.Users;

namespace MITCRMS.Models.DTOs.Department
{
    public class CreateDepartmentRequestModel
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
#pragma warning disable CS8618
        public string DepartmentName { get; set; }
#pragma warning restore CS8618 
#pragma warning disable CS8618
        public string DepartmentCode { get; set; }
#pragma warning restore CS8618
        public List<UserDto> Users { get; set; } = new();

    }
}
