using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Resources;

namespace DigitalniProdukty.Models.Account;

public sealed class ProfileInputModel
{
    // Zobrazené (jen pro čtení) nebo editované pole profilu podle konkrétního UI.
    [Display(Name = nameof(Annotations.Field_Email), ResourceType = typeof(Annotations))]
    public string Email { get; set; } = string.Empty;

    [Display(Name = nameof(Annotations.Field_PhoneNumber), ResourceType = typeof(Annotations))]
    public string? PhoneNumber { get; set; }
}

/*
Podrobnosti (vazby a použité části)

- Účel: input model pro úpravu profilu (email/telefon) v účtu.
- Použité atributy:
    - `[Display]` s lokalizací přes `Resources/Annotations*.resx`.
- Vazby na zbytek aplikace:
    - Používá `Controllers/AccountController` a view `Views/Account/Profile.cshtml`.
    - Ukládání se typicky děje přes ASP.NET Identity (`IdentityUser.Email`, `IdentityUser.PhoneNumber`).
*/

