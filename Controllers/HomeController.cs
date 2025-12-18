using System.Diagnostics;
using Htmx;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DigitalniProdukty.Models;

namespace DigitalniProdukty.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (Request.IsHtmx())
            {
                return PartialView();
            }

            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            if (Request.IsHtmx())
            {
                return PartialView();
            }

            return View();
        }

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

