using System;
using System.Collections.Generic;
using Backend.models;

namespace Backend.DTOs
{
    public class SessionDTO
    {
        public Guid SessionId { get; set; }

        public Guid UserId { get; set; }

        public Guid? DocumentId { get; set; }

        public string? SessionTitle { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? LegalTopics { get; set; }

        public string? ContextWindow { get; set; }

        public string? AnalysisParameter { get; set; }

        public List<MessageDTO> Messages { get; set; } = new List<MessageDTO>();

        public List<ContextSummaryDTO> contextSummaries { get; set; } = new List<ContextSummaryDTO>();
    }
}
