// PlayersController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EsportsLeagueApi01.Data;
using EsportsLeagueApi01.Models;
using EsportsLeagueApi01.DTOs;

namespace EsportsLeagueApi01.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly LeagueDbContext _db;

    public PlayersController(LeagueDbContext db)
    {
        _db = db;
    }

    private PlayerResponse ToResponse(Player p) => new()
    {
        Id = p.Id, 
        Username = p.Username, 
        Role = p.Role, 
        Skill = p.Skill, 
        IsActive = p.IsActive, 
        TeamId = p.TeamId, 
        TeamName = p.Team?.Name ?? string.Empty
    };


    // Get all players in the league with optional filter by IsActive state
    // GET api/players, api/players?isActive=true, api/players?isActive=false
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive)
    {
        var query = _db.Players
            .Include(p => p.Team)
            .AsQueryable();
        if (isActive is not null) query = query.Where(p => p.IsActive == isActive);

        var players = await query.ToArrayAsync();
        
        return Ok(players.Select(ToResponse));
    }

    // Get all players of a team
    // GET api/players/team{teamId}
    [HttpGet("team{teamId}")]
    public async Task<IActionResult> GetTeam(int teamId)
    {
        var players = await _db.Players
            .Include(p => p.Team)
            .Where(p => p.TeamId == teamId)
            .ToArrayAsync();
        if (players.Length == 0) return Ok("No players found");
        
        return Ok(players.Select(ToResponse));
    }

    // Get a player
    // GET api/players/{Player.Id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var player = await _db.Players
            .Include(p => p.Team)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (player is null) return NotFound();
        
        return Ok(ToResponse(player));
    }

    // Get top x players
    // GET api/players/top
    // GET api/players/top?count={x}
    [HttpGet("top")]
    public async Task<IActionResult> GetTop(int count = 10)
    {
        var players = await _db.Players
            .Include(p => p.Team)
            .ToArrayAsync();
        if (players.Length == 0) return Ok("No players found");
        count = Math.Clamp(count, 1, players.Length);
        players = players
            .OrderByDescending(p => p.Skill)
            .ThenBy(p => p.Role)
            .ThenBy(p => p.Team.Name)
            .ThenBy(p => p.Username)
            .Take(count)
            .ToArray();

        List<TopPlayerResponse> topPlayers = new(count);
        for (int i = 0; i < count; i++)
        {
            topPlayers.Add(new()
            {
                Id = players[i].Id, 
                Username = players[i].Username, 
                Role = players[i].Role, 
                Skill = players[i].Skill, 
                TeamName = players[i].Team.Name, 
                Region = players[i].Team.Region
            });
        }

        return Ok(topPlayers);
    }




    // Change value of every property of a player
    // PUT api/players/{Player.Id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdatePlayerRequest request)
    {
        if (!League.Roles.All.Contains(request.Role))
        {
            return BadRequest($"Invalid role. Must be one of: {string.Join(", ", League.Roles.All)}");
        }
        
        var player = await _db.Players
            .Include(p => p.Team)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (player is null) return NotFound();

        player.Username = request.Username;
        player.Role = request.Role;
        player.Skill = Math.Clamp(request.Skill, League.Player.SkillMin, League.Player.SkillMax);
        player.IsActive = request.IsActive;
        await _db.SaveChangesAsync();

        return Ok(ToResponse(player));
    }

    // Change the value of a specific property of a player
    // PATCH api/players/{Player.Id}
    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, PatchPlayerRequest request)
    {
        var player = await _db.Players
            .Include(p => p.Team)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (player is null) return NotFound();

        if (request.Username is not null)
        {
            if (request.Username.Length < League.Player.UsernameLengthMin 
            ||  request.Username.Length > League.Player.UsernameLengthMax )
            {
                return BadRequest($"Username length must be between {League.Player.UsernameLengthMin} and {League.Player.UsernameLengthMax}");
            }
            
            player.Username = request.Username;
        }

        if (request.Role is not null)
        {
            if (!League.Roles.All.Contains(request.Role))
            {
                return BadRequest($"Invalid role. Must be one of: {string.Join(", ", League.Roles.All)}");
            }
            
            player.Role = request.Role;
        }

        if (request.Skill is not null)
        {
            player.Skill = Math.Clamp(request.Skill.Value, League.Player.SkillMin, League.Player.SkillMax);
        }

        if (request.IsActive is not null)
        {
            player.IsActive = request.IsActive.Value;
        }

        await _db.SaveChangesAsync();

        return Ok(ToResponse(player));
    }

    // Add a player to a team
    // POST api/players/team{Team.Id}
    [HttpPost("team{teamId}")]
    public async Task<IActionResult> Create(int teamId, CreatePlayerRequest request)
    {
        var team = await _db.Teams.FindAsync(teamId);
        if (team is null) return NotFound("Team not found");

        if (!League.Roles.All.Contains(request.Role))
        {
            return BadRequest($"Invalid role. Must be one of: {string.Join(", ", League.Roles.All)}");
        }

        var playerCount = await _db.Players.CountAsync(p => p.TeamId == teamId);
        if (playerCount >= League.Team.RosterMax)
        {
            return BadRequest($"Team roster is full (max {League.Team.RosterMax} players)");
        }

        var player = new Player
        {
            Username = request.Username, 
            Role = request.Role, 
            Skill = Math.Clamp(request.Skill, League.Player.SkillMin, League.Player.SkillMax), 
            TeamId = teamId, 
            Team = team
        };

        _db.Players.Add(player);
        await _db.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetById), new { teamId, id = player.Id }, ToResponse(player));
    }

    // Delete a player
    // DELETE api/players/{Player.Id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var player = await _db.Players.FirstOrDefaultAsync(p => p.Id == id);
        if (player is null) return NotFound();

        _db.Players.Remove(player);
        await _db.SaveChangesAsync();
        
        return NoContent();
    }
}




// public class PlayerSkillComparer(bool descending = true) : IComparer<Player>
// {
//     private readonly bool _descending = descending;

//     public int Compare(Player? p1, Player? p2)
//     {
//         if (p1 is null) return p2 is null ? 0 : -1;
//         if (p2 is null) return 1;
//         int skillComparison = _descending ? p2.Skill.CompareTo(p1.Skill) : p1.Skill.CompareTo(p2.Skill);
//         if (skillComparison == 0)
//         {
//             int roleComparison = p1.Role.CompareTo(p2.Role);
//             if (roleComparison == 0)
//             {
//                 int teamNameComparison = p1.Team.Name.CompareTo(p2.Team.Name);
//                 if (teamNameComparison == 0)
//                 {
//                     return p1.Username.CompareTo(p2.Username);
//                 }

//                 return teamNameComparison;
//             }

//             return roleComparison;
//         }

//         return skillComparison;
//     }
// }