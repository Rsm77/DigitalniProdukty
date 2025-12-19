using System.Text;
using Microsoft.AspNetCore.Identity;
using DigitalniProdukty.Models.Account;

namespace DigitalniProdukty.Services;

public sealed class TwoFactorService(UserManager<IdentityUser> userManager, QrCodeService qrCodeService)
{
    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
    private const string AppName = "DigitalniProdukty";

    // Normalizuje 2FA kód z UI (odstraní mezery a pomlčky), aby prošel ověřením v Identity.
    public string NormalizeCode(string? code)
    {
        return (code ?? string.Empty)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);
    }

    // Postaví model pro stránku 2FA: klíč, formátovaný klíč, otpauth URI a případně QR SVG.
    // QR se generuje jen když 2FA ještě není zapnuté (šetří CPU a zbytečné renderování).
    public async Task<TwoFactorViewModel> BuildAsync(IdentityUser user)
    {
        var is2faEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        var recoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user);

        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            key = await userManager.GetAuthenticatorKeyAsync(user);
        }

        key ??= string.Empty;

        var email = user.Email ?? user.UserName ?? "user";
        var authenticatorUri = GenerateAuthenticatorUri(email, key);

        // QR SVG generujeme jen pokud 2FA ještě není zapnuté.
        var qrCodeSvg = is2faEnabled ? string.Empty : qrCodeService.GenerateSvg(authenticatorUri, pixelsPerModule: 3);

        return new TwoFactorViewModel
        {
            Is2faEnabled = is2faEnabled,
            RecoveryCodesLeft = recoveryCodesLeft,
            SharedKey = key,
            SharedKeyFormatted = FormatKey(key),
            AuthenticatorUri = authenticatorUri,
            QrCodeSvg = qrCodeSvg,
        };
    }

    // Vytvoří otpauth:// URI pro autentikátory (Google Authenticator, Authy, ...).
    // Tento řetězec se pak převádí na QR kód pro pohodlné spárování.
    public static string GenerateAuthenticatorUri(string email, string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;

        return string.Format(
            AuthenticatorUriFormat,
            Uri.EscapeDataString(AppName),
            Uri.EscapeDataString(email),
            key);
    }

    // Přeformátuje secret do skupin po 4 znacích (lepší čitelnost při ručním přepisu).
    private static string FormatKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;

        var chars = key.Replace(" ", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
        const int groupSize = 4;

        var result = new StringBuilder(chars.Length + chars.Length / groupSize);
        for (var i = 0; i < chars.Length; i++)
        {
            if (i > 0 && i % groupSize == 0)
            {
                result.Append(' ');
            }

            result.Append(chars[i]);
        }

        return result.ToString();
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: obsluha 2FA (TOTP) pro UI – sestavení modelu, normalizace kódů a tvorba otpauth URI.
- Závislosti:
    - UserManager<IdentityUser>: čte/zajišťuje authenticator key, zjišťuje stav 2FA a počty recovery kódů.
    - QrCodeService: převádí otpauth URI do SVG QR kódu.
    - TwoFactorViewModel: DTO pro view (v `Models/Account`).
- Vazby na zbytek aplikace:
    - Voláno z `Controllers/AccountController` (stránka pro správu 2FA).
    - Voláno z `Controllers/AuthController` při ověřování loginu s 2FA (NormalizeCode pro vstup).
- Bezpečnost:
    - TOTP secret se nikdy negeneruje „ručně“ – spravuje ho Identity a ukládá do `AspNetUserTokens`.
*/

