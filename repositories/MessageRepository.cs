using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.data;
using Backend.interfaces;
using Backend.models;
using Microsoft.EntityFrameworkCore;

namespace Backend.services
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ThesisDappDBContext _context;

        public MessageRepository(ThesisDappDBContext context)
        {
            _context = context;
        }

        public async Task<Message> CreateAsync(Message message)
        {
            message.messageID = Guid.NewGuid();
            message.createdAt = DateTime.Now;

            //* Get the next sequence number for this session
            int nextSequence = await _context.Message
                .Where(m => m.sessionID == message.sessionID)
                .CountAsync() + 1;

            message.sequenceNumber = nextSequence;

            await _context.Message.AddAsync(message);
            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<Message> GetByIdAsync(Guid messageId)
        {
            return await _context.Message
                .FirstOrDefaultAsync(m => m.messageID == messageId)
                ?? throw new InvalidOperationException("Message not found.");
        }

        public async Task<List<Message>> GetBySessionIdAsync(Guid sessionId)
        {
            return await _context.Message
                .Where(m => m.sessionID == sessionId)
                .OrderBy(m => m.sequenceNumber)
                .ToListAsync();
        }

        public async Task<Message> UpdateAsync(Message message)
        {
            _context.Message.Update(message);
            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<Message> GetLastMessageAsync(Guid sessionId)
        {
            var lastMessage = await _context.Message
                .Where(m => m.sessionID == sessionId)
                .OrderByDescending(m => m.sequenceNumber)
                .FirstOrDefaultAsync();

            return lastMessage ?? throw new InvalidOperationException("No messages found for the given session ID.");
        }
    }
}
