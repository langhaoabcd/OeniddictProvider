namespace OeniddictProvider.ViewModels;
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public string? Error { get; internal set; }
    public string? ErrorDescription { get; internal set; }
}