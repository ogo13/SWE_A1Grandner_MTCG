using Moq;
using Newtonsoft.Json;
using SWE_A1Grandner_MTCG.BusinessLogic;
using SWE_A1Grandner_MTCG.Database;
using SWE_A1Grandner_MTCG.MyEnum;

namespace SWE_A1Grandner_MTCG_Test;

internal class GetActionHandlerTests
{
    private Mock<IDataHandler> _handler;

    [SetUp]
    public void Setup()
    {
        _handler = new Mock<IDataHandler>();
        var json =
            "[{\"Id\":\"67f9048f-99b8-4ae4-b866-d8008d00c53d\", \"Name\":\"WaterGoblin\", \"Damage\": 10.0}, {\"Id\":\"aa9999a0-734c-49c6-8f4a-651864b14e62\", \"Name\":\"RegularSpell\", \"Damage\": 50.0}]";
        _handler.Setup(x => x.GetAllCards(It.IsAny<UserData>()))
            .Returns(JsonConvert.DeserializeObject<List<CardData>>(json)!);
    }

    [Test]
    public async Task GetUserBioOfNoUser()
    {
        // Arrange
        var actionHandler = new GetActionHandler(new Dictionary<string, string>
        {
            { "Authorization", "Basic admin-mtcgToken" }
        }, 
            new UserData("admin", "admin", null, null, null, 20), 
            new DataHandler());

        // Act
        var result = await actionHandler.GetUserBio();

        // Assert

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetUserBio()
    {
        // Arrange
        var actionHandler = new GetActionHandler(new Dictionary<string, string>
            {
                { "Authorization", "Basic admin-mtcgToken" },
                { "addendumPath", "admin" },
            },
            new UserData("admin", "admin", null, null, null, 20),
            _handler.Object);

        var expected = JsonConvert.SerializeObject(new UserData("admin", "admin", null, null, null, 20));

        // Act
        var result = await actionHandler.GetUserBio();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.Content, Is.EqualTo(expected));
    }

    [Test]
    public async Task GetAllCards()
    {
        // Arrange
        var actionHandler = new GetActionHandler(new Dictionary<string, string>
            {
                { "Authorization", "Basic admin-mtcgToken" }
            },
            new UserData("admin", "admin", null, null, null, 20),
            _handler.Object);

        var json = "[{\"Id\":\"67f9048f-99b8-4ae4-b866-d8008d00c53d\", \"Name\":\"WaterGoblin\", \"Damage\": 10.0}, {\"Id\":\"aa9999a0-734c-49c6-8f4a-651864b14e62\", \"Name\":\"RegularSpell\", \"Damage\": 50.0}]";
        var returnList = JsonConvert.DeserializeObject<List<CardData>>(json);
        var expected = JsonConvert.SerializeObject(returnList);

        // Act
        var result = await actionHandler.ShowAllCards();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.Content, Is.EqualTo(expected.Replace("},{", $"}},{Environment.NewLine}{{")));
    }

}

