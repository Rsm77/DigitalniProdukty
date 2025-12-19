using Htmx;
using Microsoft.AspNetCore.Mvc;

namespace DigitalniProdukty.Extensions;

public static class HtmxRedirectExtensions
{
    /// <summary>
    /// Pro HTMX request: nastaví HX-Redirect a vrátí prázdné tělo (zabrání swapnutí HTML do hx-target).
    /// Pro ne-HTMX: provede standardní LocalRedirect.
    /// </summary>
    public static IActionResult HtmxRedirectOrLocalRedirect(this Controller controller, string localUrl)
    {
        if (controller.Request.IsHtmx())
        {
            controller.Response.Htmx(h => h.Redirect(localUrl));
            return new EmptyResult();
        }

        return controller.LocalRedirect(localUrl);
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: sjednocený redirect pro HTMX i klasické požadavky.
- Chování:
    - HTMX: nastaví `HX-Redirect` a vrátí prázdnou odpověď, aby se redirect neproměnil v HTML swap.
    - Non-HTMX: použije `Controller.LocalRedirect`.
- Použití v aplikaci:
    - Controllery typu `AuthController`, `AccountController`, `CultureController` atd.
    - Pomáhá držet konzistentní UX mezi partial a full-page navigací.
*/

