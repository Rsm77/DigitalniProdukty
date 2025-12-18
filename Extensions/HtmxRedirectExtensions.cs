using Htmx;
using Microsoft.AspNetCore.Mvc;

namespace DigitalniProdukty.Extensions;

public static class HtmxRedirectExtensions
{
    /// <summary>
    /// For HTMX requests: emits HX-Redirect and returns an empty body (prevents swapping HTML into hx-target).
    /// For non-HTMX: performs a normal LocalRedirect.
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

