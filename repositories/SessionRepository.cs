using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.data;
using Backend.interfaces;
using Backend.models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace Backend.services
{
    public class SessionRepository : ISessionRepository
    {
        private readonly ThesisDappDBContext _context;

        public SessionRepository(ThesisDappDBContext context)
        {
            _context = context;
        }

        public async Task<Session> CreateAsync(Session session)
        {
            // session.sessionID = Guid.NewGuid();
            session.createdAt = DateTime.Now;
            session.updatedAt = DateTime.Now;
            session.isActive = true;
            session.legalTopics = string.Empty;

            await _context.Session.AddAsync(session);
            await _context.SaveChangesAsync();

            return session;
        }
 
        public async Task<Session> GetByIdAsync(Guid sessionId)
        {
            var session = await _context.Session
                .FirstOrDefaultAsync(s => s.sessionID == sessionId);

            if (session == null)
            {
                throw new KeyNotFoundException($"Session with ID {sessionId} was not found.");
            }

            return session;
        }

        public async Task<Session> UpdateAsync(Session session, string? legalTopics = null)
        {
            try
            {
                if (legalTopics != null)
                {
                    Console.WriteLine($"Updating session {session.sessionID} with legal topics: {legalTopics}");
                    session.legalTopics = legalTopics;
                }
            
                session.updatedAt = DateTime.Now;

                _context.Session.Update(session);
                int rowsAffected = await _context.SaveChangesAsync();
                
                Console.WriteLine($"Session update completed. Rows affected: {rowsAffected}");
                Console.WriteLine($"Updated session: ID={session.sessionID}, Title={session.sessionTitle}, ContextWindow={session.contextWindow.Substring(0, Math.Min(session.contextWindow.Length, 100))}...");

                // Refresh the session from the database to ensure we have the latest data
                await _context.Entry(session).ReloadAsync();
                
                return session;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating session: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw the exception after logging
            }
        }

        public async Task<Session> GetWithMessagesAsync(Guid sessionId)
        {
            var session = await _context.Session
                .Include(s => s.Message.OrderBy(m => m.sequenceNumber))
                .FirstOrDefaultAsync(s => s.sessionID == sessionId);

            if (session == null)
            {
                throw new KeyNotFoundException($"Session with ID {sessionId} was not found.");
            }

            return session;
        }


        // private async Task<string> GetAllLegalTopicsAsync()
        // {
        //     var legalTopics = await _context.Session
        //     .Select(s => s.legalTopics)
        //     .ToListAsync();

        //     var uniqueTopics = legalTopics
        //     .Where(topic => !string.IsNullOrEmpty(topic))
        //     .SelectMany(topic => topic.Split(", "))
        //     .Distinct();

        //     return string.Join(", ", uniqueTopics);
        // }
        
        public async Task<List<Session>> GetAllSessionsAsync()
        {
            var sessions = await _context.Session.ToListAsync() ?? throw new KeyNotFoundException($"No sessions found.");
            return sessions;
        }
    }
}
