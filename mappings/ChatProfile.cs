using System.Text.Json;
using AutoMapper;
using Backend.DTOs;
using Backend.models;

namespace Backend.services.Mapping
{
    public class ChatProfile : Profile
    {
        public ChatProfile()
        {
            CreateMap<Session, SessionDTO>()
               .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.sessionID))
               .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.userID))
               .ForMember(dest => dest.DocumentId, opt => opt.MapFrom(src => src.documentID))
               .ForMember(dest => dest.SessionTitle, opt => opt.MapFrom(src => src.sessionTitle))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.isActive ?? true))
               .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.createdAt ?? System.DateTime.Now))
               .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.updatedAt))
               .ForMember(dest => dest.LegalTopics, opt => opt.MapFrom(src => src.legalTopics))
               .ForMember(dest => dest.ContextWindow, opt => opt.MapFrom(src => src.contextWindow))
               .ForMember(dest => dest.AnalysisParameter, opt => opt.MapFrom(src => src.analysisParameter));

            CreateMap<SessionDTO, Session>()
                .ForMember(dest => dest.sessionID, opt => opt.MapFrom(src => src.SessionId))
                .ForMember(dest => dest.userID, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.documentID, opt => opt.MapFrom(src => src.DocumentId))
                .ForMember(dest => dest.sessionTitle, opt => opt.MapFrom(src => src.SessionTitle))
                .ForMember(dest => dest.isActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.createdAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.updatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.legalTopics, opt => opt.MapFrom(src => src.LegalTopics))
                .ForMember(dest => dest.contextWindow, opt => opt.MapFrom(src => src.ContextWindow ?? "{}"))
                .ForMember(dest => dest.analysisParameter, opt => opt.MapFrom(src => src.AnalysisParameter ?? "{}"));

            // Message mapping
            CreateMap<Message, MessageDTO>()
                .ForMember(dest => dest.MessageId, opt => opt.MapFrom(src => src.messageID))
                .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.sessionID))
                .ForMember(dest => dest.Prompt, opt => opt.MapFrom(src => src.prompt))
                .ForMember(dest => dest.Response, opt => opt.MapFrom(src => src.response))
                .ForMember(dest => dest.SequenceNumber, opt => opt.MapFrom(src => src.sequenceNumber))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.createdAt ?? System.DateTime.Now))
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.metadata))
                .AfterMap((src, dest) =>
                {
                    // Parse sources from metadata if available
                    if (!string.IsNullOrEmpty(src.metadata))
                    {
                        try
                        {
                            var metadata = JsonSerializer.Deserialize<JsonElement>(src.metadata);
                            if (metadata.TryGetProperty("sources", out var sourcesElement))
                            {
                                var sources = JsonSerializer.Deserialize<List<SourceCitationDTO>>(sourcesElement.GetRawText());
                                if (sources != null)
                                {
                                    dest.Sources = sources;
                                }
                            }
                        }
                        catch (JsonException)
                        {
                            // Ignore JSON parsing errors
                        }
                    }
                });

            CreateMap<MessageDTO, Message>()
                .ForMember(dest => dest.messageID, opt => opt.MapFrom(src => src.MessageId))
                .ForMember(dest => dest.sessionID, opt => opt.MapFrom(src => src.SessionId))
                .ForMember(dest => dest.prompt, opt => opt.MapFrom(src => src.Prompt))
                .ForMember(dest => dest.response, opt => opt.MapFrom(src => src.Response))
                .ForMember(dest => dest.sequenceNumber, opt => opt.MapFrom(src => src.SequenceNumber))
                .ForMember(dest => dest.createdAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.metadata, opt => opt.MapFrom(src => src.Metadata))
                .AfterMap((src, dest) =>
                {
                    // Serialize sources to metadata if available
                    if (src.Sources != null && src.Sources.Count > 0)
                    {
                        var metadata = new { sources = src.Sources };
                        dest.metadata = JsonSerializer.Serialize(metadata);
                    }
                });
        }
    }
}
