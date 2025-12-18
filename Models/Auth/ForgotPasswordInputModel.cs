using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Resources;

namespace DigitalniProdukty.Models.Auth;

public sealed class ForgotPasswordInputModel
{
    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [EmailAddress(ErrorMessageResourceName = nameof(Annotations.Validation_EmailAddress), ErrorMessageResourceType = typeof(Annotations))]
    [Display(Name = nameof(Annotations.Field_Email), ResourceType = typeof(Annotations))]
    public string Email { get; set; } = string.Empty;
}

