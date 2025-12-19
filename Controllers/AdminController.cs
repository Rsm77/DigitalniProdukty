using DigitalniProdukty.Data;
using DigitalniProdukty.Models.Licensing;
using DigitalniProdukty.Security;
using DigitalniProdukty.Services;
using Htmx;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace DigitalniProdukty.Controllers;

[Authorize(Roles = Authz.Roles.Admin)]
[Route("admin")]
public sealed class AdminController(
    ApplicationDbContext db,
    UserManager<IdentityUser> userManager,
    KeyGroupProvisioningService keyGroups,
    IStringLocalizer<SharedResources> t) : Controller
{
    private const string ToastMessageTempDataKey = "ToastMessage";
    private const string ToastKindTempDataKey = "ToastKind";

    public sealed record GroupRow(Guid Id, string Name);

    public sealed record UserRow(
        string Id,
        string Email,
        string? DisplayName,
        string Role,
        Guid? GroupId,
        string? GroupName);

    public sealed class IndexVm
    {
        public required IReadOnlyList<GroupRow> Groups { get; init; }
        public required IReadOnlyList<UserRow> Users { get; init; }
        public required IReadOnlyList<string> Roles { get; init; }
    }

    // Přehled uživatelů a skupin pro admin UI (role + skupina + display name).
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var groups = await db.KeyGroups
            .OrderBy(x => x.Name)
            .Select(g => new GroupRow(
                g.Id,
                g.Name))
            .ToListAsync(ct);

        var groupNames = groups.ToDictionary(x => x.Id, x => x.Name);

        var memberships = await db.KeyGroupMembers
            .ToDictionaryAsync(x => x.UserId, x => x.GroupId, ct);

        var profiles = await db.UserProfiles
            .ToDictionaryAsync(x => x.UserId, x => x.DisplayName, ct);

        var users = await userManager.Users
            .OrderBy(x => x.Email)
            .ToListAsync(ct);

        var userRows = new List<UserRow>(capacity: users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault(r => Authz.Roles.All.Contains(r, StringComparer.Ordinal))
                              ?? roles.FirstOrDefault()
                              ?? string.Empty;

            Guid? groupId = null;
            string? groupName = null;
            if (memberships.TryGetValue(user.Id, out var gid))
            {
                groupId = gid;
                if (groupNames.TryGetValue(gid, out var gn)) groupName = gn;
            }

            userRows.Add(new UserRow(
                Id: user.Id,
                Email: user.Email ?? user.UserName ?? user.Id,
                DisplayName: profiles.TryGetValue(user.Id, out var dn) ? dn : null,
                Role: primaryRole,
                GroupId: groupId,
                GroupName: groupName));
        }

        ViewData["Title"] = t["Admin.Index.Title"].Value;

        var vm = new IndexVm
        {
            Groups = groups,
            Users = userRows,
            Roles = Authz.Roles.AdminManageable,
        };

        if (Request.IsHtmx()) return PartialView("Index", vm);
        return View("Index", vm);
    }

    public sealed class UpdateUserInput
    {
        public string? UserId { get; set; }
        public string? Role { get; set; }
        public Guid? GroupId { get; set; }
        public string? DisplayName { get; set; }
    }

    // Upraví roli, skupinu a display name uživatele (s governance omezeními).
    [HttpPost("users/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUser(UpdateUserInput input, CancellationToken ct)
    {
        var userId = (input.UserId ?? string.Empty).Trim();
        var role = (input.Role ?? string.Empty).Trim();
        var displayName = (input.DisplayName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(userId))
        {
            TempData[ToastMessageTempDataKey] = t["Admin.Toast.InvalidUser"].Value;
            TempData[ToastKindTempDataKey] = "error";
            return RedirectToAction(nameof(Index));
        }

        if (!Authz.Roles.AdminManageable.Contains(role, StringComparer.Ordinal))
        {
            TempData[ToastMessageTempDataKey] = t["Admin.Toast.InvalidRole"].Value;
            TempData[ToastKindTempDataKey] = "error";
            return RedirectToAction(nameof(Index));
        }

        // Governance: Admin UI nesmí spravovat roli Owner (Majitel) ani účty Admin.
        if (string.Equals(role, Authz.Roles.Admin, StringComparison.Ordinal))
        {
            TempData[ToastMessageTempDataKey] = t["Admin.Toast.AdminRoleOwnerOnly"].Value;
            TempData[ToastKindTempDataKey] = "warning";
            return RedirectToAction(nameof(Index));
        }

        // Reseller/EndUser musí být explicitně přiřazeni do skupiny; Distributor má skupinu automaticky.
        if (role != Authz.Roles.Admin
            && role != Authz.Roles.Distributor
            && (input.GroupId is null || input.GroupId == Guid.Empty))
        {
            TempData[ToastMessageTempDataKey] = t["Admin.Toast.GroupRequired"].Value;
            TempData[ToastKindTempDataKey] = "warning";
            return RedirectToAction(nameof(Index));
        }

        if ((string.Equals(role, Authz.Roles.Admin, StringComparison.Ordinal)
             || string.Equals(role, Authz.Roles.Distributor, StringComparison.Ordinal)
             || string.Equals(role, Authz.Roles.Reseller, StringComparison.Ordinal))
            && string.IsNullOrWhiteSpace(displayName))
        {
            TempData[ToastMessageTempDataKey] = t["Admin.Toast.DisplayNameRequired"].Value;
            TempData[ToastKindTempDataKey] = "warning";
            return RedirectToAction(nameof(Index));
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            TempData[ToastMessageTempDataKey] = t["Admin.Toast.InvalidUser"].Value;
            TempData[ToastKindTempDataKey] = "error";
            return RedirectToAction(nameof(Index));
        }

        // Zabrání tomu, aby Admin UI měnilo účty Owner (Majitel) nebo Admin.
        var isOwner = await userManager.IsInRoleAsync(user, Authz.Roles.Owner);
        if (isOwner)
        {
            TempData[ToastMessageTempDataKey] = t["Admin.Toast.OwnerAccountReadOnly"].Value;
            TempData[ToastKindTempDataKey] = "warning";
            return RedirectToAction(nameof(Index));
        }

        var isAdmin = await userManager.IsInRoleAsync(user, Authz.Roles.Admin);
        if (isAdmin)
        {
            TempData[ToastMessageTempDataKey] = t["Admin.Toast.AdminAccountOwnerOnly"].Value;
            TempData[ToastKindTempDataKey] = "warning";
            return RedirectToAction(nameof(Index));
        }

        // Normalizace cílové skupiny (Admin není nikdy scopeovaný skupinou).
        Guid? desiredGroupId;
        desiredGroupId = input.GroupId is null || input.GroupId == Guid.Empty ? (Guid?)null : input.GroupId;

        // Security: nikdy nedovolí neumístit ne-admin role do Admin tenant/poolu.
        if (desiredGroupId == KeyGroups.AdminGroupId)
        {
            TempData[ToastMessageTempDataKey] = t["Admin.Toast.AdminGroupForbidden"].Value;
            TempData[ToastKindTempDataKey] = "warning";
            return RedirectToAction(nameof(Index));
        }

        // Distributor: skupina je vždy auto-provisionovaná (ruční volba je zakázaná).
        if (string.Equals(role, Authz.Roles.Distributor, StringComparison.Ordinal))
        {
            await keyGroups.EnsureAdminGroupExistsAsync(ct);
            var ensuredGroupId = await keyGroups.EnsureGroupForDistributorAsync(user, displayName, ct);
            desiredGroupId = ensuredGroupId;
        }

        if (desiredGroupId is not null)
        {
            var groupExists = await db.KeyGroups.AnyAsync(x => x.Id == desiredGroupId.Value, ct);
            if (!groupExists)
            {
                TempData[ToastMessageTempDataKey] = t["Admin.Toast.InvalidGroup"].Value;
                TempData[ToastKindTempDataKey] = "error";
                return RedirectToAction(nameof(Index));
            }
        }

        // Role: vynutí přesně jednu z "našich" známých rolí.
        var currentRoles = await userManager.GetRolesAsync(user);
        foreach (var knownRole in Authz.Roles.AdminManageable)
        {
            if (!string.Equals(knownRole, role, StringComparison.Ordinal) && currentRoles.Contains(knownRole, StringComparer.Ordinal))
            {
                await userManager.RemoveFromRoleAsync(user, knownRole);
            }
        }

        if (!currentRoles.Contains(role, StringComparer.Ordinal))
        {
            await userManager.AddToRoleAsync(user, role);
        }

        // Členství ve skupině: 1 řádek na uživatele (záměrný design).
        var member = await db.KeyGroupMembers.FirstOrDefaultAsync(x => x.UserId == user.Id, ct);
        if (desiredGroupId is null)
        {
            if (member is not null)
            {
                db.KeyGroupMembers.Remove(member);
            }
        }
        else
        {
            if (member is null)
            {
                db.KeyGroupMembers.Add(new Models.Licensing.KeyGroupMemberModel
                {
                    GroupId = desiredGroupId.Value,
                    UserId = user.Id,
                });
            }
            else
            {
                member.GroupId = desiredGroupId.Value;
            }
        }

        await db.SaveChangesAsync(ct);

        // Display name (v DB volitelné, ale pro některé role vyžadované politikou).
        var profile = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id, ct);
        if (string.IsNullOrWhiteSpace(displayName))
        {
            if (profile is not null)
            {
                db.UserProfiles.Remove(profile);
                await db.SaveChangesAsync(ct);
            }
        }
        else
        {
            if (profile is null)
            {
                db.UserProfiles.Add(new DigitalniProdukty.Models.Users.UserProfileModel
                {
                    UserId = user.Id,
                    DisplayName = displayName,
                });
            }
            else
            {
                profile.DisplayName = displayName;
            }

            await db.SaveChangesAsync(ct);
        }

        TempData[ToastMessageTempDataKey] = t["Admin.Toast.UserUpdated"].Value;
        TempData[ToastKindTempDataKey] = "success";
        return RedirectToAction(nameof(Index));
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: interní Admin UI pro správu uživatelů, rolí a skupin (KeyGroups).
- Routy:
    - GET `/admin`: přehled (uživatelé + jejich role + skupiny)
    - POST `/admin/users/update`: změna role/skupiny/display name
- Závislosti:
    - `ApplicationDbContext`: čtení/úprava `KeyGroups`, `KeyGroupMembers`, `UserProfiles`.
    - `UserManager`: práce s Identity uživateli a rolemi.
    - `KeyGroupProvisioningService`: zajištění skupin (Admin group, distributor group).
- Bezpečnost (governance):
    - Správa účtů `Owner` a `Admin` je zakázaná (řeší jen Owner UI).
    - Ne-admin role nesmí být přiřazeny do `KeyGroups.AdminGroupId`.
*/
