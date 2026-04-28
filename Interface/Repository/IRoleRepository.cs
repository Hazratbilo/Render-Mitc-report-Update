using MITCRMS.Models.Entities;
using System.Linq.Expressions;

namespace MITCRMS.Interface.Repository
{
    public interface IRoleRepository:IBaseRepository
    {
            Task<IEnumerable<Role>> GetRolesByIdsAsync(Expression<Func<Role, bool>> expression);
            Task<IEnumerable<Role>> GetRoles();
        }
}
