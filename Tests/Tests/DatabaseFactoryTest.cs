using NUnit.Framework;
using Shared.Repository;
using Tests.Database;

namespace Tests.Tests
{
    [TestFixture]
    public class DatabaseFactoryTest
    {
        //Given I have a connection string
        //When I create a DB factory
        //Then I should be able to retrieve the database
        [Test]
        public void DatabaseFactoryCreateTest()
        {
            //Given
            var databaseFactory = new DatabaseFactoryBase<SharedCommonDatabaseContext>("SharedCommonDatabaseContext");

            //When
            var databaseContext = databaseFactory.Get();

            //Then
            Assert.IsNotNull(databaseContext?.Database, "The database has not been returned from the factory");
        }
    }
}
