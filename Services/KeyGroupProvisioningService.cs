using DigitalniProdukty.Data;
using DigitalniProdukty.Models.Licensing;
using DigitalniProdukty.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigitalniProdukty.Services;

public sealed class KeyGroupProvisioningService(ApplicationDbContext db)
{
    private static string BuildDistributorGroupName(string? displayName, string? email, string distributorId)
    {
        var fallback = string.IsNullOrWhiteSpace(email)
            ? $"Distributor {distributorId}"
            : $"Distributor {email}";

        var dn = (displayName ?? string.Empty).Trim();
        var em = (email ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(dn))
        {
            return fallback.Length <= 200 ? fallback : fallback[..200];
        }

        if (string.IsNullOrWhiteSpace(em))
        {
            return dn.Length <= 200 ? dn : dn[..200];
        }

        const string sep = " — ";
        var maxPrefix = 200 - sep.Length - em.Length;
        if (maxPrefix < 1)
        {
            // Keep the email (likely unique) if it doesn't fit with a prefix.
            return em.Length <= 200 ? em : em[^200..];
        }

        if (dn.Length > maxPrefix) dn = dn[..maxPrefix];
        return dn + sep + em;
    }

    public async Task EnsureAdminGroupExistsAsync(CancellationToken ct = default)
    {
        var exists = await db.KeyGroups.AnyAsync(x => x.Id == KeyGroups.AdminGroupId, ct);
        if (exists) return;

        db.KeyGroups.Add(new KeyGroupModel
        {
            Id = KeyGroups.AdminGroupId,
            Name = KeyGroups.AdminGroupName,
            CreatedAt = DateTime.UtcNow,
        });

        await db.SaveChangesAsync(ct);
    }

    public async Task<Guid> EnsureGroupForDistributorAsync(IdentityUser distributor, string? displayName = null, CancellationToken ct = default)
    {
        // If already member of a group, keep it.
        var existingGroupId = await db.KeyGroupMembers
            .Where(x => x.UserId == distributor.Id)
            .Select(x => (Guid?)x.GroupId)
            .FirstOrDefaultAsync(ct);

        if (existingGroupId is not null)
        {
            // Best-effort rename if we now have a better display name.
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                var existingGroup = await db.KeyGroups.FirstOrDefaultAsync(x => x.Id == existingGroupId.Value, ct);
                if (existingGroup is not null)
                {
                    var desiredName = BuildDistributorGroupName(displayName, distributor.Email, distributor.Id);
                    if (!string.Equals(existingGroup.Name, desiredName, StringComparison.Ordinal))
                    {
                        var nameTaken = await db.KeyGroups.AnyAsync(x => x.Name == desiredName && x.Id != existingGroup.Id, ct);
                        if (!nameTaken)
                        {
                            existingGroup.Name = desiredName;
                            await db.SaveChangesAsync(ct);
                        }
                    }
                }
            }

            return existingGroupId.Value;
        }

        var group = new KeyGroupModel
        {
            Id = Guid.NewGuid(),
            Name = BuildDistributorGroupName(displayName, distributor.Email, distributor.Id),
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
