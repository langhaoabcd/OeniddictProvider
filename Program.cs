var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
services.AddControllersWithViews();
services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
           .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
           {
               options.Cookie.Name = "identity.provider";
               options.LoginPath = "/account/login";
               options.LogoutPath = "/account/logout";
               options.ReturnUrlParameter = "returnUrl";
           });
services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("OidcProviderConnection"));

    // Register the entity sets needed by OpenIddict.
    // Note: use the generic overload if you need to replace the default OpenIddict entities.
    options.UseOpenIddict();
});

// OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
// (like pruning orphaned authorizations/tokens from the database) at regular intervals.
services.AddQuartz(options =>
{
    options.UseMicrosoftDependencyInjectionJobFactory();
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
});

// Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
services.AddOpenIddict()
    // Register the OpenIddict core components.
    .AddCore(options =>
    {
        // Configure OpenIddict to use the Entity Framework Core stores and models.
        // Note: call ReplaceDefaultEntities() to replace the default entities.
        options.UseEntityFrameworkCore()
               .UseDbContext<ApplicationDbContext>();

        // Enable Quartz.NET integration.
        options.UseQuartz();
    })
    // Register the OpenIddict server components.
    .AddServer(options =>
    {
        // Enable the token endpoint.
        options.SetAuthorizationEndpointUris("connect/authorize")
                    .SetLogoutEndpointUris("connect/logout")
                    .SetIntrospectionEndpointUris("connect/introspect")
                    .SetTokenEndpointUris("connect/token")
                    .SetUserinfoEndpointUris("connect/userinfo")

                    .RegisterScopes("openid", "profile", "email", "offline_access")
                    .SetAccessTokenLifetime(TimeSpan.FromDays(1))//全局设置访问令牌过期时间，默认1小时，或者在颁发token时单独设置
                    .SetRefreshTokenLifetime(TimeSpan.FromDays(3));
        //options.RequireProofKeyForCodeExchange();//全局启用pkce,或者单独设置客户端pkce
        options.AcceptAnonymousClients();//接受匿名客户端，允许流(password flow)不传client_id

        // Enable the client credentials flow.
        options.AllowClientCredentialsFlow()
                .AllowAuthorizationCodeFlow()
                .AllowPasswordFlow()
                .AllowRefreshTokenFlow();

        // Register the signing and encryption credentials.
        // Note: in a real world application, this encryption key should be
        // stored in a safe place (e.g in Azure KeyVault, stored as a secret).
        options.DisableAccessTokenEncryption();//禁用访问令牌加密
        options.AddEncryptionKey(new SymmetricSecurityKey(
            Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

        options.AddDevelopmentEncryptionCertificate();
        options.AddDevelopmentSigningCertificate();
        //var cert1 = builder.Configuration.GetSection("Certificate:EncryptionCertificate").Value;
        //var cert2 = builder.Configuration.GetSection("Certificate:SigningCertificate").Value;
        //options.AddEncryptionCertificate(new X509Certificate2(Hex.Decode(cert1), string.Empty));
        //options.AddSigningCertificate(new X509Certificate2(Hex.Decode(cert2), string.Empty));

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableLogoutEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               .EnableUserinfoEndpointPassthrough();
        //.EnableStatusCodePagesIntegration();//启用 EnableStatusCodePagesIntegration 时，OpenIddict 将尝试从 ASP.NET Core 的状态码页面中间件中获取响应。这意味着，如果你的应用程序有一个特定的状态码页面（例如，对于 404 或 500 错误），OpenIddict 将使用这些页面来响应相应的错误，而不是默认的错误响应。
    })
    // Register the OpenIddict validation components.
    .AddValidation(options =>
    {
        // Import the configuration from the local OpenIddict server instance.
        options.UseLocalServer();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

services.AddMultiTenant<TenantInfo>()
                .WithClaimStrategy()//指定读取租户Id的策略,申明要包含(__tenant__)。可多个策略按顺序执行，
                .WithConfigurationStore();//从配置文件读取租户的连接字符串配置
services.AddDbContext<UserTenantDbContext>();
services.AddHttpContextAccessor();
services.AddScoped<UserService>();
services.AddScoped<SignInService>();
services.AddHostedService<Worker>();
services.AddRouting(options => options.LowercaseUrls = true);


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    
}
else
{
var forwardedHeaderOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    };
    forwardedHeaderOptions.KnownNetworks.Clear();
    forwardedHeaderOptions.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardedHeaderOptions);

    app.UseExceptionHandler("/Home/Error");
}
app.UseHttpsRedirection();
    app.UseHsts();

app.UseStaticFiles();
app.UseMultiTenant();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
