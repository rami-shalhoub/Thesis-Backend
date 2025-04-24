using System;
using System.Collections.Generic;

namespace Backend.models;

public partial class ContextSummary
{
    public Guid summaryID { get; set; }

    public Guid sessionID { get; set; }

    public string summaryText { get; set; } = null!;

    public DateTime? createdAt { get; set; }

    public virtual Session session { get; set; } = null!;
}
