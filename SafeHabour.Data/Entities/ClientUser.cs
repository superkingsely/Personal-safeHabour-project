using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafeHabour.Data.Entities;

public class ClientUser : BaseModel
{
    public int ClientType { get; set; }
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    public virtual User User { get; set; }
}
