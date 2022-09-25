// See https://aka.ms/new-console-template for more information

using SWE_A1Grandner_MTCG;

Console.WriteLine("Hello, World!");

Deck player1 = new Deck();
Deck player2 = new Deck();

Random rng = new Random();
int i = rng.Next(player1.CardDeck.Count);
if (player1.CardDeck[i].Damage < 100)
{
    Console.WriteLine($"{player1.CardDeck[i].Name} lost the battle");
}

