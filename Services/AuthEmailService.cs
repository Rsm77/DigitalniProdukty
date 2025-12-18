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

