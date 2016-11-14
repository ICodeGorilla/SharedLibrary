using System.Security.Principal;
using Shared.Controller;
using Shared.Logging.Contract;
using Shared.Repository;

namespace Tests.Database
{
    /// <summary>
    /// Basic contacts controller 
    /// </summary>
    public class CompositeController : ODataCompositeControllerBase<CompositeKeyTest, SharedCommonDatabaseContext, int, string>
    {
        /// <summary>
        /// Contacts controller constructor
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="logger"></param>
        public CompositeController(IPrincipal principal, IGenericLogger logger) : base(
            logger,
            new DatabaseFactoryBase<SharedCommonDatabaseContext>("SharedCommonDatabaseContext"), 
            principal, 
            primaryKeyPredicate: (firstKey, secondKey) => x => x.FirstKey == firstKey && x.SecondKey == secondKey, 
            entityFirstPrimaryKeyFunc: (entity) => entity.FirstKey, 
            secondEntityPrimaryKeyFunc: (entity) => entity.SecondKey, 
            lastModifiedByColumnName: "LastModifiedBy", 
            lastModifiedColumnName: "LastModified")
        {
        }
    }
}