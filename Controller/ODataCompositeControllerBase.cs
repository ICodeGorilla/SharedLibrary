using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Principal;
using System.Web.Http;
using System.Web.OData;
using Shared.Logging.Contract;
using Newtonsoft.Json;
using Shared.Controller.Contract;
using Shared.Repository.Contract;

namespace Shared.Controller
{
    /// <summary>
    ///     Base controller for all OData based methods
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDatabaseContex"></typeparam>
    /// <typeparam name="TFirstKey"></typeparam>
    /// <typeparam name="TSecondKey"></typeparam>
    public class ODataCompositeControllerBase<TEntity, TDatabaseContex, TFirstKey, TSecondKey> : ODataController, IODataController<TEntity, TFirstKey, TSecondKey> where TEntity : class where TDatabaseContex : DbContext, new()
    {
        private readonly TDatabaseContex _context;
        private readonly Func<TEntity, TFirstKey> _entityFirstPrimaryKeyFunc;
        private readonly Func<TEntity, TSecondKey> _secondEntityPrimaryKeyFunc;
        private readonly string _lastModifiedByColumnName;
        private readonly string _lastModifiedColumnName;
        private readonly Func<TFirstKey, TSecondKey, Expression<Func<TEntity, bool>>> _primaryKeyPredicate;
        private readonly IGenericLogger _logger;
        private readonly IPrincipal _principal;
        private readonly bool _saveAuditingInformation;

        /// <summary>
        ///     Standard Controller Constructor, no auditing
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="factory"></param>
        /// <param name="principal"></param>
        /// <param name="primaryKeyPredicate"></param>
        /// <param name="entityFirstPrimaryKeyFunc"></param>
        /// <param name="secondEntityPrimaryKeyFunc"></param>
        public ODataCompositeControllerBase(IGenericLogger logger, IDatabaseFactory<TDatabaseContex> factory, IPrincipal principal,
            Func<TFirstKey, TSecondKey, Expression<Func<TEntity, bool>>> primaryKeyPredicate, Func<TEntity, TFirstKey> entityFirstPrimaryKeyFunc, Func<TEntity, TSecondKey> secondEntityPrimaryKeyFunc)
        {
            _logger = logger;
            _principal = principal;
            _primaryKeyPredicate = primaryKeyPredicate;
            _entityFirstPrimaryKeyFunc = entityFirstPrimaryKeyFunc;
            _secondEntityPrimaryKeyFunc = secondEntityPrimaryKeyFunc;
            _context = factory.Get();
            _context.Database.Log = logger.Debug;
            _saveAuditingInformation = false;
        }

        /// <summary>
        ///     Standard Controller Constructor, with auditing
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="factory"></param>
        /// <param name="principal"></param>
        /// <param name="primaryKeyPredicate"></param>
        /// <param name="entityFirstPrimaryKeyFunc"></param>
        /// <param name="secondEntityPrimaryKeyFunc"></param>
        /// <param name="lastModifiedByColumnName"></param>
        /// <param name="lastModifiedColumnName"></param>
        public ODataCompositeControllerBase(IGenericLogger logger, IDatabaseFactory<TDatabaseContex> factory, IPrincipal principal,
            Func<TFirstKey, TSecondKey, Expression<Func<TEntity, bool>>> primaryKeyPredicate,
            Func<TEntity, TFirstKey> entityFirstPrimaryKeyFunc, Func<TEntity, TSecondKey> secondEntityPrimaryKeyFunc, string lastModifiedByColumnName, string lastModifiedColumnName)
        {
            _logger = logger;
            _principal = principal;
            _primaryKeyPredicate = primaryKeyPredicate;
            _entityFirstPrimaryKeyFunc = entityFirstPrimaryKeyFunc;
            _secondEntityPrimaryKeyFunc = secondEntityPrimaryKeyFunc;
            _lastModifiedByColumnName = lastModifiedByColumnName;
            _lastModifiedColumnName = lastModifiedColumnName;
            _saveAuditingInformation = true;
            _context = factory.Get();
            _context.Database.Log = logger.Debug;
        }

        /// <summary>
        ///     Get all the items
        /// </summary>
        /// <returns></returns>
        [EnableQuery]
        public IQueryable<TEntity> Get()
        {
            return _context.Set<TEntity>();
        }

