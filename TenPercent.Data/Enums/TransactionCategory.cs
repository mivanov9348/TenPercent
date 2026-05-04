namespace TenPercent.Data.Enums
{
    public enum TransactionCategory
    {
        InitialAllocation = 1, // Първоначално наливане на пари от Банката към Клубовете
        Wage = 2,              // Заплати
        Commission = 3,        // Твоят процент от заплата или трансфер
        SigningBonus = 4,      // Бонус "на ръка"
        TransferFee = 5,       // Трансферна сума между два клуба
        PrizeMoney = 6,        // Награден фонд в края на сезона
        Upkeep = 7,            // Такса поддръжка към банката (за изгаряне на пари)
        Tax = 8,               // Данъци
        StartupGrant = 9,
        Other = 99
    }
}