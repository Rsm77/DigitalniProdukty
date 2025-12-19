using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DigitalniProdukty.Models.Users;

// Rozšířený profil uživatele (odděleně od IdentityUser) – např. DisplayName.
public sealed class UserProfileModel
{
    [Key]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public IdentityUser? User { get; set; }
}

/*
Podrobnosti (vazby a použité části)

- Účel: držet uživatelská data, která nechceme/neumíme ukládat přímo do `IdentityUser`.
- Vazby v DB:
    - Primární klíč je `UserId` (stejný jako Identity userId) → 1:1 vztah s uživatelem.
    - Mapování 1:1 nastavuje `Data/ApplicationDbContext` (HasOne().WithOne().HasForeignKey()).
- Vazby na zbytek aplikace:
    - Používá se pro zobrazování jména v UI a při tvorbě skupin (např. provisioning).
*/
