using SWE_A1Grandner_MTCG.MyEnum;

namespace SWE_A1Grandner_MTCG.Database
{
    public class TradeData
    {
        public Guid Id { get; set; }
        public Guid CardToTrade { get; set; }
        public TradeType Type { get; set; }
        public double MinimumDamage { get; set; }
        public string? Owner { get; set; }

        public TradeData(Guid id, Guid cardToTrade, TradeType type, double minimumDamage, string? owner)
        {
            Id = id;
            CardToTrade = cardToTrade;
            Type = type;
            MinimumDamage = minimumDamage;
            Owner = owner;
        }
    }
}
