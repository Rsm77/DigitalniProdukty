using System;
using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Models.SerialNum;

namespace DigitalniProdukty.Models.Licensing
{
    // Spojovací entity mezi licencí (SerialNumber) a zařízením (Device).
    public class LicenseDeviceModel
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

        public DateTime BoundAt { get; set; }

        public DateTime? UnboundAt { get; set; }
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: reprezentuje „binding“ licenčního klíče na konkrétní zařízení.
- Vazby v DB:
    - FK na `SerialNumberModel` a `DeviceModel`.
    - Unikátní index na dvojici (`SerialNumberId`, `DeviceId`) brání duplicitnímu navázání (viz `Data/ApplicationDbContext`).
- Vazby na zbytek aplikace:
    - Používá se v licenční logice pro kontrolu počtu zařízení / aktivací.
*/
