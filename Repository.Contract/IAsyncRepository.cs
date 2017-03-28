using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Shared.Repository.Contract
{
    public interface IAsyncRepository<TEntityType, in TPrimaryKeyType> where TEntityType : class
    {
        Task<TEntityType> SaveAsync(TEntityType entity);
        Task AddRangeAsync(List<TEntityType> entities);
        Task UpdateAsync(TEntityType entity);
        Task DeleteAsync(TEntityType entity);
        Task DeleteAsync(Expression<Func<TEntityType, bool>> where);
        Task<TEntityType> GetByIdAsync(TPrimaryKeyType id);
        Task<TEntityType> GetAsync(Expression<Func<TEntityType, bool>> where);
        Task<bool> AnyAsync(Expression<Func<TEntityType, bool>> where);
        Task<bool> NoneAsync(Expression<Func<TEntityType, bool>> where);
        Task<IEnumerable<TEntityType>> GetAllAsync();
        Task<IEnumerable<TEntityType>> GetManyAsync(Expression<Func<TEntityType, bool>> where);
        Task<IEnumerable<TEntityType>> ExecuteQueryAsync(string query, params object[] parameters);
        Task<IEnumerable<TEntityType>> GetAutoCompleteItemsAsync(Expression<Func<TEntityType, bool>> where, int numberOfReturnValues);
        Task ExecuteQueryAsActionAsync(string query, params object[] parameters);
        Task<IEnumerable<T>> ExecuteQueryAsync<T>(string query, params object[] parameters);
        Task<TEntityType> SaveAndCommitAsync(TEntityType entity);
    }
}