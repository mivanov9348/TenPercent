namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Application.DTOs;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;

    [Route("api/[controller]")]
    [ApiController]
    public class InboxController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly AppDbContext _context;

        public InboxController(IMessageService messageService, AppDbContext context)
        {
            _messageService = messageService;
            _context = context;
        }

        // GET: api/inbox/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetInbox(int userId)
        {
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent?.Agency == null) return BadRequest("Нямате агенция.");

            var messages = await _messageService.GetAgencyInboxAsync(agent.Agency.Id);

            var dtos = messages.Select(m => new MessageDto
            {
                Id = m.Id,
                SenderName = m.SenderName,
                Subject = m.Subject,
                Content = m.Content,
                SentAt = m.SentAt,
                IsRead = m.IsRead,
                Type = m.Type.ToString(), // "TransferOffer", "Finance", "Info" и т.н.
                RelatedEntityId = m.RelatedEntityId
            }).ToList();

            return Ok(dtos);
        }

        // GET: api/inbox/{userId}/unread-count
        [HttpGet("{userId}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent?.Agency == null) return Ok(0);

            int count = await _messageService.GetUnreadCountAsync(agent.Agency.Id);
            return Ok(count);
        }

        // PUT: api/inbox/{userId}/read/{messageId}
        [HttpPut("{userId}/read/{messageId}")]
        public async Task<IActionResult> MarkAsRead(int userId, int messageId)
        {
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent?.Agency == null) return BadRequest("Нямате агенция.");

            var success = await _messageService.MarkAsReadAsync(messageId, agent.Agency.Id);
            if (!success) return NotFound("Съобщението не е намерено или вече е прочетено.");

            return Ok();
        }

        // DELETE: api/inbox/{userId}/delete/{messageId}
        [HttpDelete("{userId}/delete/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int userId, int messageId)
        {
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent?.Agency == null) return BadRequest("Нямате агенция.");

            var success = await _messageService.DeleteMessageAsync(messageId, agent.Agency.Id);
            if (!success) return BadRequest("Не можете да изтриете това съобщение.");

            return Ok(new { message = "Съобщението е изтрито." });
        }
    }
}