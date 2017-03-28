using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Tests.Database.EFCore
{
    public class SharedLibraryContext : DbContext
    {
        private readonly string _nameOrConnectionString;
        public SharedLibraryContext(string nameOrConnectionString = "SharedCommonDatabaseContext")
        {
            _nameOrConnectionString = nameOrConnectionString;
        }

        public SharedLibraryContext()
        {
            _nameOrConnectionString = "SharedCommonDatabaseContext";
        }

        public virtual DbSet<Account> Account { get; set; }
        public virtual DbSet<CompositeKeyTest> CompositeKeyTest { get; set; }
        public virtual DbSet<Contact> Contact { get; set; }
        public virtual DbSet<MigrationHistory> MigrationHistory { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=WIN-DTLAIS5TR8U\LOCALHOST;Integrated Security=True;Initial Catalog=SharedLibrary;MultipleActiveResultSets=True;App=EntityFramework");
            optionsBuilder.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompositeKeyTest>(entity =>
            {
                entity.HasKey(e => new { e.FirstKey, e.SecondKey })
                    .HasName("PK_dbo.CompositeKeyTest");
            });

            modelBuilder.Entity<Contact>(entity =>
            {
                entity.HasIndex(e => e.AccountId)
                    .HasName("IX_AccountID");
            });

            modelBuilder.Entity<MigrationHistory>(entity =>
            {
                entity.HasKey(e => new { e.MigrationId, e.ContextKey })
                    .HasName("PK_dbo.__MigrationHistory");
            });
        }
    }
}
