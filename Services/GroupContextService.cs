using System.Security.Claims;
using DigitalniProdukty.Data;
using DigitalniProdukty.Models.Licensing;
using DigitalniProdukty.Security;
using Microsoft.EntityFrameworkCore;

namespace DigitalniProdukty.Services;

public sealed class GroupContextService(ApplicationDbContext db)
{
    // Z DB zjistí GroupId přihlášeného uživatele (členství v KeyGroup).
    // Vrací null, pokud user nemá identitu nebo členství neexistuje.
    public async Task<Guid?> GetUserGroupIdAsync(ClaimsPrincipal user, CancellationToken ct = default)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return null;

        return await db.KeyGroupMembers
            .Where(x => x.UserId == userId)
            .Select(x => (Guid?)x.GroupId)
            .FirstOrDefaultAsync(ct);
    }

        // Varianta pro místa, kde členství musí existovat (jinak je to chyba konfigurace/provisioningu).
        // Vyhazuje InvalidOperationException, aby se chyba neztratila jako „prázdná data“.
    public async Task<Guid> GetRequiredUserGroupIdAsync(ClaimsPrincipal user, CancellationToken ct = default)
    {
        var groupId = await GetUserGroupIdAsync(user, ct);
        if (groupId is null)
        {
            throw new InvalidOperationException("Current user has no KeyGroup membership.");
        }

        return groupId.Value;
    }

        // Pomocná kontrola role pro rozhodnutí, zda je možné vybírat libovolnou skupinu.
    public static bool IsAdmin(ClaimsPrincipal user) => user.IsInRole(Authz.Roles.Admin);

        // Normalizuje prázdné GUIDy z query stringu na null (snazší práce s "nezvoleno").
    public static Guid? NormalizeRequestedGroupId(Guid? requested)
        => requested == Guid.Empty ? null : requested;
}

/*
Podrobnosti (vazby a použité části)

- Účel: sjednocený přístup k tenant-scope (KeyGroups) z kontextu aktuálního uživatele.
- Závislosti:
    - ApplicationDbContext: čte tabulku `KeyGroupMembers` (vazba userId -> groupId).
    - ClaimsPrincipal: userId se bere z claimu `ClaimTypes.NameIdentifier` (Identity).
- Vazby na zbytek aplikace:
    - Používá `Controllers/LicensingAdminController` pro scoping dotazů a validaci přístupu ke skupinám.
    - Používá `Controllers/AuthController` při vytváření uživatelů v rámci skupiny "tvůrce".
    - Role-check `IsAdmin` je navázaný na `Security/Authz`.
*/
