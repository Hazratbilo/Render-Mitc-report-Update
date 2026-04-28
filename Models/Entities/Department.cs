using MITCRMS.Contract.Entity;
using System.Collections;

namespace MITCRMS.Models.Entities
{
    public class Department : BaseEntity
    {
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }

        public ICollection<User> Users {  get; set; }

    }
}
