using Microsoft.EntityFrameworkCore;
using MITCRMS.Contract.Entity;
using MITCRMS.Interface.Repository;
using MITCRMS.Persistence.Context;
using System.Linq.Expressions;

namespace MITCRMS.Implementation.Repository
{
    public class BaseRepository : IBaseRepository
    {
        protected readonly MitcrmsContext _mitcrmsContext;
        public BaseRepository(MitcrmsContext mitcrmsContext)
        {
            _mitcrmsContext = mitcrmsContext ?? throw new ArgumentNullException(nameof(mitcrmsContext));
        }
        public virtual async Task<T> Add<T>(T entity) where T : BaseEntity
        {
            var entry = await _mitcrmsContext.Set<T>().AddAsync(entity);
            return entry.Entity;
        }


        public virtual void Delete<T>(T entity) where T : BaseEntity
        {
            throw new NotImplementedException();
        }

        public virtual async Task<T> Get<T>(Expression<Func<T, bool>> expression) where T : BaseEntity
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _mitcrmsContext.Set<T>().FirstOrDefaultAsync(expression);
#pragma warning restore CS8603 // Possible null reference return.
        }

        public virtual async Task<IReadOnlyList<T>> GetAll<T>() where T : BaseEntity
        {
            return await _mitcrmsContext.Set<T>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<T>> GetAll<T>(Expression<Func<T, bool>> expression) where T : BaseEntity
        {
            return await _mitcrmsContext.Set<T>()
                .Where(expression)
                .AsNoTracking()
                .ToListAsync();
        }

        public IQueryable<T> QueryWhere<T>(Expression<Func<T, bool>> expression) where T : BaseEntity
        {
            return _mitcrmsContext.Set<T>().Where(expression);
        }

        public void Update<T>(T entity) where T : BaseEntity => _mitcrmsContext.Set<T>().Update(entity);

    }
}