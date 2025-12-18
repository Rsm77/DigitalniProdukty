using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Resources;

namespace DigitalniProdukty.Models.Auth;

public sealed class LoginInputModel
{
    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [EmailAddress(ErrorMessageResourceName = nameof(Annotations.Validation_EmailAddress), ErrorMessageResourceType = typeof(Annotations))]
    [Display(Name = nameof(Annotations.Field_Email), ResourceType = typeof(Annotations))]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [DataType(DataType.Password)]
    [Display(Name = nameof(Annotations.Field_Password), ResourceType = typeof(Annotations))]
    public string Password { get; set; } = string.Empty;

    [Display(Name = nameof(Annotations.Field_RememberMe), ResourceType = typeof(Annotations))]
    public bool RememberMe { get; set; }
}

