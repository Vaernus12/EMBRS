using RippleDotNet;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EMBRS
{
    public class RewardEngine
    {
        public static List<Developer> Developers;
        public static List<Player> Players;

        private static Spinner spinner;

        public RewardEngine(Spinner _spinner)
        {
            spinner = _spinner;
        }

        public static void Initialize()
        {
            Developers = new List<Developer>();
            Players = new List<Player>();

            var testDeveloper = new Developer();
            testDeveloper.AppId = Settings.SteamID;
            Developers.Add(testDeveloper);
        }

        public static bool RegisteredDeveloper(uint appId, out Developer dev)
        {
            foreach(var developer in Developers)
            {
                if(developer.AppId == appId)
                {
                    dev = developer;
                    return true;
                }
            }

            dev = null;
            return false;
        }

        public static bool RegisteredPlayer(string steamId, string xrpAddress, string appId, string macAddr, out Player pl)
        {
            foreach (var player in Players)
            {
                if (player.SteamId == ulong.Parse(steamId) &&
                    player.XRPAddress == xrpAddress &&
                    player.RegisteredGame(uint.Parse(appId)) &&
                    player.MacAddr == macAddr)
                {
                    pl = player;
                    return true;
                }
            }

            pl = null;
            return false;
        }

        public static async Task SendRewardAsync()
        {
            ConsoleScreen.ClearConsoleLines();
            ConsoleScreen.InitScreen(ref spinner, " Processing Reward Payment...");

            try
            {
                IRippleClient client = new RippleClient(Settings.WebSocketUrl);
                client.Connect();
                uint sequence = await XRPL.GetLatestAccountSequence(client, Settings.RewardAddress);
                var f = await client.Fees();

                while (Convert.ToInt32(Math.Floor(f.Drops.OpenLedgerFee * Settings.FeeMultiplier)) > Settings.MaximumFee)
                {
                    ConsoleScreen.ClearConsoleLines(24);
                    ConsoleScreen.WriteMessages("Waiting...fees too high. Current Open Ledger Fee: " + f.Drops.OpenLedgerFee, "Fees configured based on fee multiplier: " + Convert.ToInt32(Math.Floor(f.Drops.OpenLedgerFee * Settings.FeeMultiplier)), "Maximum Fee Configured: " + Settings.MaximumFee);
                    Thread.Sleep(Settings.AccountLinesThrottle * 1000);
                    f = await client.Fees();
                }

                int feeInDrops = Convert.ToInt32(Math.Floor(f.Drops.OpenLedgerFee * Settings.FeeMultiplier));

                var response = await XRPL.SendXRPPaymentAsync(client, Players[0].XRPAddress, sequence, feeInDrops, Settings.TransferFee);

                //Transaction Node isn't Current. Wait for Network
                if (response.EngineResult == "noCurrent" || response.EngineResult == "noNetwork")
                {
                    int retry = 0;
                    while ((response.EngineResult == "noCurrent" || response.EngineResult == "noNetwork") && retry < 3)
                    {
                        //Throttle for node to catch up
                        Thread.Sleep(Settings.TxnThrottle * 3000);
                        response = await XRPL.SendXRPPaymentAsync(client, Players[0].XRPAddress, sequence, feeInDrops);
                        retry++;
                    }
                }
                else if (response.EngineResult == "tefPAST_SEQ")
                {
                    //Get new account sequence + try again
                    sequence = await XRPL.GetLatestAccountSequence(client, Settings.RewardAddress);
                }
                else if (response.EngineResult == "telCAN_NOT_QUEUE_FEE")
                {
                    sequence = await XRPL.GetLatestAccountSequence(client, Settings.RewardAddress);
                    //Throttle, check fees and try again
                    Thread.Sleep(Settings.TxnThrottle * 3000);
                }
                else if (response.EngineResult == "tesSUCCESS" || response.EngineResult == "terQUEUED")
                {
                    //Transaction Accepted by node successfully.
                    sequence++;
                }
                else if (response.EngineResult == "tecPATH_DRY" || response.EngineResult == "tecDST_TAG_NEEDED")
                {
                    //Trustline was removed or Destination Tag needed for address
                    sequence++;
                }
                else
                {
                    //Failed
                    sequence++;
                }

                client.Disconnect();
                spinner.Stop();
                ConsoleScreen.ClearConsoleLines();
                ConsoleScreen.WriteMessages("Reward payment finished!", "Press any key to go back to the menu...");
                Console.ReadLine();

            }
            catch (Exception ex)
            {
                spinner.Stop();
                ConsoleScreen.ClearConsoleLines();
                Console.SetCursorPosition(0, 27);
                Console.WriteLine("Error: " + ex.Message);
                Console.ReadLine();
            }
        }
    }
}
