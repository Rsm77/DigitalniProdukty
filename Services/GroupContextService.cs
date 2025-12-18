using System.Security.Claims;
using DigitalniProdukty.Data;
using DigitalniProdukty.Models.Licensing;
using DigitalniProdukty.Security;
using Microsoft.EntityFrameworkCore;

namespace DigitalniProdukty.Services;

public sealed class GroupContextService(ApplicationDbContext db)
{
    public async Task<Guid?> GetUserGroupIdAsync(ClaimsPrincipal user, CancellationToken ct = default)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return null;

        return await db.KeyGroupMembers
            .Where(x => x.UserId == userId)
            .Select(x => (Guid?)x.GroupId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Guid> GetRequiredUserGroupIdAsync(ClaimsPrincipal user, CancellationToken ct = default)
    {
        var groupId = await GetUserGroupIdAsync(user, ct);
        if (groupId is null)
        {
            throw new InvalidOperationException("Current user has no KeyGroup membership.");
        }

        return groupId.Value;
    }

    public static bool IsAdmin(ClaimsPrincipal user) => user.IsInRole(Authz.Roles.Admin);

    public static Guid? NormalizeRequestedGroupId(Guid? requested)
        => requested == Guid.Empty ? null : requested;
}
