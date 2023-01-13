using SWE_A1Grandner_MTCG.BattleLogic;
using SWE_A1Grandner_MTCG.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE_A1Grandner_MTCG.BusinessLogic
{
    internal class PostActionHandler : ActionHandler
    {
        public PostActionHandler(Dictionary<string, string> httpRequestDictionary, UserData? user, Lobby? battleLobby)
        {
            _httpRequestDictionary = httpRequestDictionary;
            _user = user;
            _battleLobby = battleLobby;
        }

}
}
