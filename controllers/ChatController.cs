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
        public async Task<ActionResult<SessionDTO>> CreateSession([FromBody] CreateSessionRequest request)
        {
            try
            {
                var session = await _chatService.CreateSessionAsync(
                    request.UserId
                    );

                return Ok(session);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("sessions/{id}")]
        public async Task<ActionResult<SessionDTO>> GetSession(Guid id)
        {
            try
            {
                var session = await _chatService.GetSessionAsync(id);
                if (session == null)
                {
                    return NotFound(new { error = $"Session with ID {id} not found" });
                }

                return Ok(session);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("sessions")]
        public async Task<ActionResult<IEnumerable<SessionDTO>>> GetAllSessions()
        {
            try
            {
                var sessions = await _chatService.GetAllSessionsAsync();
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("sessions/{id}/close")]
        public async Task<ActionResult> CloseSession(Guid id)
        {
            try
            {
                var success = await _chatService.CloseSessionAsync(id);
                if (!success)
                {
                    return NotFound(new { error = $"Session with ID {id} not found" });
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        //TODO write delete session and delete all sessions


        [HttpPost("sessions/{id}/messages")]
        public async Task<ActionResult<ChatResponseDTO>> SendMessage(Guid id, [FromBody] ChatRequestDTO request)
        {
            try
            {
                //* Override the session ID in the request with the one from the URL
                request.SessionId = id;

                var response = await _chatService.SendMessageAsync(id, request.Prompt);
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

    public class CreateSessionRequest
    {
        public Guid UserId { get; set; }
    }
}
