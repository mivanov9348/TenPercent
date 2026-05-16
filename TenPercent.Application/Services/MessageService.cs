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

        // НОВО: Добавихме `string? templateCode = null` като параметър
        public async Task<Message> SendTemplatedMessageAsync(
            int? receiverAgencyId,
            EntityType senderType,
            int senderId,
            string senderName,
            MessageType type,
            Dictionary<string, string> placeholders,
            int? relatedEntityId = null,
            string? templateCode = null) // Този параметър ще ни позволи да търсим точно определено събитие
        {
            // 1. Намираме шаблоните, базирано на Type и опционалния TemplateCode
            var query = _context.MessageTemplates.Where(t => t.Type == type);

            if (!string.IsNullOrEmpty(templateCode))
            {
                // Търсим специфичното събитие (напр. "WELCOME", "INJURY_MINOR")
                query = query.Where(t => t.TemplateCode == templateCode);
            }
            else
            {
                // Ако не е подаден код, теглим само от "общите" съобщения, за да не пратим случайно контузия като обща новина
                query = query.Where(t => string.IsNullOrEmpty(t.TemplateCode));
            }

            var templates = await query.ToListAsync();

            if (!templates.Any())
            {
                // Fallback: Ако нямаме шаблони в базата, пращаме генерично съобщение
                string fallbackSubject = string.IsNullOrEmpty(templateCode) ? "System Notification" : $"Alert: {templateCode}";
                return await SendMessageAsync(
                    receiverAgencyId, senderType, senderId, senderName,
                    fallbackSubject, "Message template missing. Ensure the database seeder has run.",
                    type, relatedEntityId);
            }

            // 2. Избираме 1 на случаен принцип измежду намерените (например от 5-те различни "WELCOME" съобщения)
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