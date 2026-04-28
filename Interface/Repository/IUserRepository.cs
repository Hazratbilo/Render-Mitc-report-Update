using MITCRMS.Models.Entities;
using System.Linq.Expressions;

namespace MITCRMS.Interface.Repository 
{
    public interface IUserRepository : IBaseRepository
    {
        public Task<User> GetUserByIdAsync(Guid UserId);
        public Task<IReadOnlyList<User>> GetByRole(Expression<Func<User, bool>> expression);
        public Task<User> GetUserAndRoles(Guid UserId);
        Task<User> GetUserByEmail(string email);
        Task<List<User>> GetAllUsersWithRolesAsync();
        Task<User> GetUserProfile(Guid UserId);
        Task<bool> Any(Expression<Func<User, bool>> expression);
        public Task<bool> DeleteUser(User user);
        public Task<User> GetUserById(Guid id);

    }
}
