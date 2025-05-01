using Backend.models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.interfaces
{
    public interface ISessionRepository
    {
        Task<Session> CreateAsync(Session session);

        Task<Session> GetByIdAsync(Guid sessionId);

        Task<Session> UpdateAsync(Session session, string? legalTopics = null);

        Task<Session> GetWithMessagesAsync(Guid sessionId);

        Task<List<Session>> GetAllSessionsAsync(Guid userId);

        Task<bool> DeleteSessionAsync(Guid sessionId);

        Task<bool> DeleteAllSessionsAsync(Guid userId);
    }
}
