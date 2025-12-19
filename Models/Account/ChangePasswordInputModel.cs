using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Resources;

namespace DigitalniProdukty.Models.Account;

public sealed class ChangePasswordInputModel
{
    // Aktuální heslo pro ověření, že změnu provádí oprávněný uživatel.
    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [DataType(DataType.Password)]
    [Display(Name = nameof(Annotations.Field_CurrentPassword), ResourceType = typeof(Annotations))]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [StringLength(100, ErrorMessageResourceName = nameof(Annotations.Validation_StringLength), ErrorMessageResourceType = typeof(Annotations), MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = nameof(Annotations.Field_NewPassword), ResourceType = typeof(Annotations))]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [DataType(DataType.Password)]
    [Display(Name = nameof(Annotations.Field_ConfirmPassword), ResourceType = typeof(Annotations))]
    [Compare(nameof(NewPassword), ErrorMessageResourceName = nameof(Annotations.Validation_PasswordsDoNotMatch), ErrorMessageResourceType = typeof(Annotations))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/*
Podrobnosti (vazby a použité části)

- Účel: input model pro změnu hesla přihlášeného uživatele.
- Použité atributy:
    - DataAnnotations pro validaci a lokalizované texty přes `Resources/Annotations*.resx`.
- Vazby na zbytek aplikace:
    - Používá `Controllers/AccountController` (akce ChangePassword) a view `Views/Account/Password.cshtml`.
    - Skutečná změna probíhá přes `UserManager<IdentityUser>.ChangePasswordAsync`.
*/

