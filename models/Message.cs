using System;
using System.Collections.Generic;

namespace Backend.models;

public partial class Message
{
    public Guid messageID { get; set; }

    public Guid sessionID { get; set; }

    public string prompt { get; set; } = null!;

    public string response { get; set; } = null!;

    public int sequenceNumber { get; set; }

    public DateTime? createdAt { get; set; }

    public string? metadata { get; set; }

    public virtual Session session { get; set; } = null!;
}
