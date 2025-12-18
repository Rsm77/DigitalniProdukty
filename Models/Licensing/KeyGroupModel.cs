using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalniProdukty.Models.Licensing;

public class KeyGroupModel
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public ICollection<KeyGroupMemberModel> Members { get; set; } = new List<KeyGroupMemberModel>();
}
