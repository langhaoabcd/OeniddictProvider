using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Validation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
//内省验证
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer("https://localhost:7099");
        options.AddAudiences("resource_server_1");

        options.UseIntrospection()
               .SetClientId("resource_server_1")
               .SetClientSecret("846B62D0-DEF9-4215-A99D-86E6B8DAB342");

        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/OpeniddictApi", [Authorize]
(IHttpContextAccessor accessor) =>
{
    var token= accessor?.HttpContext?.GetTokenAsync("access_token");
    return new[]
    {
    "freezing", "bracing", "chilly", "cool", "mild", "warm", "balmy", "hot", "sweltering", "scorching"
};
});


app.MapGet("/api/OpeniddictApi2", [AllowAnonymous] () => new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
});
app.Run();