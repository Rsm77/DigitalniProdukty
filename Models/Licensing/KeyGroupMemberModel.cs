using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DigitalniProdukty.Models.Licensing;

// Členství uživatele ve skupině; v aplikaci je 1 uživatel = právě 1 skupina.
public class KeyGroupMemberModel
{
    public int Id { get; set; }

    [Required]
    public Guid GroupId { get; set; }

    [Required]
    public KeyGroupModel Group { get; set; } = null!;

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public IdentityUser? User { get; set; }

    public DateTime AddedAt { get; set; }
}

/*
Podrobnosti (vazby a použité části)

- Účel: mapuje uživatele (IdentityUser) do jedné KeyGroup.
- Vazby v DB:
    - `UserId` má unikátní index → jeden uživatel nemůže být ve více skupinách.
    - `GroupId` je FK na `KeyGroupModel`.
- Vazby na zbytek aplikace:
    - Čte `Services/GroupContextService` pro určení scope.
    - Zapisuje `Services/KeyGroupProvisioningService` při registraci a tvorbě účtů.
*/
