namespace OeniddictProvider;

public class Worker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public Worker(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        await RegisterApplicationsAsync(scope.ServiceProvider);
        await RegisterScopesAsync(scope.ServiceProvider);

        await SetUserTenantAsync(scope.ServiceProvider);

        static async Task RegisterApplicationsAsync(IServiceProvider provider)
        {
            var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();

            // API
            if (await manager.FindByClientIdAsync("resource_server_1") == null)
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = "resource_server_1",
                    ClientSecret = "846B62D0-DEF9-4215-A99D-86E6B8DAB342",
                    Permissions =
                    {
                        Permissions.Endpoints.Introspection
                    }
                };
                await manager.CreateAsync(descriptor);
            }

            if (await manager.FindByClientIdAsync("console") == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "console",
                    ClientSecret = "388D45FA-B36B-4988-BA59-B187D329C207",
                    DisplayName = "My client application",
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.ClientCredentials,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.Prefixes.Scope+"api1",
                        Permissions.Prefixes.Scope+"api2",
                        Permissions.Prefixes.Scope+"api3",
                    }
                });
            }

            if (await manager.FindByClientIdAsync("consolePwd") == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "consolePwd",
                    ClientSecret = "388D45FA-B36B-4988-BA59-B187D329C208",
                    DisplayName = "My client application",
                    Permissions =
            {
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.Password,
                Permissions.GrantTypes.RefreshToken,
                Permissions.Scopes.Email,
                  Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
                Permissions.Prefixes.Scope+"api1",
                Permissions.Prefixes.Scope+"api2",
                Permissions.Prefixes.Scope+"api3",

            }
                });
            }
            if (await manager.FindByClientIdAsync("mvc") == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "mvc",
                    ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
                    ConsentType = ConsentTypes.Explicit,
                    DisplayName = "mvc code PKCE",
                    RedirectUris =
            {
                new Uri("https://localhost:5599/signin-oidc")
            },
                    PostLogoutRedirectUris =
            {
                new Uri("https://localhost:5599/signout-callback-oidc")
            },
                    Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Logout,
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
                Permissions.Prefixes.Scope+"api1",
                Permissions.Prefixes.Scope+"api2",
                Permissions.Prefixes.Scope+"api3",

            },
                    Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange
            }
                });
            }

            if (await manager.FindByClientIdAsync("codeFlow") == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "codeFlow",
                    ClientSecret = "901564A5-E7FE-42CB-DD0D-61EF6A8F3666",
                    ConsentType = ConsentTypes.Explicit,
                    DisplayName = "code flow",
                    RedirectUris =
            {
                new Uri("https://localhost:5678/signin-oidc")
            },
                    PostLogoutRedirectUris =
            {
                new Uri("https://www.baidu.com")
            },
                    Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Logout,
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
                Permissions.Prefixes.Scope+"api1",
                Permissions.Prefixes.Scope+"api2",
                Permissions.Prefixes.Scope+"api3",

            }
                });
            }
        }

        static async Task RegisterScopesAsync(IServiceProvider provider)
        {
            var manager = provider.GetRequiredService<IOpenIddictScopeManager>();

            if (await manager.FindByNameAsync("api1") is null)
            {
                await manager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "api1",
                    Resources =
                    {
                        "resource_server_1"
                    }
                });
            }

            if (await manager.FindByNameAsync("api2") is null)
            {
                await manager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "api2",
                    Resources =
                    {
                        "resource_server_2"
                    }
                });
            }
            if (await manager.FindByNameAsync("api3") is null)
            {
                await manager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "api3",
                    Resources =
                    {
                        "resource_server_3"
                    }
                });
            }
        }

        static async Task SetUserTenantAsync(IServiceProvider provider)
        {
            var tenantStore = provider.GetRequiredService<IMultiTenantStore<TenantInfo>>();
            var tenantA = await tenantStore.TryGetByIdentifierAsync("aaa");
            var tenantB = await tenantStore.TryGetByIdentifierAsync("bbb");

            using (var db = new UserTenantDbContext(tenantA))
            {
                var user = db.User.SingleOrDefault(b => b.Id.Equals("admin"));
                if (user is null)
                {
                    db.User.Add(new User { Id = "admin", Name = "张爷", Email = "zhangsan@163.com", Password = "123", Phone = "13900112121" });
                    db.SaveChanges();
                }
            }
            using (var db = new UserTenantDbContext(tenantB))
            {
                var user = db.User.SingleOrDefault(b => b.Id.Equals("test"));
                if (user is null)
                {
                    db.User.Add(new User { Id = "test", Name = "曾爷", Email = "zeng@163.com", Password = "123", Phone = "15922444141" });
                    db.SaveChanges();
                }
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}