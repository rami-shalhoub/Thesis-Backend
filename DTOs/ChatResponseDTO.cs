using System;
using System.Collections.Generic;

namespace Backend.DTOs
{
    public class ChatResponseDTO
    {
        public string? Response { get; set; }

        public List<SourceCitationDTO> Sources { get; set; } = new List<SourceCitationDTO>();

        public Guid SessionId { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
