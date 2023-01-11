using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text.RegularExpressions;
using SWE_A1Grandner_MTCG.BattleLogic;
using System.Xml.Linq;
using SWE_A1Grandner_MTCG.Database;
using SWE_A1Grandner_MTCG.MyEnum;

namespace SWE_A1Grandner_MTCG.BattleLogic;

public class Card
{
    
    public CardType Type { get; }
    public CardElement Element { get; }
    public double Damage { get; }
    public CardElement Strong { get; }
    public CardElement Weak { get; }
    public SpecialRule? SuperStrong { get; }
    public SpecialRule? SuperWeak { get; }


    public Card(CardData card)
    {
        Damage = card.Damage;

        var nameSplit = Regex.Replace(card.Name, "[a-z][A-Z]", m => $"{m.Value[0]} {m.Value[1]}").Split(" ");
        if (nameSplit.Length > 1)
        {
            Element = nameSplit[0].ToLower() switch
            {
                "regular" => CardElement.Normal,
                "fire" => CardElement.Fire,
                "water" => CardElement.Water,
                _ => throw new ArgumentOutOfRangeException()
            };
            Enum.TryParse(nameSplit[1], out CardType type);
            Type = type;
        }
        else
        {
            Element = CardElement.Normal;
            Enum.TryParse(nameSplit[0], out CardType type);
            Type = type;
        }

        switch (Element)
        {
            case CardElement.Fire:
                Strong = CardElement.Normal;
                Weak = CardElement.Water;
                break;
            case CardElement.Water:
                Strong = CardElement.Fire;
                Weak = CardElement.Normal;
                break;
            case CardElement.Normal:
                Strong = CardElement.Water;
                Weak = CardElement.Fire;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        SuperStrong = Type switch
        {
            CardType.Elf when Element == CardElement.Fire => SpecialRule.DragonFireElf,
            CardType.Spell when Element == CardElement.Water => SpecialRule.KnightWater,
            _ => Type switch
            {
                CardType.Dragon => SpecialRule.GoblinDragon,
                CardType.Wizard => SpecialRule.OrkWizard,
                CardType.Kraken => SpecialRule.KrakenSpell,
                _ => SpecialRule.None
            }
        };

        SuperWeak = Type switch
        {
            CardType.Goblin => SpecialRule.GoblinDragon,
            CardType.Ork => SpecialRule.OrkWizard,
            CardType.Knight => SpecialRule.KnightWater,
            CardType.Spell => SpecialRule.KrakenSpell,
            CardType.Dragon => SpecialRule.DragonFireElf,
            _ => SpecialRule.None
        };
    }

    

}