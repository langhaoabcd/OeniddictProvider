namespace OeniddictProvider.Services;
public class UserService
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IMultiTenantStore<TenantInfo> _tenantStore;

    public UserService(IHttpContextAccessor httpContextAccessor, IMultiTenantStore<TenantInfo> tenantStore)
    {
        this.httpContextAccessor = httpContextAccessor;
        this._tenantStore = tenantStore;
    }
    public async Task<UserInfo> GetUserAsync(ClaimsPrincipal principal)
    {
        var tenantInfo = httpContextAccessor.HttpContext?.GetMultiTenantContext<TenantInfo>()?.TenantInfo;
        return await GetUserInfoById(tenantInfo, principal.GetClaim(Claims.Subject));
    }

    public async Task<UserInfo> FindByIdAsync(string userId, string? tenant = null)
    {
        var tenantInfo = httpContextAccessor.HttpContext?.GetMultiTenantContext<TenantInfo>()?.TenantInfo;
        if (tenantInfo is null)
        {
            tenantInfo = await _tenantStore.TryGetByIdentifierAsync(tenant);
            if (tenantInfo is null)
            {
                throw new Exception(" Get TenantInfo error");
            }
        }
        return await GetUserInfoById(tenantInfo, userId);

    }

    public async Task<UserInfo> FindByNameAsync(string userName, string? tenant = null)
    {
        var tenantInfo = httpContextAccessor.HttpContext?.GetMultiTenantContext<TenantInfo>()?.TenantInfo;
        if (tenantInfo is null)
        {
            tenantInfo = await _tenantStore.TryGetByIdentifierAsync(tenant);
            if (tenantInfo is null)
            {
                throw new Exception(" Get TenantInfo error");
            }
        }
        return await GetUserInfoById(tenantInfo, userName);

    }

    private async Task<UserInfo> GetUserInfoById(TenantInfo tenantInfo, string sub)
    {
        UserInfo userInfo = new UserInfo();
        using var db = new UserTenantDbContext(tenantInfo);
        var user = await db.User.SingleOrDefaultAsync(b => b.Id.Equals(sub) || b.Name.Equals(sub));
        userInfo.Id = user.Id;
        userInfo.Name = user.Name;
        userInfo.Email = user.Email;
        userInfo.Phone= user.Phone;
        userInfo.Tenant = tenantInfo.Identifier;
        return userInfo;
    }

}
