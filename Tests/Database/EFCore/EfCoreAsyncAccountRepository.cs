using Shared.Repository;
using Shared.Repository.Contract;

namespace Tests.Database.EFCore
{
    public class EfCoreAsyncAccountRepository : EfCoreAsyncRepositoryBase<Database.EFCore.Account, SharedLibraryContext, int>
    {
        public EfCoreAsyncAccountRepository(IEfCoreDatabaseFactory<SharedLibraryContext> databaseFactory)
            : base(databaseFactory, entity => entity.AccountId)
        {
        }
    }
}