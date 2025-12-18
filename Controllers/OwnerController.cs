using DigitalniProdukty.Data;
using DigitalniProdukty.Security;
using Htmx;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace DigitalniProdukty.Controllers;

[Authorize(Roles = Authz.Roles.Owner)]
[Route("owner")]
public sealed class OwnerController(
    ApplicationDbContext db,
    UserManager<IdentityUser> userManager,
    IStringLocalizer<SharedResources> t) : Controller
{
    private const string ToastMessageTempDataKey = "ToastMessage";
    private const string ToastKindTempDataKey = "ToastKind";

    public sealed record AdminRow(
        string Id,
        string Email,
        string? DisplayName);

    public sealed class IndexVm
    {
        public required IReadOnlyList<AdminRow> Admins { get; init; }
    }

    public sealed class CreateAdminInput
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? DisplayName { get; set; }
    }

    public sealed class EditAdminInput
    {
        public string? UserId { get; set; }
        public string? DisplayName { get; set; }
    }

    public sealed class ResetPasswordInput
    {
        public string? UserId { get; set; }
        public string? NewPassword { get; set; }
    }

    public sealed record DeleteConfirmVm(string UserId, string Email);

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var admins = await userManager.GetUsersInRoleAsync(Authz.Roles.Admin);
        var adminIds = admins.Select(x => x.Id).ToArray();

        var profiles = await db.UserProfiles
            .Where(x => adminIds.Contains(x.UserId))
            .ToDictionaryAsync(x => x.UserId, x => x.DisplayName, ct);

        var rows = admins
            .OrderBy(x => x.Email)
            .Select(u => new AdminRow(
                Id: u.Id,
                Email: u.Email ?? u.UserName ?? u.Id,
                DisplayName: profiles.TryGetValue(u.Id, out var dn) ? dn : null))
            .ToList();

        ViewData["Title"] = t["Owner.Index.Title"].Value;

        var vm = new IndexVm { Admins = rows };
        if (Request.IsHtmx()) return PartialView("Index", vm);
        return View("Index", vm);
    }

    [HttpGet("admins/create")]
    public IActionResult CreateAdmin()
    {
        ViewData["Title"] = t["Owner.Create.Title"].Value;
        var model = new CreateAdminInput();
        if (Request.IsHtmx()) return PartialView("Create", model);
        return View("Create", model);
    }

    [HttpPost("admins/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAdmin(CreateAdminInput input, CancellationToken ct)
    {
        ViewData["Title"] = t["Owner.Create.Title"].Value;

        var email = (input.Email ?? string.Empty).Trim();
        var password = input.Password ?? string.Empty;
        var displayName = (input.DisplayName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(nameof(CreateAdminInput.Email), t["Owner.Create.Email.Required"].Value);
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(nameof(CreateAdminInput.Password), t["Owner.Create.Password.Required"].Value);
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            ModelState.AddModelError(nameof(CreateAdminInput.DisplayName), t["Owner.Create.DisplayName.Required"].Value);
        }

        if (!ModelState.IsValid)
        {
            if (Request.IsHtmx()) return PartialView("Create", input);
            return View("Create", input);
        }

        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            ModelState.AddModelError(nameof(CreateAdminInput.Email), t["Owner.Create.Email.Exists"].Value);
            if (Request.IsHtmx()) return PartialView("Create", input);
            return View("Create", input);
        }

        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };

        var create = await userManager.CreateAsync(user, password);
        if (!create.Succeeded)
        {
            foreach (var e in create.Errors)
            {
                ModelState.AddModelError(string.Empty, e.Description);
            }

            if (Request.IsHtmx()) return PartialView("Create", input);
            return View("Create", input);
        }

        await userManager.AddToRoleAsync(user, Authz.Roles.Admin);

        db.UserProfiles.Add(new DigitalniProdukty.Models.Users.UserProfileModel
        {
            UserId = user.Id,
            DisplayName = displayName,
        });
        await db.SaveChangesAsync(ct);

        TempData[ToastMessageTempDataKey] = t["Owner.Toast.AdminCreated"].Value;
        TempData[ToastKindTempDataKey] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("admins/edit")]
    public async Task<IActionResult> EditAdmin(string? userId, CancellationToken ct)
    {
        var id = (userId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(id)) return RedirectToAction(nameof(Index));

        var user = await userManager.FindByIdAsync(id);
        if (user is null) return RedirectToAction(nameof(Index));

        var isAdmin = await userManager.IsInRoleAsync(user, Authz.Roles.Admin);
        if (!isAdmin) return RedirectToAction(nameof(Index));

        var profile = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == id, ct);

        ViewData["Title"] = t["Owner.Edit.Title"].Value;
        ViewData["AdminEmail"] = user.Email ?? user.UserName ?? user.Id;

        var model = new EditAdminInput
        {
            UserId = id,
            DisplayName = profile?.DisplayName,
        };

        if (Request.IsHtmx()) return PartialView("Edit", model);
        return View("Edit", model);
    }

    [HttpPost("admins/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAdmin(EditAdminInput input, CancellationToken ct)
    {
        var userId = (input.UserId ?? string.Empty).Trim();
        var displayName = (input.DisplayName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction(nameof(Index));

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return RedirectToAction(nameof(Index));

        var isAdmin = await userManager.IsInRoleAsync(user, Authz.Roles.Admin);
        if (!isAdmin) return RedirectToAction(nameof(Index));

        ViewData["Title"] = t["Owner.Edit.Title"].Value;
        ViewData["AdminEmail"] = user.Email ?? user.UserName ?? user.Id;

        if (string.IsNullOrWhiteSpace(displayName))
        {
            ModelState.AddModelError(nameof(EditAdminInput.DisplayName), t["Owner.Edit.DisplayName.Required"].Value);
        }

        if (!ModelState.IsValid)
        {
            if (Request.IsHtmx()) return PartialView("Edit", input);
            return View("Edit", input);
        }

        var profile = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (profile is null)
        {
            db.UserProfiles.Add(new DigitalniProdukty.Models.Users.UserProfileModel
            {
                UserId = userId,
                DisplayName = displayName,
            });
        }
        else
        {
            profile.DisplayName = displayName;
        }

        await db.SaveChangesAsync(ct);

        TempData[ToastMessageTempDataKey] = t["Owner.Toast.AdminUpdated"].Value;
        TempData[ToastKindTempDataKey] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("admins/reset-password")]
    public async Task<IActionResult> ResetPassword(string? userId)
    {
        var id = (userId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(id)) return RedirectToAction(nameof(Index));

        var user = await userManager.FindByIdAsync(id);
        if (user is null) return RedirectToAction(nameof(Index));

        var isAdmin = await userManager.IsInRoleAsync(user, Authz.Roles.Admin);
        if (!isAdmin) return RedirectToAction(nameof(Index));

        ViewData["Title"] = t["Owner.ResetPassword.Title"].Value;
        ViewData["AdminEmail"] = user.Email ?? user.UserName ?? user.Id;

        var model = new ResetPasswordInput { UserId = id };
        if (Request.IsHtmx()) return PartialView("ResetPassword", model);
        return View("ResetPassword", model);
    }

    [HttpPost("admins/reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordInput input)
    {
        var userId = (input.UserId ?? string.Empty).Trim();
        var newPassword = input.NewPassword ?? string.Empty;

        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction(nameof(Index));

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return RedirectToAction(nameof(Index));

        var isAdmin = await userManager.IsInRoleAsync(user, Authz.Roles.Admin);
        if (!isAdmin) return RedirectToAction(nameof(Index));

        ViewData["Title"] = t["Owner.ResetPassword.Title"].Value;
        ViewData["AdminEmail"] = user.Email ?? user.UserName ?? user.Id;

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            ModelState.AddModelError(nameof(ResetPasswordInput.NewPassword), t["Owner.ResetPassword.NewPassword.Required"].Value);
        }

        if (!ModelState.IsValid)
        {
            if (Request.IsHtmx()) return PartialView("ResetPassword", input);
            return View("ResetPassword", input);
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
            {
                ModelState.AddModelError(string.Empty, e.Description);
            }

            if (Request.IsHtmx()) return PartialView("ResetPassword", input);
            return View("ResetPassword", input);
        }

        TempData[ToastMessageTempDataKey] = t["Owner.Toast.PasswordReset"].Value;
        TempData[ToastKindTempDataKey] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("admins/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAdmin(string? userId, CancellationToken ct)
    {
        var id = (userId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(id)) return RedirectToAction(nameof(Index));

        var user = await userManager.FindByIdAsync(id);
        if (user is null) return RedirectToAction(nameof(Index));

        var isAdmin = await userManager.IsInRoleAsync(user, Authz.Roles.Admin);
        if (!isAdmin) return RedirectToAction(nameof(Index));

        var profile = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == id, ct);
        if (profile is not null)
        {
            db.UserProfiles.Remove(profile);
            await db.SaveChangesAsync(ct);
        }

        var delete = await userManager.DeleteAsync(user);
        if (!delete.Succeeded)
        {
            TempData[ToastMessageTempDataKey] = t["Owner.Toast.AdminDeleteFailed"].Value;
            TempData[ToastKindTempDataKey] = "error";
            return RedirectToAction(nameof(Index));
        }

        TempData[ToastMessageTempDataKey] = t["Owner.Toast.AdminDeleted"].Value;
        TempData[ToastKindTempDataKey] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("admins/delete-confirm")]
    public async Task<IActionResult> DeleteAdminConfirm(string? userId)
    {
        var id = (userId ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(id)) return Content(string.Empty);

        var user = await userManager.FindByIdAsync(id);
        if (user is null) return Content(string.Empty);

        var isAdmin = await userManager.IsInRoleAsync(user, Authz.Roles.Admin);
        if (!isAdmin) return Content(string.Empty);

        var vm = new DeleteConfirmVm(
            UserId: user.Id,
            Email: user.Email ?? user.UserName ?? user.Id);

        return PartialView("_DeleteConfirm", vm);
    }

    [HttpGet("modal/clear")]
    public IActionResult ClearModal()
    {
        return Content(string.Empty);
    }
}
