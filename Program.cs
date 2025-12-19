using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Htmx;
using Serilog;
using DigitalniProdukty.Data;
using DigitalniProdukty.Services;
using DigitalniProdukty.Security;

namespace DigitalniProdukty
{
    public class Program
    {
        // Sestaví a spustí ASP.NET Core aplikaci (DI, middleware, routování).
        // Obsahuje i bootstrap seed (role, owner účet, skupiny) pro vývoj.
        public static void Main(string[] args)
        {
            // Nastaví Serilog co nejdřív (bootstrap logování během startu).
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                Log.Information("Starting DigitalniProdukty application");

                var builder = WebApplication.CreateBuilder(args);

                // Použije Serilog jako logging provider.
                builder.Host.UseSerilog();

                // Registrace služeb do DI kontejneru.
                var connectionString = builder.Configuration.GetConnectionString("DigiProdConnection") ?? throw new InvalidOperationException("Connection string 'DigiProdConnection' not found.");
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString));
                builder.Services.AddDatabaseDeveloperPageExceptionFilter();

                // Health checks včetně ověření konektivity na SQL Server.
                builder.Services.AddHealthChecks()
                    .AddSqlServer(connectionString, name: "sqlserver", tags: ["db", "sql"]);

                // Rate limiting pro auth endpointy (ochrana proti brute-force).
                builder.Services.AddRateLimiter(options =>
                {
                    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                    // Sliding window policy pro přihlašovací endpointy.
                    options.AddSlidingWindowLimiter("auth", limiterOptions =>
                    {
                        limiterOptions.PermitLimit = 10;
                        limiterOptions.Window = TimeSpan.FromMinutes(1);
                        limiterOptions.SegmentsPerWindow = 4;
                        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                        limiterOptions.QueueLimit = 2;
                    });

                    // Přísnější policy pro reset hesla (omezení enumerace přes email).
                    options.AddFixedWindowLimiter("auth-strict", limiterOptions =>
                    {
                        limiterOptions.PermitLimit = 3;
                        limiterOptions.Window = TimeSpan.FromMinutes(5);
                        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                        limiterOptions.QueueLimit = 0;
                    });
                });

                // HSTS konfigurace včetně preload.
                builder.Services.AddHsts(options =>
                {
                    options.Preload = true;
                    options.IncludeSubDomains = true;
                    options.MaxAge = TimeSpan.FromDays(365);
                });

                // Pouze Identity backend (UI je řešené přes MVC controllery/views pod /auth a /account).
                builder.Services
                    .AddIdentity<IdentityUser, IdentityRole>(options =>
                    {
                        options.SignIn.RequireConfirmedAccount = true;
                        options.User.RequireUniqueEmail = true;
                    })
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders()
                    .AddErrorDescriber<LocalizedIdentityErrorDescriber>();

                // Obrana: vyhne se prázdným typům claimů, které mohou shodit ClaimsIdentity.
                // (v praxi se objevovalo ArgumentException: "The value cannot be an empty string.")
                builder.Services.Configure<IdentityOptions>(options =>
                {
                    if (string.IsNullOrWhiteSpace(options.ClaimsIdentity.UserIdClaimType))
                        options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;

                    if (string.IsNullOrWhiteSpace(options.ClaimsIdentity.UserNameClaimType))
                        options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;

                    if (string.IsNullOrWhiteSpace(options.ClaimsIdentity.EmailClaimType))
                        options.ClaimsIdentity.EmailClaimType = ClaimTypes.Email;

                    if (string.IsNullOrWhiteSpace(options.ClaimsIdentity.RoleClaimType))
                        options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;

                    if (string.IsNullOrWhiteSpace(options.ClaimsIdentity.SecurityStampClaimType))
                        options.ClaimsIdentity.SecurityStampClaimType = "AspNet.Identity.SecurityStamp";
                });

                builder.Services.ConfigureApplicationCookie(options =>
                {
                    options.LoginPath = "/auth/login";
                    options.AccessDeniedPath = "/auth/denied";
                });

                builder.Services.AddSingleton<IEmailSender, NoOpEmailSender>();

                builder.Services.AddSingleton<TokenCodec>();
                builder.Services.AddSingleton<QrCodeService>();
                builder.Services.AddScoped<ReturnUrlService>();
                builder.Services.AddScoped<TwoFactorService>();
                builder.Services.AddScoped<AuthEmailService>();
                builder.Services.AddSingleton(TimeProvider.System);
                builder.Services.AddScoped<LicensingService>();
                builder.Services.AddScoped<GroupContextService>();
                builder.Services.AddScoped<KeyGroupProvisioningService>();

