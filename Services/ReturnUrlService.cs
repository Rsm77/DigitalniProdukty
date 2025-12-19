using Microsoft.AspNetCore.Mvc;

namespace DigitalniProdukty.Services;

public sealed class ReturnUrlService
{
    // Z returnUrl vybere bezpečný lokální návrat (neumožní open-redirect mimo aplikaci).
    // Filtruje i návrat do fragment route (`/_fragments`), které nejsou plnohodnotná stránka.
    public string GetSafeReturnUrl(IUrlHelper urlHelper, string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl)
            && urlHelper.IsLocalUrl(returnUrl)
            && !IsFragmentReturnUrl(returnUrl))
        {
            return returnUrl;
        }

        var home = urlHelper.Content("~/");
        return string.IsNullOrWhiteSpace(home) ? "/" : home;
    }

    // Rozpozná návrat do HTMX fragmentů, aby se po loginu nepřistálo na partial view.
    public bool IsFragmentReturnUrl(string returnUrl)
    {
        var trimmed = returnUrl.Trim();
        if (trimmed.Length == 0) return false;

        return trimmed.StartsWith("/_fragments", StringComparison.OrdinalIgnoreCase)
               || trimmed.StartsWith("_fragments", StringComparison.OrdinalIgnoreCase);
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: centralizace logiky kolem `returnUrl` (bezpečnost + UX).
- Závislosti:
    - IUrlHelper: používá `IsLocalUrl` a `Content("~/")`.
- Vazby na zbytek aplikace:
    - `Controllers/AuthController` používá při login/register/2FA flow.
    - `Controllers/CultureController` používá při přepnutí jazyka, aby byl návrat bezpečný.
- Bezpečnost:
    - Brání open-redirect (nepustí externí URL).
    - Brání návratu do `/_fragments`, které jsou určené jen pro HTMX.
*/

