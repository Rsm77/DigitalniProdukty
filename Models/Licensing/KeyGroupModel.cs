using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalniProdukty.Models.Licensing;

// Entity skupiny (tenant) pro scoping licencí a uživatelů.
public class KeyGroupModel
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public ICollection<KeyGroupMemberModel> Members { get; set; } = new List<KeyGroupMemberModel>();
}

/*
Podrobnosti (vazby a použité části)

- Účel: "skupina" (tenant) – logické oddělení licenčních klíčů a přístupu uživatelů.
- Vazby v DB:
    - 1:N na `KeyGroupMemberModel` (členové skupiny).
    - 1:N na `SerialNumberModel` přes `SerialNumbers.GroupId` (viz `Data/ApplicationDbContext`).
    - `Name` má unikátní index (konfigurace v `Data/ApplicationDbContext`).
- Vazby na zbytek aplikace:
    - Používá se v `Services/KeyGroupProvisioningService` a `Services/GroupContextService`.
    - Skupinový scoping používá `Controllers/LicensingAdminController`.
*/
