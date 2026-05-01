using EsportsLeagueApi01.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddControllers().AddJsonOptions(options =>
// {
//     options.JsonSerializerOptions.ReferenceHandler = 
//         System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
// });
builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddDbContext<LeagueDbContext>(options => options.UseSqlite("Data Source=league.db"));
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();


// http://localhost:5236/api/teams


// var random = new Random(19);
// List<int> teamIds = [1, 2, 3, 4, 5, 6, 7, 8];
// teamIds = teamIds.Shuffle().ToList();
// Console.WriteLine($"{string.Join(", ", teamIds)}");
// teamIds = teamIds.Shuffle().ToList();
// Console.WriteLine($"{string.Join(", ", teamIds)}");
// for (var i = 1; i <= 8; i++) Console.WriteLine($"{i}: {random.Next(10)} - {random.Next(10)}");