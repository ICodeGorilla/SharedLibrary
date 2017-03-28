using System.Threading.Tasks;

namespace Shared.Repository.Contract
{
    public interface IUnitOfWork
    {
        void Commit();
    }

    public interface IAsyncUnitOfWork
    {
        Task CommitAsync();
    }
}