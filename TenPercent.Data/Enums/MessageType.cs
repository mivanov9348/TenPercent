namespace TenPercent.Data.Enums
{
    public enum MessageType
    {
        Info = 1,                 // Общи системни (Добре дошли, Край на сезон)
        News = 2,                 // Световни новини (Златна топка, Шампиони)
        Finance = 3,              // Финанси (Комисионни, Данъци, Такси)
        TransferOffer = 4,        // СТЪПКА 1: Оферта за ПРАВАТА на играча (Иска Action)
        TransferResponse = 5,     // НОВО: Отговор на оферта (Приета, Отхвърлена, Откупна клауза)
        ScoutReport = 6,          // Скаутски доклади
        ContractNegotiation = 7,  // СТЪПКА 2: Предлагане на лични условия на играч (Иска Action)
        Medical = 8               // НОВО: Контузии и възстановяване
    }
}