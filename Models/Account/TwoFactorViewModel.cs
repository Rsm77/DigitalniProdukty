namespace DigitalniProdukty.Models.Account;

public sealed class TwoFactorViewModel
{
    public bool Is2faEnabled { get; set; }
    public int RecoveryCodesLeft { get; set; }

    public string SharedKey { get; set; } = string.Empty;
    public string SharedKeyFormatted { get; set; } = string.Empty;

    /// <summary>
    /// The otpauth:// URI for QR code generation.
    /// </summary>
    public string AuthenticatorUri { get; set; } = string.Empty;

    /// <summary>
    /// SVG markup for the QR code. Can be rendered directly in HTML using @Html.Raw().
    /// </summary>
    public string QrCodeSvg { get; set; } = string.Empty;

    public TwoFactorEnableInputModel Enable { get; set; } = new();

    public string[]? NewRecoveryCodes { get; set; }
}

