using Finbuckle.MultiTenant;

namespace OeniddictProvider.Data
{
    public class UserTenantDbContext : MultiTenantDbContext
    {
        private readonly ITenantInfo tenantInfo;

        public UserTenantDbContext(ITenantInfo tenantInfo) : base(tenantInfo)
        {
            this.tenantInfo = tenantInfo;
        }

        public UserTenantDbContext(ITenantInfo tenantInfo, DbContextOptions options) : base(tenantInfo, options)
        {
            this.tenantInfo = tenantInfo;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(tenantInfo.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>();
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> User { get; set; }
    }
}
