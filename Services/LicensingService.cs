using System.Security.Cryptography;
using System.Text;
using DigitalniProdukty.Data;
using DigitalniProdukty.Models.SerialNum;
using Microsoft.EntityFrameworkCore;

namespace DigitalniProdukty.Services;

public sealed class LicensingService(ApplicationDbContext db)
{
    // Vrátí poslední vydané licenční klíče, volitelně omezené na konkrétní skupinu.
    // Používá se pro admin přehledy (včetně HTMX fragmentů).
    public async Task<IReadOnlyList<SerialNumberModel>> GetLatestSerialsAsync(int take, Guid? groupId = null, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);

        var query = db.SerialNumbers
            .AsQueryable();

        if (groupId is not null)
        {
            query = query.Where(x => x.GroupId == groupId.Value);
        }

        return await query
            .Include(x => x.OwnerUser)
            .OrderByDescending(x => x.Id)
            .Take(take)
            .ToListAsync(ct);
    }

    // Vrátí detail licenčního klíče podle ID, volitelně omezené na skupinu.
    // Používá se pro detail/akce nad jedním klíčem v admin UI.
    public async Task<SerialNumberModel?> GetSerialAsync(int id, Guid? groupId = null, CancellationToken ct = default)
    {
        var query = db.SerialNumbers
            .AsQueryable();

        if (groupId is not null)
        {
            query = query.Where(x => x.GroupId == groupId.Value);
        }

        return await query
            .Include(x => x.OwnerUser)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    // Vygeneruje dávku licenčních klíčů a uloží je do DB (unikátní přes unikátní index na `SerialNumbers.Key`).
    // Při vzácné kolizi s DB provede jednoduchý opakovaný pokus a doplní chybějící kusy.
    public async Task<IReadOnlyList<SerialNumberModel>> GenerateAsync(
        int count,
        int groups,
        int groupLength,
        int maxDevices,
        Guid groupId,
        string? ownerUserId = null,
        CancellationToken ct = default)
    {
        count = Math.Clamp(count, 1, 5000);
        groups = Math.Clamp(groups, 1, 32);
        groupLength = Math.Clamp(groupLength, 4, 16);
        maxDevices = Math.Clamp(maxDevices, 1, 100);

        var created = new List<SerialNumberModel>(capacity: count);
        var seen = new HashSet<string>(StringComparer.Ordinal);

        // Kolize jsou extrémně nepravděpodobné; set používáme pro vyloučení duplicit v rámci jedné dávky.
        while (created.Count < count)
        {
            ct.ThrowIfCancellationRequested();

            var key = GenerateKey(groups, groupLength);
            if (!seen.Add(key)) continue;

            created.Add(new SerialNumberModel
            {
                Key = key,
                GroupId = groupId,
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
                MaxDevices = maxDevices,
                OwnerUserId = string.IsNullOrWhiteSpace(ownerUserId) ? null : ownerUserId,
            });
        }

        db.SerialNumbers.AddRange(created);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Pokud dojde ke vzácné kolizi s existujícími klíči v DB, zopakujeme generování a doplníme chybějící kusy.
            // Tuhle větev držíme schválně jednoduchou, protože kolize jsou prakticky nepravděpodobné.
            db.ChangeTracker.Clear();

            var result = new List<SerialNumberModel>(capacity: count);
            for (var i = 0; i < count; i++)
            {
                ct.ThrowIfCancellationRequested();

                while (true)
                {
                    var key = GenerateKey(groups, groupLength);
                    if (await db.SerialNumbers.AnyAsync(x => x.Key == key, ct)) continue;

                    var entity = new SerialNumberModel
                    {
                        Key = key,
                        GroupId = groupId,
                        CreatedAt = DateTime.UtcNow,
                        IsRevoked = false,
                        MaxDevices = maxDevices,
                        OwnerUserId = string.IsNullOrWhiteSpace(ownerUserId) ? null : ownerUserId,
                    };

                    db.SerialNumbers.Add(entity);
                    await db.SaveChangesAsync(ct);
                    result.Add(entity);
                    break;
                }
            }

            return result;
        }

        return created;
    }

    // Vytvoří jeden klíč ve formátu skupin oddělených pomlčkou (např. ABCDE-234FG-...).
    // Používá kryptograficky bezpečné náhodné bajty a omezenou abecedu bez snadno zaměnitelných znaků.
    private static string GenerateKey(int groups, int groupLength)
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no I/O/1/0 to reduce confusion

        var totalChars = groups * groupLength;
        var bytes = RandomNumberGenerator.GetBytes(totalChars);

        var sb = new StringBuilder(totalChars + (groups - 1));
        var idx = 0;
        for (var g = 0; g < groups; g++)
        {
            if (g > 0) sb.Append('-');

            for (var j = 0; j < groupLength; j++)
            {
                var b = bytes[idx++];
                sb.Append(alphabet[b % alphabet.Length]);
            }
        }

        return sb.ToString();
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: práce s licenčními klíči (`SerialNumberModel`) – generování a dotazy pro admin UI.
- Závislosti:
    - ApplicationDbContext: zapisuje/čte tabulku `SerialNumbers` a načítá `OwnerUser` (IdentityUser).
    - EF Core: `Include`, `OrderByDescending`, `Take`, `SaveChangesAsync`.
    - System.Security.Cryptography.RandomNumberGenerator: zdroj entropie pro generování klíčů.
- Vazby na zbytek aplikace:
    - Používá `Controllers/LicensingAdminController` (Generate, LatestKeys, Details a další akce).
    - Skupinový scoping (`groupId`) souvisí s multi-tenant logikou `KeyGroups` a `GroupContextService`.
- Integrita dat:
- Integrita dat a zamezení duplicit:
    - DB vrstva: `SerialNumberModel.Key` má unikátní index (viz konfigurace v `Data/ApplicationDbContext`). To je finální „zdroj pravdy“,
        který zabrání duplicitám i při paralelních requestech, více instancích aplikace nebo závodu mezi dvěma generováními.
    - Aplikační vrstva (v rámci jedné dávky): `GenerateAsync` používá `HashSet<string> seen`, aby se v jedné dávce nevygeneroval stejný klíč dvakrát.
        Tím se vyhne zbytečným pokusům o insert a zrychlí běh pro větší `count`.
    - Aplikační vrstva (pro případ závodu/kolize s DB): při `DbUpdateException` (typicky porušení unikátního indexu) se provede jednoduchý retry:
        `ChangeTracker.Clear()` a pak se klíče generují po jednom v cyklu s kontrolou `AnyAsync(x => x.Key == key)` před insertem.
        Tahle větev je schválně jednoduchá – kolize jsou prakticky nepravděpodobné, ale závody při souběžných generováních možné jsou.
    - Poznámka k závodům: `AnyAsync` před insertem není sama o sobě atomická ochrana (mezi SELECT a INSERT může někdo vložit stejný klíč),
        proto je klíčový unikátní index v DB, který závod deterministicky „usekne“.
- Formát klíče:
    - Abeceda vynechává I/O/1/0 pro snížení chybovosti při přepisu.
    - Počet skupin a délka skupiny jsou omezené clampy (ochrana proti extrémním vstupům).
*/
