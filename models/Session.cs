using System;
using System.Collections.Generic;

namespace Backend.models;

public partial class Session
{
    public Guid sessionID { get; set; }

    public Guid userID { get; set; }

    public Guid? documentID { get; set; }

    public string prompt { get; set; } = null!;

    public string responce { get; set; } = null!;

    public string contextWindow { get; set; } = null!;

    public string legalTopics { get; set; } = null!;

    public DateTime? createdAt { get; set; }

    public string analysisParameter { get; set; } = null!;

    public virtual ICollection<AccessLog> AccessLog { get; set; } = new List<AccessLog>();

    public virtual Document? document { get; set; }

    public virtual User user { get; set; } = null!;
}
