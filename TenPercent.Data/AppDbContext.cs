namespace TenPercent.Data
{
    using Microsoft.EntityFrameworkCore;
    using TenPercent.Data.Models;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<Player> Players { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Agent)
                .WithOne(a => a.User)
                .HasForeignKey<Agent>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Agent>()
                .HasOne(a => a.Agency)
                .WithOne(ag => ag.Agent)
                .HasForeignKey<Agency>(ag => ag.AgentId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Agency>()
                .Property(a => a.Budget)
                .HasPrecision(18, 2);
        }
    }
}