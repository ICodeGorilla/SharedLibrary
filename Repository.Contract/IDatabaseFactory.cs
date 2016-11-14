using System.Data.Entity;

namespace Shared.Repository.Contract
{
    public interface IDatabaseFactory<T> where T: DbContext
    {
        T Get();
    }
}