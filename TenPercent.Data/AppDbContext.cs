namespace TenPercent.Data
{
    using System.Reflection;
    using Microsoft.EntityFrameworkCore;
    using TenPercent.Data.Models;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // --- ХОРА И АГЕНЦИИ ---
        public DbSet<User> Users { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Agency> Agencies { get; set; }

        // --- ИГРАЧИ ---
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerAttributes> PlayerAttributes { get; set; }
        public DbSet<Position> Positions { get; set; } 

        // --- СВЯТ И ОТБОРИ ---
        public DbSet<Club> Clubs { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<WorldState> WorldStates { get; set; }

        // --- СЕЗОНИ, МАЧОВЕ И КЛАСИРАНИЯ ---
        public DbSet<Season> Seasons { get; set; }
        public DbSet<Fixture> Fixtures { get; set; }
        public DbSet<LeagueStanding> LeagueStandings { get; set; } // Живо класиране
        public DbSet<SeasonStanding> SeasonStandings { get; set; } // Архив класиране

        public DbSet<PlayerMatchPerformance> PlayerMatchPerformances { get; set; } // Текущ мач
        public DbSet<PlayerSeasonPerformance> PlayerSeasonStats { get; set; } // Архив сезон

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
