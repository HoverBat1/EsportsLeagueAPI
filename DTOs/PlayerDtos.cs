// PlayerDtos.cs

using System.ComponentModel.DataAnnotations;

namespace EsportsLeagueApi01.DTOs;

// What we send back
public class PlayerResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int Skill { get; set; }
    public bool IsActive { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
}

// What the client sends when creating a player
public class CreatePlayerRequest
{
    [Required(ErrorMessage = "Username is required")]
    [MinLength(League.Player.UsernameLengthMin)]
    [MaxLength(League.Player.UsernameLengthMax)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = string.Empty;

    [Range(League.Player.SkillMin, League.Player.SkillMax)]
    //[Range(1, 100, ErrorMessage = "Skill must be between 1 and 100")]
    public int Skill { get; set; }
}

public class UpdatePlayerRequest
{
    [Required(ErrorMessage = "Username is required")]
    [MinLength(League.Player.UsernameLengthMin)]
    [MaxLength(League.Player.UsernameLengthMax)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = string.Empty;

    [Range(League.Player.SkillMin, League.Player.SkillMax)]
    public int Skill { get; set; }

    public bool IsActive { get; set; }
}

public class PatchPlayerRequest
{
    //[Required(ErrorMessage = "Username is required")]
    //[MinLength(League.Player.UsernameLengthMin)]
    //[MaxLength(League.Player.UsernameLengthMax)]
    public string? Username { get; set; }

    //[Required(ErrorMessage = "Role is required")]
    public string? Role { get; set; }

    //[Range(League.Player.SkillMin, League.Player.SkillMax)]
    public int? Skill { get; set; }

    public bool? IsActive { get; set; }
}

public class TopPlayerResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int Skill { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
}