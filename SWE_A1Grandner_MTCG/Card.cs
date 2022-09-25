using System.Linq.Expressions;

namespace SWE_A1Grandner_MTCG;

public class Card
{
    public Card(string name, string type, string element, int damage)
    {
        Name = name;
        Damage = damage;
        Type = type.ToLower() == "monster" ? CardType.Monster : CardType.Spell;
        Element = element.ToLower() switch
        {
            "normal" => Element.Normal,
            "fire" => Element.Fire,
            "water" => Element.Water,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public string Name { get; }
    public CardType Type { get; }
    public Element Element { get; }
    public int Damage { get; }

}