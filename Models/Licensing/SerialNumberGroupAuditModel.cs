using System;
using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Models.SerialNum;

namespace DigitalniProdukty.Models.Licensing;

public class SerialNumberGroupAuditModel
{
    public long Id { get; set; }

    [Required]
    public int SerialNumberId { get; set; }

    [Required]
    public SerialNumberModel SerialNumber { get; set; } = null!;

    [Required]
    public Guid FromGroupId { get; set; }

    [Required]
    public Guid ToGroupId { get; set; }

    [Required]
    [MaxLength(450)]
    public string ChangedByUserId { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }
}
