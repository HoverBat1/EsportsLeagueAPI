# Esports League API

A RESTful API for managing an esports league — teams, players, and matches. Built with ASP.NET Core, Entity Framework Core, and SQLite.

## Tech Stack

- **ASP.NET Core** — Web API framework
- **Entity Framework Core** — ORM for database access
- **SQLite** — Lightweight local database
- **C# 12 / .NET 8**

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)

### Running Locally

```bash
git clone https://github.com/yourusername/EsportsLeagueApi.git
cd EsportsLeagueApi
dotnet ef database update
dotnet run
```

The API will be available at `http://localhost:5236`.

## Project Structure

```
EsportsLeagueApi/
├── Controllers/        # API endpoints
├── Data/               # DbContext and database config
├── DTOs/               # Request and response shapes
├── Models/             # Database entity classes
├── Constants.cs        # League constants (regions, roles, etc.)
└── Program.cs          # App configuration and DI setup
```

## API Endpoints

### Teams

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/teams` | Get all teams |
| GET | `/api/teams/{id}` | Get a team by ID |
| GET | `/api/teams/{id}/stats` | Get a team's stats |
| GET | `/api/teams/{id}/history` | Get a team's match history |
| GET | `/api/teams/stats` | Get stats for all teams |
| GET | `/api/teams/leaderboard` | Get ranked leaderboard |
| POST | `/api/teams` | Create a team |
| PUT | `/api/teams/{id}` | Update a team |
| PATCH | `/api/teams/{id}` | Partially update a team |
| DELETE | `/api/teams/{id}` | Delete a team |

### Players

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/players` | Get all players |
| GET | `/api/players?isActive=true` | Filter by active status |
| GET | `/api/players/{id}` | Get a player by ID |
| GET | `/api/players/top?count=10` | Get top N players by skill |
| GET | `/api/players/team{teamId}` | Get all players on a team |
| POST | `/api/players/team{teamId}` | Add a player to a team |
| PUT | `/api/players/{id}` | Update a player |
| PATCH | `/api/players/{id}` | Partially update a player |
| DELETE | `/api/players/{id}` | Delete a player |

### Matches

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/matches` | Get all matches |
| GET | `/api/matches?status=Completed` | Filter by status |
| GET | `/api/matches/{id}` | Get a match by ID |
| POST | `/api/matches` | Schedule a match |
| PATCH | `/api/matches/{id}/start` | Start a match |
| PATCH | `/api/matches/{id}/score` | Update match score |
| PATCH | `/api/matches/{id}/complete` | Complete a match |
| DELETE | `/api/matches/{id}` | Delete a match |

## Example Requests

### Create a Team

```http
POST /api/teams
Content-Type: application/json

{
    "name": "Team Mars",
    "region": "NA"
}
```

### Add a Player

```http
POST /api/players/team1
Content-Type: application/json

{
    "username": "ShadowBlade",
    "role": "Carry",
    "skill": 87
}
```

### Schedule a Match

```http
POST /api/matches
Content-Type: application/json

{
    "homeTeamId": 1,
    "awayTeamId": 2,
    "gameMode": "Standard",
    "scheduledAt": "2026-05-01T18:00:00Z"
}
```

## Validation Rules

**Teams**
- Name: 2–50 characters
- Region: must be one of `NA`, `EU`, `APAC`

**Players**
- Username: 2–30 characters
- Role: must be one of `Carry`, `Support`, `Tank`, `Jungler`, `Mid`
- Skill: 1–100
- Max 5 players per team

**Matches**
- Home and away teams must be different
- Game mode: must be one of `Standard`, `Tournament`, `Practice`
- Status transitions: `Scheduled → Live → Completed`

## Future Plans

- [ ] JWT authentication
- [ ] Global error handling middleware
- [ ] Frontend UI (Blazor or React)