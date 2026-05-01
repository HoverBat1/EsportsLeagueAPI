using System.ComponentModel.DataAnnotations;

namespace EsportsLeagueApi01.DTOs;

public class MatchResponse
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string GameMode { get; set; } = string.Empty;
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public MatchTeamResponse HomeTeam { get; set; } = null!;
    public MatchTeamResponse AwayTeam { get; set; } = null!;
    public string Result { get; set; } = string.Empty;
}

// Slim team representation for match responses
public class MatchTeamResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
}


public class CreateMatchRequest
{
    [Required]
    public int HomeTeamId { get; set; }

    [Required]
    public int AwayTeamId { get; set; }

    [Required]
    public string GameMode { get; set; } = League.Modes.Standard;

    [Required]
    public DateTime ScheduledAt { get; set; }
}

public class UpdateMatchRequest
{
    [Range(0, int.MaxValue)]
    public int HomeScore { get; set; }

    [Range(0, int.MaxValue)]
    public int AwayScore { get; set; }
}