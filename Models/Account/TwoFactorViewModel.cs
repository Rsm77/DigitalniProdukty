namespace DigitalniProdukty.Models.Account;

public sealed class TwoFactorViewModel
{
    public bool Is2faEnabled { get; set; }
    public int RecoveryCodesLeft { get; set; }

    public string SharedKey { get; set; } = string.Empty;
    public string SharedKeyFormatted { get; set; } = string.Empty;

    // otpauth:// URI pro autentikátory; používá se jako vstup pro generování QR.
    public string AuthenticatorUri { get; set; } = string.Empty;

    // SVG markup QR kódu; vykresluje se přímo v Razor view (typicky přes Html.Raw).
    public string QrCodeSvg { get; set; } = string.Empty;

    public TwoFactorEnableInputModel Enable { get; set; } = new();

    public string[]? NewRecoveryCodes { get; set; }
}

/*
Podrobnosti (vazby a použité části)

- Účel: ViewModel pro stránku správy 2FA (zobrazení shared key, QR, recovery codes, enable form).
- Vazby na zbytek aplikace:
    - Plní `Services/TwoFactorService.BuildAsync`, který čte stav z Identity a generuje QR přes `Services/QrCodeService`.
    - Používá `Controllers/AccountController` pro GET/POST akce 2FA.
    - `Enable` je vnořený input model pro ověření kódu při zapnutí 2FA.
- Poznámka:
    - `QrCodeSvg` se generuje jen když 2FA není zapnuté (šetří render a zbytečné sdílení secretu).
*/

