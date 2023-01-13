
using SWE_A1Grandner_MTCG.BusinessLogic;
using SWE_A1Grandner_MTCG.Database;
using SWE_A1Grandner_MTCG.MyEnum;

namespace SWE_A1Grandner_MTCG_Test;
internal class PostActionHandlerTests
{

    [SetUp]
    public void Setup()
    {
        
    }

    [Test]
    public async Task RegisterFailWithoutJson()
    {
        // Arrange
        var actionHandler = new PostActionHandler(new Dictionary<string, string>
            {
                { "Data", "" }
            }, null, null, new DataHandler());

        // Act
        var result = await actionHandler.Register();

        // Assert

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task LoginFailWithoutJson()
    {
        // Arrange
        var actionHandler = new PostActionHandler(new Dictionary<string, string>
            {
                { "Data", "" }
            }, null, null, new DataHandler());

        // Act
        var result = await actionHandler.Login();

        // Assert

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(HttpStatusCode.BadRequest));
    }


    [Test]
    public async Task CreatePackageAsNonAdminUnauthorized()
    {
        // Arrange
        var actionHandler = new PostActionHandler(new Dictionary<string, string>
            {
                { "Authorization", "Basic someuser-mtcgToken" }
            }, null, null, new DataHandler());

        // Act
        var result = await actionHandler.CreatePackage();

        // Assert

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(HttpStatusCode.Unauthorized));
    }


    [Test]
    public async Task CreatePackageWithZeroCards()
    {
        // Arrange
        var actionHandler = new PostActionHandler(new Dictionary<string, string>
        {
            { "Authorization", "Basic admin-mtcgToken" },
            { "Data", ""}
        }, null, null,
            new DataHandler());

        // Act
        var result = await actionHandler.CreatePackage();

        // Assert

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreatePackageContainingNotEnoughCards()
    {
        // Arrange
        var actionHandler = new PostActionHandler(new Dictionary<string, string>
        {
            { "Authorization", "Basic admin-mtcgToken" },
            { "Data",  "[{\"Id\":\"845f0dc7-37d0-426e-994e-43fc3ac83c08\", \"Name\":\"WaterGoblin\", \"Damage\": 10.0}, {\"Id\":\"99f8f8dc-e25e-4a95-aa2c-782823f36e2a\", \"Name\":\"Dragon\", \"Damage\": 50.0}]"}
        }, null, null, new DataHandler());

        // Act
        var result = await actionHandler.CreatePackage();

        // Assert

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(HttpStatusCode.BadRequest));
    }

}

