using System;
using System.Data.Entity.Validation;
using System.Linq;
using Shared.Logging;
using NUnit.Framework;
using Shared.Helper.Test;
using Tests.Database;

namespace Tests.Tests
{
    [TestFixture]
    public class ODataIntegrationAccountTests
    {
        //Get a new controller for each call so that the transactions are seperate
        private AccountsController GetController()
        {
            return new AccountsController(UserHelper.CreateTestCurrentUser(), new NLogLogger());
        }

        //Given:  I do not have all the information needed to create a Account
        //When:   I save it 
        //Then:   I should receive an exception
        [Test]
        public void CreateAccountFailureTest()
        {
            //Given
            var controller = GetController();
            var request = new Account();

            //When
            TestDelegate response = () => controller.Post(request);

            //Then
            Assert.Throws<DbEntityValidationException>(response);
        }

        //Given:  I have all the information needed to create a account
        //When:   I save it 
        //Then:   I should be able to return it
        [Test, Rollback]
        public void CreateAccountSuccessfullyTest()
        {
            //Given
            var controller = GetController();
            var request = GetTestAccount();

            //When
            var response = controller.Post(request);

            //Then
            var content = response as System.Web.OData.Results.CreatedODataResult<Account>;
            Assert.IsInstanceOf(typeof(System.Web.OData.Results.CreatedODataResult<Account>), response);
            Assert.IsNotNull(content, "Item not returned");
            EqualityHelper.PropertyValuesAreEqual(content.Entity, request, new[] { "AccountID", "LastModified", "ModifiedBy" });
        }

        //Given:  I have a Account id
        //When:   I delete it 
        //Then:   I should not be able to retrieve it
        [Test, Rollback]
        public void DeleteAccountTest()
        {
            //Given
            var testItem = CreateAndRetrieveTestItem();

            //When
            var response = GetController().Get(testItem.AccountID);

            //Then
            EqualityHelper.PropertyValuesAreEqual(response.Queryable.First(), testItem, new []{"LastModified","LastModifiedBy","Contacts"});
        }

        private Account CreateAndRetrieveTestItem()
        {
            var request = GetTestAccount();
            var createTestItemResponse = GetController().Post(request);
            return APIControllerTestHelper.GetContent<Account>(createTestItemResponse);
        }

        //Given:  I have a Account id that does not exist
        //When:   I retrieve it by id
        //Then:   I should receive a null response
        [Test]
        public void GetAccountByIdButDoesNotExistTest()
        {

            //Given
            var controller = GetController();

            //When
            var response = controller.Get(-1);

            //Then
            Assert.IsFalse(response.Queryable.Any(), "The item was found when it should not have been.");

        }

        //Given:  I have a Account id
        //When:   I retrieve it 
        //Then:   it should match the object retrieved from the get all
        [Test, Rollback]
        public void GetAccountByIdTest()
        {

            //Given
            var testItem = CreateAndRetrieveTestItem();

            //When
            var response = GetController().Get().First(account => account.AccountID == testItem.AccountID);

            //Then
            EqualityHelper.PropertyValuesAreEqual(response, testItem, new[] { "LastModified", "LastModifiedBy", "Contacts" });
        }

        //Given:  I have a Account
        //When:   I update
        //Then:   it should have the new value saved
        [Test, Rollback]
        public void UpdateAccountTest()
        {
            //Given
            var testItem = CreateAndRetrieveTestItem();
            testItem.AccountReference = $"{DateTime.Now.ToFileTime()}updatedTest";

            //When
            var response = GetController().Post(testItem);

            //Then
            var updatedItem = APIControllerTestHelper.GetContent<Account>(response);
            EqualityHelper.PropertyValuesAreEqual(updatedItem, testItem);
        }

        private static Account GetTestAccount()
        {
            return new Account
            {
                AccountReference = "Test Reference",
                Address = "Test Address",
                CompanyName = "Company name"
            };
        }
    }
}