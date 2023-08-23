using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SetJwksOptions(new JwkOptions("https://localhost:7099/.well-known/jwks"));
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = "https://localhost:7099/",
        ValidateAudience = true,
        ValidAudience = "resource_server_2",
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")),
        ValidateLifetime = true, 
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.FromSeconds(0),
    };
});
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/api/Openiddict2Api", [Authorize]
() => new[]
{
    "freezing", "bracing", "chilly", "cool", "mild", "warm", "balmy", "hot", "sweltering", "scorching"
})
;


app.MapGet("/api/Openiddict2Api2", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}