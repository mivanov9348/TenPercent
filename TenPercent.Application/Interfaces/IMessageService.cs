namespace TenPercent.Application.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TenPercent.Data.Enums;
    using TenPercent.Data.Models;

    public interface IMessageService
    {
        Task<List<Message>> GetAgencyInboxAsync(int agencyId);
        Task<int> GetUnreadCountAsync(int agencyId);
        Task<bool> MarkAsReadAsync(int messageId, int agencyId);
        Task<bool> DeleteMessageAsync(int messageId, int agencyId);
        Task<Message> SendMessageAsync(
            int? receiverAgencyId,
            EntityType senderType,
            int senderId,
            string senderName,
            string subject,
            string content,
            MessageType type,
            int? relatedEntityId = null);
        Task<Message> SendTemplatedMessageAsync(
            int? receiverAgencyId,
            EntityType senderType,
            int senderId,
            string senderName,
            MessageType type,
            Dictionary<string, string> placeholders,
            int? relatedEntityId = null);
    }
}