namespace OeniddictProvider.Services;
public class SignInService
{
    public SignInService() { }

    public async Task<Tuple<bool, User>> PasswordSignInAsync(TenantInfo? tenant, string account, string password)
    {
        if (tenant is null || string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
        {
            return await Task.FromResult(Tuple.Create(false, new User()));
        }
        User? user = new User();
        using var db = new UserTenantDbContext(tenant);
        user = db.User.SingleOrDefault(b => b.Id.Equals(account));
        //todo 密码加密存储
        if (user is null || !user.Password.Equals(password))
        {
            return await Task.FromResult(Tuple.Create(false, new User()));
        }
        return await Task.FromResult(Tuple.Create(true, user));
    }
}
