using System.Globalization;
using Htmx;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using DigitalniProdukty.Extensions;
using DigitalniProdukty.Services;

namespace DigitalniProdukty.Controllers;

[Route("culture")]
public sealed class CultureController(ReturnUrlService returnUrlService) : Controller
{
    private static readonly string[] SupportedCultures = ["cs", "en"];

    [HttpGet("set")]
    public IActionResult Set([FromQuery] string culture, [FromQuery] string? returnUrl = null)
    {
        var normalizedCulture = NormalizeCulture(culture);

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(normalizedCulture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
            });

        var safeReturnUrl = returnUrlService.GetSafeReturnUrl(Url, returnUrl);

        return this.HtmxRedirectOrLocalRedirect(safeReturnUrl);
    }

    private static string NormalizeCulture(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return SupportedCultures[0];
        }

        // Accept cs/cs-CZ and en/en-US, normalize to two-letter.
        var twoLetter = CultureInfo
            .GetCultureInfo(culture)
            .TwoLetterISOLanguageName;

        return SupportedCultures.Contains(twoLetter, StringComparer.OrdinalIgnoreCase)
            ? twoLetter
            : SupportedCultures[0];
    }
}

