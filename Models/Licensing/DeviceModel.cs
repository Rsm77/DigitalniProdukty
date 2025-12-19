using System;
using System.ComponentModel.DataAnnotations;

namespace DigitalniProdukty.Models.Licensing
{
    // Entity zařízení (hardware), které se váže na licenční klíč přes LicenseDevice.
    public class DeviceModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string HardwareIdentifier { get; set; } = string.Empty;

        public DateTime FirstSeenAt { get; set; }

        public DateTime LastSeenAt { get; set; }

        [MaxLength(45)]
        public string? LastIpAddress { get; set; }
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: reprezentuje fyzické/virtuální zařízení identifikované `HardwareIdentifier`.
- Vazby v DB:
    - Vazba na licence je přes spojovací tabulku `LicenseDeviceModel`.
    - `HardwareIdentifier` je unikátní (index nastavuje `Data/ApplicationDbContext`).
- Vazby na zbytek aplikace:
    - Používá se v licenční logice (binding zařízení ke klíči) a v instalacích (`InstallationModel`).
*/
