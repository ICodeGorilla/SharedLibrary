namespace Shared.Repository.Contract
{
    public interface IEfCoreDatabaseFactory<T> where T: Microsoft.EntityFrameworkCore.DbContext
    {
        T Get();
    }
}