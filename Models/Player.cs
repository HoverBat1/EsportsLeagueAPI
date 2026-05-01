// Player.cs

namespace EsportsLeagueApi01.Models;

public class Player
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int Skill { get; set; }
    public bool IsActive { get; set; } = true;

    // Foreign key - links player to a team
    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;
}