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
                    .SetUserinfoEndpointUris("connect/userinfo");

        //options.RegisterScopes("openid", "profile", "email", "offline_access");
        //options.setaccesstokenlifetime(timespan.fromminutes(3));//ȫ�����÷������ƹ���ʱ�䣬Ĭ��1Сʱ�������ڰ䷢tokenʱ��������
        //options.SetRefreshTokenLifetime(TimeSpan.FromMinutes(3));
        //options.RequireProofKeyForCodeExchange();//ȫ������pkce,���ߵ������ÿͻ���pkce
        options.AcceptAnonymousClients();//���������ͻ��ˣ�������(password flow)����client_id

        // Enable the client credentials flow.
        options.AllowClientCredentialsFlow()
                .AllowAuthorizationCodeFlow()
                .AllowPasswordFlow()
                .AllowRefreshTokenFlow();

        // Register the signing and encryption credentials.
        // Note: in a real world application, this encryption key should be
        // stored in a safe place (e.g in Azure KeyVault, stored as a secret).
        options.DisableAccessTokenEncryption();//���÷������Ƽ���
        options.AddEncryptionKey(new SymmetricSecurityKey(
            Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

        //options.AddDevelopmentEncryptionCertificate();
        //options.AddDevelopmentSigningCertificate();
        var cert1 = builder.Configuration.GetSection("Certificate:EncryptionCertificate").Value;
        var cert2 = builder.Configuration.GetSection("Certificate:SigningCertificate").Value;
        options.AddEncryptionCertificate(new X509Certificate2(Hex.Decode(cert1), string.Empty));
        options.AddSigningCertificate(new X509Certificate2(Hex.Decode(cert2), string.Empty));

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableLogoutEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               .EnableUserinfoEndpointPassthrough();
        //.EnableStatusCodePagesIntegration();//���� EnableStatusCodePagesIntegration ʱ��OpenIddict �����Դ� ASP.NET Core ��״̬��ҳ���м���л�ȡ��Ӧ������ζ�ţ�������Ӧ�ó�����һ���ض���״̬��ҳ�棨���磬���� 404 �� 500 ���󣩣�OpenIddict ��ʹ����Щҳ������Ӧ��Ӧ�Ĵ��󣬶�����Ĭ�ϵĴ�����Ӧ��
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
                .WithClaimStrategy()//ָ����ȡ�⻧Id�Ĳ���,����Ҫ����(__tenant__)���ɶ�����԰�˳��ִ�У�
                .WithConfigurationStore();//�������ļ���ȡ�⻧�������ַ�������
services.AddDbContext<UserTenantDbContext>();
services.AddHttpContextAccessor();
services.AddScoped<UserService>();
services.AddScoped<SignInService>();
services.AddHostedService<Worker>();
services.AddRouting(options => options.LowercaseUrls = true);


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHttpsRedirection();
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseMultiTenant();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
