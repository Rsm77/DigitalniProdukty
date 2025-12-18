using System.Globalization;
using System.Resources;
using Microsoft.AspNetCore.Identity;

namespace DigitalniProdukty.Services;

public sealed class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
{
    private static readonly ResourceManager ResourceManager =
        new("DigitalniProdukty.Resources.IdentityErrors", typeof(LocalizedIdentityErrorDescriber).Assembly);

    private static string Get(string name) =>
        ResourceManager.GetString(name, CultureInfo.CurrentUICulture) ?? name;

    private static string Format(string name, params object[] args) =>
        string.Format(CultureInfo.CurrentUICulture, Get(name), args);

    public override IdentityError ConcurrencyFailure() => new()
    {
        Code = nameof(ConcurrencyFailure),
        Description = Get(nameof(ConcurrencyFailure))
    };

    public override IdentityError PasswordMismatch() => new()
    {
        Code = nameof(PasswordMismatch),
        Description = Get(nameof(PasswordMismatch))
    };

    public override IdentityError InvalidToken() => new()
    {
        Code = nameof(InvalidToken),
        Description = Get(nameof(InvalidToken))
    };

    public override IdentityError LoginAlreadyAssociated() => new()
    {
        Code = nameof(LoginAlreadyAssociated),
        Description = Get(nameof(LoginAlreadyAssociated))
    };

    public override IdentityError InvalidUserName(string? userName) => new()
    {
        Code = nameof(InvalidUserName),
        Description = Format(nameof(InvalidUserName), userName ?? string.Empty)
    };

    public override IdentityError InvalidEmail(string? email) => new()
    {
        Code = nameof(InvalidEmail),
        Description = Format(nameof(InvalidEmail), email ?? string.Empty)
    };

    public override IdentityError DuplicateUserName(string? userName) => new()
    {
        Code = nameof(DuplicateUserName),
        Description = Format(nameof(DuplicateUserName), userName ?? string.Empty)
    };

    public override IdentityError DuplicateEmail(string? email) => new()
    {
        Code = nameof(DuplicateEmail),
        Description = Format(nameof(DuplicateEmail), email ?? string.Empty)
    };

    public override IdentityError InvalidRoleName(string? role) => new()
    {
        Code = nameof(InvalidRoleName),
        Description = Format(nameof(InvalidRoleName), role ?? string.Empty)
    };

    public override IdentityError DuplicateRoleName(string? role) => new()
    {
        Code = nameof(DuplicateRoleName),
        Description = Format(nameof(DuplicateRoleName), role ?? string.Empty)
    };

    public override IdentityError UserAlreadyHasPassword() => new()
    {
        Code = nameof(UserAlreadyHasPassword),
        Description = Get(nameof(UserAlreadyHasPassword))
    };

    public override IdentityError UserLockoutNotEnabled() => new()
    {
        Code = nameof(UserLockoutNotEnabled),
        Description = Get(nameof(UserLockoutNotEnabled))
    };

    public override IdentityError UserAlreadyInRole(string? role) => new()
    {
        Code = nameof(UserAlreadyInRole),
        Description = Format(nameof(UserAlreadyInRole), role ?? string.Empty)
    };

    public override IdentityError UserNotInRole(string? role) => new()
    {
        Code = nameof(UserNotInRole),
        Description = Format(nameof(UserNotInRole), role ?? string.Empty)
    };

    public override IdentityError PasswordTooShort(int length) => new()
    {
        Code = nameof(PasswordTooShort),
        Description = Format(nameof(PasswordTooShort), length)
    };

    public override IdentityError PasswordRequiresNonAlphanumeric() => new()
    {
        Code = nameof(PasswordRequiresNonAlphanumeric),
        Description = Get(nameof(PasswordRequiresNonAlphanumeric))
    };

    public override IdentityError PasswordRequiresDigit() => new()
    {
        Code = nameof(PasswordRequiresDigit),
        Description = Get(nameof(PasswordRequiresDigit))
    };

    public override IdentityError PasswordRequiresLower() => new()
    {
        Code = nameof(PasswordRequiresLower),
        Description = Get(nameof(PasswordRequiresLower))
    };

    public override IdentityError PasswordRequiresUpper() => new()
    {
        Code = nameof(PasswordRequiresUpper),
        Description = Get(nameof(PasswordRequiresUpper))
    };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new()
    {
        Code = nameof(PasswordRequiresUniqueChars),
        Description = Format(nameof(PasswordRequiresUniqueChars), uniqueChars)
    };

    public override IdentityError RecoveryCodeRedemptionFailed() => new()
    {
        Code = nameof(RecoveryCodeRedemptionFailed),
        Description = Get(nameof(RecoveryCodeRedemptionFailed))
    };
}

