namespace SWE_A1Grandner_MTCG;

public class Deck
{
    public List<Card> CardDeck = new List<Card>();
    public Deck()
    {
        CardDeck.Add(new Card("Wizard", "monster", "fire", 50));
        CardDeck.Add(new Card("Goblin", "monster", "fire", 30));
        CardDeck.Add(new Card("Dragon", "monster", "fire", 70));
        CardDeck.Add(new Card("Blast", "Spell", "fire", 90));
    }
}