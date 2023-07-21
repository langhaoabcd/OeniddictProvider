namespace OeniddictProvider.ViewModels;
public class LoginViewModel
{
    [Required(ErrorMessage = "必填")]
    public string CompanyCode { get; set; }
    [Required(ErrorMessage = "必填")]
    public string Account { get; set; }
    [Required(ErrorMessage = "必填")]
    public string Password { get; set; }
}

public class LoginQueryParams
{
    public string? returnUrl { get; set; }
}
