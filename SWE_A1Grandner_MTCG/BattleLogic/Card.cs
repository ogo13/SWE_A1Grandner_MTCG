using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text.RegularExpressions;
using SWE_A1Grandner_MTCG.BattleLogic;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore.Query;
using SWE_A1Grandner_MTCG.BattleLogic.Ruleset;
using SWE_A1Grandner_MTCG.Database;
using SWE_A1Grandner_MTCG.MyEnum;

namespace SWE_A1Grandner_MTCG.BattleLogic;

public class Card
{
    
    public CardType Type { get; }
    public CardElement Element { get; }
    public double Damage { get; }
    public StandardRules Strong { get; }
    public StandardRules Weak { get; }
    public SpecialRules? SuperStrong { get; }
    public SpecialRules? SuperWeak { get; }


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
                Strong = StandardRules.NormalFire;
                Weak = StandardRules.FireWater;
                break;
            case CardElement.Water:
                Strong = StandardRules.FireWater;
                Weak = StandardRules.WaterNormal;
                break;
            case CardElement.Normal:
                Strong = StandardRules.WaterNormal;
                Weak = StandardRules.NormalFire;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        SuperStrong = Type switch
        {
            CardType.Elf when Element == CardElement.Fire => SpecialRules.DragonFireElf,
            CardType.Spell when Element == CardElement.Water => SpecialRules.KnightWater,
            _ => Type switch
            {
                CardType.Dragon => SpecialRules.GoblinDragon,
                CardType.Wizard => SpecialRules.OrkWizard,
                CardType.Kraken => SpecialRules.KrakenSpell,
                _ => SpecialRules.None
            }
        };

        SuperWeak = Type switch
        {
            CardType.Goblin => SpecialRules.GoblinDragon,
            CardType.Ork => SpecialRules.OrkWizard,
            CardType.Knight => SpecialRules.KnightWater,
            CardType.Spell => SpecialRules.KrakenSpell,
            CardType.Dragon => SpecialRules.DragonFireElf,
            _ => SpecialRules.None
        };
    }

    

}