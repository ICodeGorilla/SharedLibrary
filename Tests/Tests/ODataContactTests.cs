using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Shared.Logging;
using NUnit.Framework;
using Shared.Controller;
using Shared.Helper.Test;
using Tests.Database;

namespace Tests.Tests
{
    [TestFixture]
    public class ODataContactTests
    {
        //Given:  I have an odata controller with items for it in the database
        //When:   I get all the items 
        //Then:   I should receive those items
        [TestCase]
        public void ODataReadOnlyWithItemsTest()
        {
            //Given:
            var data = new List<TestBasicGuid>
                {
                    new TestBasicGuid {ID =  Guid.NewGuid()},
                    new TestBasicGuid {ID =  Guid.NewGuid()},
                    new TestBasicGuid {ID =  Guid.NewGuid()}
                }.AsQueryable();
            var dbContext = DatabaseMocker.GetMockedDbContext<FakeDbContext, TestBasicGuid>(data);
            var controller = new ODataReadOnlyControllerBase<TestBasicGuid, Guid, FakeDbContext>(new NLogLogger(), UserHelper.CreateTestCurrentUser(), dbContext.Object, key => x => x.ID == key);

            //When:
            var result = controller.Get().ToList();

            //Then:
            Assert.IsNotEmpty(result, "A non empty list was expected.");
        }

        //Given:  I have an odata controller with no items for it in the database
        //When:   I get all the items 
        //Then:   I should receive an empty result
        [TestCase]
        public void ControllerHasNoDataTest()
        {
            //Given:
            var data = new List<TestBasicGuid>().AsQueryable();
            var dbContext = DatabaseMocker.GetMockedDbContext<FakeDbContext, TestBasicGuid>(data);
            var controller = new ODataReadOnlyControllerBase<TestBasicGuid, Guid, FakeDbContext>(new NLogLogger(), UserHelper.CreateTestCurrentUser(), dbContext.Object, key => x => x.ID == key);

            //When:
            var result = controller.Get().ToList();

            //Then:
            Assert.IsEmpty(result, "An empty list was expected.");
        }

        //Given:  I have an odata controller with items 
        //When:   I get an item with a specific int ID 
        //Then:   The item should match what we expect it to be
        [TestCase]
        public void GetItemByIdIdTest()
        {
            TestGetItemByID(() => new List<TestBasicInt> {
                new TestBasicInt { ID = 0 },
                new TestBasicInt { ID = 1 },
                new TestBasicInt { ID = 2 }}
                .AsQueryable(),
                data => CreateControllerWithData<TestBasicInt, int>(data, key => x => x.ID == key),
                data => data.First(),
                item => item.ID);
        }

        //Given:  I have an odata controller with items 
        //When:   I get an item with a specific int ID 
        //Then:   The item should match what we expect it to be
        [TestCase]
        public void GetItemByLongIdTest()
        {
            TestGetItemByID(() => new List<TestBasicLong> {
                new TestBasicLong {ID = 0},
                new TestBasicLong {ID = 1},
                new TestBasicLong {ID = 2}
        }.AsQueryable()
                ,
                data => CreateControllerWithData<TestBasicLong, long>(data, key => x => x.ID == key),
                data => data.First(),
                item => item.ID);
        }

        //Given:  I have an odata controller with items 
        //When:   I get an item with a specific int ID 
        //Then:   The item should match what we expect it to be
        [TestCase]
        public void GetItemByGuidIdTest()
        {
            TestGetItemByID(() => new List<TestBasicGuid>() { new TestBasicGuid { ID = new Guid() },
                new TestBasicGuid { ID = new Guid() },
                new TestBasicGuid { ID = new Guid() }
                }.AsQueryable(),
                data => CreateControllerWithData<TestBasicGuid, Guid>(data, key => x => x.ID == key),
                data => data.First(),
                item => item.ID);
        }

        private static void TestGetItemByID<TEntity, TKey>(
            Func<IQueryable<TEntity>> datafFunc,
            Func<IQueryable<TEntity>, ODataReadOnlyControllerBase<TEntity, TKey, FakeDbContext>> controllerFunc,
            Func<IQueryable<TEntity>, TEntity> getFirstItemFunc,
            Func<TEntity, TKey> getPrimaryKeyFunc) where TEntity : class
        {
            //Given:
            var data = datafFunc();
            var controller = controllerFunc(data);
            var itemToFind = getFirstItemFunc(data);
            var primaryKey = getPrimaryKeyFunc(itemToFind);

            //When:
            var result = controller.Get(primaryKey).Queryable.First();

            //Then:
            EqualityHelper.PropertyValuesAreEqual(result, itemToFind);
        }

        //Given:  I have an odata controller with items 
        //When:   I get an item with a specific guid ID that does not exist
        //Then:   An exception should be thrown
        [TestCase]
        public void GetItemThatDoesNotExistGuidTest()
        {
            TestGetItemByIdThatDoesNotExist(() => new List<TestBasicGuid>() { new TestBasicGuid { ID =  Guid.NewGuid() },
                new TestBasicGuid { ID = Guid.NewGuid() },
                new TestBasicGuid { ID =  Guid.NewGuid() }
                }.AsQueryable(),
                data => CreateControllerWithData<TestBasicGuid, Guid>(data, key => x => x.ID == key),
                 Guid.NewGuid());
        }

        //Given:  I have an odata controller with items 
        //When:   I get an item with a specific guid ID that does not exist
        //Then:   An exception should be thrown
        [TestCase]
        public void GetItemThatDoesNotExistIntTest()
        {
            TestGetItemByIdThatDoesNotExist(() => new List<TestBasicInt>() {
                new TestBasicInt { ID = 0 },
                new TestBasicInt { ID = 1 },
                new TestBasicInt { ID = 2 }
                }.AsQueryable(),
                data => CreateControllerWithData<TestBasicInt, int>(data, key => x => x.ID == key),
                3);
        }

        //Given:  I have an odata controller with items 
        //When:   I get an item with a specific guid ID that does not exist
        //Then:   An exception should be thrown
        [TestCase]
        public void GetItemThatDoesNotExistLongTest()
        {
            TestGetItemByIdThatDoesNotExist(() => new List<TestBasicLong>() {
                new TestBasicLong { ID = 0 },
                new TestBasicLong { ID = 1 },
                new TestBasicLong { ID = 2 }
                }.AsQueryable(),
                data => CreateControllerWithData<TestBasicLong, long>(data, key => x => x.ID == key),
                3);
        }

        private static void TestGetItemByIdThatDoesNotExist<TEntity, TKey>(
            Func<IQueryable<TEntity>> datafFunc,
            Func<IQueryable<TEntity>, ODataReadOnlyControllerBase<TEntity, TKey, FakeDbContext>> controllerFunc,
            TKey key) where TEntity : class
        {
            //Given:
            var data = datafFunc();
            var controller = controllerFunc(data);

            //When:
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            TestDelegate testDelegate = () => controller.Get(key).Queryable.First();

            //Then:
            Assert.Throws<InvalidOperationException>(testDelegate);
        }

        private static ODataReadOnlyControllerBase<T, TKey, FakeDbContext> CreateControllerWithData<T, TKey>(IQueryable<T> data, Func<TKey, Expression<Func<T, bool>>> primaryKeyPredicate) where T : class
        {
            var dbContext = DatabaseMocker.GetMockedDbContext<FakeDbContext, T>(data);
            var controller = new ODataReadOnlyControllerBase<T, TKey, FakeDbContext>(new NLogLogger(), UserHelper.CreateTestCurrentUser(), dbContext.Object, primaryKeyPredicate);
            return controller;
        }
    }
}