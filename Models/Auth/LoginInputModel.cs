using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Resources;

namespace DigitalniProdukty.Models.Auth;

public sealed class LoginInputModel
{
    // Přihlašovací identifikátor – v aplikaci používáme email.
    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [EmailAddress(ErrorMessageResourceName = nameof(Annotations.Validation_EmailAddress), ErrorMessageResourceType = typeof(Annotations))]
    [Display(Name = nameof(Annotations.Field_Email), ResourceType = typeof(Annotations))]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [DataType(DataType.Password)]
    [Display(Name = nameof(Annotations.Field_Password), ResourceType = typeof(Annotations))]
    public string Password { get; set; } = string.Empty;

    [Display(Name = nameof(Annotations.Field_RememberMe), ResourceType = typeof(Annotations))]
    public bool RememberMe { get; set; }
}

/*
Podrobnosti (vazby a použité části)

- Účel: input model pro přihlášení (login).
- Použité atributy:
    - DataAnnotations s lokalizací přes `Resources/Annotations*.resx`.
- Vazby na zbytek aplikace:
    - Používá `Controllers/AuthController` a view `Views/Auth/Login.cshtml`.
    - Přihlášení provádí `SignInManager<IdentityUser>` a může přejít do 2FA flow.
*/

