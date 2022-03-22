﻿using Newtonsoft.Json.Linq;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace EMBRS
{
    public class Verify
    {
        private static Spinner spinner;
        private static SteamWebInterfaceFactory webInterfaceFactory;

        public Verify(Spinner _spinner)
        {
            spinner = _spinner;
            webInterfaceFactory = new SteamWebInterfaceFactory(Settings.WebAPIKey);
        }

        public static async Task Initialize()
        {
            try
            {
                var playerIDInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>();

                ulong playerID;
                if (!ulong.TryParse(Settings.TestPlayer, out playerID))
                {
                    var playerIDResult = await playerIDInterface.ResolveVanityUrlAsync(Settings.TestPlayer);
                    playerID = playerIDResult.Data;
                }

                if (playerID != 0)
                {
                    var testPlayer = new Player();
                    testPlayer.MacAddr =
                     (
                         from nic in NetworkInterface.GetAllNetworkInterfaces()
                         where nic.OperationalStatus == OperationalStatus.Up
                         select nic.GetPhysicalAddress().ToString()
                     ).FirstOrDefault();
                    testPlayer.SteamId = playerID;
                    testPlayer.XRPAddress = Settings.TestAddress;
                    RewardEngine.Players.Add(testPlayer);
                }
            }
            catch (Exception ex)
            {
                ConsoleScreen.ClearConsoleLines();
                ConsoleScreen.WriteErrors("Error: " + ex.Message);
            }
        }

        public static void SimulateJSONMesage()
        {
            ConsoleScreen.ClearConsoleLines();
            ConsoleScreen.InitScreen(ref spinner, " Simulating JSON Message...");

            try
            {
                var player = RewardEngine.Players[0];
                var result = player.AddGame(Settings.SteamID);
                if(result != string.Empty)
                {
                    JObject incomingMessage = new JObject(new JProperty("EMBRS", result),
                                                          new JProperty("PC", player.MacAddr));
                    var encryptedString = incomingMessage["EMBRS"].ToString();
                    var decryptedString = player.Encryption.Decrypt(encryptedString, false);
                    var commaDelimitedArray = decryptedString.Split(',');
                    var steamId = commaDelimitedArray[0];
                    var xrpAddress = commaDelimitedArray[1];
                    var appId = commaDelimitedArray[2];

                    Player pl;
                    if(RewardEngine.RegisteredPlayer(steamId, xrpAddress, appId, incomingMessage["PC"].ToString(), out pl))
                    {
                        ConsoleScreen.Stop(ref spinner);
                        ConsoleScreen.ClearConsoleLines();
                        ConsoleScreen.WriteMessages("Decrypted: " + decryptedString, "Press any key to go back to the menu.");
                        Console.ReadLine();
                    }
                    else
                    {
                        ConsoleScreen.Stop(ref spinner);
                        ConsoleScreen.ClearConsoleLines();
                        ConsoleScreen.WriteMessages("Failed decryption", "Press any key to go back to the menu.");
                        Console.ReadLine();
                    }
                }
                else
                {
                    ConsoleScreen.Stop(ref spinner);
                    ConsoleScreen.ClearConsoleLines();
                    ConsoleScreen.WriteMessages("Failed encryption", "Press any key to go back to the menu.");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                spinner.Stop();
                ConsoleScreen.ClearConsoleLines();
                ConsoleScreen.WriteErrors("Error: " + ex.Message);
            }
        }

        public static async Task VerifySteamID(bool simulate = true)
        {
            ConsoleScreen.ClearConsoleLines();
            ConsoleScreen.InitScreen(ref spinner, " Verifying Steam ID...");

            try
            {
                if (RewardEngine.Players[0].SteamId != 0)
                {
                    var steamGamesInterface = webInterfaceFactory.CreateSteamWebInterface<PlayerService>();
                    var ownedGames = await steamGamesInterface.GetOwnedGamesAsync(RewardEngine.Players[0].SteamId);
                    foreach (var game in ownedGames.Data.OwnedGames)
                    {
                        if (game.AppId == RewardEngine.Developers[0].AppId)
                        {
                            var recentGames = await steamGamesInterface.GetRecentlyPlayedGamesAsync(RewardEngine.Players[0].SteamId);
                            foreach (var recentGame in recentGames.Data.RecentlyPlayedGames)
                            {
                                if (recentGame.AppId == RewardEngine.Developers[0].AppId)
                                {
                                    ConsoleScreen.Stop(ref spinner);
                                    ConsoleScreen.ClearConsoleLines();

                                    if (simulate)
                                    {
                                        ConsoleScreen.WriteMessages("Successfully verified " + RewardEngine.Players[0].SteamId + " properly owns " + RewardEngine.Developers[0].AppId + ". Reward payment simulated. Press any key to go back to the menu.");
                                        Console.ReadLine();
                                    }
                                    else
                                    {
                                        ConsoleScreen.WriteMessages("Successfully verified " + RewardEngine.Players[0].SteamId + " properly owns " + RewardEngine.Developers[0].AppId + ". Press any key to send rewards payout.");
                                        Console.ReadLine();
                                        await RewardEngine.SendRewardAsync();
                                    }

                                    return;
                                }
                            }
                        }
                    }

                    ConsoleScreen.Stop(ref spinner);
                    ConsoleScreen.ClearConsoleLines();
                    ConsoleScreen.WriteMessages("Failed verification of " + RewardEngine.Players[0].SteamId + " owning " + RewardEngine.Developers[0].AppId + ". Press any key to go back to the menu.");
                    Console.ReadLine();
                }
                else
                {
                    ConsoleScreen.Stop(ref spinner);
                    ConsoleScreen.ClearConsoleLines();
                    ConsoleScreen.WriteMessages("Failed resolving vanity URL of " + RewardEngine.Players[0].SteamId + ". Press any key to go back to the menu.");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                spinner.Stop();
                ConsoleScreen.ClearConsoleLines();
                ConsoleScreen.WriteErrors("Error: " + ex.Message);
            }
        }
    }
}
