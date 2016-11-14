using System;
using System.Transactions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Shared.Helper.Test
{
    /// <summary>
    /// Rollback Attribute wraps test execution into a transaction and cancels the transaction once the test is finished.
    /// You can use this attribute on single test methods or test classes/suites
    /// </summary>
    public class RollbackAttribute : Attribute, ITestAction
    {
        private TransactionScope _scope;

        public void BeforeTest(ITest test)
        {
            _scope = new TransactionScope();
        }

        public void AfterTest(ITest test)
        {
            _scope.Dispose();
        }

        public ActionTargets Targets => ActionTargets.Test;
    }
}

