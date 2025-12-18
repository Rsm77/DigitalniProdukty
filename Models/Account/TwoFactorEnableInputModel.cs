using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Resources;

namespace DigitalniProdukty.Models.Account;

public sealed class TwoFactorEnableInputModel
{
    [Required(ErrorMessageResourceName = nameof(Annotations.Validation_Required), ErrorMessageResourceType = typeof(Annotations))]
    [Display(Name = nameof(Annotations.Field_VerificationCode), ResourceType = typeof(Annotations))]
    public string Code { get; set; } = string.Empty;
}

