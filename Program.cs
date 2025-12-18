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
        public static void Main(string[] args)
        {
            // Configure Serilog early for bootstrap logging
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

                // Use Serilog as the logging provider
                builder.Host.UseSerilog();

                // Add services to the container.
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString));
                builder.Services.AddDatabaseDeveloperPageExceptionFilter();

                // Health checks with SQL Server connectivity check
                builder.Services.AddHealthChecks()
                    .AddSqlServer(connectionString, name: "sqlserver", tags: ["db", "sql"]);

                // Rate limiting for auth endpoints (brute-force protection)
                builder.Services.AddRateLimiter(options =>
                {
                    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                    // Sliding window policy for authentication endpoints
                    options.AddSlidingWindowLimiter("auth", limiterOptions =>
                    {
                        limiterOptions.PermitLimit = 10;
                        limiterOptions.Window = TimeSpan.FromMinutes(1);
                        limiterOptions.SegmentsPerWindow = 4;
                        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                        limiterOptions.QueueLimit = 2;
                    });

                    // Stricter policy for password reset (prevent email enumeration attacks)
                    options.AddFixedWindowLimiter("auth-strict", limiterOptions =>
                    {
                        limiterOptions.PermitLimit = 3;
                        limiterOptions.Window = TimeSpan.FromMinutes(5);
                        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                        limiterOptions.QueueLimit = 0;
                    });
                });

                // HSTS configuration with preload
                builder.Services.AddHsts(options =>
                {
                    options.Preload = true;
                    options.IncludeSubDomains = true;
                    options.MaxAge = TimeSpan.FromDays(365);
                });

                // Identity backend only (UI is implemented via MVC controllers/views under /auth and /account).
                builder.Services
                    .AddIdentity<IdentityUser, IdentityRole>(options =>
                    {
                        options.SignIn.RequireConfirmedAccount = true;
                        options.User.RequireUniqueEmail = true;
                    })
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders()
                    .AddErrorDescriber<LocalizedIdentityErrorDescriber>();

                // Defensive: avoid empty claim-type strings which can throw when creating ClaimsIdentity
                // (seen as ArgumentException: "The value cannot be an empty string.")
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

                // Our .resx files are embedded with base names like "DigitalniProdukty.SharedResources".
                // Using ResourcesPath would make the localizer look under "...Resources.*" and it would fall back to keys.
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

                // Ensure the database exists and is migrated (Development convenience).
                if (app.Environment.IsDevelopment())
                {
                    using var scope = app.Services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.Migrate();
                }

                // Seed required roles (safe to run multiple times).
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

                // Seed default owner user ("Majitel") (configurable, safe to run multiple times).
                // This role should exist on exactly one account.
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
                                // Ensure the seeded account can log in (RequireConfirmedAccount = true)
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

                // Seed key groups (safe to run multiple times).
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

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseMigrationsEndPoint();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // HSTS with preload configured in services
                    app.UseHsts();
                }

                app.UseHttpsRedirection();

                // Content Security Policy header
                app.Use(async (ctx, next) =>
                {
                    // CSP: restrict sources to same origin, allow inline styles/scripts for HTMX and Tailwind
                    ctx.Response.Headers.Append("Content-Security-Policy",
                        "default-src 'self'; " +
                        "script-src 'self' 'unsafe-inline'; " +
                        "style-src 'self' 'unsafe-inline'; " +
                        "img-src 'self' data:; " +
                        "font-src 'self'; " +
                        "frame-ancestors 'self'; " +
                        "form-action 'self'; " +
                        "base-uri 'self'");

                    // Additional security headers
                    ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                    ctx.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
                    ctx.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

                    await next();
                });

                // Rate limiting middleware
                app.UseRateLimiter();

                var supportedCultures = new[] { new CultureInfo("cs"), new CultureInfo("en") };
                var localizationOptions = new RequestLocalizationOptions
                {
                    DefaultRequestCulture = new RequestCulture("cs"),
                    SupportedCultures = supportedCultures,
                    SupportedUICultures = supportedCultures,
                    // Cookie-driven locale (no Accept-Language fallback by design)
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

                // Force users with a bootstrap-claim to update credentials before accessing the app.
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

                // Global HTMX error handling:
                // - Auth/forbid results should navigate normally (not swap login/access denied HTML into hx-target)
                // - Convert 401/403 to HX-Redirect (and also auth-related 30x redirects)
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

                    // Handle explicit 401/403 (common for APIs or custom policies)
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

                    // Handle cookie-auth redirects (most common for MVC + Identity)
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

                // Health check endpoint
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

