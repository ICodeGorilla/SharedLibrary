using System;
using Shared.Repository.Contract;

namespace Shared.Repository
{
    public class EfCoreDatabaseFactoryBase<T> : Disposable, IEfCoreDatabaseFactory<T> where T : Microsoft.EntityFrameworkCore.DbContext, new()
    {
        private readonly string _nameOrConnectionString;
        private T _context;

        public EfCoreDatabaseFactoryBase(string nameOrConnectionString)
        {
            _nameOrConnectionString = nameOrConnectionString;
        }

        T IEfCoreDatabaseFactory<T>.Get()
        {
            return Get();
        }

        public T Get()
        {
            return _context ?? (_context = (T) Activator.CreateInstance(typeof (T), _nameOrConnectionString));
        }

        protected override void DisposeCore()
        {
            _context?.Dispose();
        }
    }
}