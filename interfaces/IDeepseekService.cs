using Backend.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pgvector;

namespace Backend.interfaces
{
    public interface IDeepseekService
    {
        Task<ChatResponseDTO> GetCompletionAsync(string prompt, List<MessageDTO> conversationHistory);

        Task<string> ExtractLegalTopicsAsync(string prompt);

        Task<string> GenerateSessionTitleAsync(string prompt);

        Task<string> GenerateSummaryAsync(List<MessageDTO> conversationHistory);

        Task<Pgvector.Vector> GenerateEmbeddingAsync(string text);
    }
}
