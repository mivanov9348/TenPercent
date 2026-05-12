namespace TenPercent.Data.Models
{
    using TenPercent.Data.Enums;

    public class MessageTemplate
    {
        public int Id { get; set; }

        public MessageType Type { get; set; }

        public string SubjectTemplate { get; set; } = string.Empty;

        public string ContentTemplate { get; set; } = string.Empty;
    }
}