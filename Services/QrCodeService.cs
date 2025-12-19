using QRCoder;

namespace DigitalniProdukty.Services;

// Generuje QR kódy jako SVG řetězec (použitelné přímo v Razor view).
public sealed class QrCodeService
{
    // Vygeneruje QR kód pro daný obsah a vrátí SVG markup.
    // Prázdný obsah vrací prázdný string, aby UI nemuselo řešit null.
    public string GenerateSvg(string content, int pixelsPerModule = 4)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        using var svgQrCode = new SvgQRCode(qrCodeData);

        return svgQrCode.GetGraphic(pixelsPerModule);
    }

    // Vygeneruje QR kód se zadanými barvami (světlá/tmavá) a vrátí SVG markup.
    // Používá quiet-zone, aby byl kód dobře čitelný v UI.
    public string GenerateSvg(string content, string darkColor, string lightColor, int pixelsPerModule = 4)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        using var svgQrCode = new SvgQRCode(qrCodeData);

        return svgQrCode.GetGraphic(
            pixelsPerModule,
            darkColor,
            lightColor,
            drawQuietZones: true);
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: generování QR kódů pro 2FA (otpauth URI) jako SVG, aby nebyla potřeba ukládat obrázky.
- Závislosti:
    - Knihovna QRCoder: `QRCodeGenerator`, `SvgQRCode`.
- Vazby na zbytek aplikace:
    - Používá `TwoFactorService`, který sestaví otpauth URI a nechá ho převést na SVG.
    - Výstup (`QrCodeSvg`) se vykresluje ve view pro nastavení 2FA (typicky `Views/Account/TwoFactor.cshtml`).
- Poznámka: SVG se vrací jako string, takže ho lze vložit do HTML (pozor na správné enkódování v UI).
*/

