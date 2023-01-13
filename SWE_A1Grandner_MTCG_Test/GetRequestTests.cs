using SWE_A1Grandner_MTCG.BusinessLogic;
using SWE_A1Grandner_MTCG.Database;
using SWE_A1Grandner_MTCG.MyEnum;

namespace SWE_A1Grandner_MTCG_Test;

internal class GetRequestTests
{
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

}

