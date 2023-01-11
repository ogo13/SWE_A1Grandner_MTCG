using SWE_A1Grandner_MTCG.Database;

namespace SWE_A1Grandner_MTCG.BattleLogic;

public class Deck
{
    public List<Card> Cards { get; set; }

    public Deck(List<CardData> cards)
    {
        Cards = new List<Card>();
        foreach (var card in cards)
        {
            Cards.Add(new Card(card));
        }
    }
}