        /// <summary>
        ///     Get a single item by key
        /// </summary>
        /// <param name="firstKey"></param>
        /// <param name="secondKey"></param>
        /// <returns></returns>
        [EnableQuery]
        public SingleResult<TEntity> Get([FromODataUri]TFirstKey firstKey, [FromODataUri]TSecondKey secondKey)
        {
            _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Retrieving single item of {typeof(TEntity).Name}, by firstKey {firstKey}{secondKey}");
            var expression = _primaryKeyPredicate(firstKey, secondKey);
            return SingleResult.Create(_context.Set<TEntity>().Where(expression));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstKey"></param>
        /// <param name="secondKey"></param>
        /// <param name="patch"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IHttpActionResult Put([FromODataUri]TFirstKey firstKey, [FromODataUri] TSecondKey secondKey, Delta<TEntity> patch)
        {
            throw new NotImplementedException();
        }



        private void SaveAuditingInformation(TEntity entity)
        {
            if (_saveAuditingInformation)
            {
                _context.Entry(entity).Property(_lastModifiedByColumnName).CurrentValue =
                    _principal.Identity.Name;
                _context.Entry(entity).Property(_lastModifiedColumnName).CurrentValue = DateTime.Now;
            }
        }

        /// <summary>
        ///     Create a new item
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual IHttpActionResult Post(TEntity entity)
        {
            _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Adding a new item of {typeof(TEntity).Name}, {JsonConvert.SerializeObject(entity)} ");
            if (!ModelState.IsValid)
            {
                _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Patch for the {typeof(TEntity).Name}, failed with a bad model, {JsonConvert.SerializeObject(ModelState)}");
                return BadRequest(ModelState);
            }

            SaveAuditingInformation(entity);
            var original = _context.Set<TEntity>().Find(_entityFirstPrimaryKeyFunc(entity), _secondEntityPrimaryKeyFunc(entity));

            if (original != null)
            {
                _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Original item found, {JsonConvert.SerializeObject(original)}, updating.");
                _context.Entry(original).CurrentValues.SetValues(entity);
            }
            else
            {
                _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Adding new item, {JsonConvert.SerializeObject(entity)}.");
                _context.Set<TEntity>().Add(entity);
            }

            _context.SaveChanges();

            _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Successfully added new entity, {JsonConvert.SerializeObject(entity)}.");
            return Created(entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstKey"></param>
        /// <param name="secondKey"></param>
        /// <param name="patch"></param>
        /// <returns></returns>
        public IHttpActionResult Patch([FromODataUri]TFirstKey firstKey, [FromODataUri]TSecondKey secondKey, Delta<TEntity> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Patch for the {typeof(TEntity).Name}, failed with a bad model, {JsonConvert.SerializeObject(ModelState)}.");
                return BadRequest(ModelState);
            }

            var entity = _context.Set<TEntity>().Find(firstKey, secondKey);
            if (entity == null)
            {
                _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Patch for the {typeof(TEntity).Name}, with key, {firstKey}{secondKey}, failed with the item not being found.");
                return NotFound();
            }

            patch.Patch(entity);
            SaveAuditingInformation(entity);

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntityExists(firstKey, secondKey))
                {
                    _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Patch for the {typeof(TEntity).Name}, with key, {firstKey}{secondKey}, failed with the item not being found.");
                    return NotFound();
                }
                throw;
            }

            _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Successfully patched entity, {JsonConvert.SerializeObject(entity)}.");
            return Updated(entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstKey"></param>
        /// <param name="secondKey"></param>
        /// <returns></returns>
        public IHttpActionResult Delete([FromODataUri]TFirstKey firstKey, [FromODataUri]TSecondKey secondKey)
        {
            var entity = _context.Set<TEntity>().Find(firstKey, secondKey);
            if (entity == null)
            {
                _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Attempting to delete the entity, {typeof(TEntity).Name}, with key {firstKey},{secondKey} failed with the item not being found.");
                return NotFound();
            }

            _context.Set<TEntity>().Remove(entity);
            _context.SaveChanges();

            _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Delete for the, {typeof(TEntity).Name}, with key {firstKey},{secondKey} succeeded.");
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool EntityExists(TFirstKey firstKey, TSecondKey secondKey)
        {
            var expression = _primaryKeyPredicate(firstKey, secondKey);
            return _context.Set<TEntity>().Count(expression) > 0;
        }
    }
}