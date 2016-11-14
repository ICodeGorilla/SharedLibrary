using System.Security.Principal;
using Shared.Controller;
using Shared.Logging.Contract;
using Shared.Repository;

namespace Tests.Database
{
    /// <summary>
    /// Basic contacts controller 
    /// </summary>
    public class AccountsController : ODataControllerBase<Account,SharedCommonDatabaseContext, int>
    {
        /// <summary>
        /// Contacts controller constructor
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="logger"></param>
        public AccountsController(IPrincipal principal, IGenericLogger logger) : base(
            logger,
            new DatabaseFactoryBase<SharedCommonDatabaseContext>("SharedCommonDatabaseContext"), 
            principal, 
            primaryKeyPredicate: (key) => x => x.AccountID == key, 
            entityPrimaryKeyFunc: (entity) => entity.AccountID, 
            lastModifiedByColumnName: "LastModifiedBy", 
            lastModifiedColumnName: "LastModified")
        {
        }
    }
}