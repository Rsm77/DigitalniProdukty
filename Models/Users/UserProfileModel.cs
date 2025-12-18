using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DigitalniProdukty.Models.Users;

public sealed class UserProfileModel
{
    [Key]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public IdentityUser? User { get; set; }
}
