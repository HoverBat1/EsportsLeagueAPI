using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using EsportsLeagueApi01.Data;
using EsportsLeagueApi01.Services;
using EsportsLeagueApi01.Middleware;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddControllers().AddJsonOptions(options =>
// {
//     options.JsonSerializerOptions.ReferenceHandler = 
//         System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
// });
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddDbContext<LeagueDbContext>(options => options.UseSqlite("Data Source=league.db"));
builder.Services.AddOpenApi();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true, 
        ValidateAudience         = true, 
        ValidateLifetime         = true, 
        ValidateIssuerSigningKey = true, 
        ValidIssuer              = builder.Configuration["Jwt:Issuer"], 
        ValidAudience            = builder.Configuration["Jwt:Audience"], 
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseHttpsRedirection();
app.UseAuthentication();
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