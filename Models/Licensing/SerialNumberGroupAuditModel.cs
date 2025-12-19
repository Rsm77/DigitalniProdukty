using System;
using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Models.SerialNum;

namespace DigitalniProdukty.Models.Licensing;

// Auditní záznam změny skupiny (přesunu) licenčního klíče mezi tenaty.
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

/*
Podrobnosti (vazby a použité části)

- Účel: audit trail pro změny `SerialNumberModel.GroupId` (kdo/kdy/odkud/kam).
- Vazby v DB:
    - FK na `SerialNumberModel`.
    - Index na (`SerialNumberId`, `ChangedAt`) pro rychlé zobrazení historie.
- Vazby na zbytek aplikace:
    - Typicky se plní při reassign/re-group operacích v admin licenční správě.
    - `ChangedByUserId` je Identity userId (string), aby šlo dohledat autora změny.
*/
