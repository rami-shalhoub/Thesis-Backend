using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Backend.DTOs;
using Backend.interfaces;
using Backend.models;
using Pgvector;

namespace Backend.services.AI
{
    public class ChatService : IChatService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IContextSummaryRepository _contextSummaryRepository;
        private readonly IDeepseekService _deepseekService;
        private readonly ISourceService _sourceService;
        private readonly IMapper _mapper;

        public ChatService(
            ISessionRepository sessionRepository,
            IMessageRepository messageRepository,
            IContextSummaryRepository contextSummaryRepository,
            IDeepseekService deepseekService,
            ISourceService sourceService,
            IMapper mapper
        )
        {
            _sessionRepository = sessionRepository;
            _messageRepository = messageRepository;
            _contextSummaryRepository = contextSummaryRepository;
            _deepseekService = deepseekService;
            _sourceService = sourceService;
            _mapper = mapper;
        }

        public async Task<SessionDTO> CreateSessionAsync(Guid userId)
        {
            //* Create a session with only the required fields
            var session = new Session
            {
                sessionID = Guid.NewGuid(),
                userID = userId,
                isActive = true,

                //* Initialize empty JSON objects for contextWindow and analysisParameter
                contextWindow = "{}",
                analysisParameter = JsonSerializer.Serialize(new
                {
                    detail_level = "concise",
                    jurisdiction = "GB",
                    citation_format = "OSCOLA"
                }),
            };

            var createdSession = await _sessionRepository.CreateAsync(session);

            return MapSessionToDTO(createdSession);
        }

        public async Task<SessionDTO?> GetSessionAsync(Guid sessionId)
        {
            var session = await _sessionRepository.GetWithMessagesAsync(sessionId);
            if (session == null)
            {
                return null;
            }

            return MapSessionToDTO(session);
        }

        public async Task<List<Session>?> GetAllSessionsAsync()
        {
            var sessions = await _sessionRepository.GetAllSessionsAsync();
            if (sessions == null)
            {
                return null;
            }

            return sessions;
        }

        public async Task<bool> CloseSessionAsync(Guid sessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
            {
                return false;
            }

            session.isActive = false;
            await _sessionRepository.UpdateAsync(session);

            return true;
        }

        public async Task<ChatResponseDTO> SendMessageAsync(Guid sessionId, string prompt)
        {
            var session = await _sessionRepository.GetWithMessagesAsync(sessionId);

            //* Check is the session exists
            if (session == null)
            {
                throw new ArgumentException($"Session with ID {sessionId} not found");
            }

            //* Check if the session is active
            if (session.isActive != true)
            {
                throw new InvalidOperationException("Cannot send messages to an inactive session");
            }

            //* Get conversation history
            var messages = session.Message.OrderBy(m => m.sequenceNumber).ToList();
            var conversationHistory = messages.Select(MapMessageToDTO).ToList();

            //* If this is the first message, extract legal topics and generate a session title
            bool isFirstMessage = messages.Count == 0;
            if (isFirstMessage)
            {
                //* Extract legal topics
                string legalTopics = await _deepseekService.ExtractLegalTopicsAsync(prompt);
                // session.legalTopics = legalTopics;

                //* Generate a session title
                string sessionTitle = await _deepseekService.GenerateSessionTitleAsync(prompt);
                session.sessionTitle = sessionTitle;

                //* Update the session
                await _sessionRepository.UpdateAsync(session, legalTopics);
            }

            //* Augment the prompt with context if available
            string augmentedPrompt = prompt;
            if (!isFirstMessage && !string.IsNullOrEmpty(session.contextWindow) && session.contextWindow != "{}")
            {
                augmentedPrompt = await AugmentPromptWithContextAsync(prompt, session);
            }

            //* Send the prompt to the DeepSeek API
            var response = await _deepseekService.GetCompletionAsync(augmentedPrompt, conversationHistory);
            response.SessionId = sessionId;

            //* Save the message to the database
            var message = new Message
            {
                sessionID = sessionId,
                prompt = prompt,
                response = response.Response ?? string.Empty,
                metadata = JsonSerializer.Serialize(new { sources = response.Sources }),
            };

            await _messageRepository.CreateAsync(message);

            //* Update conversation history with the new message
            conversationHistory.Add(new MessageDTO
            {
                Prompt = prompt,
                Response = response.Response,
                SequenceNumber = messages.Count + 1
            });

            //* Generate a summary if we have enough messages (at least 4)
            if (conversationHistory.Count >= 4)
            {
                await GenerateAndStoreSummaryAsync(session, conversationHistory);
            }

            return response;
        }

