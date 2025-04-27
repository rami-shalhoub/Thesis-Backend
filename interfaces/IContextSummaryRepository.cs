using Backend.models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.interfaces
{
    public interface IContextSummaryRepository
    {
        Task<ContextSummary> CreateAsync(ContextSummary summary);

        Task<ContextSummary> GetByIdAsync(Guid summaryId);

        Task<List<ContextSummary>> GetBySessionIdAsync(Guid sessionId);

        Task<ContextSummary> GetMostRecentAsync(Guid sessionId);
        
        Task<bool> DeleteSummaryAsync(Guid sessionId);

        Task<bool> DeleteAllSummariesAsync(List<Guid> sessionIds);
    }
}
