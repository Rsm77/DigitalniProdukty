using System;
using System.ComponentModel.DataAnnotations;

namespace DigitalniProdukty.Models.Licensing
{
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
