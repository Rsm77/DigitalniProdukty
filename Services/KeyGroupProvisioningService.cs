using DigitalniProdukty.Data;
using DigitalniProdukty.Models.Licensing;
using DigitalniProdukty.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigitalniProdukty.Services;

public sealed class KeyGroupProvisioningService(ApplicationDbContext db)
{
    // Sestaví název skupiny pro distributora (limity, fallbacky a snaha o jedinečnost).
    // Drží max 200 znaků, protože `KeyGroups.Name` má v DB maxLength=200.
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
            // Zachováme email (pravděpodobně unikátní), pokud se nevejde společně s prefixem.
            return em.Length <= 200 ? em : em[^200..];
        }

        if (dn.Length > maxPrefix) dn = dn[..maxPrefix];
        return dn + sep + em;
    }

    // Zajistí existenci systémové "Admin" skupiny (GUID a název jsou definované v `Security/KeyGroups`).
    // Bez této skupiny nelze korektně provádět scoping licencí a členství.
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

    // Zajistí, že distributor má přidělenou skupinu a je jejím členem.
    // Když už členství existuje, skupinu ponechá a jen se pokusí o "lepší" název.
    public async Task<Guid> EnsureGroupForDistributorAsync(IdentityUser distributor, string? displayName = null, CancellationToken ct = default)
    {
        // Pokud už je členem nějaké skupiny, ponecháme ji.
        var existingGroupId = await db.KeyGroupMembers
            .Where(x => x.UserId == distributor.Id)
            .Select(x => (Guid?)x.GroupId)
            .FirstOrDefaultAsync(ct);

        if (existingGroupId is not null)
        {
            // Pokusíme se skupinu přejmenovat, pokud teď máme lepší display name.
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

    // Jednoduchý provisioning: pokud uživatel ještě není členem žádné skupiny, přidá ho do zvolené.
    // Schválně nedovoluje "přesun" mezi skupinami (to má řešit admin flow).
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

/*
Podrobnosti (vazby a použité části)

- Účel: vytváření a udržování KeyGroups a členství (`KeyGroupMembers`) při tvorbě účtů.
- Závislosti:
    - ApplicationDbContext: zapisuje/čte `KeyGroups` a `KeyGroupMembers`.
    - IdentityUser: zdroj identity (Id/Email) pro tvorbu vazeb.
    - `Security/KeyGroups`: obsahuje konstanty pro admin skupinu.
- Vazby na zbytek aplikace:
    - Voláno z `Controllers/AuthController` při registraci a vytváření uživatelů s rolí Distributor/Reseller.
    - Voláno z `Controllers/AdminController` při správě uživatelů (např. doplnění skupiny pro distributora).
    - Voláno i v bootstrap/provisioning části `Program.cs` (development convenience).
- Poznámky k integritě:
    - `KeyGroupMembers.UserId` má unikátní index → jeden uživatel = právě jedna skupina.
    - Názvy skupin jsou unikátní (`KeyGroups.Name`), proto se rename dělá best-effort.
*/
