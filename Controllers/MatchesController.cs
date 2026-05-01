using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EsportsLeagueApi01.Data;
using EsportsLeagueApi01.Models;
using EsportsLeagueApi01.DTOs;

namespace EsportsLeagueApi01.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly LeagueDbContext _db;

    public MatchesController(LeagueDbContext db)
    {
        _db = db;
    }

    private static MatchResponse ToResponse(Match m) => new()
    {
        Id = m.Id, 
        Status = m.Status, 
        GameMode = m.GameMode, 
        HomeScore = m.HomeScore, 
        AwayScore = m.AwayScore, 
        ScheduledAt = m.ScheduledAt, 
        CompletedAt = m.CompletedAt, 
        HomeTeam = new()
        {
            Id = m.HomeTeam.Id, 
            Name = m.HomeTeam.Name, 
            Region = m.HomeTeam.Region
        }, 
        AwayTeam = new()
        {
            Id = m.AwayTeam.Id, 
            Name = m.AwayTeam.Name, 
            Region = m.AwayTeam.Region
        }, 
        Result = m.Status switch
        {
            League.Status.Completed when m.HomeScore > m.AwayScore => $"{m.HomeTeam.Name} wins {m.HomeScore}-{m.AwayScore}", 
            League.Status.Completed when m.AwayScore > m.HomeScore => $"{m.AwayTeam.Name} wins {m.AwayScore}-{m.HomeScore}", 
            League.Status.Completed                                => $"Draw: {m.HomeTeam.Name} {m.HomeScore} - {m.AwayScore} {m.AwayTeam.Name}", 
            League.Status.Live                                     => $"Live: {m.HomeTeam.Name} {m.HomeScore} - {m.AwayScore} {m.AwayTeam.Name}", 
            League.Status.Scheduled                                => $"Upcoming: {m.HomeTeam.Name} vs {m.AwayTeam.Name}", 
            _                                                      => "Unknown match state"
            // League.Status.Completed when Math.Abs(m.HomeScore - m.AwayScore) > 3 => $"Dominant victory for {(m.HomeScore > m.AwayScore ? m.HomeTeam.Name : m.AwayTeam.Name)}. ({m.HomeTeam.Name} {m.HomeScore} - {m.AwayScore} {m.AwayTeam.Name})", 
            // League.Status.Completed when Math.Abs(m.HomeScore - m.AwayScore) > 0 => $"Close m - {(m.HomeScore > m.AwayScore ? m.HomeTeam.Name : m.AwayTeam.Name)} edges out. ({m.HomeTeam.Name} {m.HomeScore} - {m.AwayScore} {m.AwayTeam.Name})", 
        }
    };

    private async Task<Match?> FindMatch(int id)
    {
        return await _db.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    // Get all matches
    // All matches example url: http://localhost:5236/api/matches
    // By status example url: http://localhost:5236/api/matches?status=Completed
    // GET api/matches
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var query = _db.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .AsQueryable();
        if (status is not null) query = query.Where(m => m.Status == status);

        var matches = await query.ToArrayAsync();

        return Ok(matches.Select(ToResponse));
    }

    // Get a match
    // GET api/matches/{Match.Id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var match = await FindMatch(id);
        if (match is null) return NotFound();

        return Ok(ToResponse(match));
    }

    // Create a match
    // POST api/matches
    [HttpPost]
    public async Task<IActionResult> Create(CreateMatchRequest request)
    {
        if (request.HomeTeamId == request.AwayTeamId)
        {
            return BadRequest("A team cannot play against itself");
        }
        
        if (!League.Modes.All.Contains(request.GameMode))
        {
            return BadRequest($"Invalid game mode. Must be one of: {string.Join(", ", League.Modes.All)}");
        }
        
        var homeTeam = await _db.Teams.FindAsync(request.HomeTeamId);
        if (homeTeam is null) return NotFound($"Home team {request.HomeTeamId} not found");
        
        var awayTeam = await _db.Teams.FindAsync(request.AwayTeamId);
        if (awayTeam is null) return NotFound($"Away team {request.AwayTeamId} not found");

        var match = new Match
        {
            HomeTeamId = request.HomeTeamId, 
            AwayTeamId = request.AwayTeamId, 
            GameMode = request.GameMode, 
            ScheduledAt = request.ScheduledAt, 
            Status = League.Status.Scheduled
        };

        _db.Matches.Add(match);
        await _db.SaveChangesAsync();

        // Reload with navigation properties for the response
        var created = await FindMatch(match.Id);

        return CreatedAtAction(nameof(GetById), new { id = match.Id }, ToResponse(created!));
    }

    // Start a match
    // PATCH api/matches/{Match.Id}/start
    [HttpPatch("{id}/start")]
    public async Task<IActionResult> Start(int id)
    {
        var match = await FindMatch(id);
        if (match is null) return NotFound();
        
        if (match.Status != League.Status.Scheduled)
        {
            return BadRequest($"Match {id} has not been scheduled");
        }
        
        match.Status = League.Status.Live;
        match.StartedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ToResponse(match));
    }

    // Change a match's score
    // PATCH api/matches/{Match.Id}/score
    [HttpPatch("{id}/score")]
    public async Task<IActionResult> UpdateScore(int id, UpdateMatchRequest request)
    {
        var match = await FindMatch(id);
        if (match is null) return NotFound();
        
        if (match.Status != League.Status.Live)
        {
            return BadRequest($"Match {id} is not live");
        }
        
        match.HomeScore = request.HomeScore;
        match.AwayScore = request.AwayScore;
        await _db.SaveChangesAsync();

        return Ok(ToResponse(match));
    }

    // Complete a match
    // PATCH api/matches/{Match.Id}/complete
    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        var match = await FindMatch(id);
        if (match is null) return NotFound();
        
        if (match.Status != League.Status.Live)
        {
            return BadRequest($"Match {id} is not live");
        }
        
        match.Status = League.Status.Completed;
        match.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ToResponse(match));
    }

    // Delete a match
    // DELETE api/matches/{Match.Id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var match = await FindMatch(id);
        if (match is null) return NotFound();

        if (match.Status == League.Status.Live)
        {
            return BadRequest($"Cannot delete a live match");
        }

        _db.Matches.Remove(match);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}