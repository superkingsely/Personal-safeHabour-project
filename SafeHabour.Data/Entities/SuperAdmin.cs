using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafeHabour.Data.Entities;

public class SuperAdmin : BaseModel
{
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
