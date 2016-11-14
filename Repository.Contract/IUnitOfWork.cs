namespace Shared.Repository.Contract
{
    public interface IUnitOfWork
    {
        void Commit();
    }
}