using System.Text;
using Htmx;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;
using DigitalniProdukty.Extensions;
using DigitalniProdukty.Models.Auth;
using DigitalniProdukty.Services;
using DigitalniProdukty.Security;
using DigitalniProdukty.Data;
using Microsoft.EntityFrameworkCore;
using DigitalniProdukty.Models.Users;

namespace DigitalniProdukty.Controllers;

[Route("auth")]
[EnableRateLimiting("auth")]
public sealed class AuthController(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    ReturnUrlService returnUrlService,
    TwoFactorService twoFactorService,
    TokenCodec tokenCodec,
    AuthEmailService authEmailService,
    KeyGroupProvisioningService keyGroups,
    GroupContextService groupContext,
    ApplicationDbContext db,
    IWebHostEnvironment env,
    ILogger<AuthController> logger,
    IStringLocalizer<IdentityUi> t) : Controller
{
    private const string TempDataDevConfirmLinkKey = "AuthDevConfirmLink";
    private const string TempDataDevResetLinkKey = "AuthDevResetLink";

    private static readonly HashSet<string> CreatableAccountTypes = new(StringComparer.Ordinal)
    {
        Authz.Roles.EndUser,
        Authz.Roles.Reseller,
        Authz.Roles.Distributor,
    };

    private HashSet<string> GetAllowedAccountTypesForCreator()
    {
        // Admin can create: Distributor, Reseller, EndUser
        if (User.IsInRole(Authz.Roles.Admin))
        {
            return CreatableAccountTypes;
        }

        // Distributor can create: Reseller, EndUser
        if (User.IsInRole(Authz.Roles.Distributor))
        {
            return new HashSet<string>(StringComparer.Ordinal)
            {
                Authz.Roles.EndUser,
                Authz.Roles.Reseller,
            };
        }

        // Reseller can create: EndUser
        return new HashSet<string>(StringComparer.Ordinal)
        {
            Authz.Roles.EndUser,
        };
    }

    private async Task LoadGroupsForAdminAsync(CancellationToken ct)
    {
        if (!User.IsInRole(Authz.Roles.Admin)) return;

        await keyGroups.EnsureAdminGroupExistsAsync(ct);
        ViewData["KeyGroups"] = await db.KeyGroups
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (signInManager.IsSignedIn(User))
        {
            return RedirectToLocal(returnUrl);
        }

        ViewData["Title"] = t["Identity.Login.Title"].Value;
        ViewData["ReturnUrl"] = returnUrlService.GetSafeReturnUrl(Url, returnUrl);

        if (Request.IsHtmx()) return PartialView("Login", new LoginInputModel());
        return View("Login", new LoginInputModel());
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInputModel input, string? returnUrl = null)
    {
        var safeReturnUrl = returnUrlService.GetSafeReturnUrl(Url, returnUrl);
        ViewData["Title"] = t["Identity.Login.Title"].Value;
        ViewData["ReturnUrl"] = safeReturnUrl;

        if (!ModelState.IsValid)
        {
            if (Request.IsHtmx()) return PartialView("Login", input);
            return View("Login", input);
        }

        var result = await signInManager.PasswordSignInAsync(
            input.Email,
            input.Password,
            input.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            logger.LogInformation("User {Email} logged in successfully", input.Email);
            return RedirectToLocal(safeReturnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            logger.LogInformation("User {Email} requires 2FA", input.Email);
            var url = Url.Action(nameof(Login2fa), "Auth", new { returnUrl = safeReturnUrl, rememberMe = input.RememberMe })
                      ?? Url.Content("~");

            return this.HtmxRedirectOrLocalRedirect(url);
        }

        logger.LogWarning("Failed login attempt for {Email}", input.Email);
        ModelState.AddModelError(string.Empty, t["Identity.Login.InvalidAttempt"].Value);
        if (Request.IsHtmx()) return PartialView("Login", input);
        return View("Login", input);
    }

    [HttpGet("login-2fa")]
    public async Task<IActionResult> Login2fa(string? returnUrl = null, bool rememberMe = false)
    {
        var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user is null)
        {
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        ViewData["Title"] = t["Identity.Login2fa.Title"].Value;
        ViewData["ReturnUrl"] = returnUrlService.GetSafeReturnUrl(Url, returnUrl);
        ViewData["RememberMe"] = rememberMe;

        if (Request.IsHtmx()) return PartialView("Login2fa", new Login2faInputModel());
        return View("Login2fa", new Login2faInputModel());
    }

    [HttpPost("login-2fa")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login2fa(Login2faInputModel input, string? returnUrl = null, bool rememberMe = false)
    {
        var safeReturnUrl = returnUrlService.GetSafeReturnUrl(Url, returnUrl);
        ViewData["Title"] = t["Identity.Login2fa.Title"].Value;
        ViewData["ReturnUrl"] = safeReturnUrl;
        ViewData["RememberMe"] = rememberMe;

        if (!ModelState.IsValid)
        {
            if (Request.IsHtmx()) return PartialView("Login2fa", input);
            return View("Login2fa", input);
        }

        var code = twoFactorService.NormalizeCode(input.Code);

        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(code, rememberMe, input.RememberMachine);
        if (result.Succeeded)
        {
            return RedirectToLocal(safeReturnUrl);
        }

        ModelState.AddModelError(string.Empty, t["Identity.Login2fa.InvalidCode"].Value);
        if (Request.IsHtmx()) return PartialView("Login2fa", input);
        return View("Login2fa", input);
    }

    [Authorize(Policy = Authz.Policies.Users_CreateEndUser)]
    [HttpGet("register")]
    public async Task<IActionResult> Register(string? returnUrl = null, CancellationToken ct = default)
    {
        ViewData["Title"] = t["Identity.Register.Title"].Value;
        ViewData["ReturnUrl"] = returnUrlService.GetSafeReturnUrl(Url, returnUrl);

        var model = new RegisterInputModel
        {
            AccountType = User.IsInRole(Authz.Roles.Admin) ? Authz.Roles.Distributor : Authz.Roles.EndUser,
            TargetGroupId = KeyGroups.AdminGroupId,
        };

        if (!User.IsInRole(Authz.Roles.Admin) || !string.Equals(model.AccountType, Authz.Roles.Distributor, StringComparison.Ordinal))
        {
            await LoadGroupsForAdminAsync(ct);
        }

        if (Request.IsHtmx()) return PartialView("Register", model);
        return View("Register", model);
    }

    [Authorize(Policy = Authz.Policies.Users_CreateEndUser)]
    [HttpGet("register-meta")]
    public async Task<IActionResult> RegisterAccountTypeMeta(RegisterInputModel input, CancellationToken ct = default)
    {
        if (!User.IsInRole(Authz.Roles.Admin) && !User.IsInRole(Authz.Roles.Distributor))
        {
            return Content(string.Empty);
        }

        var accountType = (input.AccountType ?? string.Empty).Trim();
        if (User.IsInRole(Authz.Roles.Admin) && !string.Equals(accountType, Authz.Roles.Distributor, StringComparison.Ordinal))
        {
            await LoadGroupsForAdminAsync(ct);
        }

        return PartialView("_RegisterAccountTypeMeta", input);
    }

    [Authorize(Policy = Authz.Policies.Users_CreateEndUser)]
    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterInputModel input, string? returnUrl = null, CancellationToken ct = default)
    {
        var safeReturnUrl = returnUrlService.GetSafeReturnUrl(Url, returnUrl);
        ViewData["Title"] = t["Identity.Register.Title"].Value;
        ViewData["ReturnUrl"] = safeReturnUrl;

        var allowedAccountTypes = GetAllowedAccountTypesForCreator();

        var accountType = Authz.Roles.EndUser;
        if (!string.IsNullOrWhiteSpace(input.AccountType))
        {
            accountType = input.AccountType.Trim();
        }

        if (!CreatableAccountTypes.Contains(accountType) || !allowedAccountTypes.Contains(accountType))
        {
            ModelState.AddModelError(nameof(RegisterInputModel.AccountType), DigitalniProdukty.Resources.Annotations.Validation_InvalidAccountType);
        }

        Guid? requestedTargetGroupId = null;
        if (User.IsInRole(Authz.Roles.Admin) && !string.Equals(accountType, Authz.Roles.Distributor, StringComparison.Ordinal))
        {
            requestedTargetGroupId = input.TargetGroupId is null || input.TargetGroupId == Guid.Empty
                ? null
                : input.TargetGroupId;

            if (requestedTargetGroupId is null)
            {
                ModelState.AddModelError(nameof(RegisterInputModel.TargetGroupId), t["Identity.Register.TargetGroup.Required"].Value);
            }
            else
            {
                await keyGroups.EnsureAdminGroupExistsAsync(ct);
                var exists = await db.KeyGroups.AnyAsync(x => x.Id == requestedTargetGroupId.Value, ct);
                if (!exists)
                {
                    ModelState.AddModelError(nameof(RegisterInputModel.TargetGroupId), t["Identity.Register.TargetGroup.Invalid"].Value);
                }
            }
        }

        var requestedDisplayName = (input.DisplayName ?? string.Empty).Trim();
        if ((string.Equals(accountType, Authz.Roles.Reseller, StringComparison.Ordinal)
             && (User.IsInRole(Authz.Roles.Admin) || User.IsInRole(Authz.Roles.Distributor)))
            || (string.Equals(accountType, Authz.Roles.Distributor, StringComparison.Ordinal) && User.IsInRole(Authz.Roles.Admin)))
        {
            if (string.IsNullOrWhiteSpace(requestedDisplayName))
            {
                ModelState.AddModelError(nameof(RegisterInputModel.DisplayName), t["Identity.Register.DisplayName.Required"].Value);
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadGroupsForAdminAsync(ct);
            if (Request.IsHtmx()) return PartialView("Register", input);
            return View("Register", input);
        }

        var user = new IdentityUser
        {
            UserName = input.Email,
            Email = input.Email
        };

        var result = await userManager.CreateAsync(user, input.Password);
        if (result.Succeeded)
        {
            var addRole = await userManager.AddToRoleAsync(user, accountType);
            if (!addRole.Succeeded)
            {
                logger.LogWarning("Failed to assign default role to new user {Email}: {Errors}", input.Email, string.Join(", ", addRole.Errors.Select(e => e.Code)));
                await userManager.DeleteAsync(user);
                ModelState.AddModelError(string.Empty, "Registrace se nezdařila. Zkuste to prosím znovu.");
                await LoadGroupsForAdminAsync(ct);
                if (Request.IsHtmx()) return PartialView("Register", input);
                return View("Register", input);
            }

            try
            {
                await keyGroups.EnsureAdminGroupExistsAsync(ct);

                if (!string.IsNullOrWhiteSpace(requestedDisplayName))
                {
                    var existingProfile = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id, ct);
                    if (existingProfile is null)
                    {
                        db.UserProfiles.Add(new UserProfileModel
                        {
                            UserId = user.Id,
                            DisplayName = requestedDisplayName,
                            CreatedAt = DateTime.UtcNow,
                        });
                    }
                    else
                    {
                        existingProfile.DisplayName = requestedDisplayName;
                    }

                    await db.SaveChangesAsync(ct);
                }

                if (User.IsInRole(Authz.Roles.Admin))
                {
                    if (string.Equals(accountType, Authz.Roles.Distributor, StringComparison.Ordinal))
                    {
                        await keyGroups.EnsureGroupForDistributorAsync(user, requestedDisplayName, ct);
                    }
                    else
                    {
                        var targetGroupId = requestedTargetGroupId ?? KeyGroups.AdminGroupId;
                        await keyGroups.EnsureUserInGroupAsync(user.Id, targetGroupId, ct);
                    }
                }
                else if (User.IsInRole(Authz.Roles.Distributor) || User.IsInRole(Authz.Roles.Reseller))
                {
                    var creatorGroupId = await groupContext.GetRequiredUserGroupIdAsync(User, ct);
                    await keyGroups.EnsureUserInGroupAsync(user.Id, creatorGroupId, ct);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to provision KeyGroup for new user {Email}", input.Email);
                await userManager.DeleteAsync(user);
                ModelState.AddModelError(string.Empty, "Registrace se nezdařila. Zkuste to prosím znovu.");
                await LoadGroupsForAdminAsync(ct);
                if (Request.IsHtmx()) return PartialView("Register", input);
                return View("Register", input);
            }

            logger.LogInformation("New user registered: {Email} (Role: {Role})", input.Email, accountType);
            var callbackUrl = await authEmailService.SendConfirmEmailAsync(user, input.Email, Url, Request.Scheme);
            if (env.IsDevelopment()) TempData[TempDataDevConfirmLinkKey] = callbackUrl;

            var url = Url.Action(nameof(RegisterConfirmation), "Auth", new { email = input.Email })
                      ?? Url.Content("~");

            return this.HtmxRedirectOrLocalRedirect(url);
        }

        logger.LogWarning("Failed registration attempt for {Email}: {Errors}", input.Email, string.Join(", ", result.Errors.Select(e => e.Code)));
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        await LoadGroupsForAdminAsync(ct);
        if (Request.IsHtmx()) return PartialView("Register", input);
        return View("Register", input);
    }

    [HttpGet("register-confirmation")]
    public IActionResult RegisterConfirmation(string? email = null)
    {
        ViewData["Title"] = t["Identity.RegisterConfirmation.Title"].Value;
        ViewData["Email"] = email ?? string.Empty;
        ViewData["DevLink"] = TempData[TempDataDevConfirmLinkKey] as string;

        if (Request.IsHtmx()) return PartialView("RegisterConfirmation");
        return View("RegisterConfirmation");
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string? userId = null, string? code = null)
    {
        ViewData["Title"] = t["Identity.ConfirmEmail.Title"].Value;

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
        {
            ViewData["ConfirmEmailStatus"] = t["Identity.ConfirmEmail.Failure"].Value;
            ViewData["ConfirmEmailStatusKind"] = "error";
            if (Request.IsHtmx()) return PartialView("ConfirmEmail");
            return View("ConfirmEmail");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            ViewData["ConfirmEmailStatus"] = t["Identity.ConfirmEmail.UserNotFound"].Value;
            ViewData["ConfirmEmailStatusKind"] = "error";
            if (Request.IsHtmx()) return PartialView("ConfirmEmail");
            return View("ConfirmEmail");
        }

        var decodedCode = tokenCodec.DecodeOrRaw(code);

        var result = await userManager.ConfirmEmailAsync(user, decodedCode);
        ViewData["ConfirmEmailStatus"] = result.Succeeded
            ? t["Identity.ConfirmEmail.Success"].Value
            : t["Identity.ConfirmEmail.Failure"].Value;
        ViewData["ConfirmEmailStatusKind"] = result.Succeeded ? "success" : "error";

        if (Request.IsHtmx()) return PartialView("ConfirmEmail");
        return View("ConfirmEmail");
    }

    [HttpGet("forgot-password")]
    public IActionResult ForgotPassword()
    {
        ViewData["Title"] = t["Identity.ForgotPassword.Title"].Value;

        if (Request.IsHtmx()) return PartialView("ForgotPassword", new ForgotPasswordInputModel());
        return View("ForgotPassword", new ForgotPasswordInputModel());
    }

    [HttpPost("forgot-password")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth-strict")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordInputModel input)
    {
        ViewData["Title"] = t["Identity.ForgotPassword.Title"].Value;

        if (!ModelState.IsValid)
        {
            if (Request.IsHtmx()) return PartialView("ForgotPassword", input);
            return View("ForgotPassword", input);
        }

        // Log the request (but not whether user exists - prevent enumeration)
        logger.LogInformation("Password reset requested for email: {Email}", input.Email);

        var user = await userManager.FindByEmailAsync(input.Email);
        if (user is not null && await userManager.IsEmailConfirmedAsync(user))
        {
            var callbackUrl = await authEmailService.SendResetPasswordEmailAsync(user, input.Email, Url, Request.Scheme);
            if (env.IsDevelopment()) TempData[TempDataDevResetLinkKey] = callbackUrl;
        }

        var url = Url.Action(nameof(ForgotPasswordConfirmation), "Auth") ?? Url.Content("~");
        return this.HtmxRedirectOrLocalRedirect(url);
    }

    [HttpGet("forgot-password-confirmation")]
    public IActionResult ForgotPasswordConfirmation()
    {
        ViewData["Title"] = t["Identity.ForgotPasswordConfirmation.Title"].Value;
        ViewData["DevLink"] = TempData[TempDataDevResetLinkKey] as string;

        if (Request.IsHtmx()) return PartialView("ForgotPasswordConfirmation");
        return View("ForgotPasswordConfirmation");
    }

    [HttpGet("reset-password")]
    public IActionResult ResetPassword(string? email = null, string? code = null)
    {
        ViewData["Title"] = t["Identity.ResetPassword.Title"].Value;

        var model = new ResetPasswordInputModel
        {
            Email = email ?? string.Empty,
            Code = code ?? string.Empty
        };

        if (Request.IsHtmx()) return PartialView("ResetPassword", model);
        return View("ResetPassword", model);
    }

    [HttpPost("reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordInputModel input)
    {
        ViewData["Title"] = t["Identity.ResetPassword.Title"].Value;

        if (!ModelState.IsValid)
        {
            if (Request.IsHtmx()) return PartialView("ResetPassword", input);
            return View("ResetPassword", input);
        }

        var user = await userManager.FindByEmailAsync(input.Email);
        if (user is null)
        {
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        var decodedCode = tokenCodec.DecodeOrRaw(input.Code);

        var result = await userManager.ResetPasswordAsync(user, decodedCode, input.Password);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        if (Request.IsHtmx()) return PartialView("ResetPassword", input);
        return View("ResetPassword", input);
    }

    [HttpGet("reset-password-confirmation")]
    public IActionResult ResetPasswordConfirmation()
    {
        ViewData["Title"] = t["Identity.ResetPasswordConfirmation.Title"].Value;

        if (Request.IsHtmx()) return PartialView("ResetPasswordConfirmation");
        return View("ResetPasswordConfirmation");
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await signInManager.SignOutAsync();
        return RedirectToLocal(returnUrl);
    }

    [HttpGet("denied")]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = t["Identity.AccessDenied.Title"].Value;
        if (Request.IsHtmx()) return PartialView("AccessDenied");
        return View("AccessDenied");
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        var safe = returnUrlService.GetSafeReturnUrl(Url, returnUrl);
        return this.HtmxRedirectOrLocalRedirect(safe);
    }
}

