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
    public class ODataCompositeIntegrationTests
    {
        //Get a new controller for each call so that the transactions are seperate
        private CompositeController GetController()
        {
            return new CompositeController(UserHelper.CreateTestCurrentUser(), new NLogLogger());
        }

        //Given:  I do not have all the information needed to create a CompositeKeyTest item
        //When:   I save it 
        //Then:   I should receive an exception
        [Test]
        public void CreateCompositeFailureTest()
        {
            //Given
            var controller = GetController();
            var request = new CompositeKeyTest();

            //When
            TestDelegate response = () => controller.Post(request);

            //Then
            Assert.Throws<DbEntityValidationException>(response);
        }

        //Given:  I have all the information needed to create a composite item
        //When:   I save it 
        //Then:   I should be able to return it
        [Test, Rollback]
        public void CreateCompositeSuccessfullyTest()
        {
            //Given
            var controller = GetController();
            var request = GetTestObject();

            //When
            var response = controller.Post(request);

            //Then
            var content = response as System.Web.OData.Results.CreatedODataResult<CompositeKeyTest>;
            Assert.IsInstanceOf(typeof(System.Web.OData.Results.CreatedODataResult<CompositeKeyTest>), response);
            Assert.IsNotNull(content, "Item not returned");
            EqualityHelper.PropertyValuesAreEqual(content.Entity, request, new[] {"LastModified", "ModifiedBy" });
        }

        //Given:  I have a composite item first and second key
        //When:   I delete it 
        //Then:   I should not be able to retrieve it
        [Test]
        public void DeleteCompositeItemTest()
        {
            //Given
            var testItem = CreateAndRetrieveTestItem();

            //When
            GetController().Delete(testItem.FirstKey, testItem.SecondKey);

            //Then
            Assert.IsNull(GetController().Get(testItem.FirstKey, testItem.SecondKey).Queryable.FirstOrDefault(), "The item was found when it should not have been.");            
        }

        private CompositeKeyTest CreateAndRetrieveTestItem()
        {
            var request = GetTestObject();
            var createTestItemResponse = GetController().Post(request);
            return APIControllerTestHelper.GetContent<CompositeKeyTest>(createTestItemResponse);
        }

        //Given:  I have a CompositeKeyTest id that does not exist
        //When:   I retrieve it by id
        //Then:   I should receive a null response
        [Test]
        public void GetAccountByIdButDoesNotExistTest()
        {
            //Given
            var controller = GetController();

            //When
            var response = controller.Get(-1, "Words");

            //Then
            Assert.IsFalse(response.Queryable.Any(), "The item was found when it should not have been.");
        }

        //Given:  I have a CompositeKeyTest id
        //When:   I retrieve it 
        //Then:   it should match the object retrieved from the get all
        [Test, Rollback]
        public void GetAccountByIdTest()
        {

            //Given
            var compositeKeyTest = CreateAndRetrieveTestItem();

            //When
            var response = GetController().Get().First(keyTest => keyTest.FirstKey == compositeKeyTest.FirstKey && keyTest.SecondKey == compositeKeyTest.SecondKey);

            //Then
            EqualityHelper.PropertyValuesAreEqual(response, compositeKeyTest, new[] { "LastModified", "LastModifiedBy" });
        }

        //Given:  I have a CompositeKeyTest
        //When:   I update
        //Then:   it should have the new value saved
        [Test, Rollback]
        public void UpdateAccountTest()
        {
            //Given
            var testItem = CreateAndRetrieveTestItem();
            testItem.Value = $"{DateTime.Now.ToFileTime()}updatedTest";

            //When
            var response = GetController().Post(testItem);

            //Then
            var updatedItem = APIControllerTestHelper.GetContent<CompositeKeyTest>(response);
            EqualityHelper.PropertyValuesAreEqual(updatedItem, testItem);
        }

        private static CompositeKeyTest GetTestObject()
        {
            return new CompositeKeyTest
            {
                FirstKey = 0,
                SecondKey = "0",
                Value = "Test"
            };
        }
    }
}