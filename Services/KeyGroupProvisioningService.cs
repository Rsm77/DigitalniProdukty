using DigitalniProdukty.Data;
using DigitalniProdukty.Models.Licensing;
using DigitalniProdukty.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigitalniProdukty.Services;

public sealed class KeyGroupProvisioningService(ApplicationDbContext db)
{
    public async Task EnsureAdminGroupExistsAsync(CancellationToken ct = default)
    {
        var exists = await db.KeyGroups.AnyAsync(x => x.Id == KeyGroups.AdminGroupId, ct);
        if (exists) return;

        db.KeyGroups.Add(new KeyGroupModel
        {
            Id = KeyGroups.AdminGroupId,
            Name = KeyGroups.AdminGroupName,
            Role = Authz.Roles.Admin,
            CreatedAt = DateTime.UtcNow,
        });

        await db.SaveChangesAsync(ct);
    }

    public async Task<Guid> EnsureGroupForDistributorAsync(IdentityUser distributor, CancellationToken ct = default)
    {
        // If already member of a group, keep it.
        var existingGroupId = await db.KeyGroupMembers
            .Where(x => x.UserId == distributor.Id)
            .Select(x => (Guid?)x.GroupId)
            .FirstOrDefaultAsync(ct);

        if (existingGroupId is not null) return existingGroupId.Value;

        var group = new KeyGroupModel
        {
            Id = Guid.NewGuid(),
            Name = string.IsNullOrWhiteSpace(distributor.Email)
                ? $"Distributor {distributor.Id}"
                : $"Distributor {distributor.Email}",
            Role = Authz.Roles.Distributor,
            CreatedAt = DateTime.UtcNow,
        };

        db.KeyGroups.Add(group);
        db.KeyGroupMembers.Add(new KeyGroupMemberModel
        {
            GroupId = group.Id,
            UserId = distributor.Id,
            AddedAt = DateTime.UtcNow,
        });

        await db.SaveChangesAsync(ct);
        return group.Id;
    }

    public async Task EnsureUserInGroupAsync(string userId, Guid groupId, CancellationToken ct = default)
    {
        var existing = await db.KeyGroupMembers.AnyAsync(x => x.UserId == userId, ct);
        if (existing) return;

        db.KeyGroupMembers.Add(new KeyGroupMemberModel
        {
            GroupId = groupId,
            UserId = userId,
            AddedAt = DateTime.UtcNow,
        });

        await db.SaveChangesAsync(ct);
    }
}
