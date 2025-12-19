using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Resources;

namespace DigitalniProdukty.Models.Account;

public sealed class TwoFactorEnableInputModel
{
        // Ověřovací kód z autentikátoru (TOTP) pro potvrzení zapnutí 2FA.
    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [Display(Name = nameof(Annotations.Field_VerificationCode), ResourceType = typeof(Annotations))]
    public string Code { get; set; } = string.Empty;
}

/*
Podrobnosti (vazby a použité části)

- Účel: jednoduchý input model pro potvrzení zapnutí 2FA.
- Vazby na zbytek aplikace:
    - Je součástí `TwoFactorViewModel.Enable` a zpracovává ho `Controllers/AccountController`.
    - Validace a ověření kódu probíhá přes ASP.NET Identity (Authenticator token).
*/

