using System.Data.Entity;
using System.Linq;
using Moq;

namespace Shared.Helper.Test
{
    public static class DatabaseMocker
    {
        public static Mock<TDb> GetMockedDbContext<TDb, TEntity>(IQueryable<TEntity> data) where TDb : DbContext where TEntity : class
        {
            var mockSet = CreateMockDbSet(data);
            var dbContext = new Mock<TDb>();
            dbContext.Setup(context => context.Set<TEntity>()).Returns(mockSet.Object);
            return dbContext;
        }

        public static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }
    }
}
