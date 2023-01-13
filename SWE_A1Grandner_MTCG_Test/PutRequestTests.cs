using SWE_A1Grandner_MTCG.BusinessLogic;
using SWE_A1Grandner_MTCG.MyEnum;
using SWE_A1Grandner_MTCG.Database;

namespace SWE_A1Grandner_MTCG_Test;

public class PutRequestTests
{

    [Test]
    public async Task ConfigureDeckWithZeroCards()
    {
        // Arrange
        var actionHandler = new PutActionHandler(new Dictionary<string, string>
        {
            { "Data", ""}
        }, null, new DataHandler());

        // Act
        var result = await actionHandler.ConfigureDeck();

        // Assert

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(result.Content, Is.EqualTo("No cards declared"));
    }

    [Test]
    public async Task ConfigureDeckWithAnythinButFourCards()
    {
        // Arrange
        var actionHandler = new PutActionHandler(new Dictionary<string, string>
        {
            { "Data",  "[\"aa9999a0-734c-49c6-8f4a-651864b14e62\", \"d6e9c720-9b5a-40c7-a6b2-bc34752e3463\"]"}
        }, null, new DataHandler());

        // Act
        var result = await actionHandler.ConfigureDeck();

        // Assert

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(result.Content, Is.EqualTo("Declare exactly four cards"));
    }

    [Test]
    public async Task ConfigureUserWithoutPath()
    {
        // Arrange
        var actionHandler = new PutActionHandler(new Dictionary<string, string>(), 
            null,
            new DataHandler());

        // Act
        var result = await actionHandler.ConfigureUser();

        // Assert

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(result.Content, Is.EqualTo("Something went wrong."));
    }

    [Test]
    public async Task ConfigureOtherUser()
    {
        // Arrange
        var actionHandler = new PutActionHandler(new Dictionary<string, string>
        {
            {"addendumPath", "someuser"}
        }, 
            new UserData("myuser", "admin", null, null, null, 20),
            new DataHandler());

        // Act
        var result = await actionHandler.ConfigureUser();

        // Assert

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(result.Content, Is.EqualTo("Unauthorized"));
    }
}

