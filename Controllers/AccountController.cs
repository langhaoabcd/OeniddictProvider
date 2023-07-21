using Finbuckle.MultiTenant;
using System.Security.Principal;

namespace OeniddictProvider.Controllers;
public class AccountController : Controller
{
    private readonly SignInService _signInService;
    private readonly IMultiTenantStore<TenantInfo> _tenantStore;

    public AccountController(
        SignInService signInService,
        IMultiTenantStore<TenantInfo> tenantStore,
        ILogger<AccountController> logger)
    {
        _signInService = signInService;
        _tenantStore = tenantStore;
    }

    public IActionResult Login()
    {
        var IsAuthenticated = User?.Identity?.IsAuthenticated;
        if (IsAuthenticated ?? false)
        {
            Console.WriteLine("已经认证");
        }
        else
        {
            Console.WriteLine("尚未认证");
        }
        ViewData["IsAuthenticated"] = IsAuthenticated;

        var returnUri = Request.Query["ReturnUrl"];
        ViewData["ReturnUrl"] = returnUri;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogInAsync(
        [FromForm][Bind("CompanyCode", "Account", "Password")] LoginViewModel model,
        [FromQuery] LoginQueryParams parms)
    {
        var returnUrl = parms.returnUrl;
        var tenantInfo = await _tenantStore.TryGetByIdentifierAsync(model.CompanyCode);
        if (tenantInfo is null)
        {
            TempData["Error"] = "公司代码不存在";
            return View(model);
        }
        returnUrl ??= Url.Content("~/");
        //尝试设置租户信息
        var setTi = HttpContext.TrySetTenantInfo(tenantInfo, true);
        if (!setTi)
        {
            TempData["Error"] = "公司代码异常";
            return View(model);
        }
        var ti = HttpContext.GetMultiTenantContext<TenantInfo>()?.TenantInfo;
        var signResult = await _signInService.PasswordSignInAsync(ti, model.Account, model.Password);
        if (!signResult.Item1)
        {
            TempData["Error"] = "用户名或密码不正确";
            return View(model);
        }
        var user = signResult.Item2;
        var employeeClaims = new List<Claim>
        {
            new Claim("__tenant__",model.CompanyCode),
            new Claim(ClaimTypes.Name,user.Name),
            new Claim(ClaimTypes.Email,user.Email)
        };
        var licencesClaims = new List<Claim>
        {
            new Claim("sub",user.Id),
            new Claim(ClaimTypes.MobilePhone,user.Phone)
        };
        var shcoolIdentity = new ClaimsIdentity(employeeClaims, "employee identity");
        var licenesIdentity = new ClaimsIdentity(licencesClaims, "licences identity");

        var principal = new ClaimsPrincipal(new[] { shcoolIdentity, licenesIdentity });
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                principal);
        // Clear the existing external cookie
        if (signResult.Item1)
        {
            return LocalRedirect(returnUrl);
        }

        return View();
    }

    [HttpPost("~/Logout"), ValidateAntiForgeryToken]
    public async Task<ActionResult> LogoutAsync(string returnUrl)
    {
        // Retrieve the identity stored in the local authentication cookie. If it's not available,
        // this indicate that the user is already logged out locally (or has not logged in yet).
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (result is not { Succeeded: true })
        {
            // Only allow local return URLs to prevent open redirect attacks.
            return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
        }

        // Remove the local authentication cookie before triggering a redirection to the remote server.
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //未退出oidc
        return Redirect("/account/login?returnUrl=" + returnUrl);
    }
}
