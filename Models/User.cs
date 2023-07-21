namespace OeniddictProvider.Models;
public class User
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Phone { get; set; }
    //public IList<string> Roles { get; set; } = new List<string>() { 
    //    "admin","user"
    //};
}
