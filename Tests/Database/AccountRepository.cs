using Shared.Repository;
using Shared.Repository.Contract;

namespace Tests.Database
    {
        public class AccountRepository : RepositoryBase<Account,SharedCommonDatabaseContext,int>
        {
            public AccountRepository(IDatabaseFactory<SharedCommonDatabaseContext> databaseFactory)
                : base(databaseFactory, entity => entity.AccountID)
            {
            }
        }
    }


