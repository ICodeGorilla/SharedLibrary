using System.Threading.Tasks;
using Shared.Repository.Contract;

namespace Shared.Repository
{
    public class EfCoreAsyncUnitOfWork<T> : Disposable, IAsyncUnitOfWork where T : Microsoft.EntityFrameworkCore.DbContext    {        private readonly IEfCoreDatabaseFactory<T> _databaseFactory;        private T _dataContext;        public EfCoreAsyncUnitOfWork(IEfCoreDatabaseFactory<T> databaseFactory)        {            _databaseFactory = databaseFactory;        }        protected Microsoft.EntityFrameworkCore.DbContext DataContext => _dataContext ?? (_dataContext = _databaseFactory.Get());        public async Task CommitAsync()        {            await DataContext.SaveChangesAsync().ConfigureAwait(true);        }    }
}