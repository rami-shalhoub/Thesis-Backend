using Backend.DTOs;
using Backend.models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.interfaces
{
    public interface IChatService
    {
        Task<SessionDTO> CreateSessionAsync(Guid userId);

        Task<SessionDTO?> GetSessionAsync(Guid sessionId);

        Task<bool> CloseSessionAsync(Guid sessionId);

        Task<ChatResponseDTO> SendMessageAsync(Guid sessionId, string prompt);

        Task<List<Session>?> GetAllSessionsAsync(Guid userId);

        Task<bool> DeleteSessionAsync(Guid sessionId);
        
        Task<bool> DeleteAllSessionsAsync(Guid userId);
    }
}
