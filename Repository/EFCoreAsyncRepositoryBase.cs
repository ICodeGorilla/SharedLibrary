using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shared.Repository.Contract;

namespace Shared.Repository
{
    public abstract class EfCoreAsyncRepositoryBase<TEntityType, TDatabseType, TPrimaryKeyType> :
        IAsyncRepository<TEntityType, TPrimaryKeyType> where TEntityType : class
        where TDatabseType : DbContext
    {
        private readonly Func<TEntityType, TPrimaryKeyType> _entityPrimaryKeyFunc;
        //Removed IDbSet as it is not implementing all the members we need http://stackoverflow.com/questions/31539750/why-theres-no-addrange-removerange-method-in-idbset-interface-in-entity-6
        private readonly DbSet<TEntityType> _dbset;
        private DbContext _dataDbContext;

        protected EfCoreAsyncRepositoryBase(IEfCoreDatabaseFactory<TDatabseType> databaseFactory,
            Func<TEntityType, TPrimaryKeyType> entityPrimaryKeyFunc)
        {
            _entityPrimaryKeyFunc = entityPrimaryKeyFunc;
            DatabaseFactory = databaseFactory;
            _dbset = DataContext.Set<TEntityType>();
        }

        protected IEfCoreDatabaseFactory<TDatabseType> DatabaseFactory { get; }

        protected DbContext DataContext => _dataDbContext ?? (_dataDbContext = DatabaseFactory.Get());
        public async  Task<TEntityType> SaveAsync(TEntityType entity)
        {
            var original = await DataContext.Set<TEntityType>().FindAsync(_entityPrimaryKeyFunc(entity)).ConfigureAwait(true);
            if (original != null)
            {
                DataContext.Entry(original).CurrentValues.SetValues(entity);
            }
            else
            {
                await DataContext.Set<TEntityType>().AddAsync(entity).ConfigureAwait(true);
            }
            return entity;
        }

        public async Task AddRangeAsync(List<TEntityType> entities)
        {
            foreach (var entity in entities)
            {
                await SaveAndCommitAsync(entity);
            }
        }

        public async Task UpdateAsync(TEntityType entity)
        {
            await SaveAsync(entity).ConfigureAwait(true);
        }

        public async Task DeleteAsync(TEntityType entity)
        {
            await Task.Run(() => _dbset.Remove(entity)).ConfigureAwait(true);
        }
        
        public async Task DeleteAsync(Expression<Func<TEntityType, bool>> @where)
        {
            var objects = await _dbset.Where(@where).ToAsyncEnumerable().ToList().ConfigureAwait(true);
            await Task.Run(() => _dbset.RemoveRange(objects)).ConfigureAwait(true);
        }

        public async Task<TEntityType> GetByIdAsync(TPrimaryKeyType id)
        {
            return await DataContext.Set<TEntityType>().FindAsync(id).ConfigureAwait(true);
        }

        public async Task<TEntityType> GetAsync(Expression<Func<TEntityType, bool>> @where)
        {
            try
            {
                return await _dbset.SingleAsync(@where).ConfigureAwait(true);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntityType, bool>> @where)
        {
            return await _dbset.AnyAsync(@where).ConfigureAwait(true);
        }

        public async Task<bool> NoneAsync(Expression<Func<TEntityType, bool>> @where)
        {
            return !await AnyAsync(@where).ConfigureAwait(true);
        }

        public async Task<IEnumerable<TEntityType>> GetAllAsync()
        {
            return  await _dbset.ToListAsync().ConfigureAwait(true);
        }

        public async Task<IEnumerable<TEntityType>> GetManyAsync(Expression<Func<TEntityType, bool>> @where)
        {
            return await _dbset.Where(@where).ToListAsync().ConfigureAwait(true);
        }

        public async Task<IEnumerable<TEntityType>> ExecuteQueryAsync(string query, params object[] parameters)
        {
            var result = await Task.Run(() => _dbset.FromSql(query, parameters)).ConfigureAwait(true);
            return await result.ToListAsync().ConfigureAwait(true);
        }

        public async Task<IEnumerable<TEntityType>> GetAutoCompleteItemsAsync(Expression<Func<TEntityType, bool>> @where, int numberOfReturnValues)
        {
            return await _dbset.Where(@where).Take(numberOfReturnValues).ToListAsync().ConfigureAwait(true);
        }

        public async Task ExecuteQueryAsActionAsync(string query, params object[] parameters)
        {
            await Task.Run(() => _dataDbContext.Database.ExecuteSqlCommand(query, parameters)).ConfigureAwait(true);
        }

        public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string query, params object[] parameters)
        {
            return (IEnumerable<T>) await _dbset.FromSql(query, parameters).ToListAsync().ConfigureAwait(true);
        }

        public async Task<TEntityType> SaveAndCommitAsync(TEntityType entity)
        {
            var original = await DataContext.Set<TEntityType>().FindAsync(_entityPrimaryKeyFunc(entity)).ConfigureAwait(true);
            if (original != null)
            {
                DataContext.Entry(original).CurrentValues.SetValues(entity);
                DataContext.SaveChanges();
            }
            else
            {
                await DataContext.Set<TEntityType>().AddAsync(entity).ConfigureAwait(true);
                DataContext.SaveChanges();
                DataContext.Entry(entity).GetDatabaseValues();
            }
            return entity;
        }
    }


}