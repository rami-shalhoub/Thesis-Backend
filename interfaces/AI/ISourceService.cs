using Backend.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.interfaces
{
    public interface ISourceService
    {
        Task<List<SourceCitationDTO>> ExtractSourcesAsync(string response);

        Task<List<SourceCitationDTO>> ValidateSourcesAsync(List<SourceCitationDTO> sources);
        
        Task<List<SourceCitationDTO>> FormatSourcesAsync(List<SourceCitationDTO> sources);
    }
}
