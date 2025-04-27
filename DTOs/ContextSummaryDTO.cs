using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Backend.DTOs
{
    public class ContextSummaryDTO
    {
        public Guid summaryID { get; set; }


        public string summaryText { get; set; } = null!;

        public Vector<float>? embedding { get; set; }

        public DateTime? createdAt { get; set; }
    }
}