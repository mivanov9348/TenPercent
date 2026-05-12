namespace TenPercent.Data
{
    using System.Reflection;
    using Microsoft.EntityFrameworkCore;
    using TenPercent.Data.Models;
    using TenPercent.Data.Models.Finance;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

    // Казваме на Identity да използва нашия User, базов IdentityRole и int за ключове
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // --- ХОРА И АГЕНЦИИ ---
        // ПРЕМАХНАТО: DbSet<User> GameUsers; (Вече се ползва вграденото _context.Users)
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
        public DbSet<LeagueStanding> LeagueStandings { get; set; }
        public DbSet<SeasonStanding> SeasonStandings { get; set; }

        public DbSet<PlayerMatchPerformance> PlayerMatchPerformances { get; set; }
        public DbSet<PlayerSeasonPerformance> PlayerSeasonStats { get; set; }

        public DbSet<ClubContract> ClubContracts { get; set; }
        public DbSet<RepresentationContract> RepresentationContracts { get; set; }

        public DbSet<Bank> Banks { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<EconomySettings> EconomySettings { get; set; }

        public DbSet<AgencyShortlist> AgencyShortlists { get; set; }
        public DbSet<ScoutReport> ScoutReports { get; set; }
        public DbSet<ScoutTemplate> ScoutTemplates { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageTemplate> MessageTemplates { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
            configurationBuilder.Properties<decimal>().HavePrecision(18, 4);
        }
    }
}