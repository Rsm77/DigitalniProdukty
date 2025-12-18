using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Resources;

namespace DigitalniProdukty.Models.Account;

public sealed class ProfileInputModel
{
    [Display(Name = nameof(Annotations.Field_Email), ResourceType = typeof(Annotations))]
    public string Email { get; set; } = string.Empty;

    [Display(Name = nameof(Annotations.Field_PhoneNumber), ResourceType = typeof(Annotations))]
    public string? PhoneNumber { get; set; }
}

