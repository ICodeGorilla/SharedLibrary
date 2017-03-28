using Shared.Repository;
using Shared.Repository.Contract;

namespace Tests.Database.EFCore
{
    public class EfCoreAccountRepository : EfCoreRepositoryBase<Account, SharedLibraryContext, int>
    {
        public EfCoreAccountRepository(IEfCoreDatabaseFactory<SharedLibraryContext> databaseFactory)
            : base(databaseFactory, entity => entity.AccountId)
        {
        }
    }
}