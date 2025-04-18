using System;
using System.Collections.Generic;

namespace Backend.models;

public partial class AccessLog
{
    public Guid logID { get; set; }

    public Guid userID { get; set; }

    public Guid documentID { get; set; }

    public Guid sessionID { get; set; }

    public string action { get; set; } = null!;

    public DateTime timeStamp { get; set; }

    public virtual Document document { get; set; } = null!;

    public virtual Session session { get; set; } = null!;

    public virtual User user { get; set; } = null!;
}
