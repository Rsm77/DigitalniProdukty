using System.Security.Cryptography;
using System.Text;
using DigitalniProdukty.Data;
using DigitalniProdukty.Models.SerialNum;
using Microsoft.EntityFrameworkCore;

namespace DigitalniProdukty.Services;

public sealed class LicensingService(ApplicationDbContext db)
{
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

    public async Task<IReadOnlyList<SerialNumberModel>> GenerateAsync(
        int count,
        int groups,
        int groupLength,
        int maxDevices,
        Guid groupId,
        CancellationToken ct = default)
    {
        count = Math.Clamp(count, 1, 5000);
        groups = Math.Clamp(groups, 1, 32);
        groupLength = Math.Clamp(groupLength, 4, 16);
        maxDevices = Math.Clamp(maxDevices, 1, 100);

        var created = new List<SerialNumberModel>(capacity: count);
        var seen = new HashSet<string>(StringComparer.Ordinal);

        // Collisions are extremely unlikely; we still keep a set to avoid duplicates within this batch.
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
            });
        }

        db.SerialNumbers.AddRange(created);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // If a rare collision happens against existing DB keys, retry by regenerating missing keys.
            // We keep this path simple because collisions are practically nonexistent.
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
