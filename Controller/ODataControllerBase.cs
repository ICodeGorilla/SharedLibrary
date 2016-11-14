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
    /// <typeparam name="TKey"></typeparam>
    public class ODataControllerBase<TEntity,TDatabaseContex, TKey> : ODataController, IODataController<TEntity,TKey> where TEntity : class where TDatabaseContex : DbContext, new()
    {
        private readonly TDatabaseContex _context;
        private readonly Func<TEntity, TKey> _entityPrimaryKeyFunc;
        private readonly string _lastModifiedByColumnName;
        private readonly string _lastModifiedColumnName;
        private readonly Func<TKey, Expression<Func<TEntity, bool>>> _primaryKeyPredicate;
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
        /// <param name="entityPrimaryKeyFunc"></param>
        public ODataControllerBase(IGenericLogger logger, IDatabaseFactory<TDatabaseContex> factory, IPrincipal principal,
            Func<TKey, Expression<Func<TEntity, bool>>> primaryKeyPredicate, Func<TEntity, TKey> entityPrimaryKeyFunc)
        {
            _logger = logger;
            _principal = principal;
            _primaryKeyPredicate = primaryKeyPredicate;
            _entityPrimaryKeyFunc = entityPrimaryKeyFunc;
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
        /// <param name="entityPrimaryKeyFunc"></param>
        /// <param name="lastModifiedByColumnName"></param>
        /// <param name="lastModifiedColumnName"></param>
        public ODataControllerBase(IGenericLogger logger, IDatabaseFactory<TDatabaseContex> factory, IPrincipal principal,
            Func<TKey, Expression<Func<TEntity, bool>>> primaryKeyPredicate,
            Func<TEntity, TKey> entityPrimaryKeyFunc, string lastModifiedByColumnName, string lastModifiedColumnName)
        {
            _logger = logger;
            _principal = principal;
            _primaryKeyPredicate = primaryKeyPredicate;
            _entityPrimaryKeyFunc = entityPrimaryKeyFunc;
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
        public virtual IQueryable<TEntity> Get()
        {
            return _context.Set<TEntity>();
        }

        /// <summary>
        ///     Get a single item by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery]
        public virtual SingleResult<TEntity> Get([FromODataUri] TKey key)
        {
            _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Retrieving single item of {typeof(TEntity).Name}, by key {key}");
            var expression = _primaryKeyPredicate(key);
            return SingleResult.Create(_context.Set<TEntity>().Where(expression));
        }

        /// <summary>
        ///     Update the item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="patch"></param>
        /// <returns></returns>
        public virtual IHttpActionResult Put([FromODataUri] TKey key, Delta<TEntity> patch)
        {
            _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Starting patch for the {typeof(TEntity).Name}, with key {key} , and patch, {JsonConvert.SerializeObject(patch)}");
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Patch for the {typeof(TEntity).Name}, with key {key}, failed with a bad model, {JsonConvert.SerializeObject(ModelState)}");
                return BadRequest(ModelState);
            }

            var entity = _context.Set<TEntity>().Find(key);
            if (entity == null)
            {
                _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Patch for the {typeof(TEntity).Name}, with key {key}, failed with the item not being found.");
                return NotFound();
            }

            SaveAuditingInformation(entity);
            patch.Put(entity);

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntityExists(key))
                {
                    _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Patch for the {typeof(TEntity).Name}, with key, {key}, failed with the item not being found.");
                    return NotFound();
                }
                throw;
            }

            _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Patch for the {typeof(TEntity).Name}, with key, {key}, has succeeded.");
            return Updated(entity);
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
            var original = _context.Set<TEntity>().Find(_entityPrimaryKeyFunc(entity));

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
        /// </summary>
        /// <param name="key"></param>
        /// <param name="patch"></param>
        /// <returns></returns>
        [AcceptVerbs("PATCH", "MERGE")]
        public virtual IHttpActionResult Patch([FromODataUri] TKey key, Delta<TEntity> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Patch for the {typeof(TEntity).Name}, failed with a bad model, {JsonConvert.SerializeObject(ModelState)}.");
                return BadRequest(ModelState);
            }

            var entity = _context.Set<TEntity>().Find(key);
            if (entity == null)
            {
                _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Patch for the {typeof(TEntity).Name}, with key, {key}, failed with the item not being found.");
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
                if (!EntityExists(key))
                {
                    _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Patch for the {typeof(TEntity).Name}, with key, {key}, failed with the item not being found.");
                    return NotFound();
                }
                throw;
            }

            _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Successfully patched entity, {JsonConvert.SerializeObject(entity)}.");
            return Updated(entity);
        }

        /// <summary>
        ///     Delete an item by specifying its key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual IHttpActionResult Delete([FromODataUri] TKey key)
        {
            var entity = _context.Set<TEntity>().Find(key);
            if (entity == null)
            {
                _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Attempting to delete the entity, {typeof(TEntity).Name}, with key {key} failed with the item not being found.");
                return NotFound();
            }

            _context.Set<TEntity>().Remove(entity);
            _context.SaveChanges();

            _logger.Info($"Autocar.API.Framework {_principal.Identity.Name} Delete for the, {typeof(TEntity).Name}, with key {key} succeeded.");
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

        private bool EntityExists(TKey key)
        {
            var expression = _primaryKeyPredicate(key);
            return _context.Set<TEntity>().Count(expression) > 0;
        }
    }
}