using System;
using System.Threading.Tasks;

namespace Shared.Helper
{
    public static class DelayedExecutionHelper
    {
        public static void Execute(Action action, int timeoutInSeconds)
        {
            var milliSeconds = 1000*timeoutInSeconds;
            Task.Delay(milliSeconds).ContinueWith(t => action());
        }
    }
}
