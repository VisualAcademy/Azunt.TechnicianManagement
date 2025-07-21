using Microsoft.EntityFrameworkCore;

namespace Azunt.TechnicianManagement
{
    public class TechnicianAppDbContext : DbContext
    {
        public TechnicianAppDbContext(DbContextOptions<TechnicianAppDbContext> options)
            : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Technician>()
                .Property(m => m.Created)
                .HasDefaultValueSql("GetDate()");
        }

        public DbSet<Technician> Technicians { get; set; } = null!;
    }
}