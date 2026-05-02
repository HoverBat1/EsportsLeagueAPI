// TeamsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EsportsLeagueApi01.Data;
using EsportsLeagueApi01.Models;
using EsportsLeagueApi01.DTOs;

namespace EsportsLeagueApi01.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly LeagueDbContext _db;

    public TeamsController(LeagueDbContext db)
    {
        _db = db;
    }

    // Maps a Team model to a TeamResponse DTO
    private static TeamResponse ToResponse(Team team) => new()
    {
        Id = team.Id, 
        Name = team.Name, 
        Region = team.Region, 
        IsActive = team.IsActive, 
        CreatedAt = team.CreatedAt, 
        Players = team.Players.Select(p => new PlayerResponse
        {
            Id = p.Id, 
            Username = p.Username, 
            Role = p.Role, 
            Skill = p.Skill, 
            IsActive = p.IsActive, 
            TeamId = p.TeamId, 
            TeamName = team.Name
        })
        .ToList()
    };

    private TeamStatsResponse ToStatsResponse(TeamStats stats) => new()
    {
        Id = stats.TeamId, 
        Name = stats.TeamName, 
        Region = stats.Region, 
        IsActive = stats.IsActive, 
        MatchesPlayed = stats.Played, 
        Wins = stats.Wins, 
        Losses = stats.Losses, 
        Ties = stats.Ties, 
        Goals = stats.Goals, 
        WinPercent =  stats.WinPercent
    };

    // private TeamStatsResponse ToStatsResponse2(Tuple<Team, TeamStats> tuple) => new()
    // {
    //     Id = tuple.Item1.Id, 
    //     Name = tuple.Item1.Name, 
    //     Region = tuple.Item1.Region, 
    //     IsActive = tuple.Item1.IsActive, 
    //     MatchesPlayed = tuple.Item2.Played, 
    //     Wins = tuple.Item2.Wins, 
    //     Losses = tuple.Item2.Losses, 
    //     Ties = tuple.Item2.Ties, 
    //     Goals = tuple.Item2.Goals, 
    //     WinPercent =  tuple.Item2.WinPercent
    // };




    // Get all teams
    // GET api/teams
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var teams = await _db.Teams.Include(t => t.Players).ToArrayAsync();
        if (teams.Length == 0) return Ok("No teams found");
        
        return Ok(teams.Select(ToResponse));
    }

    // Get a team. Returns team info and each of its player's info
    // GET api/teams/{Team.Id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var team = await _db.Teams.Include(t => t.Players).FirstOrDefaultAsync(t => t.Id == id);
        if (team is null) return NotFound($"Team {id} not found");

        return Ok(ToResponse(team));
    }

    // Get every team's stats
    // GET api/teams/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var teams = await _db.Teams.ToArrayAsync();
        if (teams.Length == 0) return Ok("No teams found");
        var matches = await _db.Matches.ToArrayAsync();

        TeamStatsResponse[] responses = new TeamStatsResponse[teams.Length];
        for (var i = 0; i < teams.Length; i++)
        {
            responses[i] = ToStatsResponse(TeamStats.GetStats(teams[i], matches));
        }

        return Ok(responses);
    }

    // Get a team's stats
    // GET api/teams/{Team.Id}/stats
    [HttpGet("{id}/stats")]
    public async Task<IActionResult> GetStatsById(int id)
    {
        var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == id);
        if (team is null) return NotFound($"Team {id} not found");
        var matches = await _db.Matches.ToArrayAsync();

        return Ok(ToStatsResponse(TeamStats.GetStats(team, matches)));
    }

    // Get leaderboard
    // GET api/teams/leaderboard
    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard()
    {
        var teams = await _db.Teams.ToArrayAsync();
        if (teams.Length == 0) return Ok("No teams found");
        var matches = await _db.Matches.ToArrayAsync();

        List<TeamStats> teamsStats = [];
        foreach (var team in teams) teamsStats.Add(TeamStats.GetStats(team, matches));
        teamsStats = teamsStats
            .OrderByDescending(s => s.Wins)
            .ThenBy(s => s.WinPercent)
            .ThenBy(s => s.Ties)
            .ThenBy(s => s.Goals)
            .ToList();
        //teamsStats.Sort(new TeamWinsComparer());

        int rank = 0;
        int wins = 0;
        double winPercent = 0.0;
        int ties = 0;
        int goals = 0;
        List<LeaderboardResponse> leaderboard = [];
        foreach (var stats in teamsStats)
        {
            if (rank <= 0 
            ||  stats.Wins != wins 
            ||  stats.WinPercent != winPercent 
            ||  stats.Ties != ties 
            ||  stats.Goals != goals )
            {
                rank++;
            }
            wins = stats.Wins;
            winPercent = stats.WinPercent;
            ties = stats.Ties;
            goals = stats. Goals;

            leaderboard.Add(new()
            {
                Rank = rank, 
                Id = stats.TeamId, 
                Name = stats.TeamName, 
                Region = stats.Region, 
                Matches = stats.Played, 
                Wins = stats.Wins, 
                Losses = stats.Losses, 
                Ties = stats.Ties, 
                Goals = stats.Goals, 
                WinPercent = stats.WinPercent
            });
        }

        return Ok(leaderboard);
    }

    // Get a team's history
    // GET api/teams/{Team.Id}/history
    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == id);
        if (team is null) return NotFound($"Team {id} not found");

        var matchesPlayed = await _db.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Where(m 
                => (m.HomeTeamId == team.Id || m.AwayTeamId == team.Id) 
                && m.Status == League.Status.Completed)
            .OrderBy(m => m.ScheduledAt)
            .ToArrayAsync();
        if (matchesPlayed.Length == 0) return Ok($"Team {id} hasn't played any matches");

        List<TeamHistoryMatch> matchHistory = [];
        foreach (var match in matchesPlayed)
        {
            int goals = match.HomeTeamId == id ? match.HomeScore : match.AwayScore;
            int opponentGoals = match.HomeTeamId == id ? match.AwayScore : match.HomeScore;
            string result = "Tied";
            if (goals != opponentGoals) result = goals > opponentGoals ? "Win" : "Lose";
            matchHistory.Add(new()
            {
                MatchId = match.Id, 
                Opponent = match.HomeTeamId == id ? match.AwayTeam.Name : match.HomeTeam.Name, 
                HomeOrAway = match.HomeTeamId == id ? "Home" : "Away", 
                Goals = goals, 
                OpponentGoals = opponentGoals, 
                Result = result, 
                PlayedAt = match.StartedAt ?? match.ScheduledAt
            });
        }

        return Ok(new TeamHistoryResponse()
        {
            Id = team.Id, 
            Name = team.Name, 
            Region = team.Region, 
            IsActive = team.IsActive, 
            MatchHistory = matchHistory
        });
    }




    // Change all team properties
    // PUT api/teams/{Team.Id}
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateTeamRequest request)
    {
        if (!League.Regions.All.Contains(request.Region))
        {
            return BadRequest(League.Regions.InvalidError);
        }
        
        var team = await _db.Teams
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (team is null) return NotFound($"Team {id} not found");

        team.Name = request.Name;
        team.Region = request.Region;
        team.IsActive = request.IsActive;
        await _db.SaveChangesAsync();

        return Ok(ToResponse(team));
    }

    // Change specific team properties
    // PATCH api/teams/{Team.Id}
    [Authorize]
    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, PatchTeamRequest request)
    {
        var team = await _db.Teams
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (team is null) return NotFound($"Team {id} not found");

        if (request.Name is not null)
        {
            if (request.Name.Length < League.Team.NameLengthMin 
            ||  request.Name.Length > League.Team.NameLengthMax )
            {
                return BadRequest(League.Team.NameLengthRangeError);
            }
            
            team.Name = request.Name;
        }

        if (request.Region is not null)
        {
            if (!League.Regions.All.Contains(request.Region))
            {
                return BadRequest(League.Regions.InvalidError);
            }
            
            team.Region = request.Region;
        }

        if (request.IsActive is not null)
        {
            team.IsActive = request.IsActive.Value;
        }

        await _db.SaveChangesAsync();

        return Ok(ToResponse(team));
    }

    // Create a team
    // POST api/teams
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreateTeamRequest request)
    {
        // Custom validation - region must be a known value
        if (!League.Regions.All.Contains(request.Region))
        {
            return BadRequest(League.Regions.InvalidError);
        }

        var team = new Team
        {
            Name = request.Name, 
            Region = request.Region
            //IsActive = request.IsActive
        };

        _db.Teams.Add(team);
        await _db.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetById), new { id = team.Id }, ToResponse(team));
    }

    // Delete a team
    // DELETE api/teams/{Team.Id}
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var team = await _db.Teams.FindAsync(id);
        if (team is null) return NotFound($"Team {id} not found");

        _db.Teams.Remove(team);
        await _db.SaveChangesAsync();
        
        return NoContent();
    }
}




// public class TeamWinsComparer(bool descending = true) : IComparer<TeamStats>
// {
//     private readonly bool _descending = descending;

//     public int Compare(TeamStats? t1, TeamStats? t2)
//     {
//         if (t1 is null) return t2 is null ? 0 : -1;
//         if (t2 is null) return 1;
//         int winsComparison = _descending ? t2.Wins.CompareTo(t1.Wins) : t1.Wins.CompareTo(t2.Wins);
//         if (winsComparison == 0)
//         {
//             int winPercentComparison = _descending ? t2.WinPercent.CompareTo(t1.WinPercent) : t1.WinPercent.CompareTo(t2.WinPercent);
//             if (winPercentComparison == 0)
//             {
//                 int tiesComparison = t1.Ties.CompareTo(t2.Ties);
//                 if (tiesComparison == 0)
//                 {
//                     int goalsComparison = _descending ? t2.Goals.CompareTo(t1.Goals) : t1.Goals.CompareTo(t2.Goals);
//                     return goalsComparison == 0 ? t1.TeamName.CompareTo(t2.TeamName) : goalsComparison;
//                 }

//                 return tiesComparison;
//             }

//             return winPercentComparison;
//         }
        
//         return winsComparison;
//     }
// }