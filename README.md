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