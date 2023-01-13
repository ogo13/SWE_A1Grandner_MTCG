using SWE_A1Grandner_MTCG.Database;
using SWE_A1Grandner_MTCG.BattleLogic;
using SWE_A1Grandner_MTCG.MyEnum;

namespace SWE_A1Grandner_MTCG_Test;
public class BattleRuleTests
{
    private Battle? _battle;
    private UserData? _user1;
    private UserData? _user2;
    private Deck? _deck1;
    private Deck? _deck2;


    [SetUp]
    public void Setup()
    {
        _user1 = new UserData("user", "name", null , null, null, 20);
        _user2 = new UserData("user", "name", null, null, null, 20);
        _deck1 = new Deck(new List<CardData>());
        _deck2 = new Deck(new List<CardData>());
        _battle = new Battle(_user1, _user2, _deck1, _deck2);
    }

    [Test]
    public void GoblinVsDragonDragonWin()
    {
        // Arrange
        _battle!.Player1Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "Dragon", 1, null, true)));
        _battle.Player2Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "WaterGoblin", 10, null, true)));

        // Act
        var result = _battle.Fight();
        

        // Assert
        
        Assert.That(result.Outcome, Is.EqualTo(BattleOutcome.Player1Win));
    }

    [Test]
    public void OrkVsWizardWizardWin()
    {
        // Arrange
        _battle!.Player1Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "Wizard", 10, null, true)));
        _battle.Player2Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "Ork", 10, null, true)));

        // Act
        var result = _battle.Fight();


        // Assert

        Assert.That(result.Outcome, Is.EqualTo(BattleOutcome.Player1Win));
    }

    [Test]
    public void KnightVsWaterSpellSpellWin()
    {
        // Arrange
        _battle!.Player1Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "WaterSpell", 10, null, true)));
        _battle.Player2Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "Knight", 10, null, true)));

        // Act
        var result = _battle.Fight();


        // Assert

        Assert.That(result.Outcome, Is.EqualTo(BattleOutcome.Player1Win));
    }

    [Test]
    public void StrongKnightVsFireSpellKnightWin()
    {
        // Arrange
        _battle!.Player1Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "FireSpell", 10, null, true)));
        _battle.Player2Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "Knight", 50, null, true)));

        // Act
        var result = _battle.Fight();


        // Assert

        Assert.That(result.Outcome, Is.EqualTo(BattleOutcome.Player2Win));
    }

    [Test]
    public void SpellVsKrakenKrakenWin()
    {
        // Arrange
        _battle!.Player1Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "Kraken", 10, null, true)));
        _battle.Player2Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "RegularSpell", 10, null, true)));

        // Act
        var result = _battle.Fight();


        // Assert

        Assert.That(result.Outcome, Is.EqualTo(BattleOutcome.Player1Win));
    }

    [Test]
    public void DragonVsFireElfFireElfWin()
    {
        // Arrange
        _battle!.Player1Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "Dragon", 10, null, true)));
        _battle.Player2Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "FireElf", 10, null, true)));

        // Act
        var result = _battle.Fight();


        // Assert

        Assert.That(result.Outcome, Is.EqualTo(BattleOutcome.Player2Win));
    }

    [Test]
    public void FireGoblinVsWaterGoblinDraw()
    {
        // Arrange
        _battle!.Player1Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "FireGoblin", 10, null, true)));
        _battle.Player2Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "WaterGoblin", 10, null, true)));

        // Act
        var result = _battle.Fight();


        // Assert

        Assert.That(result.Outcome, Is.EqualTo(BattleOutcome.Draw));
    }

    [Test]
    public void FireGoblinVsWaterSpellWaterSpellWin()
    {
        // Arrange
        _battle!.Player1Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "FireGoblin", 10, null, true)));
        _battle.Player2Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "WaterSpell", 10, null, true)));

        // Act
        var result = _battle.Fight();


        // Assert

        Assert.That(result.Outcome, Is.EqualTo(BattleOutcome.Player2Win));
    }

    [Test]
    public void VeryStrongFireGoblinVsWaterSpellFireGoblinWin()
    {
        // Arrange
        _battle!.Player1Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "FireGoblin", 100, null, true)));
        _battle.Player2Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "WaterSpell", 10, null, true)));

        // Act
        var result = _battle.Fight();


        // Assert

        Assert.That(result.Outcome, Is.EqualTo(BattleOutcome.Player1Win));
    }

    [Test]
    public void FourTimesAsStrongFireGoblinVsWaterSpellDraw()
    {
        // Arrange
        _battle!.Player1Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "FireGoblin", 40, null, true)));
        _battle.Player2Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "WaterSpell", 10, null, true)));

        // Act
        var result = _battle.Fight();


        // Assert

        Assert.That(result.Outcome, Is.EqualTo(BattleOutcome.Draw));
    }

    [Test]
    public void TwoTimesAsStrongFireGoblinVsWaterSpellWaterSpellWin()
    {
        // Arrange
        _battle!.Player1Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "FireGoblin", 20, null, true)));
        _battle.Player2Deck.Cards.Add(new Card(new CardData(Guid.NewGuid(), "WaterSpell", 10, null, true)));

        // Act
        var result = _battle.Fight();


        // Assert

        Assert.That(result.Outcome, Is.EqualTo(BattleOutcome.Player2Win));
    }
}

