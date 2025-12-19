using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Resources;

namespace DigitalniProdukty.Models.Auth;

public sealed class Login2faInputModel
{
    // Kód z autentikátoru (TOTP) pro dokončení přihlášení s 2FA.
    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [Display(Name = nameof(Annotations.Field_VerificationCode), ResourceType = typeof(Annotations))]
    public string Code { get; set; } = string.Empty;

    public bool RememberMachine { get; set; }
}

/*
Podrobnosti (vazby a použité části)

- Účel: input model pro 2FA krok při loginu.
- Vazby na zbytek aplikace:
    - Používá `Controllers/AuthController` a view `Views/Auth/Login2fa.cshtml`.
    - Ověření provádí `SignInManager<IdentityUser>.TwoFactorAuthenticatorSignInAsync` (nebo ekvivalent).
*/