        private async Task GenerateAndStoreSummaryAsync(Session session, List<MessageDTO> conversationHistory)
        {
            try
            {
                //* Take the last 4 messages
                var recentMessages = conversationHistory
                    .OrderByDescending(m => m.SequenceNumber)
                    .Take(4)
                    .OrderBy(m => m.SequenceNumber)
                    .ToList();

                //* Generate a summary
                string summaryText = await _deepseekService.GenerateSummaryAsync(recentMessages);

                // Generate an embedding for the summary
                var embedding = await _deepseekService.GenerateEmbeddingAsync(summaryText);

                // Create a new context summary
                var summary = new ContextSummary
                {
                    sessionID = session.sessionID,
                    summaryText = summaryText,
                    embedding = embedding
                };

                // Save the summary
                var createdSummary = await _contextSummaryRepository.CreateAsync(summary);

                // Update the context window
                await UpdateContextWindowAsync(session, createdSummary.summaryID);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error generating summary: {ex.Message}");
            }
        }
         
        private async Task UpdateContextWindowAsync(Session session, Guid latestSummaryId)
        {
            try
            {
                //* Get previous summaries
                var summaries = await _contextSummaryRepository.GetBySessionIdAsync(session.sessionID);
                var previousSummaryIds = summaries
                    .OrderByDescending(s => s.createdAt)
                    .Select(s => s.summaryID)
                    .Take(5) //? Keep track of the last 5 summaries
                    .ToList();

                //* Extract legal topics with null check
                List<string> legalTopics;
                if (string.IsNullOrEmpty(session.legalTopics))
                {
                    Console.WriteLine("Warning: legalTopics is null or empty, using default topics");
                    legalTopics = new List<string> { "UK Law" };
                }
                else
                {
                    legalTopics = session.legalTopics.Split(',')
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();
                    
                    // If after filtering we have no topics, add a default
                    if (legalTopics.Count == 0)
                    {
                        legalTopics.Add("UK Law");
                    }
                }

                //* Create context window JSON
                var contextWindow = new
                {
                    current_focus = latestSummaryId,
                    previous_summaries = previousSummaryIds,
                    jurisdiction = "GB",
                    legal_topics = legalTopics
                };

                //* Update session with detailed logging
                var contextWindowJson = JsonSerializer.Serialize(contextWindow);
                Console.WriteLine($"Updating context window: {contextWindowJson}");
                
                session.contextWindow = contextWindowJson;
                var updatedSession = await _sessionRepository.UpdateAsync(session);
                
                Console.WriteLine($"Context window updated successfully. Session ID: {updatedSession.sessionID}, Updated at: {updatedSession.updatedAt}");
            }
            catch (Exception ex)
            {
                // Log the exception with more details
                Console.WriteLine($"Error updating context window: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
         
        private async Task<string> AugmentPromptWithContextAsync(string prompt, Session session)
        {
            try
            {
                //* Deserialize context window
                var contextWindow = JsonSerializer.Deserialize<JsonElement>(session.contextWindow);

                //* Check if current_focus exists
                if (!contextWindow.TryGetProperty("current_focus", out var currentFocusElement))
                {
                    return prompt;
                }

                //* Get current focus summary ID
                var currentFocusString = currentFocusElement.GetString();
                if (string.IsNullOrEmpty(currentFocusString))
                {
                    throw new InvalidOperationException("Current focus ID is missing or invalid.");
                }
                var currentFocusId = Guid.Parse(currentFocusString);

                //* Get the summary
                var summary = await _contextSummaryRepository.GetByIdAsync(currentFocusId);
                if (summary == null)
                {
                    return prompt;
                }

                // Augment prompt
                return $"Previous context: {summary.summaryText}\n\nUser query: {prompt}";
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error augmenting prompt: {ex.Message}");
                return prompt;
            }
        }

        #region Helper Methods

        private SessionDTO MapSessionToDTO(Session session)
        {
            var sessionDto = _mapper.Map<SessionDTO>(session);

            if (session.Message != null)
            {
                sessionDto.Messages = session.Message.Select(MapMessageToDTO).ToList();
            }

            return sessionDto;
        }

        private MessageDTO MapMessageToDTO(Message message)
        {
            return _mapper.Map<MessageDTO>(message);
        }

        #endregion
    }
}
