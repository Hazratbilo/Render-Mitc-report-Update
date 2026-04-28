using Microsoft.EntityFrameworkCore.Storage;
using MITCRMS.Interface.Repository;
using MITCRMS.Persistence.Context;

namespace MITCRMS.Implementation.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MitcrmsContext _mitcrmsContext;

        public UnitOfWork(MitcrmsContext mitcrmsContext)
        {
            _mitcrmsContext = mitcrmsContext ?? throw new ArgumentNullException(nameof(mitcrmsContext));
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _mitcrmsContext.Database.BeginTransactionAsync();
        }

        public IExecutionStrategy CreateExecutionStrategy()
        {
            return _mitcrmsContext.Database.CreateExecutionStrategy();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await _mitcrmsContext.SaveChangesAsync(cancellationToken);
        }
    }
}
