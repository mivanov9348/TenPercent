namespace TenPercent.Data
{
    using Microsoft.EntityFrameworkCore;
    using TenPercent.Data.Models;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Всички таблици (DbSets)
        public DbSet<User> Users { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<Fixture> Fixtures { get; set; }
        public DbSet<PlayerPerformance> PlayerPerformances { get; set; }
        public DbSet<WorldState> WorldStates { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- ОСНОВНИ ВРЪЗКИ ---

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
                .HasMany(a => a.Players)
                .WithOne(p => p.Agency)
                .HasForeignKey(p => p.AgencyId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Club>()
                .HasMany(c => c.Players)
                .WithOne(p => p.Club)
                .HasForeignKey(p => p.ClubId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<League>()
                .HasMany(l => l.Clubs)
                .WithOne(c => c.League)
                .HasForeignKey(c => c.LeagueId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- СЛОЖНИ ВРЪЗКИ (МАЧОВЕ И ОЦЕНКИ) ---

            // Мачът има Домакин и Гост. SQL Server мрази, когато два ключа сочат към една таблица (Multiple Cascade Paths).
            // Затова изрично казваме да НЕ трие каскадно!
            modelBuilder.Entity<Fixture>()
                .HasOne(f => f.HomeClub)
                .WithMany()
                .HasForeignKey(f => f.HomeClubId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Fixture>()
                .HasOne(f => f.AwayClub)
                .WithMany()
                .HasForeignKey(f => f.AwayClubId)
                .OnDelete(DeleteBehavior.Restrict);

            // Връзка Мач -> Оценки на играчите
            modelBuilder.Entity<PlayerPerformance>()
                .HasOne(pp => pp.Fixture)
                .WithMany(f => f.Performances)
                .HasForeignKey(pp => pp.FixtureId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlayerPerformance>()
                .HasOne(pp => pp.Player)
                .WithMany()
                .HasForeignKey(pp => pp.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
           

            // --- DECIMAL ПРЕЦИЗНОСТ (За пари и рейтинги) ---
            modelBuilder.Entity<Agency>().Property(a => a.Budget).HasPrecision(18, 2);
            modelBuilder.Entity<Club>().Property(c => c.TransferBudget).HasPrecision(18, 2);
            modelBuilder.Entity<Club>().Property(c => c.WageBudget).HasPrecision(18, 2);
            modelBuilder.Entity<Player>().Property(p => p.MarketValue).HasPrecision(18, 2);
            modelBuilder.Entity<Player>().Property(p => p.WeeklyWage).HasPrecision(18, 2);

            modelBuilder.Entity<PlayerPerformance>().Property(pp => pp.MatchRating).HasPrecision(4, 1); // напр. 8.5

            
        }
    }
}