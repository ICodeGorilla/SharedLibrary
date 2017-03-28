using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NUnit.Framework;
using Shared.Helper.Test;
using Shared.Repository;
using Tests.Database.EFCore;
using Tests.Helper;
using Account = Tests.Database.EFCore.Account;

namespace Tests.Tests
{
    [TestFixture]
    public class EfCoreAsyncRepositoryAsyncTests
    {
        private IDbContextTransaction _transaction;
        private EfCoreDatabaseFactoryBase<SharedLibraryContext> _databaseFactory;

        [SetUp]
        public void Init()
        {
            _databaseFactory = new EfCoreDatabaseFactoryBase<SharedLibraryContext>("SharedLibraryContext");
            _transaction = _databaseFactory.Get().Database.BeginTransaction();
        }

        [TearDown]
        public void Cleanup()
        {
            _transaction.Rollback();
        }

        //Given I have an Item
        //When I SaveAsync it and CommitAsync it
        //Then It should have an ID
        [Test]
        public async Task AddItemTestAsync()
        {
            //Given 
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);

            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                //When
                var testAccount = AccountEntityHelper.CreateEfTestAccount();
                var addedAccount = await repository.SaveAsync(testAccount).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);

                //Then
                Assert.AreNotEqual(addedAccount.AccountId, 0, "The account ID was not updated.");
                EqualityHelper.PropertyValuesAreEqual(addedAccount, testAccount, new[] {"AccountID"});
            }
        }

        //Given I have an item that is missing some information
        //When I SaveAsync it
        //Then I should receive an exception
        [Test]
        public async Task AddItemFailsValidationTestAsync()
        {
            //Given
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
            var testAccount = AccountEntityHelper.CreateEfTestAccount();
            testAccount.CompanyName = null;
            await repository.SaveAsync(testAccount).ConfigureAwait(true);

            //when
            AsyncTestDelegate asyncTestDelegate = async () =>
                await new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory).CommitAsync()
                    .ConfigureAwait(true);

            //Then
            Assert.ThrowsAsync<DbUpdateException>(
                asyncTestDelegate);
        }

        //Given I have multiple accounts
        //When I add them
        //Then I should be able to retrieve all of them
        [Test]
        public async Task AddListOfItemsTestAsync()
        {
            //Given
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
            var listOfItems = AccountEntityHelper.CreateEfCoreTestAccounts(3);
            var originalItems = await repository.GetAllAsync().ConfigureAwait(true);

            //When
            await repository.AddRangeAsync(listOfItems).ConfigureAwait(true);

            //Then
            var allNewItems = await repository.GetAllAsync().ConfigureAwait(true);
            var itemsAdded = allNewItems.Except(originalItems).ToList();
            EqualityHelper.AssertListsAreEqual(itemsAdded, listOfItems,
                new[] {"AccountID", "LastModified", "Contacts"});

        }

        //Given I have multiple accounts
        //When I add them
        //Then I should receive an exception
        [Test]
        public void AddListOfItemsFailsValidationTestAsync()
        {
            //Given
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
            var listOfItems = AccountEntityHelper.CreateEfCoreTestAccounts(3);
            listOfItems[1].CompanyName = null;

            //When
            AsyncTestDelegate asyncTestDelegate = async () => await repository.AddRangeAsync(listOfItems).ConfigureAwait(true);

            //Then
            Assert.ThrowsAsync<DbUpdateException>(
               asyncTestDelegate);
        }

        //Given I have a Saved account
        //When I update the compAnyAsync name
        //Then I should be able to retrieve it with its new account name
        [Test]
        public async Task UpdateSuccessItemTestAsync()
        {
            //Given
             
            Account testAccount;
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);

            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                testAccount = await repository.SaveAsync(AccountEntityHelper.CreateEfTestAccount()).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);

                //When
                testAccount.CompanyName = "Updated account";
                await repository.SaveAsync(testAccount).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);
            }

            //Then
            var finalRepo = new EfCoreAsyncAccountRepository(_databaseFactory);
            var itemToCheck = await finalRepo.GetByIdAsync(testAccount.AccountId).ConfigureAwait(true);
            EqualityHelper.PropertyValuesAreEqual(itemToCheck, testAccount, new[] { "LastModified", "Contact" });
        }

        //Given I have a Saved account
        //When I update the compAnyAsync name to null
        //Then I should not be able to retrieve it with its new account name
        [Test]
        public async Task UpdateFailItemTestAsync()
        {
            //Given
            Account testAccount;
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);

            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                testAccount = await repository.SaveAsync(AccountEntityHelper.CreateEfTestAccount()).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(false);
            }

            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                //When
                testAccount.CompanyName = null;
                await repository.SaveAsync(testAccount).ConfigureAwait(true);
                // ReSharper disable once AccessToDisposedClosure
                AsyncTestDelegate testDelegate = async () => await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);
                Assert.ThrowsAsync<DbUpdateException>(testDelegate);
            }

            //Then
            //GetAsync fresh database factory
            var finalDatabaseFactory = new EfCoreDatabaseFactoryBase<SharedLibraryContext>("SharedLibraryContext");
            var finalRepo = new EfCoreAsyncAccountRepository(finalDatabaseFactory);
            var itemToCheck = await finalRepo.GetAllAsync().ConfigureAwait(true);
            Assert.AreEqual(testAccount.CompanyName, itemToCheck.FirstOrDefault()?.CompanyName, "The compAnyAsync name was updated when it should not have been");

        }

        //Given I have an item
        //When I DeleteAsync it
        //Then I should not be able to retrieve it
        [Test]
        public async Task DeleteAsyncItemSuccessTestAsync()
        {
            //Given
            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                //Given
                var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
                var testAccount = await repository.SaveAsync(AccountEntityHelper.CreateEfTestAccount()).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);

                //When
                await repository.DeleteAsync(testAccount).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);

                //Then
                var retrievedAccount = await repository.GetByIdAsync(testAccount.AccountId).ConfigureAwait(true);
                Assert.IsNull(retrievedAccount,"The account was not DeleteAsyncd.");
            }
        }

        //Given I have an items that are Saved
        //When I DeleteAsync them using a where clause
        //Then I should not be able to retrieve them
        [Test]
        public async Task DeleteAsyncWithWhereClauseSuccessTestAsync()
        {
            //Given
            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
                var listOfItems = AccountEntityHelper.CreateEfCoreTestAccounts(3);
                var originalItems = await repository.GetAllAsync().ConfigureAwait(true);
                await repository.AddRangeAsync(listOfItems).ConfigureAwait(true);
                var allNewItems = await repository.GetAllAsync().ConfigureAwait(true);
                var newItems = allNewItems as IList<Account> ?? allNewItems.ToList();
                var itemsAdded = newItems.Except(originalItems).ToList();

                //When
                var idsToDeleteAsync = itemsAdded.Select(x => x.AccountId);
                await repository.DeleteAsync(x => idsToDeleteAsync.Contains(x.AccountId)).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);
                var allItems = await repository.GetAllAsync().ConfigureAwait(true);
                //Then
                Assert.AreEqual(0, allItems.Except(newItems).Count(), "The items have not been DeleteAsyncd.");
            }
        }

        //Given I have an item in the database
        //When I GetAsync it by ID
        //Then it should match the item in the database
        [Test]
        public async Task RetrieveItemByIdTestAsync()
        {
            //Given
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
            
            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                var addedAccount = await repository.SaveAsync(AccountEntityHelper.CreateEfTestAccount()).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);

                //When
                var retrievedItem = await repository.GetByIdAsync(addedAccount.AccountId).ConfigureAwait(true);

                //Then
                EqualityHelper.PropertyValuesAreEqual(retrievedItem, addedAccount);
            }
        }

        //Given I have an ID of an Item that does not exist
        //When I GetAsync it by ID
        //Then I should receive an exception
        [Test]
        public async Task GetAsyncByIdAsyncExceptionTestAsync()
        {
            //Given
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);

            //When
            var shouldBeNull = await repository.GetByIdAsync(-1).ConfigureAwait(true);

            //Then
            Assert.IsNull(shouldBeNull,"The item was found when it should have never existed.");
        }

        //Given I have items in a db
        //When I GetAsync it by a where clause
        //Then I should receive the first item that matches it
        [Test]
        public async Task GetAsyncWithWhereClauseSuccessTestAsync()
        {
            //Given
            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
                var listOfItems = AccountEntityHelper.CreateEfCoreTestAccounts(3);
                listOfItems[2].CompanyName = "TestReferenceOtherValue";
                await repository.AddRangeAsync(listOfItems).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);

                //When
                var items = await repository.GetAsync(x => x.CompanyName.Contains("TestReference")).ConfigureAwait(true);

                //Then
                EqualityHelper.PropertyValuesAreEqual(items, listOfItems[2],
                    new[] {"AccountID", "LastModified", "LastModifiedBy", "Contacts"});
            }
        }

        //Given I have items no items in a db
        //When I GetAsync it by a where clause
        //Then I should receive a null
        [Test]
        public async Task GetAsyncWithWhereClauseButNoneAsyncMatchSuccessTestAsync()
        {
            //Given
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);

            //When
            var shouldBeNull = await repository.GetAsync(x => false).ConfigureAwait(true);

            //Then
            Assert.IsNull(shouldBeNull, "No item should have been found.");
        }

        //Given I items in a DB that matches a clause
        //When I check if AnyAsync are in the database with the clause
        //Then I should receive true
        [Test]
        public async Task GetAsyncAnyAsyncSuccessTestAsync()
        {
            //Given
            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
                var listOfItems = AccountEntityHelper.CreateEfCoreTestAccounts(3);
                listOfItems[2].CompanyName = "TestReferenceOtherValue";
                await repository.AddRangeAsync(listOfItems).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);

                //When
                var hasAnyAsync = await repository.AnyAsync(x => x.CompanyName.Contains("TestReference")).ConfigureAwait(true);

                //Then
                Assert.IsTrue(hasAnyAsync,"NoneAsync were found when there should have been some.");
            }
        }

        //Given I items in a DB but NoneAsync matches a clause
        //When I check if AnyAsync are in the database with the clause
        //Then I should receive false
        [Test]
        public async Task GetAsyncAnyAsyncButNoneAsyncExistTestAsync()
        {
            //Given
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);

            //When
            var hasAnyAsync = await repository.AnyAsync(x => false).ConfigureAwait(true);

            //Then
            Assert.IsFalse(hasAnyAsync,"No items should have been found.");
        }

        //Given I items in a DB that matches a clause
        //When I check if NoneAsync are in the database with the clause
        //Then I should receive false
        [Test]
        public async Task GetAsyncNoneAsyncButSomeExistTestAsync()
        {
            //Given
            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
                var listOfItems = AccountEntityHelper.CreateEfCoreTestAccounts(3);
                listOfItems[2].CompanyName = "TestReferenceOtherValue";
                await repository.AddRangeAsync(listOfItems).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);

                //When
                var hasNoneAsync = await repository.NoneAsync(x => x.CompanyName.Contains("TestReference")).ConfigureAwait(true);

                //Then
                Assert.IsFalse(hasNoneAsync, "NoneAsync were found when there should have been some.");
            }
        }

        //Given I items in a DB but NoneAsync matches a clause
        //When I check if NoneAsync are in the database with the clause
        //Then I should receive true
        [Test]
        public async Task GetAsyncNoneAsyncSuccessTestAsync()
        {
            //Given
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);

            //When
            var hasNoneAsync = await repository.NoneAsync(x => false).ConfigureAwait(true);

            //Then
            Assert.IsTrue(hasNoneAsync, "No items should have been found.");
        }
        
        //Given I have some items in the database
        //When I GetAsync them all
        //Then they should match the items
        [Test]
        public async Task GetAllAsyncSuccessTestAsync()
        {
            //Given
            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
                var listOfItems = AccountEntityHelper.CreateEfCoreTestAccounts(3);
                listOfItems[0].CompanyName = "1";
                listOfItems[1].CompanyName = "2";
                listOfItems[2].CompanyName = "3";

                await repository.AddRangeAsync(listOfItems).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);

                //When
                var allItems = await repository.GetAllAsync().ConfigureAwait(true);

                //Then
                EqualityHelper.AssertListsAreEqual(allItems.OrderBy(x => x.CompanyName).ToList(),listOfItems.OrderBy(x => x.CompanyName).ToList(),
                    new[] { "AccountID", "LastModified", "LastModifiedBy", "Contacts" });
            }
        }

        //Given I have some items in the database
        //When I GetAsync them all
        //Then they should match the items
        [Test]
        public async Task GetAllAsyncButNoneAsyncExistTestAsync()
        {
            //Given
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);

            //When
            var allItems = await repository.GetAllAsync().ConfigureAwait(true);

            //Then
            Assert.AreEqual(0,allItems.Count(),"Some items were returned where NoneAsync should have been");
        }

        //Given I have items in a db
        //When I GetAsync it by a where clause
        //Then I should receive all the items that matches it
        [Test]
        public async Task GetAsyncMAnyAsyncWithWhereClauseSuccessTestAsync()
        {
            //Given
            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
                var reference = "TestReference";
                var listOfItems = GetAsyncItemsWithTwoItemsContainingTestReference(reference);
                await repository.AddRangeAsync(listOfItems).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);

                //When
                var items = await repository.GetManyAsync(x => x.CompanyName.Contains(reference)).ConfigureAwait(true);

                //Then
                EqualityHelper.AssertListsAreEqual(items.OrderBy(x => x.CompanyName).ToList(), listOfItems.OrderBy(x => x.CompanyName).Take(2).ToList(),new[] { "AccountID", "LastModified", "LastModifiedBy", "Contacts" });
            }
        }

        //Given I have items no items in a db
        //When I GetAsync it by a where clause
        //Then I should receive an empty list
        [Test]
        public async Task GetAsyncMAnyAsyncWithWhereClauseButNoneAsyncMatchSuccessTestAsync()
        {
            //Given
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);

            //When
            var shouldBeEmpty = await repository.GetManyAsync(x => false).ConfigureAwait(true);

            //Then
            Assert.IsFalse(shouldBeEmpty.Any(), "No item should have been found.");
        }

        //Given I have items in a db
        //When I GetAsync it by a where clause for an autocomplete
        //Then I should receive the first item that matches it
        [Test,Rollback]
        public async Task GetAsyncAutoCompleteItemsTestAsync()
        {
            //Given
            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
                var reference = "TestReference";
                var listOfItems = GetAsyncItemsWithTwoItemsContainingTestReference(reference);
                await repository.AddRangeAsync(listOfItems).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);

                //When
                var item = await repository.GetAutoCompleteItemsAsync(x => x.CompanyName.Contains(reference),1).ConfigureAwait(true);

                //Then
                EqualityHelper.PropertyValuesAreEqual(item.First(),listOfItems[0], new[] { "AccountID", "LastModified", "LastModifiedBy", "Contacts" });
            }
        }

        //Given I have items no items in a db
        //When I GetAsync it by a where clause
        //Then I should receive an empty list
        [Test]
        public async Task GetAsyncAutocompleteItemsButNoneExistTestAsync()
        {
            //Given
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);

            //When
            var shouldBeEmpty = await repository.GetAutoCompleteItemsAsync(x => false, 1).ConfigureAwait(true);

            //Then
            Assert.IsFalse(shouldBeEmpty.Any(), "No item should have been found.");
        }

        //Given I have a stored procedure and accounts in the database
        //When I GetAsync all the items via the stored procedure with a filter
        //Then it should match the all the accounts in the database that match the filter
        [Test]
        public async Task GetAsyncItemsViaStoredProcedureWithParameterTestAsync()
        {
            //Given
            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
                var reference = "TestReference";
                var listOfItems = GetAsyncItemsWithTwoItemsContainingTestReference(reference);
                await repository.AddRangeAsync(listOfItems).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);

                //When
                var filter = new SqlParameter("@CompanyName", SqlDbType.VarChar) {Value = $"%{reference}%"};
                var items = await repository.ExecuteQueryAsync("exec GetAccounts @CompanyName", filter).ConfigureAwait(true);

                //Then
                EqualityHelper.AssertListsAreEqual(items.OrderBy(x => x.CompanyName).ToList(), listOfItems.OrderBy(x => x.CompanyName).Take(2).ToList(), new[] { "AccountID", "LastModified", "LastModifiedBy", "Contacts" });
            }
        }

        //Given I have a stored procedure and accounts in the database
        //When I GetAsync all the items via the stored procedure
        //Then it should match the all the accounts in the database
        [Test]
        public async Task GetAsyncItemsViaStoredProcedureWithNoParameterTestAsync()
        {
            //Given
            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
                var listOfItems = AccountEntityHelper.CreateEfCoreTestAccounts(3);
                await repository.AddRangeAsync(listOfItems).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);

                //When
                var items = await repository.ExecuteQueryAsync("exec GetAccounts").ConfigureAwait(true);

                //Then
                EqualityHelper.AssertListsAreEqual(items.OrderBy(x => x.CompanyName).ToList(), listOfItems.OrderBy(x => x.CompanyName).ToList(), new[] { "AccountID", "LastModified", "LastModifiedBy", "Contacts" });
            }
        }

        //Given I have a stored procedure and no accounts in the database
        //When I GetAsync all the items via the stored procedure
        //Then it should return an empty list
        [Test]
        public async Task GetAsyncItemsViaStoredProcedureWithNoParameterButNoAccountsExistTestAsync()
        {
            //Given
            //When
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
            var items = await repository.ExecuteQueryAsync("exec GetAccounts").ConfigureAwait(true);

            //Then
            Assert.AreEqual(0, items.ToList().Count, "Some items were returned where None should have been");
        }

        //Given I have a broken query
        //When I execute the query
        //Then I should receive an exception
        [Test]
        public async Task ExecuteStoredProcedureWithIncorrectQueryTestAsync()
        {
            //Given
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);

            //When
            AsyncTestDelegate testDelegate = async () => await repository.ExecuteQueryAsync("exec GetAccountsFake").ConfigureAwait(true);

            //Then
            Assert.ThrowsAsync<SqlException>(testDelegate, "The expected exception was not thrown.");
        }

        //Given I have a stored procedure and accounts in the database
        //When I GetAsync all the items via the stored procedure with a filter
        //Then it should match the all the accounts in the database that match the filter
        [Test]
        public async Task GetAsyncItemsViaStoredProcedureWithParameterNotParameterisedTestAsync()
        {
            //Given
            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
                var reference = "TestReference";
                var listOfItems = GetAsyncItemsWithTwoItemsContainingTestReference(reference);
                await repository.AddRangeAsync(listOfItems).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);

                //When
                var filter = new SqlParameter("@CompanyName", SqlDbType.VarChar) { Value = $"%{reference}%" };
                var items = await repository.ExecuteQueryAsync<Account>("exec GetAccounts @CompanyName", filter).ConfigureAwait(true);

                //Then
                EqualityHelper.AssertListsAreEqual(items.OrderBy(x => x.CompanyName).ToList(), listOfItems.OrderBy(x => x.CompanyName).Take(2).ToList(), new[] { "AccountID", "LastModified", "LastModifiedBy", "Contacts" });
            }
        }

        //Given I have a stored procedure and accounts in the database
        //When I GetAsync all the items via the stored procedure
        //Then it should match the all the accounts in the database
        [Test]
        public async Task GetAsyncItemsViaStoredProcedureWithNoParameterNotParameterisedTestAsync()
        {
            //Given
            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
                var listOfItems = AccountEntityHelper.CreateEfCoreTestAccounts(3);
                await repository.AddRangeAsync(listOfItems).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);

                //When
                var items = await repository.ExecuteQueryAsync<Account>("exec GetAccounts").ConfigureAwait(true);

                //Then
                EqualityHelper.AssertListsAreEqual(items.OrderBy(x => x.CompanyName).ToList(), listOfItems.OrderBy(x => x.CompanyName).ToList(), new[] { "AccountID", "LastModified", "LastModifiedBy", "Contacts" });
            }
        }

        //Given I have a stored procedure and no accounts in the database
        //When I GetAsync all the items via the stored procedure
        //Then it should return an empty list
        [Test]
        public async Task GetAsyncItemsViaStoredProcedureWithNoParameterButNoAccountsExistNotParameterisedTestAsync()
        {
            //Given
            //When
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
            var items = await repository.ExecuteQueryAsync<Account>("exec GetAccounts").ConfigureAwait(true);

            //Then
            Assert.AreEqual(0, items.ToList().Count, "Some items were returned where None should have been");
        }

        //Given I have a stored procedure and accounts in the database
        //When I clear all the items in the database via the stored procedure
        //Then I should not be able to retrieve the item
        [Test]
        public async Task ExecuteStoredProcedureAsActionTestAsync()
        {
            //Given
            using (var efCoreAsyncUnitOfWork = new EfCoreAsyncUnitOfWork<SharedLibraryContext>(_databaseFactory))
            {
                var repository = new EfCoreAsyncAccountRepository(_databaseFactory);
                var listOfItems = AccountEntityHelper.CreateEfCoreTestAccounts(3);
                await repository.AddRangeAsync(listOfItems).ConfigureAwait(true);
                await efCoreAsyncUnitOfWork.CommitAsync().ConfigureAwait(true);

                //When
                await repository.ExecuteQueryAsActionAsync("exec DeleteAllAccounts").ConfigureAwait(true);

                //Then
                var allItems = await repository.GetAllAsync().ConfigureAwait(true);
                Assert.AreEqual(0, allItems.ToList().Count(), "There are still items in the database.");
            }
        }

        //Given I have a broken query
        //When Execute the stored procedure
        //Then I should receive an exception
        [Test]
        public void ExecuteStoredProcedureAsActionWithBrokenSyntaxTestAsync()
        {
            //Given
            var repository = new EfCoreAsyncAccountRepository(_databaseFactory);

            //When
            AsyncTestDelegate testDelegate = async () => await repository.ExecuteQueryAsActionAsync("exec DeleteAsyncAllAccountsFake").ConfigureAwait(true);

            //Then
            Assert.ThrowsAsync<SqlException>(testDelegate);
        }

       

        private static List<Account> GetAsyncItemsWithTwoItemsContainingTestReference(string reference)
        {
            var listOfItems = AccountEntityHelper.CreateEfCoreTestAccounts(3);
            listOfItems[0].CompanyName = $"1{reference}";
            listOfItems[1].CompanyName = $"2{reference}OtherValue";
            listOfItems[2].CompanyName = "3";
            return listOfItems;
        }
    }
}
