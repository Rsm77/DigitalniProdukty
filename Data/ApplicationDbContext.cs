using DigitalniProdukty.Models.Licensing;
using DigitalniProdukty.Models.SerialNum;
using DigitalniProdukty.Models.Users;
using DigitalniProdukty.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DigitalniProdukty.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<SerialNumberModel> SerialNumbers => Set<SerialNumberModel>();
        public DbSet<DeviceModel> Devices => Set<DeviceModel>();
        public DbSet<LicenseDeviceModel> LicenseDevices => Set<LicenseDeviceModel>();
        public DbSet<InstallationModel> Installations => Set<InstallationModel>();

        public DbSet<KeyGroupModel> KeyGroups => Set<KeyGroupModel>();
        public DbSet<KeyGroupMemberModel> KeyGroupMembers => Set<KeyGroupMemberModel>();
        public DbSet<SerialNumberGroupAuditModel> SerialNumberGroupAudits => Set<SerialNumberGroupAuditModel>();

        public DbSet<UserProfileModel> UserProfiles => Set<UserProfileModel>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<SerialNumberModel>(e =>
            {
                e.HasIndex(x => x.Key).IsUnique();
                e.HasIndex(x => x.OwnerUserId);
                e.HasIndex(x => x.GroupId);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                e.Property(x => x.GroupId).HasDefaultValue(DigitalniProdukty.Security.KeyGroups.AdminGroupId);

                e.HasOne(x => x.Group)
                    .WithMany()
                    .HasForeignKey(x => x.GroupId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.OwnerUser)
                    .WithMany()
                    .HasForeignKey(x => x.OwnerUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<KeyGroupModel>(e =>
            {
                e.HasIndex(x => x.Name).IsUnique();
                e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            builder.Entity<KeyGroupMemberModel>(e =>
            {
                e.HasIndex(x => new { x.GroupId, x.UserId }).IsUnique();
                e.HasIndex(x => x.UserId).IsUnique();
                e.Property(x => x.AddedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                e.HasOne(x => x.Group)
                    .WithMany(x => x.Members)
                    .HasForeignKey(x => x.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SerialNumberGroupAuditModel>(e =>
            {
                e.HasIndex(x => new { x.SerialNumberId, x.ChangedAt });
                e.Property(x => x.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                e.HasOne(x => x.SerialNumber)
                    .WithMany()
                    .HasForeignKey(x => x.SerialNumberId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

                    builder.Entity<UserProfileModel>(e =>
                    {
                    e.HasKey(x => x.UserId);
                    e.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
                    e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                    e.HasOne(x => x.User)
                        .WithOne()
                        .HasForeignKey<UserProfileModel>(x => x.UserId)
                        .OnDelete(DeleteBehavior.Cascade);
                    });

            builder.Entity<DeviceModel>(e =>
            {
                e.HasIndex(x => x.HardwareIdentifier).IsUnique();
            });

            builder.Entity<LicenseDeviceModel>(e =>
            {
                e.HasIndex(x => new { x.SerialNumberId, x.DeviceId }).IsUnique();

                e.HasOne(x => x.SerialNumber)
                    .WithMany(x => x.LicenseDevices)
                    .HasForeignKey(x => x.SerialNumberId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Device)
                    .WithMany()
                    .HasForeignKey(x => x.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<InstallationModel>(e =>
            {
                e.HasIndex(x => new { x.SerialNumberId, x.OccurredAt });
                e.HasIndex(x => new { x.DeviceId, x.OccurredAt });

                e.HasOne(x => x.SerialNumber)
                    .WithMany(x => x.Installations)
                    .HasForeignKey(x => x.SerialNumberId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Device)
                    .WithMany()
                    .HasForeignKey(x => x.DeviceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

/*
Vysvětlení `ApplicationDbContext`

- `ApplicationDbContext` je EF Core kontext: definuje entity (tabulky) přes `DbSet<>` a jejich mapování/relace v `OnModelCreating`.
- Kontext dědí z `IdentityDbContext`, takže zároveň obsahuje i tabulky ASP.NET Identity (uživatelé/role/claims).

DbSety (tabulky)
- `SerialNumbers`: licenční klíče (vydané sériové číslo / licence).
- `Devices`: zařízení identifikované `HardwareIdentifier` (typicky hash/fingerprint počítače).
- `LicenseDevices`: přiřazení licence ↔ zařízení (umožní limitovat počet zařízení a "odpojovat" zařízení bez smazání historie).
- `Installations`: auditní log událostí (aktivace/spuštění/deaktivace) pro konkrétní licenci a zařízení.

Konfigurace v `OnModelCreating`
- `SerialNumberModel`
  - unikátní index na `Key` zabrání duplicitním klíčům.
  - `CreatedAt` má default z databáze (CURRENT_TIMESTAMP).

- `DeviceModel`
  - unikátní index na `HardwareIdentifier` zabrání duplicitním zařízením.

- `LicenseDeviceModel`
  - unikátní kombinace (`SerialNumberId`, `DeviceId`) zabrání duplicitnímu párování.
  - `Cascade` mazání je zde OK: pokud odstraníš licenci nebo zařízení, odstraní se i jejich vazby.

- `InstallationModel`
  - indexy (`SerialNumberId`, `OccurredAt`) a (`DeviceId`, `OccurredAt`) zrychlují výpis historie v čase.
  - `Restrict` mazání u FK na `SerialNumbers`/`Devices` chrání audit: nelze omylem smazat licenci/zařízení, pokud existují záznamy instalací.
    (Audit se běžně nemaže; licence se raději "zablokuje" pomocí `IsRevoked`).
*/

