using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Resources;

namespace DigitalniProdukty.Models.Auth;

public sealed class RegisterInputModel
{
    // For admins: allow selecting which type of account to create.
    // For non-admins: the controller will always force EndUser.
    [Display(Name = nameof(Annotations.Field_AccountType), ResourceType = typeof(Annotations))]
    public string? AccountType { get; set; }

    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [EmailAddress(ErrorMessageResourceName = nameof(Annotations.Validation_EmailAddress), ErrorMessageResourceType = typeof(Annotations))]
    [Display(Name = nameof(Annotations.Field_Email), ResourceType = typeof(Annotations))]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [StringLength(100, ErrorMessageResourceName = nameof(Annotations.Validation_StringLength), ErrorMessageResourceType = typeof(Annotations), MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = nameof(Annotations.Field_Password), ResourceType = typeof(Annotations))]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [DataType(DataType.Password)]
    [Display(Name = nameof(Annotations.Field_ConfirmPassword), ResourceType = typeof(Annotations))]
    [Compare(nameof(Password), ErrorMessageResourceName = nameof(Annotations.Validation_PasswordsDoNotMatch), ErrorMessageResourceType = typeof(Annotations))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

