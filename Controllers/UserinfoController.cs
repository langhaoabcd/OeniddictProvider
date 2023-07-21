namespace OeniddictProvider.Controllers;
public class UserinfoController : Controller
{
    private readonly UserService userService;

    public UserinfoController(UserService userService)
    {
        this.userService = userService;
    }

    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo"), HttpPost("~/connect/userinfo"), Produces("application/json")]
    public async Task<IActionResult> Userinfo()
    {
        var ti = User.GetClaim("__tenant__");
        var sub = User.GetClaim(Claims.Subject);
        var user = await userService.FindByIdAsync(sub, ti);
        if (user == null)
        {
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The specified access token is bound to an account that no longer exists."
                }));
        }

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [Claims.Subject] = user.Id,
            [Claims.Name] = user.Name,
            [Claims.Email] = user.Email,
            [Claims.PhoneNumber] = user.Phone
        };
        return Ok(claims);
    }
}
