namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Enums;
    using TenPercent.Data.Models;

    public class MessageService : IMessageService
    {
        private readonly AppDbContext _context;
        private readonly Random _rand = new Random();

        public MessageService(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. ЧЕТЕНЕ И УПРАВЛЕНИЕ
        // ==========================================

        public async Task<List<Message>> GetAgencyInboxAsync(int agencyId)
        {
            // Взимаме съобщенията конкретно за тази агенция + глобалните (където ReceiverAgencyId е null)
            return await _context.Messages
                .Where(m => m.ReceiverAgencyId == agencyId || m.ReceiverAgencyId == null)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int agencyId)
        {
            return await _context.Messages
                .CountAsync(m => (m.ReceiverAgencyId == agencyId || m.ReceiverAgencyId == null) && !m.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(int messageId, int agencyId)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && (m.ReceiverAgencyId == agencyId || m.ReceiverAgencyId == null));

            if (message == null || message.IsRead) return false;

            message.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMessageAsync(int messageId, int agencyId)
        {
            // Позволяваме да трият само ЛИЧНИТЕ си съобщения. Глобалните не могат да се трият от един играч.
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.ReceiverAgencyId == agencyId);

            if (message == null) return false;

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        // ==========================================
        // 2. ИЗПРАЩАНЕ НА СЪОБЩЕНИЯ
        // ==========================================

        public async Task<Message> SendMessageAsync(
            int? receiverAgencyId,
            EntityType senderType,
            int senderId,
            string senderName,
            string subject,
            string content,
            MessageType type,
            int? relatedEntityId = null)
        {
            var message = new Message
            {
                ReceiverAgencyId = receiverAgencyId,
                SenderType = senderType,
                SenderId = senderId,
                SenderName = senderName,
                Subject = subject,
                Content = content,
                Type = type,
                RelatedEntityId = relatedEntityId,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return message;
        }

        // ==========================================
        // 3. УМНО ИЗПРАЩАНЕ ЧРЕЗ ШАБЛОНИ
        // ==========================================

        public async Task<Message> SendTemplatedMessageAsync(
            int? receiverAgencyId,
            EntityType senderType,
            int senderId,
            string senderName,
            MessageType type,
            Dictionary<string, string> placeholders,
            int? relatedEntityId = null)
        {
            // 1. Намираме всички шаблони за този ТИП съобщение
            var templates = await _context.MessageTemplates
                .Where(t => t.Type == type)
                .ToListAsync();

            if (!templates.Any())
            {
                // Fallback: Ако нямаме шаблони в базата, пращаме генерично съобщение, за да не крашне играта
                return await SendMessageAsync(
                    receiverAgencyId, senderType, senderId, senderName,
                    "System Notification", "Message template missing for type: " + type.ToString(),
                    type, relatedEntityId);
            }

            // 2. Избираме 1 на случаен принцип (за разнообразие)
            var selectedTemplate = templates[_rand.Next(templates.Count)];

            // 3. Заместваме плейсхолдърите
            string subject = ReplacePlaceholders(selectedTemplate.SubjectTemplate, placeholders);
            string content = ReplacePlaceholders(selectedTemplate.ContentTemplate, placeholders);

            // 4. Пращаме генерираното съобщение
            return await SendMessageAsync(
                receiverAgencyId, senderType, senderId, senderName,
                subject, content, type, relatedEntityId);
        }

        // --- ПОМОЩНИК ЗА ЗАМЕСТВАНЕ ---
        private string ReplacePlaceholders(string templateText, Dictionary<string, string> placeholders)
        {
            if (string.IsNullOrWhiteSpace(templateText) || placeholders == null || !placeholders.Any())
                return templateText;

            string result = templateText;

            foreach (var kvp in placeholders)
            {
                // Търсим нещо от сорта на {PlayerName} и го заменяме с реалната стойност
                result = result.Replace($"{{{kvp.Key}}}", kvp.Value);
            }

            return result;
        }
    }
}