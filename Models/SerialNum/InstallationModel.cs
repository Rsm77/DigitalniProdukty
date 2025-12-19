using System;
using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Models.Licensing;

namespace DigitalniProdukty.Models.SerialNum
{
    // Entity jedné instalační/aktivační události (vazba klíč ↔ zařízení).
    public class InstallationModel
    {
        public int Id { get; set; }

        [Required]
        public int SerialNumberId { get; set; }

        [Required]
        public SerialNumberModel SerialNumber { get; set; } = null!;

        [Required]
        public int DeviceId { get; set; }

        [Required]
        public DeviceModel Device { get; set; } = null!;

        public InstallationEventType EventType { get; set; }

        public DateTime OccurredAt { get; set; }

        [Required]
        [MaxLength(45)]
        public string IpAddress { get; set; } = string.Empty;
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: zaznamenat události (Activate/Deactivate/Run) pro konkrétní licenční klíč a zařízení.
- Vazby v DB:
    - FK na `SerialNumberModel` a `DeviceModel`.
    - Indexy na (`SerialNumberId`, `OccurredAt`) a (`DeviceId`, `OccurredAt`) pro časové dotazy (viz `Data/ApplicationDbContext`).
- Vazby na zbytek aplikace:
    - Event typ je `Models/Licensing/InstallationEventType`.
    - Používá se v licenční logice pro audit/limity (podle implementace služeb/controllers).
*/
