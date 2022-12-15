using System.Net;

namespace SWE_A1Grandner_MTCG.BattleLogic;

public class Stack
{
    public List<Card> CardStack = new List<Card>();

    public Stack()
    {
        CardStack.Add(new Card("Wizard", "monster", "fire", 50));
        CardStack.Add(new Card("Goblin", "monster", "fire", 30));
        CardStack.Add(new Card("Dragon", "monster", "fire", 70));
        CardStack.Add(new Card("Blast", "Spell", "fire", 90));
    }
}