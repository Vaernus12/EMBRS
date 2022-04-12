using System.Collections.Generic;

namespace EMBRS
{
    public class Player
    {
        private ulong _steamId;
        private string _xrpAddress;
        private List<Game> _addedGames;
        private Encryption _encryption;

        public Player(ulong steamId, string xrpAddress)
        {
            _steamId = steamId;
            _xrpAddress = xrpAddress;
            _addedGames = new List<Game>();
            _encryption = new Encryption();
        }

        public void SetSteamId(ulong steamId)
        {
            _steamId = steamId;
        }

        public ulong GetSteamId()
        {
            return _steamId;
        }

        public void SetXRPAddress(string xrpAddress)
        {
            _xrpAddress = xrpAddress;
        }

        public string GetXRPAddress()
        {
            return _xrpAddress;
        }

        public Encryption GetEncryption()
        {
            return _encryption;
        }

        public string AddGame(uint AppId)
        {
            Game addedGame;
            if (RewardEngine.RegisteredGame(AppId, out addedGame))
            {
                _addedGames.Add(addedGame);
                return _encryption.Encrypt(_steamId.ToString() + "," + _xrpAddress + "," + AppId.ToString(), false);
            }

            return string.Empty;
        }

        public Game GetGame(uint appId)
        {
            foreach (var game in _addedGames)
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
            return _addedGames;
        }

        public bool IsGameRegisteredToPlayer(uint appId)
        {
            foreach (var game in _addedGames)
            {
                if (game.GetAppId() == appId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
