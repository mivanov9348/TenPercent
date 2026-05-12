namespace TenPercent.Data.Enums
{
    public enum MessageType
    {
        Info = 1,           // Обикновено съобщение (напр. "Сезонът приключи")
        News = 2,           // Новини (напр. "Меси спечели Златната топка")
        Finance = 3,        // Финансов отчет (напр. "Платихте $10,000 данъци")
        TransferOffer = 4,  // Оферта от друг агент за твой играч (Изисква Action: Accept/Reject)
        ContractOffer = 5,  // Оферта от клуб за твой играч (Изисква Action)
        ScoutReport = 6     // Готов скаутски репорт (Може би с линк към играча)
    }
}