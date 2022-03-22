using System.Collections.Generic;

namespace EMBRS
{
    public class Player
    {
        public ulong SteamId;
        public string XRPAddress;
        public List<Developer> AddedGames;
        public Encryption Encryption;
        public string MacAddr;

        public Player()
        {
            SteamId = 0;
            XRPAddress = string.Empty;
            AddedGames = new List<Developer>();
            Encryption = new Encryption();
        }

        public string AddGame(uint AppId)
        {
            Developer addedGame;
            if (RewardEngine.RegisteredDeveloper(AppId, out addedGame))
            {
                AddedGames.Add(addedGame);
                return Encryption.Encrypt(SteamId.ToString() + "," + XRPAddress + "," + AppId.ToString(), false);
            }

            return string.Empty;
        }

        public bool RegisteredGame(uint appId)
        {
            foreach (var game in AddedGames)
            {
                if (game.AppId == appId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
