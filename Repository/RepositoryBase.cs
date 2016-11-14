﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using EntityFramework.BulkInsert.Extensions;
using Shared.Repository.Contract;

namespace Shared.Repository
{
    public abstract class RepositoryBase<TEntityType, TDatabseType, TPrimaryKeyType> : IRepository<TEntityType, TPrimaryKeyType> where TEntityType  : class 
                                                                                               where TDatabseType : DbContext
    {
        private readonly Func<TEntityType, TPrimaryKeyType> _entityPrimaryKeyFunc;
        //Removed IDbSet as it is not implementing all the members we need http://stackoverflow.com/questions/31539750/why-theres-no-addrange-removerange-method-in-idbset-interface-in-entity-6
        private readonly DbSet<TEntityType> _dbset;
        private DbContext _dataDbContext;

        protected RepositoryBase(IDatabaseFactory<TDatabseType> databaseFactory, Func<TEntityType, TPrimaryKeyType> entityPrimaryKeyFunc)
        {
            _entityPrimaryKeyFunc = entityPrimaryKeyFunc;
            DatabaseFactory = databaseFactory;
            _dbset = DataContext.Set<TEntityType>();
        }

        protected IDatabaseFactory<TDatabseType> DatabaseFactory { get; }

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

        //Will return ID from dbb
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
            DataContext.BulkInsert(entities);
        }

        public virtual void Update(TEntityType entity)
        {
            DataContext.Set<TEntityType>().AddOrUpdate(entity);
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
            return _dataDbContext.Database.SqlQuery<TEntityType>(query, parameters);
        }

        public IEnumerable<T> ExecuteQuery<T>(string query, params object[] parameters)
        {
            return _dataDbContext.Database.SqlQuery<T>(query, parameters);
        }

        public void ExecuteQueryAsAction(string query, params object[] parameters)
        {
            _dataDbContext.Database.ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction,query, parameters);
        }

        public virtual IEnumerable<TEntityType> GetAutoCompleteItems(Expression<Func<TEntityType, bool>> where, int numberOfReturnValues)
        {
            return _dbset.Where(where).Take(numberOfReturnValues).ToList();
        }
    }
}