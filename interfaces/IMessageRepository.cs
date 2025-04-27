using Backend.models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.interfaces
{
    public interface IMessageRepository
    {
        Task<Message> CreateAsync(Message message);

        Task<Message> GetByIdAsync(Guid messageId);

        Task<List<Message>> GetBySessionIdAsync(Guid sessionId);

        Task<Message> UpdateAsync(Message message);

        Task<Message> GetLastMessageAsync(Guid sessionId);
        
        Task<bool> DeleteMessageAsync(Guid sessionId);

        Task<bool> DeleteAllMessagesAsync(List<Guid> sessionIds);
    }
}
