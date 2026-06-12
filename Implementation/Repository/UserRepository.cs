using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MITCRMS.Interface.Repository;
using MITCRMS.Models.Entities;
using MITCRMS.Persistence.Context;
using System.Linq.Expressions;

namespace MITCRMS.Implementation.Repository
{
    public class UserRepository : BaseRepository, IUserRepository

    {
        public UserRepository(MitcrmsContext mitcrmsContext) : base(mitcrmsContext)
        {
        }

        public async Task<bool> Any(Expression<Func<User, bool>> expression)
        {
            return await _mitcrmsContext.Set<User>()
             .AnyAsync(expression);
        }

        public async Task<IReadOnlyList<User>> GetByRole(Expression<Func<User, bool>> expression)
        {
            return await _mitcrmsContext.Set<User>()
                .Where(expression)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<User> GetUserAndRoles(Guid UserId)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _mitcrmsContext.Set<User>()
                .Where(u => u.Id == UserId)
                .Include(u => u.UserRoles)
                .ThenInclude(u => u.Role)
                .SingleOrDefaultAsync();
#pragma warning restore CS8603 // Possible null reference return.
        }

        public async Task<User> GetUserByEmail(string email)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _mitcrmsContext.Set<User>()
                .Include(u => u.Department)
                .Include(u => u.UserRoles)
                .ThenInclude(u => u.Role)
                .SingleOrDefaultAsync(u => u.Email == email);
#pragma warning restore CS8603 // Possible null reference return.

        }

        public async Task<User> GetUserByIdAsync(Guid UserId)

        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _mitcrmsContext.Set<User>()
                .Include(a => a.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == UserId);
#pragma warning restore CS8603 // Possible null reference return.
        }


        public async Task<int> GetAdminCounts()
        {
            return await _mitcrmsContext.Set<User>().CountAsync();
        }


        public async Task<User> GetUserProfile(Guid UserId)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _mitcrmsContext.Set<User>()
                  .Include(p => p.Department)
                  .Include(u => u.UserRoles)
                  .ThenInclude(ur => ur.Role)
                   .Where(u => u.Id == UserId)
                   .AsSplitQuery()
                  .AsNoTracking()
                  .SingleOrDefaultAsync();
#pragma warning restore CS8603 // Possible null reference return.
        }
    
   public async Task<List<User>> GetAllUsersWithRolesAsync()
        {
            return await _mitcrmsContext.Set<User>()
                .Include(u => u.Department)
                 .Include(u => u.UserRoles)
                 .ThenInclude(ur => ur.Role)
                 .Where(ur => ur.UserRoles.Any(ur => ur.Role.RoleName != "SuperAdmin"))
                 .AsNoTracking()
                 .ToListAsync();
        }
        public async Task<User> GetUserById(Guid id)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _mitcrmsContext.Set<User>()
                .Include(u => u.Department)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
#pragma warning restore CS8603 // Possible null reference return.

        }
        public async Task<bool> DeleteUser(User user)
        {
            _mitcrmsContext.Set<User>().Remove(user);
            return await _mitcrmsContext.SaveChangesAsync() > 0 ? true : false;

        }
    }
}
