using System.Data.Entity;

namespace Tests.Database
{
    public partial class SharedCommonDatabaseContext : DbContext
    {
        public SharedCommonDatabaseContext()
            : base("name=SharedCommonDatabaseContext")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public SharedCommonDatabaseContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            Configuration.LazyLoadingEnabled = false;
        }
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<CompositeKeyTest> CompositeKeyTests { get; set; }
        public virtual DbSet<Contact> Contacts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                .Property(e => e.CompanyName)
                .IsUnicode(false);

            modelBuilder.Entity<Account>()
                .Property(e => e.Address)
                .IsUnicode(false);

            modelBuilder.Entity<Account>()
                .Property(e => e.LastModifiedBy)
                .IsUnicode(false);

            modelBuilder.Entity<Account>()
                .Property(e => e.AccountReference)
                .IsUnicode(false);

            modelBuilder.Entity<Account>()
                .HasMany(e => e.Contacts)
                .WithRequired(e => e.Account)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CompositeKeyTest>()
                .Property(e => e.Value)
                .IsUnicode(false);

            modelBuilder.Entity<CompositeKeyTest>()
                .Property(e => e.LastModifiedBy)
                .IsUnicode(false);

            modelBuilder.Entity<Contact>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Contact>()
                .Property(e => e.EmailAddress)
                .IsUnicode(false);

            modelBuilder.Entity<Contact>()
                .Property(e => e.LastModifiedBy)
                .IsUnicode(false);
        }
    }
}