                // .resx jsou embedded s base name např. "DigitalniProdukty.SharedResources".
                // Nastavení ResourcesPath by způsobilo lookup pod "...Resources.*" a fallback na klíče.
                builder.Services.AddLocalization();

                builder.Services
                    .AddControllersWithViews()
                    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                    .AddDataAnnotationsLocalization();

                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy(Authz.Policies.Users_CreateEndUser, policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireAssertion(ctx =>
                            ctx.User.IsInRole(Authz.Roles.Admin)
                            || ctx.User.IsInRole(Authz.Roles.Reseller)
                            || ctx.User.IsInRole(Authz.Roles.Distributor));
                    });

                    options.AddPolicy(Authz.Policies.SerialNumbers_ViewOwn, policy =>
                    {
                        policy.RequireAuthenticatedUser();
                    });

                    options.AddPolicy(Authz.Policies.SerialNumbers_Sell, policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireAssertion(ctx =>
                            ctx.User.IsInRole(Authz.Roles.Admin)
                            || ctx.User.IsInRole(Authz.Roles.Reseller)
                            || ctx.User.IsInRole(Authz.Roles.Distributor));
                    });

                    options.AddPolicy(Authz.Policies.SerialNumbers_Generate, policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireAssertion(ctx =>
                            ctx.User.IsInRole(Authz.Roles.Admin)
                            || ctx.User.IsInRole(Authz.Roles.Distributor));
                    });

                    options.AddPolicy(Authz.Policies.LicensingAdmin, policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireAssertion(ctx =>
                            ctx.User.IsInRole(Authz.Roles.Admin)
                            || ctx.User.IsInRole(Authz.Roles.Distributor));
                    });
                });

                var app = builder.Build();

                // Vývojová pomůcka: DB se při startu zmigruje.
                if (app.Environment.IsDevelopment())
                {
                    using var scope = app.Services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.Migrate();
                }

                // Seed rolí (bezpečné opakované spuštění).
                {
                    using var scope = app.Services.CreateScope();
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                    foreach (var role in Authz.Roles.All)
                    {
                        var exists = roleManager.RoleExistsAsync(role).GetAwaiter().GetResult();
                        if (exists) continue;

                        var result = roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                        if (result.Succeeded) continue;

                        Log.Warning("Failed to create role {Role}: {Errors}", role, string.Join(", ", result.Errors.Select(e => e.Code)));
                    }
                }

                // Seed výchozího owner uživatele ("Majitel") (konfigurovatelné, bezpečné opakovaně).
                // Role by měla existovat přesně na jednom účtu.
                {
                    using var scope = app.Services.CreateScope();
                    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                    var enabled = config.GetValue("BootstrapOwner:Enabled", false);
                    var email = config["BootstrapOwner:Email"];
                    var password = config["BootstrapOwner:Password"];
                    var requireChange = config.GetValue("BootstrapOwner:RequireChangeOnFirstLogin", true);

                    if (enabled && !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
                    {
                        var existingOwners = userManager.GetUsersInRoleAsync(Authz.Roles.Owner).GetAwaiter().GetResult();
                        if (existingOwners.Count > 1)
                        {
                            Log.Warning("Multiple users are in role {Role}. Expected exactly one.", Authz.Roles.Owner);
                        }

                        // If there is already an owner, do not create another.
                        if (existingOwners.Count == 0)
                        {
                            var user = userManager.FindByEmailAsync(email).GetAwaiter().GetResult();

                            if (user is null)
                            {
                                user = new IdentityUser
                                {
                                    UserName = email,
                                    Email = email,
                                    EmailConfirmed = true
                                };

                                var create = userManager.CreateAsync(user, password).GetAwaiter().GetResult();
                                if (!create.Succeeded)
                                {
                                    Log.Warning("Failed to create bootstrap owner user {Email}: {Errors}", email, string.Join(", ", create.Errors.Select(e => e.Code)));
                                    user = null;
                                }
                            }
                            else
                            {
                                // Zajistí, že seedovaný účet je přihlásitelný (RequireConfirmedAccount = true)
                                if (!user.EmailConfirmed)
                                {
                                    user.EmailConfirmed = true;
                                    userManager.UpdateAsync(user).GetAwaiter().GetResult();
                                }
                            }

                            if (user is not null)
                            {
                                var inRole = userManager.IsInRoleAsync(user, Authz.Roles.Owner).GetAwaiter().GetResult();
                                if (!inRole)
                                {
                                    var addRole = userManager.AddToRoleAsync(user, Authz.Roles.Owner).GetAwaiter().GetResult();
                                    if (!addRole.Succeeded)
                                    {
                                        Log.Warning("Failed to add bootstrap owner user {Email} to role {Role}: {Errors}", email, Authz.Roles.Owner, string.Join(", ", addRole.Errors.Select(e => e.Code)));
                                    }
                                }

                                if (requireChange)
                                {
                                    var claims = userManager.GetClaimsAsync(user).GetAwaiter().GetResult();
                                    var has = claims.Any(c => c.Type == Authz.Claims.ForceCredentialsChange);
                                    if (!has)
                                    {
                                        var addClaim = userManager.AddClaimAsync(user, new Claim(Authz.Claims.ForceCredentialsChange, "1")).GetAwaiter().GetResult();
                                        if (!addClaim.Succeeded)
                                        {
                                            Log.Warning("Failed to add bootstrap claim {Claim} to user {Email}: {Errors}", Authz.Claims.ForceCredentialsChange, email, string.Join(", ", addClaim.Errors.Select(e => e.Code)));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Seed skupin (KeyGroups) (bezpečné opakované spuštění).
                {
                    using var scope = app.Services.CreateScope();
                    var provisioning = scope.ServiceProvider.GetRequiredService<KeyGroupProvisioningService>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                    provisioning.EnsureAdminGroupExistsAsync().GetAwaiter().GetResult();

                    var distributors = userManager.GetUsersInRoleAsync(Authz.Roles.Distributor).GetAwaiter().GetResult();
                    foreach (var distributor in distributors)
                    {
                        provisioning.EnsureGroupForDistributorAsync(distributor).GetAwaiter().GetResult();
                    }

                    var resellers = userManager.GetUsersInRoleAsync(Authz.Roles.Reseller).GetAwaiter().GetResult();
                    foreach (var reseller in resellers)
                    {
                        provisioning.EnsureUserInGroupAsync(reseller.Id, KeyGroups.AdminGroupId).GetAwaiter().GetResult();
                    }
                }

                // Konfigurace HTTP pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseMigrationsEndPoint();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // HSTS s preload je nakonfigurované ve službách.
                    app.UseHsts();
                }

                app.UseHttpsRedirection();

                // Bezpečnostní hlavičky včetně Content Security Policy.
                app.Use(async (ctx, next) =>
                {
                    // CSP: zdroje jen z same-origin; inline style/script je povolený kvůli HTMX a Tailwind.
                    ctx.Response.Headers.Append("Content-Security-Policy",
                        "default-src 'self'; " +
                        "script-src 'self' 'unsafe-inline'; " +
                        "style-src 'self' 'unsafe-inline'; " +
                        "img-src 'self' data:; " +
                        "font-src 'self'; " +
                        "frame-ancestors 'self'; " +
                        "form-action 'self'; " +
                        "base-uri 'self'");

                    // Další bezpečnostní hlavičky.
                    ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                    ctx.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
                    ctx.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

                    await next();
                });

                // Middleware pro rate limiting.
                app.UseRateLimiter();

                var supportedCultures = new[] { new CultureInfo("cs"), new CultureInfo("en") };
                var localizationOptions = new RequestLocalizationOptions
                {
                    DefaultRequestCulture = new RequestCulture("cs"),
                    SupportedCultures = supportedCultures,
                    SupportedUICultures = supportedCultures,
                    // Locale je řízené cookie (bez fallbacku na Accept-Language).
                    RequestCultureProviders = new IRequestCultureProvider[]
                    {
                        new CookieRequestCultureProvider()
                    }
                };
                app.UseRequestLocalization(localizationOptions);

                app.Use(async (ctx, next) =>
                {
                    ctx.Response.Headers.Append("Vary", "HX-Request");
                    await next();
                });
                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();

                // Uživatelé s bootstrap claimem musí nejdřív změnit přihlašovací údaje.
                app.Use(async (ctx, next) =>
                {
                    if (ctx.User?.Identity?.IsAuthenticated == true
                        && ctx.User.HasClaim(c => c.Type == Authz.Claims.ForceCredentialsChange))
                    {
                        var path = ctx.Request.Path.Value ?? string.Empty;
                        var isStatic = Path.HasExtension(path);

                        bool allowed = isStatic
                                       || path.StartsWith("/account/first-login", StringComparison.OrdinalIgnoreCase)
                                       || path.StartsWith("/auth/logout", StringComparison.OrdinalIgnoreCase)
                                       || path.StartsWith("/auth/login", StringComparison.OrdinalIgnoreCase)
                                       || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
                                       || path.StartsWith("/culture", StringComparison.OrdinalIgnoreCase);

                        if (!allowed)
                        {
                            var url = "/account/first-login";
                            if (ctx.Request.IsHtmx())
                            {
                                ctx.Response.Htmx(h => h.Redirect(url));
                                ctx.Response.StatusCode = StatusCodes.Status200OK;
                                ctx.Response.ContentLength = 0;
                                return;
                            }

                            ctx.Response.Redirect(url);
                            return;
                        }
                    }

                    await next();
                });

                // Globální HTMX error handling:
                // - Auth/forbid výsledky mají navigovat (ne swapnout HTML login/denied do hx-target)
                // - 401/403 převádí na HX-Redirect (stejně tak auth-related 30x redirecty)
                app.Use(async (ctx, next) =>
                {
                    await next();

                    if (!ctx.Request.IsHtmx()) return;
                    if (ctx.Response.HasStarted) return;

                    var cookieOptionsMonitor = ctx.RequestServices.GetRequiredService<IOptionsMonitor<Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions>>();
                    var cookieOptions = cookieOptionsMonitor.Get(IdentityConstants.ApplicationScheme);

                    var loginPath = ctx.Request.PathBase.Add(cookieOptions.LoginPath).ToString();
                    var accessDeniedPath = ctx.Request.PathBase.Add(cookieOptions.AccessDeniedPath).ToString();

                    string currentUrl = ctx.Request.PathBase + ctx.Request.Path + ctx.Request.QueryString;
                    string loginWithReturnUrl = loginPath + Microsoft.AspNetCore.Http.QueryString.Create("returnUrl", currentUrl);

                    // Pozná redirect na login/denied (u cookie auth je to nejčastější 302).
                    bool IsAuthRedirectLocation(string? location)
                    {
                        if (string.IsNullOrWhiteSpace(location)) return false;
                        if (!Uri.TryCreate(location, UriKind.RelativeOrAbsolute, out var uri)) return false;

                        string path = uri.IsAbsoluteUri
                            ? uri.AbsolutePath
                            : (Uri.TryCreate("http://local" + location, UriKind.Absolute, out var tmp) ? tmp.AbsolutePath : location);
                        return path.StartsWith(loginPath, StringComparison.OrdinalIgnoreCase)
                               || path.StartsWith(accessDeniedPath, StringComparison.OrdinalIgnoreCase);
                    }

                    // Zpracuje explicitní 401/403 (běžné pro API nebo custom policies).
                    if (ctx.Response.StatusCode == StatusCodes.Status401Unauthorized)
                    {
                        ctx.Response.Htmx(h => h.Redirect(loginWithReturnUrl));
                        ctx.Response.StatusCode = StatusCodes.Status200OK;
                        ctx.Response.ContentLength = 0;
                        return;
                    }

                    if (ctx.Response.StatusCode == StatusCodes.Status403Forbidden)
                    {
                        ctx.Response.Htmx(h => h.Redirect(accessDeniedPath));
                        ctx.Response.StatusCode = StatusCodes.Status200OK;
                        ctx.Response.ContentLength = 0;
                        return;
                    }

                    // Zpracuje cookie-auth redirecty (nejčastější případ pro MVC + Identity).
                    if (ctx.Response.StatusCode is StatusCodes.Status301MovedPermanently
                        or StatusCodes.Status302Found
                        or StatusCodes.Status303SeeOther
                        or StatusCodes.Status307TemporaryRedirect
                        or StatusCodes.Status308PermanentRedirect)
                    {
                        var location = ctx.Response.Headers.Location.ToString();
                        if (!IsAuthRedirectLocation(location)) return;

                        ctx.Response.Headers.Remove("Location");
                        ctx.Response.Htmx(h => h.Redirect(location));
                        ctx.Response.StatusCode = StatusCodes.Status200OK;
                        ctx.Response.ContentLength = 0;
                    }
                });

                // Health check endpoint.
                app.MapHealthChecks("/health");

                app.MapStaticAssets();
                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
                    .WithStaticAssets();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: bootstrap aplikace (DI registrace, middleware pipeline, routování) a vývojové seedování.
- Závislosti (výběr):
    - EF Core + `ApplicationDbContext`: DB a migrace.
    - ASP.NET Identity: autentizace, role, tokeny; UI je řešené přes controllery `/auth` a `/account`.
    - Serilog: logování do konzole a souboru (rolling).
    - Localization: `.resx` marker typy `SharedResources` a `IdentityUi`.
    - Rate limiting: politiky `auth` a `auth-strict` pro ochranu auth endpointů.
    - HTMX: Vary hlavička, a middleware který převádí 401/403/redirecty na `HX-Redirect`.
- Seedování:
    - Role z `Authz.Roles.All` (idempotentně).
    - Owner (Majitel) účet z konfigurace `BootstrapOwner:*` + claim `ForceCredentialsChange`.
    - KeyGroups: admin group + skupiny pro distributory, členství pro resellery.
- Bezpečnost:
    - CSP + základní bezpečnostní hlavičky.
    - Vynucení „first-login“ pro bootstrap claim (pouští jen whitelisted cesty a statiku).
*/

