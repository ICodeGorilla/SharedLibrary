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
    public static class APIControllerTestHelper
    {
        public static async Task<T> GetContent<T>(HttpResponseMessage message)
        {
            var data = await message.Content.ReadAsStringAsync();
            var jSserializer = new JavaScriptSerializer();
            return jSserializer.Deserialize<T>(data);
        }

        public static async Task<List<T>> GetContentList<T>(HttpResponseMessage message)
        {
            var data = await message.Content.ReadAsStringAsync();
            var jSserializer = new JavaScriptSerializer();
            return jSserializer.Deserialize<List<T>>(data);
        }

        public static List<T> GetAwaitedContentList<T>(HttpResponseMessage message)
        {
            var task = GetContentList<T>(message);
            return Task.WhenAll(task).Result.First();
        }

        public static T GetAwaitedContent<T>(HttpResponseMessage message)
        {
            var task = GetContent<T>(message);
            return Task.WhenAll(task).Result.First();
        }

        public static T GetContent<T>(IHttpActionResult createTestItemResponse)
        {
            var testItemContent = (System.Web.OData.Results.CreatedODataResult<T>)createTestItemResponse;
            return testItemContent.Entity;
        }

        public static List<T> GetContentList<T>(IHttpActionResult createTestItemResponse)
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
    }
}