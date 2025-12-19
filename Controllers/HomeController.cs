using System.Diagnostics;
using Htmx;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DigitalniProdukty.Models;

namespace DigitalniProdukty.Controllers
{
    public class HomeController : Controller
    {
        // Výchozí veřejná stránka; při HTMX vrací pouze partial bez layoutu.
        public IActionResult Index()
        {
            if (Request.IsHtmx())
            {
                return PartialView();
            }

            return View();
        }

        // Ukázková chráněná stránka (vyžaduje přihlášení); podporuje i HTMX.
        [Authorize]
        public IActionResult Privacy()
        {
            if (Request.IsHtmx())
            {
                return PartialView();
            }

            return View();
        }

        // Standardní chybová stránka (bez cache) s RequestId pro diagnostiku.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };

            if (Request.IsHtmx())
            {
                return PartialView("~/Views/Shared/Error.cshtml", model);
            }

            return View(model);
        }
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: základní veřejné stránky a společné error zobrazení.
- Routy:
    - GET `/`: `Index()`
    - GET `/Home/Privacy`: `Privacy()` (vyžaduje přihlášení)
    - GET `/Home/Error`: `Error()` (vykresluje i pro výjimky/pády)
- HTMX:
    - Pokud je request HTMX, vrací `PartialView()` (bez layoutu).
- Vazby na views:
    - [Views/Home/Index.cshtml](Views/Home/Index.cshtml), [Views/Home/Privacy.cshtml](Views/Home/Privacy.cshtml)
    - [Views/Shared/Error.cshtml](Views/Shared/Error.cshtml)
*/

