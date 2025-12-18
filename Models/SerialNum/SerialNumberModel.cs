using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Models.Licensing;
using Microsoft.AspNetCore.Identity;

namespace DigitalniProdukty.Models.SerialNum {
    public class SerialNumberModel {
        public int Id { get; set; }

        public Guid GroupId { get; set; }

        public KeyGroupModel? Group { get; set; }

        [Required]
        [MaxLength(64)]
        public string Key { get; set; } = string.Empty;

        public int SerialNumberIndex { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsRevoked { get; set; }

        public int MaxDevices { get; set; } = 1;

        public int? MaxActivations { get; set; }

        [MaxLength(450)]
        public string? OwnerUserId { get; set; }

        public IdentityUser? OwnerUser { get; set; }

        public ICollection<LicenseDeviceModel> LicenseDevices { get; set; } = new List<LicenseDeviceModel>();

        public ICollection<InstallationModel> Installations { get; set; } = new List<InstallationModel>();
    }
}
