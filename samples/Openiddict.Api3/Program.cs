using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        // 注意：验证处理程序使用OpenID Connect发现来检索用于验证令牌的颁发者[签名密钥]。
        options.SetIssuer("https://localhost:7099/");
        options.AddAudiences("resource_server_3");

        // 注册[加密凭据]。此示例使用对称 服务器和Api2样本之间共享的加密密钥（执行本地令牌验证而不是使用内省）。
        // 注意：在实际应用程序中，此加密密钥应该存储在安全的地方（例如Azure KeyVault中，作为机密存储）。
        options.AddEncryptionKey(new SymmetricSecurityKey(Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

        // Register the System.Net.Http integration.
        options.UseSystemNetHttp();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });
builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

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



app.MapGet("/api/Openiddict3Api", [Authorize]
() => summaries)
//() => (ClaimsPrincipal user) => $"{user?.Identity?.Name} is allowed to access Openiddict3Api.");
;

app.MapGet("/weatherforecast", () =>
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