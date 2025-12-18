using System.Text;
using Microsoft.AspNetCore.Identity;
using DigitalniProdukty.Models.Account;

namespace DigitalniProdukty.Services;

public sealed class TwoFactorService(UserManager<IdentityUser> userManager, QrCodeService qrCodeService)
{
    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
    private const string AppName = "DigitalniProdukty";

    public string NormalizeCode(string? code)
    {
        return (code ?? string.Empty)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);
    }

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

        // Generate QR code SVG only if 2FA is not yet enabled
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

    /// <summary>
    /// Generates an otpauth:// URI for authenticator apps (Google Authenticator, Authy, etc.)
    /// This URI can be encoded into a QR code for easy setup.
    /// </summary>
    public static string GenerateAuthenticatorUri(string email, string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;

        return string.Format(
            AuthenticatorUriFormat,
            Uri.EscapeDataString(AppName),
            Uri.EscapeDataString(email),
            key);
    }

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

