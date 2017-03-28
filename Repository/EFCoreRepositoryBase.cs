using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Shared.Repository.Contract;

namespace Shared.Repository
{
    public static class Extensions
    {
        public static TEntity Find<TEntity>(this DbSet<TEntity> set, params object[] keyValues) where TEntity : class
        {
            var context = set.GetService<DbContext>();

            var entityType = context.Model.FindEntityType(typeof(TEntity));
            var key = entityType.FindPrimaryKey();

            var entries = context.ChangeTracker.Entries<TEntity>();

            var i = 0;
            foreach (var property in key.Properties)
            {
                entries = Enumerable.Where(entries, e => e.Property(property.Name).CurrentValue == keyValues[i]);
                i++;
            }

            var entry = entries.FirstOrDefault();
            if (entry != null)
            {
                // Return the local object if it exists.
                return entry.Entity;
            }

            // TODO: Build the real LINQ Expression
            // set.Where(x => x.Id == keyValues[0]);
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var query = Queryable.Where(set, (Expression<Func<TEntity, bool>>)
                Expression.Lambda(
                    Expression.Equal(
                        Expression.Property(parameter, "Id"),
                        Expression.Constant(keyValues[0])),
                    parameter));

            // Look in the database
            return query.FirstOrDefault();
        }
    }

    public abstract class EfCoreRepositoryBase<TEntityType, TDatabseType, TPrimaryKeyType> : IRepository<TEntityType, TPrimaryKeyType> where TEntityType  : class 
                                                                                               where TDatabseType : DbContext
    {
        private readonly Func<TEntityType, TPrimaryKeyType> _entityPrimaryKeyFunc;
        //Removed IDbSet as it is not implementing all the members we need http://stackoverflow.com/questions/31539750/why-theres-no-addrange-removerange-method-in-idbset-interface-in-entity-6
        private readonly DbSet<TEntityType> _dbset;
        private DbContext _dataDbContext;

        protected EfCoreRepositoryBase(IEfCoreDatabaseFactory<TDatabseType> databaseFactory, Func<TEntityType, TPrimaryKeyType> entityPrimaryKeyFunc)
        {
            _entityPrimaryKeyFunc = entityPrimaryKeyFunc;
            DatabaseFactory = databaseFactory;
            _dbset = DataContext.Set<TEntityType>();
        }

        protected IEfCoreDatabaseFactory<TDatabseType> DatabaseFactory { get; }

        protected DbContext DataContext => _dataDbContext ?? (_dataDbContext = DatabaseFactory.Get());

        public virtual TEntityType Save(TEntityType entity)
        {
            var original = DataContext.Set<TEntityType>().Find(_entityPrimaryKeyFunc(entity));
            if (original != null)
            {
                DataContext.Entry(original).CurrentValues.SetValues(entity);
            }
            else
            {
                DataContext.Set<TEntityType>().Add(entity);
            }
            return entity;
        }

        //Will return ID from db
        public virtual TEntityType SaveAndCommit(TEntityType entity)
        {
            var original = DataContext.Set<TEntityType>().Find(_entityPrimaryKeyFunc(entity));
            if (original != null)
            {
                DataContext.Entry(original).CurrentValues.SetValues(entity);
                DataContext.SaveChanges();
            }
            else
            {
                DataContext.Set<TEntityType>().Add(entity);
                DataContext.SaveChanges();
                DataContext.Entry(entity).GetDatabaseValues();
            }
            return entity;
        }

#warning Will commit directly.  Bypasses unit of work.
        public virtual void AddRange(List<TEntityType> entities)
        {
            foreach (var entity in entities)
            {
                SaveAndCommit(entity);
            }
        }

        public virtual void Update(TEntityType entity)
        {
            Save(entity);
        }

        public virtual void Delete(TEntityType entity)
        {
            _dbset.Remove(entity);
        }

        public virtual void Delete(Expression<Func<TEntityType, bool>> where)
        {
            var objects = _dbset.Where(where).AsEnumerable();
            _dbset.RemoveRange(objects);
        }

        public virtual TEntityType GetById(TPrimaryKeyType id)
        {
            return _dbset.Find(id);
        }

        public virtual IEnumerable<TEntityType> GetAll()
        {
            return _dbset.ToList();
        }

        public virtual IEnumerable<TEntityType> GetMany(Expression<Func<TEntityType, bool>> where)
        {
            return _dbset.Where(where).ToList();
        }

        public virtual bool Any(Expression<Func<TEntityType, bool>> where)
        {
            return _dbset.Any(where);
        }

        public virtual bool None(Expression<Func<TEntityType, bool>> where)
        {
            return !Any(where);
        }

        public TEntityType Get(Expression<Func<TEntityType, bool>> where)
        {
            return _dbset.Where(where).FirstOrDefault();
        }

        public IEnumerable<TEntityType> ExecuteQuery(string query, params object[] parameters)
        {
            return _dbset.FromSql(query, parameters);
        }

        public IEnumerable<T> ExecuteQuery<T>(string query, params object[] parameters)
        {
            return (IEnumerable<T>) _dbset.FromSql(query, parameters);
        }

        public void ExecuteQueryAsAction(string query, params object[] parameters)
        {
            _dataDbContext.Database.ExecuteSqlCommand(query, parameters);
        }

        public virtual IEnumerable<TEntityType> GetAutoCompleteItems(Expression<Func<TEntityType, bool>> where, int numberOfReturnValues)
        {
            return _dbset.Where(where).Take(numberOfReturnValues).ToList();
        }
    }

    
}