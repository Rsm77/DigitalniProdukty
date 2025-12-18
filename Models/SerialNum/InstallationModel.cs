using System;
using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Models.Licensing;

namespace DigitalniProdukty.Models.SerialNum
{
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
