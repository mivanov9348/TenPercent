namespace TenPercent.Data
{
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

        // --- СВЯТ И ОТБОРИ ---
        public DbSet<Club> Clubs { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<WorldState> WorldStates { get; set; }

        // --- СЕЗОНИ, МАЧОВЕ И КЛАСИРАНИЯ ---
        public DbSet<Season> Seasons { get; set; }
        public DbSet<Fixture> Fixtures { get; set; }
        public DbSet<LeagueStanding> LeagueStandings { get; set; } // Живо класиране
        public DbSet<SeasonStanding> SeasonStandings { get; set; } // Архив класиране

        // --- СТАТИСТИКИ ---
        public DbSet<PlayerMatchPerformance> PlayerMatchPerformances { get; set; } // Текущ мач
        public DbSet<PlayerSeasonPerformance> PlayerSeasonStats { get; set; } // Архив сезон

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

            // --- ВРЪЗКА ИГРАЧ <-> АТРИБУТИ ---
            modelBuilder.Entity<Player>()
                .HasOne(p => p.Attributes)
                .WithOne(pa => pa.Player)
                .HasForeignKey<PlayerAttributes>(pa => pa.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- ВРЪЗКИ: МАЧОВЕ И СЕЗОНИ ---
            modelBuilder.Entity<Fixture>()
                .HasOne(f => f.Season)
                .WithMany(s => s.Fixtures)
                .HasForeignKey(f => f.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

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

            // --- ВРЪЗКИ: ПРЕДСТАВЯНЕ В МАЧ (MATCH PERFORMANCE) ---
            modelBuilder.Entity<PlayerMatchPerformance>()
                .HasOne(pmp => pmp.Fixture)
                .WithMany(f => f.Performances)
                .HasForeignKey(pmp => pmp.FixtureId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlayerMatchPerformance>()
                .HasOne(pmp => pmp.Player)
                .WithMany()
                .HasForeignKey(pmp => pmp.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- ЖИВО КЛАСИРАНЕ (LeagueStanding) ---
            modelBuilder.Entity<LeagueStanding>()
                .HasOne(ls => ls.League)
                .WithMany(l => l.LiveStandings)
                .HasForeignKey(ls => ls.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LeagueStanding>()
                .HasOne(ls => ls.Club)
                .WithOne(c => c.CurrentStanding)
                .HasForeignKey<LeagueStanding>(ls => ls.ClubId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- ИСТОРИЧЕСКИ ВРЪЗКИ (SeasonStanding и Архив Статистики) ---
            modelBuilder.Entity<SeasonStanding>()
                .HasOne(ss => ss.Season)
                .WithMany(s => s.Standings)
                .HasForeignKey(ss => ss.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SeasonStanding>()
                .HasOne(ss => ss.League)
                .WithMany()
                .HasForeignKey(ss => ss.LeagueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SeasonStanding>()
                .HasOne(ss => ss.Club)
                .WithMany()
                .HasForeignKey(ss => ss.ClubId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlayerSeasonPerformance>()
                .HasOne(pss => pss.Season)
                .WithMany(s => s.PlayerStats)
                .HasForeignKey(pss => pss.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlayerSeasonPerformance>()
                .HasOne(pss => pss.Player)
                .WithMany()
                .HasForeignKey(pss => pss.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlayerSeasonPerformance>()
                .HasOne(pss => pss.Club)
                .WithMany()
                .HasForeignKey(pss => pss.ClubId)
                .OnDelete(DeleteBehavior.SetNull);

            // --- DECIMAL ПРЕЦИЗНОСТ ---
            modelBuilder.Entity<Agency>().Property(a => a.Budget).HasPrecision(18, 2);
            modelBuilder.Entity<Club>().Property(c => c.TransferBudget).HasPrecision(18, 2);
            modelBuilder.Entity<Club>().Property(c => c.WageBudget).HasPrecision(18, 2);
            modelBuilder.Entity<Player>().Property(p => p.MarketValue).HasPrecision(18, 2);
            modelBuilder.Entity<Player>().Property(p => p.WeeklyWage).HasPrecision(18, 2);
            modelBuilder.Entity<PlayerMatchPerformance>().Property(pmp => pmp.MatchRating).HasPrecision(4, 1);
            modelBuilder.Entity<PlayerSeasonPerformance>().Property(pss => pss.AverageRating).HasPrecision(4, 1);
        }
    }
}