namespace SWE_A1Grandner_MTCG.Database;

public class CardData
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public double Damage { get; set; }
    public string? Owner { get; set; }
    public bool? Deck { get; set; }

    public CardData(Guid id, string name, double damage, string? owner, bool? deck)
    {
        Id = id;
        Name = name;
        Damage = damage;
        Owner = owner;
        Deck = deck;
    }
}

