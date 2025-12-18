using System;
using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Models.SerialNum;

namespace DigitalniProdukty.Models.Licensing
{
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
