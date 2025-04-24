using System;
using System.Collections.Generic;

namespace Backend.models;

public partial class Session
{
    public Guid sessionID { get; set; }

    public Guid userID { get; set; }

    public Guid? documentID { get; set; }

    public string contextWindow { get; set; } = null!;

    public string legalTopics { get; set; } = null!;

    public DateTime? createdAt { get; set; }

    public string analysisParameter { get; set; } = null!;

    public string? sessionTitle { get; set; }

    public bool? isActive { get; set; }

    public DateTime? updatedAt { get; set; }

    public virtual ICollection<AccessLog> AccessLog { get; set; } = new List<AccessLog>();

    public virtual ICollection<ContextSummary> ContextSummary { get; set; } = new List<ContextSummary>();

    public virtual ICollection<Message> Message { get; set; } = new List<Message>();

    public virtual Document? document { get; set; }

    public virtual User user { get; set; } = null!;
}
