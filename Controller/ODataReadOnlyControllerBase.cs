using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Web.OData;
using Shared.Logging.Contract;
using System.Web.Http;
using Shared.Controller.Contract;
using Shared.Repository.Contract;

namespace Shared.Controller
{
    /// <summary>
    ///     Base controller for all OData based methods
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDatabaseContex"></typeparam>
    public class ODataReadOnlyControllerBase<TEntity, TKey, TDatabaseContex> : ODataController, IODataReadOnlyControllerBase<TEntity,TKey> where TEntity : class where TDatabaseContex : DbContext
    {
        private readonly DbContext _context;
        private readonly IGenericLogger _logger;
        private readonly IPrincipal _principal;
        private readonly Func<TKey, Expression<Func<TEntity, bool>>> _primaryKeyPredicate;

        /// <summary>
        ///     Standard Controller Constructor, no auditing
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="principal"></param>
        /// <param name="factory"></param>
        /// <param name="primaryKeyPredicate"></param>
        public ODataReadOnlyControllerBase(IGenericLogger logger, IPrincipal principal, IDatabaseFactory<TDatabaseContex> factory,
            Func<TKey, Expression<Func<TEntity, bool>>> primaryKeyPredicate)
        {
            _logger = logger;
            _principal = principal;
            _primaryKeyPredicate = primaryKeyPredicate;
            _context = factory.Get();
            _context.Database.Log = logger.Debug;
        }

        /// <summary>
        /// Parameterless constructor for testing
        /// </summary>
        public ODataReadOnlyControllerBase(IGenericLogger logger, IPrincipal principal, DbContext context, Func<TKey, Expression<Func<TEntity, bool>>> primaryKeyPredicate)
        {
            _logger = logger;
            _principal = principal;
            _primaryKeyPredicate = primaryKeyPredicate;
            _context = context;
            _context.Database.Log = logger.Debug;
        }

        /// <summary>
        ///     Get all the items
        /// </summary>
        /// <returns></returns>
        [EnableQuery]
        public virtual IQueryable<TEntity> Get()
        {
            _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Reading data on {typeof(TEntity).Name}");
            return _context.Set<TEntity>();
        }

        /// <summary>
        ///     Get a single item by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery]
        public SingleResult<TEntity> Get([FromODataUri] TKey key)
        {
            _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Retrieving single item of {typeof(TEntity).Name}, by key {key}");
            var expression = _primaryKeyPredicate(key);
            return SingleResult.Create(_context.Set<TEntity>().Where(expression));
        }
    }
}