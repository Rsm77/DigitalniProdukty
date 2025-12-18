using QRCoder;

namespace DigitalniProdukty.Services;

/// <summary>
/// Service for generating QR codes as SVG strings.
/// </summary>
public sealed class QrCodeService
{
    /// <summary>
    /// Generates a QR code as an SVG string.
    /// </summary>
    /// <param name="content">The content to encode in the QR code.</param>
    /// <param name="pixelsPerModule">Size of each module (pixel) in the QR code. Default is 4.</param>
    /// <returns>SVG markup string that can be embedded directly in HTML.</returns>
    public string GenerateSvg(string content, int pixelsPerModule = 4)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        using var svgQrCode = new SvgQRCode(qrCodeData);

        return svgQrCode.GetGraphic(pixelsPerModule);
    }

    /// <summary>
    /// Generates a QR code as an SVG string with custom colors.
    /// </summary>
    /// <param name="content">The content to encode in the QR code.</param>
    /// <param name="darkColor">Color for the dark modules (e.g., "#000000").</param>
    /// <param name="lightColor">Color for the light modules (e.g., "#ffffff").</param>
    /// <param name="pixelsPerModule">Size of each module (pixel) in the QR code. Default is 4.</param>
    /// <returns>SVG markup string that can be embedded directly in HTML.</returns>
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

