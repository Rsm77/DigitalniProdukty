using System.Globalization;
using System.Resources;

namespace DigitalniProdukty.Resources;

// Statické accessory pro DataAnnotations atributy (Display/ErrorMessageResource...).
public static class Annotations
{
    private static readonly ResourceManager ResourceManager =
        new("DigitalniProdukty.Resources.Annotations", typeof(Annotations).Assembly);

    // Vrátí lokalizovaný string pro aktuální UI kulturu; fallback je název klíče.
    private static string GetString(string name) =>
        ResourceManager.GetString(name, CultureInfo.CurrentUICulture) ?? name;

    public static string Field_Email => GetString(nameof(Field_Email));
    public static string Field_Password => GetString(nameof(Field_Password));
    public static string Field_CurrentPassword => GetString(nameof(Field_CurrentPassword));
    public static string Field_NewPassword => GetString(nameof(Field_NewPassword));
    public static string Field_ConfirmPassword => GetString(nameof(Field_ConfirmPassword));
    public static string Field_RememberMe => GetString(nameof(Field_RememberMe));
    public static string Field_PhoneNumber => GetString(nameof(Field_PhoneNumber));
    public static string Field_VerificationCode => GetString(nameof(Field_VerificationCode));
    public static string Field_AccountType => GetString(nameof(Field_AccountType));

    public static string Validation_Required => GetString(nameof(Validation_Required));
    public static string Validation_EmailAddress => GetString(nameof(Validation_EmailAddress));
    public static string Validation_StringLength => GetString(nameof(Validation_StringLength));
    public static string Validation_PasswordsDoNotMatch => GetString(nameof(Validation_PasswordsDoNotMatch));
    public static string Validation_InvalidAccountType => GetString(nameof(Validation_InvalidAccountType));

    public static string ResetPassword_MissingCode => GetString(nameof(ResetPassword_MissingCode));
}

/*
Podrobnosti (vazby a použité části)

- Účel: pohodlný typově-bezpečný přístup k `.resx` řetězcům používaným v DataAnnotations.
- Závislosti:
    - `ResourceManager` a `CultureInfo.CurrentUICulture` pro výběr překladu.
- Použití v aplikaci:
    - Modely s atributy `[Display]`, `[Required]`, apod. odkazují na `DigitalniProdukty.Resources.Annotations`.
    - Klíče odpovídají položkám v [Resources/Annotations.resx](Resources/Annotations.resx) a jazykových variantách.
- Poznámka:
    - Pokud překlad chybí, vrací se `name` (lepší než null v UI).
*/

