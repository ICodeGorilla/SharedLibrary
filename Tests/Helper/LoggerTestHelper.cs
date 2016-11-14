using System;

namespace Tests.Helper
{
    internal static class LoggerTestHelper
    {
        public static void RunForcedExceptionWithMessage(Action<string, Exception> loggingAction)
        {
            try
            {
                WillThrowException();
            }
            catch (Exception exception)
            {
                loggingAction("Error exception message", exception);
            }
        }

        public static void RunForcedException(Action<Exception> loggingAction)
        {
            try
            {
                WillThrowException();
            }
            catch (Exception exception)
            {
                loggingAction(exception);
            }
        }

        public static void WillThrowException()
        {
            var zero = 0;
            // ReSharper disable once UnusedVariable
            var impossibleNumber = 100 / zero;
        }
    }
}
