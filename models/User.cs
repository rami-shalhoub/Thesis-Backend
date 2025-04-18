using System;
using System.Collections.Generic;

namespace Backend.models;

public partial class User
{
    public Guid userID { get; set; }

    public string name { get; set; } = null!;

    public string email { get; set; } = null!;

    /// <summary>
    /// hased
    /// </summary>
    public string password { get; set; } = null!;

    public string role { get; set; } = null!;

    public string organisationID { get; set; } = null!;

    public DateTime? createdAt { get; set; }

    public DateTime? lastLogin { get; set; }

    public bool isActive { get; set; }

    public string refreshToken { get; set; } = null!;

    public DateTime tokenExpiry { get; set; }

    public virtual ICollection<AccessLog> AccessLog { get; set; } = new List<AccessLog>();

    public virtual ICollection<Document> Document { get; set; } = new List<Document>();

    public virtual ICollection<RevokedToken> RevokedToken { get; set; } = new List<RevokedToken>();

    public virtual ICollection<Session> Session { get; set; } = new List<Session>();
}
