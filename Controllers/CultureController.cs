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

    // Nastaví kulturu (jazyk) aplikace do cookie a bezpečně přesměruje zpět.
    // Podporuje full-page i HTMX (vrací HX-Redirect / LocalRedirect).
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

    // Normalizuje vstup kultury (např. cs-CZ -> cs) a vynutí pouze podporované jazyky.
    // Nevalidní/vynechané hodnoty mapuje na výchozí kulturu.
    private static string NormalizeCulture(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return SupportedCultures[0];
        }

        // Přijímá cs/cs-CZ i en/en-US a normalizuje na dvoupísmenný kód.
        var twoLetter = CultureInfo
            .GetCultureInfo(culture)
            .TwoLetterISOLanguageName;

        return SupportedCultures.Contains(twoLetter, StringComparer.OrdinalIgnoreCase)
            ? twoLetter
            : SupportedCultures[0];
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: uživatelské přepnutí jazyka/cultury přes cookie (`CookieRequestCultureProvider`).
- Routy:
    - GET `/culture/set?culture=cs&returnUrl=...`: uloží kulturu a přesměruje na bezpečný návrat.
- Závislosti:
    - `ReturnUrlService`: validuje `returnUrl` (zabrání open-redirect a návratu do `/_fragments`).
    - `HtmxRedirectExtensions`: HTMX-friendly redirect (HX-Redirect) vs. standardní redirect.
- Poznámky:
    - `SupportedCultures` je záměrně malý seznam; neznámé hodnoty padají na výchozí.
*/

