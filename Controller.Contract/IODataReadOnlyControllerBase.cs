using System.Linq;
using System.Web.Http;
using System.Web.OData;

namespace Shared.Controller.Contract
{
    public interface IODataReadOnlyControllerBase<TEntity,TKey>
    {
        IQueryable<TEntity> Get();
        SingleResult<TEntity> Get([FromODataUri] TKey key);
    }
}