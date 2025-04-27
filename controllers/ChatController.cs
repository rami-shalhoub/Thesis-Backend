using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;
using Backend.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.controllers
{
    [ApiController]
    [Route("api/ai")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("session")]
        public async Task<ActionResult<SessionDTO>> CreateSession(Guid userId)
        {
            try
            {
                var session = await _chatService.CreateSessionAsync(userId);

                return Ok(session);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("session/{sessionid}")]
        public async Task<ActionResult<SessionDTO>> GetSession(Guid sessionid)
        {
            try
            {
                var session = await _chatService.GetSessionAsync(sessionid);
                if (session == null)
                {
                    return NotFound(new { error = $"Session with ID {sessionid} not found" });
                }

                return Ok(session);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("sessions")]
        public async Task<ActionResult<IEnumerable<SessionDTO>>> GetAllSessions(Guid userId)
        {
            try
            {
                var sessions = await _chatService.GetAllSessionsAsync(userId);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("session/{sessionId}/close")]
        public async Task<ActionResult> CloseSession(Guid sessionId)
        {
            try
            {
                var success = await _chatService.CloseSessionAsync(sessionId);
                if (!success)
                {
                    return NotFound(new { error = $"Session with ID {sessionId} not found" });
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        [HttpDelete("session/{sessionId}")]
        public async Task<ActionResult> DeleteSession(Guid sessionId)
        {
            try
            {
                var success = await _chatService.DeleteSessionAsync(sessionId);
                if (!success)
                {
                    return NotFound(new { error = $"Session with ID {sessionId} not found" });
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpDelete("sessions")]
        public async Task<ActionResult> DeleteAllSessions(Guid userId)
        {
            try
            {
                var success = await _chatService.DeleteAllSessionsAsync( userId);
                if (!success)
                {
                    return NotFound(new { error = "No sessions found" });
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpPost("session/{sessionId}/messages")]
        public async Task<ActionResult<ChatResponseDTO>> SendMessage(Guid sessionId, [FromBody] ChatRequestDTO request)
        {
            try
            {

                var response = await _chatService.SendMessageAsync(sessionId, request.Prompt);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
