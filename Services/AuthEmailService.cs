using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DigitalniProdukty.Services;

public sealed class AuthEmailService(
    UserManager<IdentityUser> userManager,
    IEmailSender emailSender,
    TokenCodec tokenCodec,
    IStringLocalizer<IdentityUi> t)
{
    // Vygeneruje potvrzovací token, zabalí ho do URL-safe podoby a odešle email.
    // Vrací callback URL (hodí se do logu / pro zobrazení ve vývoji).
    public async Task<string> SendConfirmEmailAsync(IdentityUser user, string email, IUrlHelper urlHelper, string scheme)
    {
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedCode = tokenCodec.Encode(code);

        var callbackUrl = urlHelper.Action(
            action: "ConfirmEmail",
            controller: "Auth",
            values: new { userId = user.Id, code = encodedCode },
            protocol: scheme) ?? urlHelper.Content("~/");

        var subject = t["Identity.Email.ConfirmEmail.Subject"].Value;
        var body = string.Format(t["Identity.Email.ConfirmEmail.BodyHtml"].Value, callbackUrl);
        await emailSender.SendEmailAsync(email, subject, body);

        return callbackUrl;
    }

    // Vygeneruje token pro reset hesla, zakóduje ho a odešle email s odkazem.
    // Vrací callback URL, kterou pak UI používá jen jako „kam jsme poslali odkaz“.
    public async Task<string> SendResetPasswordEmailAsync(IdentityUser user, string email, IUrlHelper urlHelper, string scheme)
    {
        var code = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedCode = tokenCodec.Encode(code);

        var callbackUrl = urlHelper.Action(
            action: "ResetPassword",
            controller: "Auth",
            values: new { email, code = encodedCode },
            protocol: scheme) ?? urlHelper.Content("~/");

        var subject = t["Identity.Email.ResetPassword.Subject"].Value;
        var body = string.Format(t["Identity.Email.ResetPassword.BodyHtml"].Value, callbackUrl);
        await emailSender.SendEmailAsync(email, subject, body);

        return callbackUrl;
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: centrální služba pro sestavení a odesílání emailů Identity (potvrzení emailu, reset hesla).
- Závislosti:
    - UserManager<IdentityUser>: generuje bezpečné tokeny (`GenerateEmailConfirmationTokenAsync`, `GeneratePasswordResetTokenAsync`).
    - TokenCodec: převod tokenu do Base64Url formátu vhodného do query stringu (bez `+`, `/`, `=`).
    - IEmailSender: abstrakce odeslání (v dev režimu typicky `NoOpEmailSender`).
    - IStringLocalizer<IdentityUi>: bere lokalizované subject/body z `Resources/IdentityUi*.resx`.
- Vazby na zbytek aplikace:
    - Voláno z `Controllers/AuthController` při registraci a při "Forgot password" flow.
    - Odkazy míří na akce `AuthController.ConfirmEmail` a `AuthController.ResetPassword`.
*/

