using System;
using System.Collections.Generic;

namespace Backend.models;

public partial class RevokedToken
{
    public string tokenID { get; set; } = null!;

    public Guid userID { get; set; }

    public DateTime? expiry { get; set; }

    public DateTime? revokedAt { get; set; }

    public virtual User user { get; set; } = null!;
}
