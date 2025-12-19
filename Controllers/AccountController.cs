using Htmx;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using DigitalniProdukty.Extensions;
using DigitalniProdukty.Models.Account;
using DigitalniProdukty.Services;
using DigitalniProdukty.Data;
using DigitalniProdukty.Security;
using Microsoft.EntityFrameworkCore;

namespace DigitalniProdukty.Controllers;

[Authorize]
[Route("account")]
public sealed class AccountController(
    ApplicationDbContext db,
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    TwoFactorService twoFactorService,
    IStringLocalizer<SharedResources> t) : Controller
{
    private const string ToastMessageTempDataKey = "ToastMessage";
    private const string ToastKindTempDataKey = "ToastKind";
    private const string RecoveryCodesTempDataKey = "AccountRecoveryCodes";

    // Zobrazí profil přihlášeného uživatele (email jen pro čtení, telefon lze upravit).
    [HttpGet("")]
    public async Task<IActionResult> Profile()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var model = new ProfileInputModel
        {
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber
        };

        ViewData["Title"] = t["Account.Profile.Title"];

        if (Request.IsHtmx()) return PartialView("Profile", model);
        return View("Profile", model);
    }

    // Uloží změny profilu (aktuálně jen telefon) a obnoví signin cookie.
    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileInputModel input)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = t["Account.Profile.Title"];
            if (Request.IsHtmx()) return PartialView("Profile", input);
            return View("Profile", input);
        }

        var currentPhone = user.PhoneNumber ?? string.Empty;
        var requestedPhone = input.PhoneNumber ?? string.Empty;

        if (!string.Equals(currentPhone, requestedPhone, StringComparison.Ordinal))
        {
            var setPhone = await userManager.SetPhoneNumberAsync(user, input.PhoneNumber);
            if (!setPhone.Succeeded)
            {
                foreach (var err in setPhone.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }

                ViewData["Title"] = t["Account.Profile.Title"];
                if (Request.IsHtmx()) return PartialView("Profile", input);
                return View("Profile", input);
            }

            await signInManager.RefreshSignInAsync(user);
        }

        TempData[ToastMessageTempDataKey] = t["Account.Profile.Saved"].Value;
        TempData[ToastKindTempDataKey] = "success";
        if (Request.IsHtmx()) return await Profile();
        return RedirectSelf(nameof(Profile));
    }

    // Zobrazí změnu hesla a určí, zda má uživatel lokální heslo nastavené.
    [HttpGet("password")]
    public async Task<IActionResult> Password()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var hasPassword = await userManager.HasPasswordAsync(user);
        ViewData["HasPassword"] = hasPassword;

        ViewData["Title"] = t["Account.Password.Title"];

        if (Request.IsHtmx()) return PartialView("Password", new ChangePasswordInputModel());
        return View("Password", new ChangePasswordInputModel());
    }

    // Provede změnu hesla a po úspěchu obnoví signin cookie.
    [HttpPost("password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Password(ChangePasswordInputModel input)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var hasPassword = await userManager.HasPasswordAsync(user);
        ViewData["HasPassword"] = hasPassword;

        if (!hasPassword)
        {
            ModelState.AddModelError(string.Empty, t["Account.Password.NoPassword"].Value);
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = t["Account.Password.Title"];
            if (Request.IsHtmx()) return PartialView("Password", input);
            return View("Password", input);
        }

        var result = await userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }

            ViewData["Title"] = t["Account.Password.Title"];
            if (Request.IsHtmx()) return PartialView("Password", input);
            return View("Password", input);
        }

        await signInManager.RefreshSignInAsync(user);
        TempData[ToastMessageTempDataKey] = t["Account.Password.Changed"].Value;
        TempData[ToastKindTempDataKey] = "success";
        if (Request.IsHtmx()) return await Password();
        return RedirectSelf(nameof(Password));
    }

    // Zobrazí stav a nastavení 2FA (včetně nově vygenerovaných recovery codes z TempData).
    [HttpGet("2fa")]
    public async Task<IActionResult> TwoFactor()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var model = await twoFactorService.BuildAsync(user);

        if (TempData.TryGetValue(RecoveryCodesTempDataKey, out var codesObj)
            && codesObj is string codesText
            && !string.IsNullOrWhiteSpace(codesText))
        {
            model.NewRecoveryCodes = codesText
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        ViewData["Title"] = t["Account.TwoFactor.Title"];

        if (Request.IsHtmx()) return PartialView("TwoFactor", model);
        return View("TwoFactor", model);
    }

    // Zobrazí poslední klíče (Serials) vlastněné uživatelem; role Owner je sem nepouští.
    [HttpGet("serials")]
    public async Task<IActionResult> Serials()
    {
        // Role Owner (Majitel) v aplikaci nikdy nevlastní licence.
        if (User.IsInRole(Authz.Roles.Owner)) return Forbid();

        var userId = userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var items = await db.SerialNumbers
            .Where(x => x.OwnerUserId == userId)
            .OrderByDescending(x => x.Id)
            .Take(200)
            .ToListAsync();

        ViewData["Title"] = t["Account.Serials.Title"];

        if (Request.IsHtmx()) return PartialView("Serials", items);
        return View("Serials", items);
    }

    // První přihlášení: vynutí změnu emailu a hesla (typicky pro nově založený účet).
    [HttpGet("first-login")]
    public async Task<IActionResult> FirstLogin()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        ViewData["Title"] = t["Account.FirstLogin.Title"].Value;
        ViewData["CurrentEmail"] = user.Email ?? string.Empty;

        var model = new FirstLoginInputModel
        {
            Email = user.Email ?? string.Empty,
        };

        if (Request.IsHtmx()) return PartialView("FirstLogin", model);
        return View("FirstLogin", model);
    }

    // Uloží změnu emailu+hesla a odstraní claim pro vynucení změny přihlašovacích údajů.
    [HttpPost("first-login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FirstLogin(FirstLoginInputModel input)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        ViewData["Title"] = t["Account.FirstLogin.Title"].Value;
        ViewData["CurrentEmail"] = user.Email ?? string.Empty;

        if (!ModelState.IsValid)
        {
            if (Request.IsHtmx()) return PartialView("FirstLogin", input);
            return View("FirstLogin", input);
        }

        var currentEmail = user.Email ?? string.Empty;
        var requestedEmail = (input.Email ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(requestedEmail))
        {
            ModelState.AddModelError(nameof(input.Email), t["Account.FirstLogin.EmailRequired"].Value);
        }
        else if (string.Equals(currentEmail, requestedEmail, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(input.Email), t["Account.FirstLogin.EmailMustChange"].Value);
        }

        if (string.Equals(input.CurrentPassword, input.NewPassword, StringComparison.Ordinal))
        {
            ModelState.AddModelError(nameof(input.NewPassword), t["Account.FirstLogin.PasswordMustChange"].Value);
        }

        if (!ModelState.IsValid)
        {
            if (Request.IsHtmx()) return PartialView("FirstLogin", input);
            return View("FirstLogin", input);
        }

        // Ověří, že nový email/username není obsazen jiným účtem.
        var byName = await userManager.FindByNameAsync(requestedEmail);
        if (byName is not null && byName.Id != user.Id)
        {
            ModelState.AddModelError(nameof(input.Email), t["Account.FirstLogin.EmailTaken"].Value);
        }

        var byEmail = await userManager.FindByEmailAsync(requestedEmail);
        if (byEmail is not null && byEmail.Id != user.Id)
        {
            ModelState.AddModelError(nameof(input.Email), t["Account.FirstLogin.EmailTaken"].Value);
        }

        if (!ModelState.IsValid)
        {
            if (Request.IsHtmx()) return PartialView("FirstLogin", input);
            return View("FirstLogin", input);
        }

        // Email + username (jedna aktualizace, aby nevznikl částečný stav)
        user.UserName = requestedEmail;
        user.Email = requestedEmail;

        // Udrží účet přihlásitelný v této aplikaci (RequireConfirmedAccount = true)
        user.EmailConfirmed = true;

        var update = await userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            foreach (var msg in update.Errors.Select(e => e.Description).Distinct(StringComparer.Ordinal))
            {
                ModelState.AddModelError(string.Empty, msg);
            }
        }

        // Heslo
        var hasPassword = await userManager.HasPasswordAsync(user);
        if (!hasPassword)
        {
            var addPassword = await userManager.AddPasswordAsync(user, input.NewPassword);
            if (!addPassword.Succeeded)
            {
                foreach (var msg in addPassword.Errors.Select(e => e.Description).Distinct(StringComparer.Ordinal))
                {
                    ModelState.AddModelError(string.Empty, msg);
                }
            }
        }
        else
        {
            var changePassword = await userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);
            if (!changePassword.Succeeded)
            {
                foreach (var msg in changePassword.Errors.Select(e => e.Description).Distinct(StringComparer.Ordinal))
                {
                    ModelState.AddModelError(string.Empty, msg);
                }
            }
        }

        if (!ModelState.IsValid)
        {
            if (Request.IsHtmx()) return PartialView("FirstLogin", input);
            return View("FirstLogin", input);
        }

        // Odstraní claim, který vynucuje změnu přihlašovacích údajů.
        var claims = await userManager.GetClaimsAsync(user);
        var forceClaims = claims.Where(c => c.Type == Authz.Claims.ForceCredentialsChange).ToList();
        foreach (var c in forceClaims)
        {
            await userManager.RemoveClaimAsync(user, c);
        }

        await signInManager.RefreshSignInAsync(user);
        TempData[ToastMessageTempDataKey] = t["Account.FirstLogin.Changed"].Value;
        TempData[ToastKindTempDataKey] = "success";

        if (Request.IsHtmx()) return await Profile();
        return RedirectSelf(nameof(Profile));
    }

    // Zapne 2FA po ověření kódu z authenticatoru a vygeneruje recovery codes.
    [HttpPost("2fa/enable")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnableTwoFactor(TwoFactorEnableInputModel input)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var code = twoFactorService.NormalizeCode(input.Code);

        var is2faTokenValid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            userManager.Options.Tokens.AuthenticatorTokenProvider,
            code);

        if (!is2faTokenValid)
        {
            TempData[ToastMessageTempDataKey] = t["Account.TwoFactor.InvalidCode"].Value;
            TempData[ToastKindTempDataKey] = "error";
            if (Request.IsHtmx()) return await TwoFactor();
            return RedirectSelf(nameof(TwoFactor));
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);
        await signInManager.RefreshSignInAsync(user);

        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        TempData[RecoveryCodesTempDataKey] = string.Join('\n', (recoveryCodes ?? []).Where(c => !string.IsNullOrWhiteSpace(c)));
        TempData[ToastMessageTempDataKey] = t["Account.TwoFactor.Enabled"].Value;
        TempData[ToastKindTempDataKey] = "success";

        if (Request.IsHtmx()) return await TwoFactor();
        return RedirectSelf(nameof(TwoFactor));
    }

    // Vypne 2FA pro aktuálního uživatele.
    [HttpPost("2fa/disable")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DisableTwoFactor()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        await userManager.SetTwoFactorEnabledAsync(user, false);
        await signInManager.RefreshSignInAsync(user);

        TempData[ToastMessageTempDataKey] = t["Account.TwoFactor.Disabled"].Value;
        TempData[ToastKindTempDataKey] = "success";
        if (Request.IsHtmx()) return await TwoFactor();
        return RedirectSelf(nameof(TwoFactor));
    }

    // Resetuje authenticator key a současně vypne 2FA (uživatel musí znovu párovat aplikaci).
    [HttpPost("2fa/reset-key")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetAuthenticatorKey()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        await userManager.SetTwoFactorEnabledAsync(user, false);
        await userManager.ResetAuthenticatorKeyAsync(user);
        await signInManager.RefreshSignInAsync(user);

        TempData[ToastMessageTempDataKey] = t["Account.TwoFactor.ResetKey"].Value;
        TempData[ToastKindTempDataKey] = "success";
        if (Request.IsHtmx()) return await TwoFactor();
        return RedirectSelf(nameof(TwoFactor));
    }

    // Vygeneruje nové recovery codes (jen pokud je 2FA zapnutá).
    [HttpPost("2fa/recovery-codes")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateRecoveryCodes()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var is2faEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        if (!is2faEnabled)
        {
            TempData[ToastMessageTempDataKey] = t["Account.TwoFactor.NotEnabled"].Value;
            TempData[ToastKindTempDataKey] = "warning";
            if (Request.IsHtmx()) return await TwoFactor();
            return RedirectSelf(nameof(TwoFactor));
        }

        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        TempData[RecoveryCodesTempDataKey] = string.Join('\n', (recoveryCodes ?? []).Where(c => !string.IsNullOrWhiteSpace(c)));
        TempData[ToastMessageTempDataKey] = t["Account.TwoFactor.RecoveryCodesGenerated"].Value;
        TempData[ToastKindTempDataKey] = "success";
        if (Request.IsHtmx()) return await TwoFactor();
        return RedirectSelf(nameof(TwoFactor));
    }

    // Sestaví URL pro self-redirect na akci AccountControlleru a vrátí HTMX-friendly redirect.
    private IActionResult RedirectSelf(string actionName)
    {
        var url = Url.Action(actionName, "Account") ?? Url.Content("~");
        return this.HtmxRedirectOrLocalRedirect(url);
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: správa účtu přihlášeného uživatele (profil, heslo, 2FA, první přihlášení, přehled licencí).
- Routy:
    - GET/POST `/account`: profil (telefon)
    - GET/POST `/account/password`: změna hesla
    - GET `/account/2fa`: zobrazení stavu 2FA
    - POST `/account/2fa/enable|disable|reset-key|recovery-codes`: správa 2FA
    - GET `/account/serials`: poslední klíče uživatele (kromě role Owner)
    - GET/POST `/account/first-login`: vynucená změna emailu+hesla
- Závislosti:
    - `UserManager`/`SignInManager`: práce s Identity účtem, obnova sign-in cookie.
    - `TwoFactorService`: normalizace kódů a tvorba view-modelu pro 2FA.
    - `ApplicationDbContext`: čtení licenčních klíčů (SerialNumbers).
- Bezpečnost a UX:
    - `ValidateAntiForgeryToken` na mutacích.
    - Podpora HTMX: při HTMX vrací partial a redirect přes `HtmxRedirectOrLocalRedirect`.
*/

