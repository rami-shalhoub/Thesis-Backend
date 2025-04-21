using System;
using System.Collections.Generic;

namespace Backend.models;

public partial class LegalResource
{
    public Guid resourceID { get; set; }

    public string title { get; set; } = null!;

    public string contentType { get; set; } = null!;

    /// <summary>
    /// Focus on UK
    /// </summary>
    public string jurisdiction { get; set; } = null!;

    /// <summary>
    /// when the law was enacted
    /// </summary>
    public DateOnly effectiveDate { get; set; }

    public string ipfsCID { get; set; } = null!;

    public string content { get; set; } = null!;

    /// <summary>
    /// VECTOR(1536)
    /// </summary>
    public long vectorEmbedding { get; set; }

    public DateTime lastUpdated { get; set; }
}
