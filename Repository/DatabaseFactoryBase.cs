using System;
using System.Data.Entity;
using Shared.Repository.Contract;

namespace Shared.Repository
{
    public class DatabaseFactoryBase<T> : Disposable, IDatabaseFactory<T> where T : DbContext, new()
    {
        private readonly string _nameOrConnectionString;
        private T _context;

        public DatabaseFactoryBase(string nameOrConnectionString)
        {
            _nameOrConnectionString = nameOrConnectionString;
        }

        T IDatabaseFactory<T>.Get()
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