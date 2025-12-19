using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Resources;

namespace DigitalniProdukty.Models.Auth;

public sealed class ForgotPasswordInputModel
{
        // Email účtu, pro který se má spustit reset hesla.
    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [EmailAddress(ErrorMessageResourceName = nameof(Annotations.Validation_EmailAddress), ErrorMessageResourceType = typeof(Annotations))]
    [Display(Name = nameof(Annotations.Field_Email), ResourceType = typeof(Annotations))]
    public string Email { get; set; } = string.Empty;
}

/*
Podrobnosti (vazby a použité části)

- Účel: input model pro obrazovku "Zapomenuté heslo".
- Vazby na zbytek aplikace:
    - Používá `Controllers/AuthController` a view `Views/Auth/ForgotPassword.cshtml`.
    - Odeslání emailu řeší `Services/AuthEmailService` přes `IEmailSender`.
*/

