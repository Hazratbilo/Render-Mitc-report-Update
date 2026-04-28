using Microsoft.EntityFrameworkCore;
using MITCRMS.Interface.Repository;
using MITCRMS.Models.Entities;
using MITCRMS.Persistence.Context;
using System.Linq.Expressions;

namespace MITCRMS.Implementation.Repository
{
    public class RoleRepository : BaseRepository, IRoleRepository
    {
        public RoleRepository(MitcrmsContext mitcrmsContext) : base(mitcrmsContext)
        {

        }

        public async Task<IEnumerable<Role>> GetRoles()
        {
            return await _mitcrmsContext.Set<Role>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetRolesByIdsAsync(Expression<Func<Role, bool>> expression)
        {
            return await _mitcrmsContext.Set<Role>()
                .Where(expression)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}