using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MITCRMS.Models.Entities;
using MITCRMS.Persistence.Context;

namespace MITCRMS.Identity
{

    public class RoleStore : IRoleStore<Role>, IQueryableRoleStore<Role>
    {
        private readonly MitcrmsContext _context;
        public RoleStore(MitcrmsContext context)
        {
            _context = context;
        }


        public IQueryable<Role> Roles => _context.Set<Role>();

        public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            await _context.AddAsync(role, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            _context.Entry(role).State = EntityState.Deleted;
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

#pragma warning disable CS8613
        public async Task<Role> FindByIdAsync(string roleId, CancellationToken cancellationToken)
#pragma warning restore CS8613 
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(roleId))
            {
                throw new ArgumentNullException(nameof(roleId));
            }
#pragma warning disable CS8603
            return await _context.Set<Role>().FindAsync(new object[] { Guid.Parse(roleId) }, cancellationToken);
#pragma warning restore CS8603 
        }

#pragma warning disable CS8613 
        public async Task<Role> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
#pragma warning restore CS8613 
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(normalizedRoleName))
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }
#pragma warning disable CS8603 
            return await _context.Set<Role>().FirstOrDefaultAsync(u => u.RoleName == normalizedRoleName, cancellationToken);
#pragma warning restore CS8603 
        }

#pragma warning disable CS8613
        public Task<string> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
#pragma warning restore CS8613
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            return Task.FromResult(role.RoleName.ToLower());
        }

        public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            return Task.FromResult(role.Id.ToString());
        }

#pragma warning disable CS8613 
        public Task<string> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
#pragma warning restore CS8613 
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            return Task.FromResult(role.RoleName.ToLower());
        }

#pragma warning disable CS8767 
        public Task SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
#pragma warning restore CS8767
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            role.RoleName = normalizedName.ToLower();
            return Task.CompletedTask;
        }

#pragma warning disable CS8767
        public Task SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
#pragma warning restore CS8767
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            role.RoleName = roleName.ToLower();
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            _context.Entry(role).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }
    }
}
