using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMBRS
{
    public class Developer
    {
        private string _name;
        private List<Game> _games;

        public Developer(string name)
        {
            _name = name;
            _games = new List<Game>();
        }

        public void SetDeveloperName(string name)
        {
            _name = name;
        }

        public string GetDeveloperName()
        {
            return _name;
        }

        public void AddGame(string name, uint appId)
        {
            var newgame = new Game(name, appId);
            _games.Add(newgame);
        }

        public Game GetGame(uint appId)
        {
            foreach (var game in _games)
            {
                if (game.GetAppId() == appId)
                {
                    return game;
                }
            }

            return null;
        }

        public List<Game> GetGames()
        {
            return _games;
        }

        public bool IsGameRegisteredToDeveloper(uint appId)
        {
            foreach (var game in _games)
            {
                if (game.GetAppId() == appId)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task UpdateGameFiles(uint appId)
        {
            if(IsGameRegisteredToDeveloper(appId))
            {
                var game = GetGame(appId);
                await game.UpdateGameFiles();
            }
        }

        public async Task GetGameFiles(uint appId)
        {
            if (IsGameRegisteredToDeveloper(appId))
            {
                var game = GetGame(appId);
                await game.GetGameFiles();
            }
        }
    }
}
