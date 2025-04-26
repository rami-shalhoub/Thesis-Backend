using System;

namespace Backend.DTOs
{
    public class SourceCitationDTO
    {
        public string? Url { get; set; }
        
        public string? Title { get; set; }
        
        public string? SourceType { get; set; }

        public string? Citation { get; set; }

        public string? Website { get; set; }
    }
}
