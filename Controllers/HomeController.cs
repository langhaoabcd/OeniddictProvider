namespace OeniddictProvider.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        string errorMessage = string.Empty;
        var response = HttpContext.GetOpenIddictServerResponse();
        if (response == null)
        {
            //使用IExceptionHandlerFeature接口来获取错误信息。从HttpContext中获取这个接口的实例，获取异常对象
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionFeature != null)
            {
                errorMessage = exceptionFeature.Error.Message;
            }
            return View("Error", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Error = errorMessage,
                ErrorDescription = errorMessage
            });
        }

        return View("Error", new ErrorViewModel
        {
            Error = response.Error,
            ErrorDescription = response.ErrorDescription
        });
    }
}