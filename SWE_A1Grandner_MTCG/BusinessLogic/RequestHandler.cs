using SWE_A1Grandner_MTCG.BattleLogic;
using System.Net.Sockets;
using SWE_A1Grandner_MTCG.Enum;
using Newtonsoft.Json;
using Npgsql;
using SWE_A1Grandner_MTCG.Databank;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.IO;
using System.Reflection.Metadata;

namespace SWE_A1Grandner_MTCG.BusinessLogic;

internal class RequestHandler
{
    private User? _user = null;
    private readonly Dictionary<string, string> _dataDictionary;
    private readonly TcpClient _client;

    public RequestHandler(Dictionary<string, string> dataDictionary, TcpClient client)
    {
        _dataDictionary = dataDictionary;
        _client = client;
        
    }

    public async Task Start()
    {
        await this.HandleRequest();
    }
    public async Task<HttpResponse> HandleRequest()
    {
        if (_dataDictionary["Method"] == "GET")
        {
            return await HandleGetRequest();
        }
        else if (_dataDictionary["Method"] == "POST")
        {
            return await HandlePostRequest();
        }
        else if (_dataDictionary["Method"] == "PUT")
        {
            return await HandlePutRequest();
        }

        return new HttpResponse(HttpStatusCode.BadRequest);
    }

    private async Task<HttpResponse> HandleGetRequest()
    {

        //check Authorization exception
        try
        {
            //_user = CheckAuthorization(_dataDictionary["Authorization"]);
        }
        catch
        {

        }

        switch (_dataDictionary["Path"])
        {
            case "/cards":
            {

                break;
            }
            case "/deck":
            {

                break;
            }
            case "/users": //noch ned ganz fertig
            {


                //edit user data
                break;
            }
            case "/stats":
            {


                //check stats


                break;
            }
            case "/score":
            {


                //check score   


                break;
            }
            case "/tradings":
            {


                //check trades   


                break;
            }

        }

        return new HttpResponse(HttpStatusCode.OK);
    }

    private async Task<HttpResponse> HandlePostRequest()
    {

        if (_dataDictionary["Path"] == "/users")
        {
            var userData = JsonConvert.DeserializeObject<UserData>(_dataDictionary["Data"]);
            //create user on database
            Task<int> registerTask;
            try
            {
                //registerTask = Register(userData);
                //await registerTask;+

                return new HttpResponse(HttpStatusCode.OK, "sup");
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
            }

        }
        else if (_dataDictionary["Path"] == "/sessions")
        {
            UserData? userData = JsonConvert.DeserializeObject<UserData>(_dataDictionary["Data"]);
            Task<string> loginTask;
            //login user from database
            try
            {
                loginTask = Login(userData);
                var stream = _client.GetStream();
                //await stream.WriteAsync(Encoding.UTF8.GetBytes(res), 0, Encoding.UTF8.GetBytes(res).Length);
                return new HttpResponse(HttpStatusCode.OK, "sup");

            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.ParamName + " " + e.Message);
                //var res = await HttpResponse("wrong Credentials", HttpStatusCode.BadRequest);
                var stream = _client.GetStream();
                //await stream.WriteAsync(Encoding.UTF8.GetBytes(res), 0, Encoding.UTF8.GetBytes(res).Length);
                return new HttpResponse(HttpStatusCode.BadRequest);
            }
            catch (ValidationException e)
            {
                Console.WriteLine(e.Message);
                //var res = await HttpResponse("wrong Credentials", HttpStatusCode.BadRequest);
                //var stream = client.GetStream();
               // await stream.WriteAsync(Encoding.UTF8.GetBytes(res), 0, Encoding.UTF8.GetBytes(res).Length);
                return new HttpResponse(HttpStatusCode.BadRequest);
            }
            finally
            {
            }
        }

        //check Authorization
        try
        {
            //_user = CheckAuthorization(_dataDictionary["Authorization"]);
        }
        catch
        {
            throw;
        }
        
        switch (_dataDictionary["Path"])
        {
            case "/packages":
                {
                    if (!_user.IsAdmin) //vlt if(!user.isAdmin())
                    {
                        //not admin
                        return new HttpResponse(HttpStatusCode.Unauthorized);
                    }
                    List<CardData>? cards = JsonConvert.DeserializeObject<List<CardData>>(_dataDictionary["Data"]);
                    break;
                }
            case "/tradings":
                {

                    TradeData? trade = JsonConvert.DeserializeObject<TradeData>(_dataDictionary["Data"]);

                    break;
                }
        }

        return new HttpResponse(HttpStatusCode.OK);
    }

    private async Task<HttpResponse> HandlePutRequest()
    {
        //check Authorization
        try
        {
            //_user = CheckAuthorization(_dataDictionary["Authorization"]);
        }
        catch
        {
        }



        switch (_dataDictionary["Path"])
        {
            case "/users": //fehlt noch etwas
            {
                //configure user

                break;
            }
            case "/deck":
            {
                //configure deck

                break;
            }
        }
        return new HttpResponse(HttpStatusCode.OK);
    }

    

    private async Task<string> Login(UserData? userData)
    {
        if (userData == null)
        {
            throw new ArgumentNullException(nameof(userData), "Userdata was null.");
        }
        //get users token from DB
        DataHandler dataHandler = new DataHandler();
        var dbData = await dataHandler.GetUserBy("username", userData.Username);
        if (dbData == null)
        {
            throw new ArgumentNullException(nameof(dbData), "Userdata was null.");
        }

        if (string.CompareOrdinal(userData.Password, dbData.Rows[0]["password"].ToString()) != 0)
        {
            throw new ValidationException("Password was wrong.");
        }

        string authorizationToken = dbData.Rows[0]["token"].ToString()!;

        return authorizationToken;
    }
}



