using Microsoft.EntityFrameworkCore;
using EsportsLeagueApi01.Models;

namespace EsportsLeagueApi01.Data;

public class LeagueDbContext : DbContext
{
    public LeagueDbContext(DbContextOptions<LeagueDbContext> options) : base(options) {}

    public DbSet<Team> Teams { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Match> Matches { get; set; }
}