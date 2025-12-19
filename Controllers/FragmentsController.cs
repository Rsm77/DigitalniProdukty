using Microsoft.AspNetCore.Mvc;

namespace DigitalniProdukty.Controllers;

[Route("_fragments")]
public class FragmentsController : Controller
{
    // Vrací partial s login/uživatelským menu pro layout (HTMX/fragment endpoint).
    [HttpGet("login")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult LoginPartial()
    {
        return PartialView("~/Views/Shared/_LoginPartial.cshtml");
    }

    // Vrací partial s navigací v sidebaru (renderuje se samostatně přes fragment route).
    [HttpGet("sidebar-nav")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult SidebarNav()
    {
        return PartialView("~/Views/Shared/_SidebarNav.cshtml");
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: centralizace drobných UI fragmentů načítaných samostatně (typicky HTMX).
- Routy:
    - GET `/_fragments/login`: [Views/Shared/_LoginPartial.cshtml](Views/Shared/_LoginPartial.cshtml)
    - GET `/_fragments/sidebar-nav`: [Views/Shared/_SidebarNav.cshtml](Views/Shared/_SidebarNav.cshtml)
- Cache:
    - `ResponseCache(NoStore=true)`: fragmenty mají odrážet aktuální session/role.
- Poznámka k návratům:
    - `ReturnUrlService` explicitně blokuje návrat na `/_fragments`, aby se po loginu nepřistálo na partial view.
*/

