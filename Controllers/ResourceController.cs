namespace OeniddictProvider.Controllers;

[Route("api")]
public class ResourceController : Controller
{
    private readonly UserService userService;

    public ResourceController(UserService userService)
    {
        this.userService = userService;
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("message")]
    public async Task<IActionResult> GetMessage()
    {
        var ti = User.GetClaim("__tenant__");
        var sub = User.GetClaim(Claims.Subject);
        var user = await userService.FindByIdAsync(sub,ti);
        if (user is null)
        {
            return Challenge(
                authenticationSchemes: OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictValidationAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictValidationAspNetCoreConstants.Properties.ErrorDescription] =
                        "The specified access token is bound to an account that no longer exists."
                }));
        }

        return Content($"{user.Name} has been successfully authenticated.");
    }
}
