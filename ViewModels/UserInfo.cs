using OeniddictProvider.Models;

namespace OeniddictProvider.ViewModels
{
    public class UserInfo:User
    {
        public string? Tenant { get; set; }
    }
}
