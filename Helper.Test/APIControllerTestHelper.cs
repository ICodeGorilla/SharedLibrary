using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Script.Serialization;
using Autofac;

namespace Shared.Helper.Test
{
    public static class ApiControllerTestHelper
    {
        public static async Task<T> GetContentAsync<T>(HttpResponseMessage message)
        {
            var data = await message.Content.ReadAsStringAsync().ConfigureAwait(true);
            var jSserializer = new JavaScriptSerializer();
            return jSserializer.Deserialize<T>(data);
        }

        public static async Task<List<T>> GetContentListAsync<T>(HttpResponseMessage message)
        {
            var data = await message.Content.ReadAsStringAsync().ConfigureAwait(true);
            var jSserializer = new JavaScriptSerializer();
            return jSserializer.Deserialize<List<T>>(data);
        }

        public static List<T> GetAwaitedContentList<T>(HttpResponseMessage message)
        {
            var task = GetContentListAsync<T>(message);
            return Task.WhenAll(task).Result.First();
        }

        public static T GetAwaitedContent<T>(HttpResponseMessage message)
        {
            var task = GetContentAsync<T>(message);
            return Task.WhenAll(task).Result.First();
        }

        public static T GetContentAsync<T>(System.Web.Http.IHttpActionResult createTestItemResponse)
        {
            var testItemContent = (System.Web.OData.Results.CreatedODataResult<T>)createTestItemResponse;
            return testItemContent.Entity;
        }

        public static List<T> GetContentListAsync<T>(System.Web.Http.IHttpActionResult createTestItemResponse)
        {
            var testItemContent = (System.Web.OData.Results.CreatedODataResult<List<T>>)createTestItemResponse;
            return testItemContent.Entity;
        }

        public static T ResolveController<T>(ILifetimeScope lifetime)
            where T:ApiController
        {
            var controller = lifetime.Resolve<T>();
            controller.Request = new HttpRequestMessage();
            controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            return controller;
        }

        
        public static T GetAwaitedContent<T>(System.Web.Http.IHttpActionResult message)
        {
            return GetAwaitedContent<T>(message.GetHttpResponseMessage());
        }

        public static List<T> GetAwaitedContentList<T>(System.Web.Http.IHttpActionResult message)
        {
            return GetAwaitedContentList<T>(message.GetHttpResponseMessage());
        }
    }
}