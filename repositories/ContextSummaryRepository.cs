using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.data;
using Backend.interfaces;
using Backend.models;
using Microsoft.EntityFrameworkCore;

namespace Backend.repositories
{
    public class ContextSummaryRepository : IContextSummaryRepository
    {
        private readonly ThesisDappDBContext _context;

        public ContextSummaryRepository(ThesisDappDBContext context)
        {
            _context = context;
        }

        public async Task<ContextSummary> CreateAsync(ContextSummary summary)
        {
            summary.summaryID = Guid.NewGuid();
            summary.createdAt = DateTime.Now;

            await _context.ContextSummary.AddAsync(summary);
            await _context.SaveChangesAsync();

            return summary;
        }

        public async Task<ContextSummary> GetByIdAsync(Guid summaryId)
        {
            var result = await _context.ContextSummary
                .FirstOrDefaultAsync(s => s.summaryID == summaryId);

            return result ?? throw new InvalidOperationException("ContextSummary not found.");
        }

        public async Task<List<ContextSummary>> GetBySessionIdAsync(Guid sessionId)
        {
            return await _context.ContextSummary
                .Where(s => s.sessionID == sessionId)
                .OrderByDescending(s => s.createdAt)
                .ToListAsync();
        }

        public async Task<ContextSummary> GetMostRecentAsync(Guid sessionId)
        {
            var result = await _context.ContextSummary
                .Where(s => s.sessionID == sessionId)
                .OrderByDescending(s => s.createdAt)
                .FirstOrDefaultAsync();

            return result ?? throw new InvalidOperationException("No recent ContextSummary found for the given session ID.");
        }
    }
}
