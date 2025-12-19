using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Resources;

namespace DigitalniProdukty.Models.Account;

public sealed class FirstLoginInputModel
{
    // Email se na prvním přihlášení typicky doplňuje/potvrzuje (flow podle pravidel aplikace).
    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [EmailAddress(ErrorMessageResourceName = nameof(Annotations.Validation_EmailAddress), ErrorMessageResourceType = typeof(Annotations))]
    [Display(Name = nameof(Annotations.Field_Email), ResourceType = typeof(Annotations))]
    public string Email { get; set; } = string.Empty;

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

- Účel: vstupní model pro "první přihlášení" (vynucená změna hesla / doplnění emailu).
- Použité atributy:
    - DataAnnotations (`[Required]`, `[EmailAddress]`, `[StringLength]`, `[Compare]`) s lokalizací přes `Resources/Annotations*.resx`.
- Vazby na zbytek aplikace:
    - Zpracovává `Controllers/AccountController` (typicky akce FirstLogin).
    - Password operace provádí `UserManager<IdentityUser>` (ChangePassword / ResetPassword dle flow).
*/
