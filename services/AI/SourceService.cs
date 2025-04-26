using Backend.DTOs;
using Backend.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Backend.services.AI
{
    public class SourceService : ISourceService
    {
        private readonly string[] _allowedDomains = new[] 
        { 
            "legislation.gov.uk", 
            "bailii.org" 
        };

        public async Task<List<SourceCitationDTO>> ExtractSourcesAsync(string response)
        {
            var sources = new List<SourceCitationDTO>();
            
            // Extract URLs using regex
            var urlRegex = new Regex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
            var matches = urlRegex.Matches(response);
            
            foreach (Match match in matches)
            {
                string url = match.Value;
                
                // Check if the URL is from an allowed domain
                if (_allowedDomains.Any(domain => url.Contains(domain)))
                {
                    // Try to extract title and citation information from surrounding text
                    string surroundingText = ExtractSurroundingText(response, url, 200);
                    
                    var source = new SourceCitationDTO
                    {
                        Url = url,
                        Title = ExtractTitle(surroundingText, url),
                        SourceType = DetermineSourceType(url),
                        Citation = ExtractCitation(surroundingText),
                        Website = _allowedDomains.First(domain => url.Contains(domain))
                    };
                    
                    sources.Add(source);
                }
            }
            
            // Also look for citations in a reference section
            ExtractReferenceSectionCitations(response, sources);
            
            return await Task.FromResult(sources);
        }

        public async Task<List<SourceCitationDTO>> ValidateSourcesAsync(List<SourceCitationDTO> sources)
        {
            // Filter out sources that don't come from allowed domains
            var validSources = sources.Where(s => 
                _allowedDomains.Any(domain => s.Url != null && s.Url.Contains(domain))).ToList();
            
            return await Task.FromResult(validSources);
        }

        public async Task<List<SourceCitationDTO>> FormatSourcesAsync(List<SourceCitationDTO> sources)
        {
            foreach (var source in sources)
            {
                // Format the title if it's empty or too long
                if (string.IsNullOrWhiteSpace(source.Title) || source.Title.Length > 100)
                {
                    source.Title = source.Url != null ? FormatTitleFromUrl(source.Url) : "Unknown Title";
                }
                
                // Format the citation if needed
                if (!string.IsNullOrWhiteSpace(source.Citation))
                {
                    if (!string.IsNullOrWhiteSpace(source.SourceType))
                    {
                        source.Citation = FormatCitation(source.Citation, source.SourceType);
                    }
                }
            }
            
            return await Task.FromResult(sources);
        }

        #region Helper Methods

        private string ExtractSurroundingText(string fullText, string target, int charCount)
        {
            int index = fullText.IndexOf(target);
            if (index == -1) return string.Empty;
            
            int startIndex = Math.Max(0, index - charCount);
            int endIndex = Math.Min(fullText.Length, index + target.Length + charCount);
            
            return fullText.Substring(startIndex, endIndex - startIndex);
        }

        private string ExtractTitle(string text, string url)
        {
            // Try to find a title in brackets or quotes near the URL
            var titleRegex = new Regex(@"[""'\[]([^""'\]]+)[""'\]]");
            var matches = titleRegex.Matches(text);
            
            foreach (Match match in matches)
            {
                if (text.IndexOf(match.Value) < text.IndexOf(url) + url.Length + 100 &&
                    text.IndexOf(match.Value) > text.IndexOf(url) - 100)
                {
                    return match.Groups[1].Value.Trim();
                }
            }
            
            return FormatTitleFromUrl(url);
        }

        private string FormatTitleFromUrl(string url)
        {
            // Extract a title from the URL
            var uri = new Uri(url);
            string path = uri.AbsolutePath;
            
            // Remove file extensions and split by slashes or hyphens
            path = Regex.Replace(path, @"\.\w+$", "");
            string[] parts = path.Split(new[] { '/', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length > 0)
            {
                // Take the last meaningful part and format it
                string lastPart = parts.Last();
                lastPart = Regex.Replace(lastPart, @"([a-z])([A-Z])", "$1 $2"); // Add spaces between camelCase
                lastPart = lastPart.Replace('-', ' ').Replace('_', ' ');
                
                // Capitalize first letter of each word
                var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
                return textInfo.ToTitleCase(lastPart.ToLower());
            }
            
            return uri.Host;
        }

        private string DetermineSourceType(string url)
        {
            if (url.Contains("legislation.gov.uk"))
            {
                if (url.Contains("/ukpga/")) return "Primary Legislation";
                if (url.Contains("/uksi/")) return "Secondary Legislation";
                if (url.Contains("/ukla/")) return "Local Act";
                return "Legislation";
            }
            else if (url.Contains("bailii.org"))
            {
                if (url.Contains("/uk/cases/UKSC/")) return "Supreme Court Case";
                if (url.Contains("/ew/cases/EWHC/")) return "High Court Case";
                if (url.Contains("/ew/cases/EWCA/")) return "Court of Appeal Case";
                return "Case Law";
            }
            
            return "Legal Resource";
        }

        private string ExtractCitation(string text)
        {
            // Look for common citation patterns
            var patterns = new[]
            {
                @"\[(\d{4})\]\s+(\w+)\s+(\d+)", // [2020] UKSC 12
                @"(\d{4})\s+(\w+)\s+(\d+)",     // 2020 UKSC 12
                @"section\s+(\d+[A-Za-z]?)",    // section 12
                @"s\.?\s*(\d+[A-Za-z]?)",       // s. 12 or s12
                @"paragraph\s+(\d+[A-Za-z]?)",  // paragraph 12
                @"para\.?\s*(\d+[A-Za-z]?)",    // para. 12 or para12
                @"schedule\s+(\d+[A-Za-z]?)",   // schedule 1
                @"sch\.?\s*(\d+[A-Za-z]?)"      // sch. 1 or sch1
            };
            
            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Value;
                }
            }
            
            return string.Empty;
        }

        private string FormatCitation(string citation, string sourceType)
        {
            // Format the citation based on the source type
            if (sourceType.Contains("Case"))
            {
                // Ensure case citations are properly formatted
                citation = Regex.Replace(citation, @"(\d{4})\s+(\w+)\s+(\d+)", "[$1] $2 $3");
            }
            else if (sourceType.Contains("Legislation"))
            {
                // Format legislation citations
                citation = Regex.Replace(citation, @"s\.?\s*(\d+)", "Section $1");
                citation = Regex.Replace(citation, @"para\.?\s*(\d+)", "Paragraph $1");
                citation = Regex.Replace(citation, @"sch\.?\s*(\d+)", "Schedule $1");
            }
            
            return citation;
        }

        private void ExtractReferenceSectionCitations(string response, List<SourceCitationDTO> sources)
        {
            // Look for a references or sources section
            var referenceRegex = new Regex(@"(?:References|Sources|Citations):\s*\n(.*?)(?:\n\n|\Z)", 
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
            
            var match = referenceRegex.Match(response);
            if (match.Success)
            {
                string referencesSection = match.Groups[1].Value;
                var lines = referencesSection.Split('\n');
                
                foreach (var line in lines)
                {
                    // Extract URL from the line
                    var urlMatch = Regex.Match(line, @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
                    if (urlMatch.Success)
                    {
                        string url = urlMatch.Value;
                        
                        // Check if this URL is already in our sources
                        if (!sources.Any(s => s.Url == url) && 
                            _allowedDomains.Any(domain => url.Contains(domain)))
                        {
                            var source = new SourceCitationDTO
                            {
                                Url = url,
                                Title = ExtractTitle(line, url),
                                SourceType = DetermineSourceType(url),
                                Citation = ExtractCitation(line),
                                Website = _allowedDomains.First(domain => url.Contains(domain))
                            };
                            
                            sources.Add(source);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
