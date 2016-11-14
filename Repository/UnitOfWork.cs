using System.Data.Entity;
using Shared.Repository.Contract;

namespace Shared.Repository
{
    public class UnitOfWork<T> : Disposable, IUnitOfWork where T : DbContext
    {
        private readonly IDatabaseFactory<T> _databaseFactory;
        private DbContext _dataContext;

        public UnitOfWork(IDatabaseFactory<T> databaseFactory)
        {
            _databaseFactory = databaseFactory;
        }

        protected DbContext DataContext => _dataContext ?? (_dataContext = _databaseFactory.Get());

        public void Commit()
        {
            DataContext.SaveChanges();
        }
    }
}