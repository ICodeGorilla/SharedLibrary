using System.Net.Http;
using System.Threading;

namespace Shared.Helper.Test
{
    public static class IHttpActionResult
    {
        public static HttpResponseMessage GetHttpResponseMessage(this System.Web.Http.IHttpActionResult message)
        {
            return message.ExecuteAsync(CancellationToken.None).Result;
        }
    }
}