using System;
using System.Collections.Generic;

namespace Backend.DTOs
{
    public class MessageDTO
    {
        public Guid MessageId { get; set; }

        public Guid SessionId { get; set; }

        public string? Prompt { get; set; }

        public string? Response { get; set; }

        public int SequenceNumber { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? Metadata { get; set; }

        public List<SourceCitationDTO> Sources { get; set; } = new List<SourceCitationDTO>();
    }
}
