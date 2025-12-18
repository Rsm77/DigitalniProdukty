using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DigitalniProdukty.Security;

namespace DigitalniProdukty.Models.Licensing;

public class KeyGroupModel
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = Authz.Roles.Distributor;

    public DateTime CreatedAt { get; set; }

    public ICollection<KeyGroupMemberModel> Members { get; set; } = new List<KeyGroupMemberModel>();
}
