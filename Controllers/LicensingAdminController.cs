using DigitalniProdukty.Data;
using DigitalniProdukty.Security;
using DigitalniProdukty.Services;
using Htmx;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace DigitalniProdukty.Controllers;

[Authorize(Policy = Authz.Policies.SerialNumbers_Sell)]
[Route("licensing")]
public sealed class LicensingAdminController(
    LicensingService licensing,
    ApplicationDbContext db,
    UserManager<IdentityUser> userManager,
    GroupContextService groupContext,
    IStringLocalizer<SharedResources> t) : Controller
{
    private const string ToastMessageTempDataKey = "ToastMessage";
    private const string ToastKindTempDataKey = "ToastKind";

    // Určí skupinu (KeyGroup) v rámci autorizace: admin může vybrat groupId, jinak se použije scope uživatele.
    private async Task<Guid?> GetScopedGroupIdAsync(Guid? requestedGroupId, CancellationToken ct)
    {
        requestedGroupId = GroupContextService.NormalizeRequestedGroupId(requestedGroupId);

        if (GroupContextService.IsAdmin(User))
        {
            return requestedGroupId;
        }

        return await groupContext.GetUserGroupIdAsync(User, ct);
    }

    // Vrátí scoped groupId nebo připraví toast a vrátí null (používá se pro Forbid flow).
    private async Task<Guid?> RequireScopedGroupIdOrForbidAsync(Guid? requestedGroupId, CancellationToken ct)
    {
        var scoped = await GetScopedGroupIdAsync(requestedGroupId, ct);
        if (GroupContextService.IsAdmin(User)) return scoped;

        if (scoped is null)
        {
            TempData[ToastMessageTempDataKey] = "Chybí přiřazení do skupiny. Kontaktujte prosím administrátora.";
            TempData[ToastKindTempDataKey] = "error";
            return null;
        }

        return scoped;
    }

    // Načte seznam skupin do ViewData pro admin výběr (pro ne-adminy se nic nenačítá).
    private async Task LoadGroupsForAdminAsync(Guid? selectedGroupId, CancellationToken ct)
    {
        if (!GroupContextService.IsAdmin(User)) return;

        ViewData["KeyGroups"] = await db.KeyGroups
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        ViewData["SelectedGroupId"] = selectedGroupId;
    }

    // Najde skupinu (KeyGroup) pro uživatele přes tabulku členství (KeyGroupMembers).
    private async Task<Guid?> GetGroupIdForUserIdAsync(string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId)) return null;

        return await db.KeyGroupMembers
            .Where(x => x.UserId == userId)
            .Select(x => (Guid?)x.GroupId)
            .FirstOrDefaultAsync(ct);
    }

            // Přehled posledních klíčů ve scoped skupině (admin může přepínat skupiny).
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] Guid? groupId, CancellationToken ct)
    {
        var selectedGroupId = GroupContextService.NormalizeRequestedGroupId(groupId);
        var scopedGroupId = await RequireScopedGroupIdOrForbidAsync(selectedGroupId, ct);
        if (!GroupContextService.IsAdmin(User) && scopedGroupId is null) return Forbid();

        var items = await licensing.GetLatestSerialsAsync(take: 50, groupId: scopedGroupId, ct);
        ViewData["Title"] = t["LicensingAdmin.Index.Title"].Value;
        await LoadGroupsForAdminAsync(selectedGroupId, ct);

        if (Request.IsHtmx()) return PartialView("Index", items);
        return View("Index", items);
    }

    // Zobrazí stránku pro generování klíčů (nejdřív ověří scope skupiny).
    [Authorize(Policy = Authz.Policies.SerialNumbers_Generate)]
    [HttpGet("generate")]
    public async Task<IActionResult> GeneratePage([FromQuery] Guid? groupId, CancellationToken ct)
    {
        var selectedGroupId = GroupContextService.NormalizeRequestedGroupId(groupId);

        // Na stránce generování nastaví adminovi default na Admin pool.
        if (GroupContextService.IsAdmin(User) && selectedGroupId is null)
        {
            selectedGroupId = KeyGroups.AdminGroupId;
        }

        var scopedGroupId = await RequireScopedGroupIdOrForbidAsync(selectedGroupId, ct);
        if (!GroupContextService.IsAdmin(User) && scopedGroupId is null) return Forbid();

        ViewData["Title"] = t["LicensingAdmin.Generate.Title"].Value;
        await LoadGroupsForAdminAsync(selectedGroupId, ct);

        var empty = Array.Empty<DigitalniProdukty.Models.SerialNum.SerialNumberModel>();
        if (Request.IsHtmx()) return PartialView("Generate", empty);
        return View("Generate", empty);
    }

    // Vrátí partial s posledními klíči pro aktuální výběr skupiny (používá se pro HTMX refresh).
    [HttpGet("latest-keys")]
    public async Task<IActionResult> LatestKeys([FromQuery] Guid? groupId, CancellationToken ct)
    {
        var selectedGroupId = GroupContextService.NormalizeRequestedGroupId(groupId);
        var scopedGroupId = await RequireScopedGroupIdOrForbidAsync(selectedGroupId, ct);
        if (!GroupContextService.IsAdmin(User) && scopedGroupId is null) return Forbid();

        var items = await licensing.GetLatestSerialsAsync(take: 50, groupId: scopedGroupId, ct);
        ViewData["SelectedGroupId"] = selectedGroupId;
        return PartialView("_LatestKeys", items);
    }

    // Vrátí detail klíče jako partial (pro „details“ panel/modal).
    [HttpGet("details/{id:int}")]
    public async Task<IActionResult> Details(int id, [FromQuery] Guid? groupId, CancellationToken ct)
    {
        var selectedGroupId = GroupContextService.NormalizeRequestedGroupId(groupId);
        var scopedGroupId = await RequireScopedGroupIdOrForbidAsync(selectedGroupId, ct);
        if (!GroupContextService.IsAdmin(User) && scopedGroupId is null) return Forbid();

        var item = await licensing.GetSerialAsync(id, groupId: scopedGroupId, ct);
        if (item is null) return NotFound();

        await LoadGroupsForAdminAsync(selectedGroupId, ct);

        return PartialView("_Details", item);
    }

    public sealed class GenerateInput
    {
        public int Count { get; set; } = 1;
        public int Groups { get; set; } = 4;
        public int GroupLength { get; set; } = 5;
        public int MaxDevices { get; set; } = 1;

        public string? EndUserEmail { get; set; }
    }

    // Vygeneruje klíče do DB (volitelně rovnou přiřadí na end-user email) a vrátí seznam vytvořených.
    [Authorize(Policy = Authz.Policies.SerialNumbers_Generate)]
    [HttpPost("generate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(GenerateInput input, [FromForm] Guid? groupId, CancellationToken ct)
    {
        var selectedGroupId = GroupContextService.NormalizeRequestedGroupId(groupId);

        var endUserEmail = (input.EndUserEmail ?? string.Empty).Trim();
        ViewData["EndUserEmail"] = endUserEmail;

        string? ownerUserId = null;

        Guid targetGroupId;
        if (!string.IsNullOrWhiteSpace(endUserEmail))
        {
            var endUser = await userManager.FindByEmailAsync(endUserEmail);
            if (endUser is null)
            {
                TempData[ToastMessageTempDataKey] = string.Format(t["LicensingAdmin.Toast.UserNotFound"].Value, endUserEmail);
                TempData[ToastKindTempDataKey] = "error";

                ViewData["Title"] = t["LicensingAdmin.Generate.Title"].Value;
                await LoadGroupsForAdminAsync(selectedGroupId, ct);
                var empty = Array.Empty<DigitalniProdukty.Models.SerialNum.SerialNumberModel>();
                if (Request.IsHtmx()) return PartialView("Generate", empty);
                return View("Generate", empty);
            }

            if (!await userManager.IsInRoleAsync(endUser, Authz.Roles.EndUser))
            {
                TempData[ToastMessageTempDataKey] = t["LicensingAdmin.Toast.EndUserRequired"].Value;
                TempData[ToastKindTempDataKey] = "warning";

                ViewData["Title"] = t["LicensingAdmin.Generate.Title"].Value;
                await LoadGroupsForAdminAsync(selectedGroupId, ct);
                var empty = Array.Empty<DigitalniProdukty.Models.SerialNum.SerialNumberModel>();
                if (Request.IsHtmx()) return PartialView("Generate", empty);
                return View("Generate", empty);
            }

            var endUserGroupId = await GetGroupIdForUserIdAsync(endUser.Id, ct);
            if (endUserGroupId is null)
            {
                TempData[ToastMessageTempDataKey] = t["LicensingAdmin.Toast.EndUserNoGroup"].Value;
                TempData[ToastKindTempDataKey] = "error";

                ViewData["Title"] = t["LicensingAdmin.Generate.Title"].Value;
                await LoadGroupsForAdminAsync(selectedGroupId, ct);
                var empty = Array.Empty<DigitalniProdukty.Models.SerialNum.SerialNumberModel>();
                if (Request.IsHtmx()) return PartialView("Generate", empty);
                return View("Generate", empty);
            }

            // Při generování přímo pro end-usera je cílová skupina převzatá z jeho členství.
            targetGroupId = endUserGroupId.Value;
            ownerUserId = endUser.Id;
            selectedGroupId = targetGroupId;

            // Ne-admin smí přiřazovat pouze v rámci své vlastní skupiny.
            if (!GroupContextService.IsAdmin(User))
            {
                var scoped = await RequireScopedGroupIdOrForbidAsync(null, ct);
                if (scoped is null) return Forbid();

                if (scoped.Value != targetGroupId)
                {
                    TempData[ToastMessageTempDataKey] = t["LicensingAdmin.Toast.EndUserOutsideScope"].Value;
                    TempData[ToastKindTempDataKey] = "error";

                    ViewData["Title"] = t["LicensingAdmin.Generate.Title"].Value;
                    await LoadGroupsForAdminAsync(selectedGroupId, ct);
                    var empty = Array.Empty<DigitalniProdukty.Models.SerialNum.SerialNumberModel>();
                    if (Request.IsHtmx()) return PartialView("Generate", empty);
                    return View("Generate", empty);
                }
            }
        }
        else
        {
            if (GroupContextService.IsAdmin(User))
            {
                targetGroupId = selectedGroupId ?? KeyGroups.AdminGroupId;
            }
            else
            {
                var scoped = await RequireScopedGroupIdOrForbidAsync(null, ct);
                if (scoped is null) return Forbid();
                targetGroupId = scoped.Value;
            }
        }

        var created = await licensing.GenerateAsync(input.Count, input.Groups, input.GroupLength, input.MaxDevices, targetGroupId, ownerUserId, ct);

        TempData[ToastMessageTempDataKey] = string.Format(t["LicensingAdmin.Toast.Generated"].Value, input.Count);
        TempData[ToastKindTempDataKey] = "success";

        ViewData["Title"] = t["LicensingAdmin.Generate.Title"].Value;
        await LoadGroupsForAdminAsync(selectedGroupId, ct);

        if (Request.IsHtmx()) return PartialView("Generate", created);
        return View("Generate", created);
    }

    // Přiřadí klíč konkrétnímu uživateli podle emailu (jen pokud ještě nemá OwnerUserId).
    [HttpPost("assign/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int id, [FromForm] string email, [FromForm] Guid? groupId, [FromForm] string? context, CancellationToken ct)
    {
        var selectedGroupId = GroupContextService.NormalizeRequestedGroupId(groupId);
        var scopedGroupId = await RequireScopedGroupIdOrForbidAsync(selectedGroupId, ct);
        if (!GroupContextService.IsAdmin(User) && scopedGroupId is null) return Forbid();

        var serialQuery = db.SerialNumbers.AsQueryable();
        if (scopedGroupId is not null) serialQuery = serialQuery.Where(x => x.GroupId == scopedGroupId.Value);

        var serial = await serialQuery.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (serial is null)
        {
            TempData[ToastMessageTempDataKey] = t["LicensingAdmin.Toast.KeyNotFound"].Value;
            TempData[ToastKindTempDataKey] = "error";
            return await ReturnAfterMutation(id, selectedGroupId, context, ct);
        }

        email = (email ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData[ToastMessageTempDataKey] = t["LicensingAdmin.Toast.EmailRequired"].Value;
            TempData[ToastKindTempDataKey] = "warning";
            return await ReturnAfterMutation(id, selectedGroupId, context, ct);
        }

        if (!string.IsNullOrWhiteSpace(serial.OwnerUserId))
        {
            TempData[ToastMessageTempDataKey] = string.Format(t["LicensingAdmin.Toast.AlreadyAssigned"].Value, serial.OwnerUserId);
            TempData[ToastKindTempDataKey] = "warning";
            return await ReturnAfterMutation(id, selectedGroupId, context, ct);
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            TempData[ToastMessageTempDataKey] = string.Format(t["LicensingAdmin.Toast.UserNotFound"].Value, email);
            TempData[ToastKindTempDataKey] = "error";
            return await ReturnAfterMutation(id, selectedGroupId, context, ct);
        }

        serial.OwnerUserId = user.Id;
        await db.SaveChangesAsync(ct);

        TempData[ToastMessageTempDataKey] = string.Format(t["LicensingAdmin.Toast.Assigned"].Value, email);
        TempData[ToastKindTempDataKey] = "success";

        TriggerLatestKeysReload(context);
        return await ReturnAfterMutation(id, selectedGroupId, context, ct);
    }

    // Změní přiřazení klíče na jiný email (přepíše OwnerUserId).
    [HttpPost("reassign/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reassign(int id, [FromForm] string email, [FromForm] Guid? groupId, [FromForm] string? context, CancellationToken ct)
    {
        var selectedGroupId = GroupContextService.NormalizeRequestedGroupId(groupId);
        var scopedGroupId = await RequireScopedGroupIdOrForbidAsync(selectedGroupId, ct);
        if (!GroupContextService.IsAdmin(User) && scopedGroupId is null) return Forbid();

        var serialQuery = db.SerialNumbers.AsQueryable();
        if (scopedGroupId is not null) serialQuery = serialQuery.Where(x => x.GroupId == scopedGroupId.Value);

        var serial = await serialQuery.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (serial is null)
        {
            TempData[ToastMessageTempDataKey] = t["LicensingAdmin.Toast.KeyNotFound"].Value;
            TempData[ToastKindTempDataKey] = "error";
            return await ReturnAfterMutation(id, selectedGroupId, context, ct);
        }

        email = (email ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData[ToastMessageTempDataKey] = t["LicensingAdmin.Toast.EmailRequired"].Value;
            TempData[ToastKindTempDataKey] = "warning";
            return await ReturnAfterMutation(id, selectedGroupId, context, ct);
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            TempData[ToastMessageTempDataKey] = string.Format(t["LicensingAdmin.Toast.UserNotFound"].Value, email);
            TempData[ToastKindTempDataKey] = "error";
            return await ReturnAfterMutation(id, selectedGroupId, context, ct);
        }

        serial.OwnerUserId = user.Id;
        await db.SaveChangesAsync(ct);

        TempData[ToastMessageTempDataKey] = string.Format(t["LicensingAdmin.Toast.Reassigned"].Value, email);
        TempData[ToastKindTempDataKey] = "success";

        TriggerLatestKeysReload(context);
        return await ReturnAfterMutation(id, selectedGroupId, context, ct);
    }

    // Odebere přiřazení klíče (nastaví OwnerUserId na null).
    [HttpPost("unassign/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unassign(int id, [FromForm] Guid? groupId, [FromForm] string? context, CancellationToken ct)
    {
        var selectedGroupId = GroupContextService.NormalizeRequestedGroupId(groupId);
        var scopedGroupId = await RequireScopedGroupIdOrForbidAsync(selectedGroupId, ct);
        if (!GroupContextService.IsAdmin(User) && scopedGroupId is null) return Forbid();

        var serialQuery = db.SerialNumbers.AsQueryable();
        if (scopedGroupId is not null) serialQuery = serialQuery.Where(x => x.GroupId == scopedGroupId.Value);

        var serial = await serialQuery.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (serial is null)
        {
            TempData[ToastMessageTempDataKey] = t["LicensingAdmin.Toast.KeyNotFound"].Value;
            TempData[ToastKindTempDataKey] = "error";
            return await ReturnAfterMutation(id, selectedGroupId, context, ct);
        }

        if (string.IsNullOrWhiteSpace(serial.OwnerUserId))
        {
            TempData[ToastMessageTempDataKey] = t["LicensingAdmin.Toast.NotAssigned"].Value;
            TempData[ToastKindTempDataKey] = "warning";
            return await ReturnAfterMutation(id, selectedGroupId, context, ct);
        }

        serial.OwnerUserId = null;
        await db.SaveChangesAsync(ct);

        TempData[ToastMessageTempDataKey] = t["LicensingAdmin.Toast.Unassigned"].Value;
        TempData[ToastKindTempDataKey] = "success";

        TriggerLatestKeysReload(context);
        return await ReturnAfterMutation(id, selectedGroupId, context, ct);
    }

    // Revokuje klíč (IsRevoked = true), aby ho nebylo možné použít pro aktivaci.
    [Authorize(Policy = Authz.Policies.LicensingAdmin)]
    [HttpPost("revoke/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(int id, [FromForm] Guid? groupId, [FromForm] string? context, CancellationToken ct)
    {
        var selectedGroupId = GroupContextService.NormalizeRequestedGroupId(groupId);
        var scopedGroupId = await RequireScopedGroupIdOrForbidAsync(selectedGroupId, ct);
        if (!GroupContextService.IsAdmin(User) && scopedGroupId is null) return Forbid();

        var serialQuery = db.SerialNumbers.AsQueryable();
        if (scopedGroupId is not null) serialQuery = serialQuery.Where(x => x.GroupId == scopedGroupId.Value);

        var serial = await serialQuery.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (serial is null)
        {
            TempData[ToastMessageTempDataKey] = t["LicensingAdmin.Toast.KeyNotFound"].Value;
            TempData[ToastKindTempDataKey] = "error";
            return await ReturnAfterMutation(id, selectedGroupId, context, ct);
        }

        serial.IsRevoked = true;
        await db.SaveChangesAsync(ct);

        TempData[ToastMessageTempDataKey] = t["LicensingAdmin.Toast.KeyRevoked"].Value;
        TempData[ToastKindTempDataKey] = "success";

        TriggerLatestKeysReload(context);
        return await ReturnAfterMutation(id, selectedGroupId, context, ct);
    }

    // Zruší revokaci klíče (IsRevoked = false).
    [Authorize(Policy = Authz.Policies.LicensingAdmin)]
    [HttpPost("unrevoke/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unrevoke(int id, [FromForm] Guid? groupId, [FromForm] string? context, CancellationToken ct)
    {
        var selectedGroupId = GroupContextService.NormalizeRequestedGroupId(groupId);
        var scopedGroupId = await RequireScopedGroupIdOrForbidAsync(selectedGroupId, ct);
        if (!GroupContextService.IsAdmin(User) && scopedGroupId is null) return Forbid();

        var serialQuery = db.SerialNumbers.AsQueryable();
        if (scopedGroupId is not null) serialQuery = serialQuery.Where(x => x.GroupId == scopedGroupId.Value);

        var serial = await serialQuery.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (serial is null)
        {
            TempData[ToastMessageTempDataKey] = t["LicensingAdmin.Toast.KeyNotFound"].Value;
            TempData[ToastKindTempDataKey] = "error";
            return await ReturnAfterMutation(id, selectedGroupId, context, ct);
        }

        serial.IsRevoked = false;
        await db.SaveChangesAsync(ct);

        TempData[ToastMessageTempDataKey] = t["LicensingAdmin.Toast.KeyUnrevoked"].Value;
        TempData[ToastKindTempDataKey] = "success";

        TriggerLatestKeysReload(context);
        return await ReturnAfterMutation(id, selectedGroupId, context, ct);
    }

    // Přesune klíč mezi skupinami (pouze admin) a zapíše audit do SerialNumberGroupAudits.
    [Authorize(Roles = Authz.Roles.Admin)]
    [HttpPost("transfer/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transfer(int id, [FromForm] Guid toGroupId, [FromForm] Guid? groupId, [FromForm] string? context, CancellationToken ct)
    {
        var selectedGroupId = GroupContextService.NormalizeRequestedGroupId(groupId);

        if (toGroupId == Guid.Empty)
        {
            TempData[ToastMessageTempDataKey] = "Skupina je povinná.";
            TempData[ToastKindTempDataKey] = "warning";
            return await ReturnAfterMutation(id, selectedGroupId, context, ct);
        }

        var exists = await db.KeyGroups.AnyAsync(x => x.Id == toGroupId, ct);
        if (!exists)
        {
            TempData[ToastMessageTempDataKey] = "Skupina nebyla nalezena.";
            TempData[ToastKindTempDataKey] = "error";
            return await ReturnAfterMutation(id, selectedGroupId, context, ct);
        }

        var serial = await db.SerialNumbers.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (serial is null)
        {
            TempData[ToastMessageTempDataKey] = t["LicensingAdmin.Toast.KeyNotFound"].Value;
            TempData[ToastKindTempDataKey] = "error";
            return await ReturnAfterMutation(id, selectedGroupId, context, ct);
        }

        if (serial.GroupId == toGroupId)
        {
            TempData[ToastMessageTempDataKey] = "Klíč už je v této skupině.";
            TempData[ToastKindTempDataKey] = "warning";
            return await ReturnAfterMutation(id, selectedGroupId, context, ct);
        }

        var fromGroupId = serial.GroupId;
        serial.GroupId = toGroupId;

        var changedByUserId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? string.Empty;
        db.SerialNumberGroupAudits.Add(new DigitalniProdukty.Models.Licensing.SerialNumberGroupAuditModel
        {
            SerialNumberId = serial.Id,
            FromGroupId = fromGroupId,
            ToGroupId = toGroupId,
            ChangedByUserId = changedByUserId,
            ChangedAt = DateTime.UtcNow,
        });

        await db.SaveChangesAsync(ct);

        TempData[ToastMessageTempDataKey] = "Klíč byl přesunut do jiné skupiny.";
        TempData[ToastKindTempDataKey] = "success";

        TriggerLatestKeysReload(context);
        return await ReturnAfterMutation(id, selectedGroupId, context, ct);
    }

    // Přehled instalačních událostí (Installations) ve scoped skupině.
    [Authorize(Policy = Authz.Policies.LicensingAdmin)]
    [HttpGet("installations")]
    public async Task<IActionResult> Installations([FromQuery] Guid? groupId, CancellationToken ct)
    {
        var selectedGroupId = GroupContextService.NormalizeRequestedGroupId(groupId);
        var scopedGroupId = await RequireScopedGroupIdOrForbidAsync(selectedGroupId, ct);
        if (!GroupContextService.IsAdmin(User) && scopedGroupId is null) return Forbid();

        var items = await db.Installations
            .Include(x => x.SerialNumber)
            .Include(x => x.Device)
            .Where(x => scopedGroupId == null || x.SerialNumber.GroupId == scopedGroupId)
            .OrderByDescending(x => x.OccurredAt)
            .Take(200)
            .ToListAsync(ct);

        ViewData["Title"] = t["LicensingAdmin.Installations.Title"].Value;
        ViewData["SelectedGroupId"] = selectedGroupId;

        if (Request.IsHtmx()) return PartialView("Installations", items);
        return View("Installations", items);
    }

    // Po mutaci vrátí detail partial (pokud byl kontext "details"), jinak vrátí Index.
    private async Task<IActionResult> ReturnAfterMutation(int id, Guid? groupId, string? context, CancellationToken ct)
    {
        if (Request.IsHtmx() && string.Equals(context, "details", StringComparison.OrdinalIgnoreCase))
        {
            var selectedGroupId = GroupContextService.NormalizeRequestedGroupId(groupId);
            var scopedGroupId = await RequireScopedGroupIdOrForbidAsync(selectedGroupId, ct);
            if (!GroupContextService.IsAdmin(User) && scopedGroupId is null) return Forbid();

            var item = await licensing.GetSerialAsync(id, groupId: scopedGroupId, ct);
            if (item is null) return NotFound();

            await LoadGroupsForAdminAsync(selectedGroupId, ct);
            return PartialView("_Details", item);
        }

        return await Index(groupId, ct);
    }

    // Vyvolá HTMX trigger pro reload „latest keys“ (pouze při update z detailu).
    private void TriggerLatestKeysReload(string? context)
    {
        if (!Request.IsHtmx()) return;
        if (!string.Equals(context, "details", StringComparison.OrdinalIgnoreCase)) return;

        Response.Headers["HX-Trigger"] = "licensing-updated";
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: admin/distributor/reseller UI pro generování a správu licenčních klíčů (SerialNumbers).
- Routy (výběr):
    - GET `/licensing`: přehled posledních klíčů
    - GET/POST `/licensing/generate`: generování klíčů
    - GET `/licensing/latest-keys`: partial pro HTMX refresh
    - GET `/licensing/details/{id}`: detail klíče (partial)
    - POST `/licensing/assign|reassign|unassign/{id}`: přiřazení na uživatele
    - POST `/licensing/revoke|unrevoke/{id}`: revokace
    - POST `/licensing/transfer/{id}`: přesun mezi skupinami (pouze admin) + audit
    - GET `/licensing/installations`: instalační události
- Scope a bezpečnost:
    - `GroupContextService` určuje, zda je uživatel admin, a jaká skupina je povolená.
    - Ne-admin uživatelé pracují pouze v rámci své skupiny; admin může přepínat `groupId`.
    - Mutace jsou chráněné `ValidateAntiForgeryToken`.
- Závislosti:
    - `LicensingService`: generování a čtení klíčů (včetně pravidel a validace).
    - `ApplicationDbContext`: SerialNumbers + Installations + audity.
    - `UserManager`: lookup uživatelů podle emailu a role.
    - HTMX: `HX-Trigger` signalizuje, že se má refreshnout seznam klíčů.
*/
