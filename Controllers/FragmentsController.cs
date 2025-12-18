using Microsoft.AspNetCore.Mvc;

namespace DigitalniProdukty.Controllers;

[Route("_fragments")]
public class FragmentsController : Controller
{
    [HttpGet("login")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult LoginPartial()
    {
        return PartialView("~/Views/Shared/_LoginPartial.cshtml");
    }

    [HttpGet("sidebar-nav")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult SidebarNav()
    {
        return PartialView("~/Views/Shared/_SidebarNav.cshtml");
    }
}

