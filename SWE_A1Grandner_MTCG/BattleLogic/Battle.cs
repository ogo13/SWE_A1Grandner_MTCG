using System.Reflection.Metadata.Ecma335;
using SWE_A1Grandner_MTCG.Database;
using SWE_A1Grandner_MTCG.MyEnum;
using System.Text;

namespace SWE_A1Grandner_MTCG.BattleLogic;

public class Battle
{
    public UserData Player1 { get; set; }
    public UserData Player2 { get; set; }

    public Deck Player1Deck { get; set; }
    public Deck Player2Deck { get; set; }

    public Battle(UserData player1, UserData player2)
    {
        this.Player1 = player1;
        this.Player2 = player2;
        var dataHandler = new DataHandler();
        Player1Deck = new Deck(dataHandler.GetDeck(player1));
        Player2Deck = new Deck(dataHandler.GetDeck(player2));
    }

    public BattleLog Fight()
    {
        var fightOn = true;
        var roundCounter = 0;

        while (fightOn)
        {
            Round();
            roundCounter++;
            if(Player1Deck.Cards.Count <= 0)
                fightOn = false;
            if (Player2Deck.Cards.Count <= 0)
                fightOn = false;
            if (roundCounter >= 100)
                fightOn = false;
        }

        if (roundCounter >= 100)
        {
            return new BattleLog(BattleOutcome.Draw, "Draw");
        }

        return Player1Deck.Cards.Count <= 0 ? new BattleLog(BattleOutcome.Player2Win, "Lose") : new BattleLog(BattleOutcome.Player1Win, "Win");
    }
    private static bool MonsterVsMonsterFight(Card card1, Card card2)
    {
        return card1.Type != CardType.Spell && card2.Type != CardType.Spell;
    }

    private static Card RandomCardFromDeck(Deck deck)
    {
        var random = new Random();
        var rand = random.Next(deck.Cards.Count);
        Console.WriteLine(rand);
        return deck.Cards[rand];
    }

    private void Round()
    {
        var card1 = RandomCardFromDeck(Player1Deck);
        var card2 = RandomCardFromDeck(Player2Deck);

        var winner = CalculateWinner(card1, card2);
        if (winner == null) return;
        if (card1 == winner)
        {
            Player1Deck.Cards.Add(card2);
            Player2Deck.Cards.Remove(card2);
            Console.WriteLine("Player1 wins");
        }
        else
        {
            Player2Deck.Cards.Add(card1);
            Player1Deck.Cards.Remove(card1);
            Console.WriteLine("Player2 wins");
        }
    }

    private static Card? CalculateWinner(Card card1, Card card2)
    {
        if (card1.SuperWeak == card2.SuperStrong)
        {
            return card2;
        }

        if (card2.SuperWeak == card1.SuperStrong)
        {
            return card1;
        }

        if (MonsterVsMonsterFight(card1, card2))
        {
            if(Math.Abs(card1.Damage - card2.Damage) < 0.0001)
                return null;
            return card1.Damage > card2.Damage ? card1 : card2;
        }

        double multiplier1 = 1;
        double multiplier2 = 1;

        if(card1.Weak == card2.Strong)
        {
            multiplier1 = 0.5;
            multiplier2 = 2;
        }

        if (card2.Weak == card1.Strong)
        {
            multiplier2 = 0.5;
            multiplier1 = 2;
        }

        if (Math.Abs(card1.Damage * multiplier1 - card2.Damage * multiplier2) < 0.0001)
            return null;
        return card1.Damage * multiplier1 > card2.Damage * multiplier2 ? card1 : card2;
    }
}