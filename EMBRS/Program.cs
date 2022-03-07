using System;
using System.Threading.Tasks;

namespace EMBRS
{
    class Program
    {
        private static RewardEngine rewardEngine;
        private static Spinner spinner;
        private static Verify verify;

        static async Task Main(string[] args)
        {
            bool showMenu = true;

            Settings.Initialize();
            spinner = new Spinner(0, 23);
            rewardEngine = new RewardEngine(spinner);
            RewardEngine.Initialize();
            verify = new Verify(spinner);
            await Verify.Initialize();

            while (showMenu)
            {
                showMenu = await MainMenuAsync();
            }
        }

        private static async Task<bool> MainMenuAsync()
        {
            Console.Clear();
            Console.WriteLine("1.Simulate Reward Payout | 4.Register Player");
            Console.WriteLine("2.Do Reward Payout       | 5.Exit");
            Console.WriteLine("3.Register Developer");
            Console.Write("\r\nSelect an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    Console.WriteLine("*** Are you sure you want to do this? This will simulate an EMBRS rewards payout *** Y or N");
                    switch (Console.ReadLine())
                    {
                        case "Y":
                        case "y":
                            await Verify.VerifySteamID(true);
                            return true;
                        case "N":
                        case "n":
                            return true;
                    }
                    return true;
                case "2":
                    Console.WriteLine("*** Are you sure you want to do this? This will send an EMBRS rewards payout *** Y or N");
                    switch (Console.ReadLine())
                    {
                        case "Y":
                        case "y":
                            await Verify.VerifySteamID(false);
                            return true;
                        case "N":
                        case "n":
                            return true;
                    }
                    return true;
                case "3":
                    Console.WriteLine("*** Please type in a valid Steam AppID ***");
                    var appIDString = Console.ReadLine();
                    uint appID;
                    if(uint.TryParse(appIDString, out appID))
                    {
                        var newDeveloper = new Developer();
                        newDeveloper.AppId = appID;
                        RewardEngine.Developers.Add(newDeveloper);
                    }
 
                    return true;
                case "4":
                    Console.WriteLine("*** Please type in a valid player's SteamID ***");
                    var steamIDString = Console.ReadLine();
                    ulong steamID;
                    if (ulong.TryParse(steamIDString, out steamID))
                    {
                        Console.WriteLine("*** Please type in a valid XRP address ***");
                        var xrpAddress = Console.ReadLine();

                        var newPlayer = new Player();
                        newPlayer.SteamId = steamID;
                        newPlayer.XRPAddress = xrpAddress;
                        RewardEngine.Players.Add(newPlayer);
                    }

                    return true;
                case "5":
                    return false;
                default:
                    return true;
            }
        }
    }
}
