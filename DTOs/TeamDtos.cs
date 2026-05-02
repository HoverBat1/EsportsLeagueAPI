// TeamDtos.cs

using System.ComponentModel.DataAnnotations;
using EsportsLeagueApi01.Models;

namespace EsportsLeagueApi01.DTOs;

// What we send back
public class TeamResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PlayerResponse> Players { get; set; } = new();
}


// What the client sends when creating a team
public class CreateTeamRequest
{
    [Required(ErrorMessage = "Team name is required")]
    [MinLength(League.Team.NameLengthMin, ErrorMessage = League.Team.NameLengthRangeError)]
    [MaxLength(League.Team.NameLengthMax, ErrorMessage = League.Team.NameLengthRangeError)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Region is required")]
    public string Region { get; set; } = string.Empty;

    // public bool IsActive { get; set; }
}

public class UpdateTeamRequest
{
    [Required(ErrorMessage = "Team name is required")]
    [MinLength(League.Team.NameLengthMin, ErrorMessage = League.Team.NameLengthRangeError)]
    [MaxLength(League.Team.NameLengthMax, ErrorMessage = League.Team.NameLengthRangeError)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Region is required")]
    public string Region { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}

public class PatchTeamRequest
{
    // [Required(ErrorMessage = "Team name is required")]
    // [MinLength(League.Team.NameLengthMin, ErrorMessage = League.Team.NameLengthRangeError)]
    // [MaxLength(League.Team.NameLengthMax, ErrorMessage = League.Team.NameLengthRangeError)]
    public string? Name { get; set; }

    // [Required(ErrorMessage = "Region is required")]
    public string? Region { get; set; }

    public bool? IsActive { get; set; }
}

public class TeamStatsResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Ties { get; set; }
    public int Goals { get; set; }
    public double WinPercent { get; set; }
}

public class LeaderboardResponse
{
    public int Rank { get; set; }
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public int Matches { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Ties { get; set; }
    public int Goals { get; set; }
    public double WinPercent { get; set; }
}

public class TeamHistoryMatch
{
    public int MatchId { get; set; }
    public string Opponent { get; set; } = string.Empty;
    public string HomeOrAway { get; set; } = string.Empty;
    public int Goals { get; set; }
    public int OpponentGoals { get; set; }
    public string Result { get; set; } = string.Empty;
    public DateTime PlayedAt { get; set; }
}

public class TeamHistoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<TeamHistoryMatch> MatchHistory { get; set; } = new();
}




public class TeamStats
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Played { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Ties { get; set; }
    public int Goals { get; set; }
    public double WinPercent { get; set; }

    public static TeamStats GetStats(Team team, Match[] matches)
    {
        var playedMatches = matches
            .Where(m => m.Status == League.Status.Completed)
            .Where(m => m.HomeTeamId == team.Id || m.AwayTeamId == team.Id)
            .ToArray();
        
        var wonMatches = playedMatches
            .Where(m 
                => (m.HomeTeamId == team.Id && m.HomeScore > m.AwayScore) 
                || (m.AwayTeamId == team.Id && m.AwayScore > m.HomeScore) )
            .ToArray();
        
        int played = playedMatches.Length;
        int wins = wonMatches.Length;
        int ties = playedMatches.Count(m => m.HomeScore == m.AwayScore);
        int losses = played - wins - ties;
        int goals 
            = playedMatches.Where(m => m.HomeTeamId == team.Id).Sum(m => m.HomeScore) 
            + playedMatches.Where(m => m.AwayTeamId == team.Id).Sum(m => m.AwayScore);
        double winPercent = played == 0 ? 0.0 : (double)wins / (double)played * 100.0;

        return new TeamStats()
        {
            TeamId = team.Id, 
            TeamName = team.Name, 
            Region = team.Region, 
            IsActive = team.IsActive, 
            Played = played, 
            Wins = wins, 
            Losses = losses, 
            Ties = ties, 
            Goals = goals, 
            WinPercent = winPercent
        };
    }
}