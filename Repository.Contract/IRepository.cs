using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Shared.Repository.Contract
{
    public interface IRepository<TEntityType, in TPrimaryKeyType> where TEntityType : class
    {
        TEntityType Save(TEntityType entity);
        void AddRange(List<TEntityType> entities);
        void Update(TEntityType entity);
        void Delete(TEntityType entity);
        void Delete(Expression<Func<TEntityType, bool>> where);
        TEntityType GetById(TPrimaryKeyType id);
        TEntityType Get(Expression<Func<TEntityType, bool>> where);
        bool Any(Expression<Func<TEntityType, bool>> where);
        bool None(Expression<Func<TEntityType, bool>> where);
        IEnumerable<TEntityType> GetAll();
        IEnumerable<TEntityType> GetMany(Expression<Func<TEntityType, bool>> where);
        IEnumerable<TEntityType> ExecuteQuery(string query, params object[] parameters);
        IEnumerable<TEntityType> GetAutoCompleteItems(Expression<Func<TEntityType, bool>> where, int numberOfReturnValues);
        void ExecuteQueryAsAction(string query, params object[] parameters);
        IEnumerable<T> ExecuteQuery<T>(string query, params object[] parameters);
        TEntityType SaveAndCommit(TEntityType entity);
    }
}