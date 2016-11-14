using System.Data.Entity;

namespace Tests.Database
{
    public class FakeDbContext : DbContext
    {
        public virtual DbSet<TestBasicGuid> GuidEntities { get; set; }
    }
}