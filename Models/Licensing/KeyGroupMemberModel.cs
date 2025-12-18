using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DigitalniProdukty.Models.Licensing;

public class KeyGroupMemberModel
{
    public int Id { get; set; }

    [Required]
    public Guid GroupId { get; set; }

    [Required]
    public KeyGroupModel Group { get; set; } = null!;

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public IdentityUser? User { get; set; }

    public DateTime AddedAt { get; set; }
}
