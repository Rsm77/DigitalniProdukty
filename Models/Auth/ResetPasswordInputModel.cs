using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Resources;

namespace DigitalniProdukty.Models.Auth;

public sealed class ResetPasswordInputModel
{
    // Email se používá jako identifikátor účtu při resetu (společně s tokenem).
    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [EmailAddress(ErrorMessageResourceName = nameof(Annotations.Validation_EmailAddress), ErrorMessageResourceType = typeof(Annotations))]
    [Display(Name = nameof(Annotations.Field_Email), ResourceType = typeof(Annotations))]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [StringLength(100, ErrorMessageResourceName = nameof(Annotations.Validation_StringLength), ErrorMessageResourceType = typeof(Annotations), MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = nameof(Annotations.Field_NewPassword), ResourceType = typeof(Annotations))]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [DataType(DataType.Password)]
    [Display(Name = nameof(Annotations.Field_ConfirmPassword), ResourceType = typeof(Annotations))]
    [Compare(nameof(Password), ErrorMessageResourceName = nameof(Annotations.Validation_PasswordsDoNotMatch), ErrorMessageResourceType = typeof(Annotations))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    public string Code { get; set; } = string.Empty;
}

/*
Podrobnosti (vazby a použité části)

- Účel: input model pro dokončení resetu hesla (nové heslo + token).
- Použité atributy:
    - DataAnnotations (validace) + lokalizované texty přes `Resources/Annotations*.resx`.
- Vazby na zbytek aplikace:
    - Používá `Controllers/AuthController` a view `Views/Auth/ResetPassword.cshtml`.
    - Token (`Code`) je typicky Base64Url varianta; dekódování řeší `Services/TokenCodec`.
*/

