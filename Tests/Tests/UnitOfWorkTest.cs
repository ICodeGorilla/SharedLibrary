using System.Linq;
using NUnit.Framework;
using Shared.Helper.Test;
using Shared.Repository;
using Tests.Database;
using Tests.Helper;

namespace Tests.Tests
{
    [TestFixture]
    public class UnitOfWorkTest
    {
        //Given I have a databaseFactory
        //When I create a unit of work and commit using it
        //Then all changes that are saved within this unit of work should be retrievable
        [Test, Rollback]
        public void UnitOfWorkShouldSaveItemsTest()
        {
            //Given
            var databaseFactory = new DatabaseFactoryBase<SharedCommonDatabaseContext>("SharedCommonDatabaseContext");

            //When
            using (var unitOfWork = new UnitOfWork<SharedCommonDatabaseContext>(databaseFactory))
            {
                var database = databaseFactory.Get();
                var countBeforeInsert = database.Accounts.Count();
                var account = database.Accounts.Add(AccountEntityHelper.CreateTestAccount());
                unitOfWork.Commit();
                var countAfterInsert = database.Accounts.Count();

                //Then
                Assert.IsNotNull(account);
                Assert.IsTrue(countBeforeInsert < countAfterInsert, "Item was not inserted.");
            }
        }

        //Given I have a databaseFactory and I added an item
        //When I create a unit of work and dispose it
        //Then all changes should be rolled back
        [Test]
        public void UnitOfWorkShouldRollbackItemsTest()
        {
            //Given
            var databaseFactory = new DatabaseFactoryBase<SharedCommonDatabaseContext>("SharedCommonDatabaseContext");   
            using (var unitOfWork = new UnitOfWork<SharedCommonDatabaseContext>(databaseFactory))
            {
                var database = databaseFactory.Get();
                var countBeforeInsert = database.Accounts.Count();
                database.Accounts.Add(AccountEntityHelper.CreateTestAccount());

                //When
                unitOfWork.Dispose();

                //Then
                var countAfterInsert = database.Accounts.Count();
                Assert.AreEqual(countBeforeInsert, countAfterInsert, "Item was inserted.");
            }
        }
    }
}
