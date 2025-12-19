using System.Globalization;
using System.Resources;
using Microsoft.AspNetCore.Identity;

namespace DigitalniProdukty.Services;

public sealed class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
{
    private static readonly ResourceManager ResourceManager =
        new("DigitalniProdukty.Resources.IdentityErrors", typeof(LocalizedIdentityErrorDescriber).Assembly);

    // Vrátí lokalizovaný řetězec z .resx podle aktuální UI kultury (fallback je název klíče).
    private static string Get(string name) =>
        ResourceManager.GetString(name, CultureInfo.CurrentUICulture) ?? name;

    // Načte lokalizovanou šablonu a aplikuje string.Format v aktuální UI kultuře.
    private static string Format(string name, params object[] args) =>
        string.Format(CultureInfo.CurrentUICulture, Get(name), args);

    // Lokalizovaná chyba pro konflikt souběhu (optimistic concurrency).
    public override IdentityError ConcurrencyFailure() => new()
    {
        Code = nameof(ConcurrencyFailure),
        Description = Get(nameof(ConcurrencyFailure))
    };

    // Lokalizovaná chyba pro špatně zadané aktuální heslo.
    public override IdentityError PasswordMismatch() => new()
    {
        Code = nameof(PasswordMismatch),
        Description = Get(nameof(PasswordMismatch))
    };

    // Lokalizovaná chyba pro neplatný token (např. reset hesla / potvrzení emailu).
    public override IdentityError InvalidToken() => new()
    {
        Code = nameof(InvalidToken),
        Description = Get(nameof(InvalidToken))
    };

    // Lokalizovaná chyba pro provider login už připojený k jinému účtu.
    public override IdentityError LoginAlreadyAssociated() => new()
    {
        Code = nameof(LoginAlreadyAssociated),
        Description = Get(nameof(LoginAlreadyAssociated))
    };

    // Lokalizovaná chyba pro neplatné uživatelské jméno.
    public override IdentityError InvalidUserName(string? userName) => new()
    {
        Code = nameof(InvalidUserName),
        Description = Format(nameof(InvalidUserName), userName ?? string.Empty)
    };

    // Lokalizovaná chyba pro neplatný email.
    public override IdentityError InvalidEmail(string? email) => new()
    {
        Code = nameof(InvalidEmail),
        Description = Format(nameof(InvalidEmail), email ?? string.Empty)
    };

    // Lokalizovaná chyba pro duplicitní uživatelské jméno.
    public override IdentityError DuplicateUserName(string? userName) => new()
    {
        Code = nameof(DuplicateUserName),
        Description = Format(nameof(DuplicateUserName), userName ?? string.Empty)
    };

    // Lokalizovaná chyba pro duplicitní email.
    public override IdentityError DuplicateEmail(string? email) => new()
    {
        Code = nameof(DuplicateEmail),
        Description = Format(nameof(DuplicateEmail), email ?? string.Empty)
    };

    // Lokalizovaná chyba pro neplatný název role.
    public override IdentityError InvalidRoleName(string? role) => new()
    {
        Code = nameof(InvalidRoleName),
        Description = Format(nameof(InvalidRoleName), role ?? string.Empty)
    };

    // Lokalizovaná chyba pro duplicitní název role.
    public override IdentityError DuplicateRoleName(string? role) => new()
    {
        Code = nameof(DuplicateRoleName),
        Description = Format(nameof(DuplicateRoleName), role ?? string.Empty)
    };

    // Lokalizovaná chyba pro situaci, kdy uživatel už heslo má.
    public override IdentityError UserAlreadyHasPassword() => new()
    {
        Code = nameof(UserAlreadyHasPassword),
        Description = Get(nameof(UserAlreadyHasPassword))
    };

    // Lokalizovaná chyba pro situaci, kdy lockout není pro uživatele povolený.
    public override IdentityError UserLockoutNotEnabled() => new()
    {
        Code = nameof(UserLockoutNotEnabled),
        Description = Get(nameof(UserLockoutNotEnabled))
    };

    // Lokalizovaná chyba pro uživatele, který už roli má.
    public override IdentityError UserAlreadyInRole(string? role) => new()
    {
        Code = nameof(UserAlreadyInRole),
        Description = Format(nameof(UserAlreadyInRole), role ?? string.Empty)
    };

    // Lokalizovaná chyba pro uživatele, který roli nemá.
    public override IdentityError UserNotInRole(string? role) => new()
    {
        Code = nameof(UserNotInRole),
        Description = Format(nameof(UserNotInRole), role ?? string.Empty)
    };

    // Lokalizovaná chyba pro příliš krátké heslo.
    public override IdentityError PasswordTooShort(int length) => new()
    {
        Code = nameof(PasswordTooShort),
        Description = Format(nameof(PasswordTooShort), length)
    };

    // Lokalizovaná chyba pro chybějící nealfanumerický znak v hesle.
    public override IdentityError PasswordRequiresNonAlphanumeric() => new()
    {
        Code = nameof(PasswordRequiresNonAlphanumeric),
        Description = Get(nameof(PasswordRequiresNonAlphanumeric))
    };

    // Lokalizovaná chyba pro chybějící číslici v hesle.
    public override IdentityError PasswordRequiresDigit() => new()
    {
        Code = nameof(PasswordRequiresDigit),
        Description = Get(nameof(PasswordRequiresDigit))
    };

    // Lokalizovaná chyba pro chybějící malé písmeno v hesle.
    public override IdentityError PasswordRequiresLower() => new()
    {
        Code = nameof(PasswordRequiresLower),
        Description = Get(nameof(PasswordRequiresLower))
    };

    // Lokalizovaná chyba pro chybějící velké písmeno v hesle.
    public override IdentityError PasswordRequiresUpper() => new()
    {
        Code = nameof(PasswordRequiresUpper),
        Description = Get(nameof(PasswordRequiresUpper))
    };

    // Lokalizovaná chyba pro minimální počet unikátních znaků v hesle.
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new()
    {
        Code = nameof(PasswordRequiresUniqueChars),
        Description = Format(nameof(PasswordRequiresUniqueChars), uniqueChars)
    };

    // Lokalizovaná chyba pro špatný recovery kód při obnově 2FA.
    public override IdentityError RecoveryCodeRedemptionFailed() => new()
    {
        Code = nameof(RecoveryCodeRedemptionFailed),
        Description = Get(nameof(RecoveryCodeRedemptionFailed))
    };
}

/*
Podrobnosti (vazby a použité části)

- Účel: přepsání standardních Identity chyb tak, aby texty byly lokalizované přes .resx.
- Závislosti:
  - ResourceManager: čte `Resources/IdentityErrors*.resx` podle `CultureInfo.CurrentUICulture`.
  - IdentityErrorDescriber: ASP.NET Identity používá tyto metody pro tvorbu validačních chyb.
- Vazby na zbytek aplikace:
  - Registrováno v `Program.cs` přes `.AddErrorDescriber<LocalizedIdentityErrorDescriber>()`.
  - Chyby se pak zobrazují ve view pro registraci/login/reset hesla (Razor + DataAnnotations/Identity).
- Poznámka:
  - Klíče v resx odpovídají názvům metod (`nameof(...)`), takže se udržují jednoduše.
*/

