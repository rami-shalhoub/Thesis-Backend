using System;
using System.Collections.Generic;
using Pgvector;

namespace Backend.models;

public partial class Document
{
    public Guid documentID { get; set; }

    public string ipfsCID { get; set; } = null!;

    public string blockchainTxID { get; set; } = null!;

    public Guid ownerID { get; set; }

    public string title { get; set; } = null!;

    public string description { get; set; } = null!;

    public string documentType { get; set; } = null!;

    /// <summary>
    /// Focus on UK
    /// </summary>
    public string jurisdiction { get; set; } = null!;

    public byte[] encryptionKey { get; set; } = null!;

    public DateTime? createdAt { get; set; }

    public DateTime? updatedAt { get; set; }

    public string accessControl { get; set; } = null!;

    public Vector? embedding { get; set; }

    public virtual ICollection<AccessLog> AccessLog { get; set; } = new List<AccessLog>();

    public virtual ICollection<Session> Session { get; set; } = new List<Session>();

    public virtual User owner { get; set; } = null!;
}
