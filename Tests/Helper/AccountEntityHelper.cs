using System;
using System.Collections.Generic;
using Tests.Database;

namespace Tests.Helper
{
    static class AccountEntityHelper
    {
        public static Account CreateTestAccount()
        {
            return new Account { CompanyName = "Test Company Name", AccountReference = "Test Account Reference", Address = "Test Address", LastModified = DateTime.Now, LastModifiedBy = "UnitOfWorkShouldSaveItemsTest" };
        }

        public static Database.EFCore.Account CreateEfTestAccount()
        {
            return new Database.EFCore.Account { CompanyName = "Test Company Name", AccountReference = "Test Account Reference", Address = "Test Address", LastModified = DateTime.Now, LastModifiedBy = "UnitOfWorkShouldSaveItemsTest" };
        }


        public static List<Account> CreateTestAccounts(int numberOfTestAccounts)
        {
            var result = new List<Account>();
            var count = 0;
            while (count < numberOfTestAccounts)
            {
                result.Add(CreateTestAccount());
                count++;
            }
            return result;
        }

        public static List<Database.EFCore.Account> CreateEfCoreTestAccounts(int numberOfTestAccounts)
        {
            var result = new List<Database.EFCore.Account>();
            var count = 0;
            while (count < numberOfTestAccounts)
            {
                result.Add(CreateEfTestAccount());
                count++;
            }
            return result;
        }
    }
}
