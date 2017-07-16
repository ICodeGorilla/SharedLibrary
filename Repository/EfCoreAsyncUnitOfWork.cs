﻿using System.Threading.Tasks;
using Shared.Repository.Contract;

namespace Shared.Repository
{
    public class EfCoreAsyncUnitOfWork<T> : Disposable, IAsyncUnitOfWork where T : Microsoft.EntityFrameworkCore.DbContext
}