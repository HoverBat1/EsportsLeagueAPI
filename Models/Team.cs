// Team.cs

namespace EsportsLeagueApi01.Models;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Navigation property - EF will populate this
    public List<Player> Players { get; set; } = new();
}