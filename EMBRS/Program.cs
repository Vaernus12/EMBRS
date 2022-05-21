﻿using Discord;
using Discord.Net;
using Discord.WebSocket;
using EMBRS_Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XUMM.NET.SDK.EMBRS;

namespace EMBRS
{
    public class Program
    {
        private readonly Commands _commands;
        private static DiscordSocketClient _discordClient;
        private readonly XummWebSocket _webSocketClient;
        private readonly XummMiscAppStorageClient _appStorageClient;
        private readonly XummMiscClient _miscClient;
        private readonly XummPayloadClient _payloadClient;
        private readonly XummHttpClient _httpClient;

        private readonly bool _updateCommands = true;

        private static bool _running = false;
        private static bool _ready = false;

        private static bool _sundayMessage = false;
        private static bool _mondayMessage = false;
        private static bool _tuesdayTournamentMessage = false;
        private static bool _wednesdayTournamentMessage = false;
        private static bool _thursdayTournamentMessage = false;
        private static bool _fridayTournamentMessage = false;
        private static bool _saturdayMessage = false;

        private static double _timeBetweenMessagesInHours = 8;
        private static List<string> _randomMessages = new List<string>();
        private static DateTime _timeSinceLastMessage = DateTime.MinValue;

        static Task Main(string[] args)
        {
            return new Program().MainAsync();
        }

        private Program()
        {
            Settings.Initialize();
            _discordClient = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.All,
                LogLevel = LogSeverity.Info,
            });

            _httpClient = new XummHttpClient();
            _webSocketClient = new XummWebSocket();
            _appStorageClient = new XummMiscAppStorageClient(_httpClient);
            _miscClient = new XummMiscClient(_httpClient);
            _payloadClient = new XummPayloadClient(_httpClient, _webSocketClient);
            _commands = new Commands(_discordClient, _webSocketClient, _appStorageClient, _miscClient, _payloadClient, _httpClient);

            _discordClient.Log += Log;
            _running = true;

            _randomMessages.Add("You can play Emberlight for free! Download the game at: https://store.steampowered.com/app/1048880/Emberlight/");
            _randomMessages.Add("Emberlight tournaments run weekly! You can win EMBRS just for participating, with special prizes randomly drawn on Mondays! Sign-ups are between Tuesday and Thursday. The tournament itself is between Friday night to Monday morning. Use the /tournament command to sign-up!");
            _randomMessages.Add("Having trouble using the EMBRS commands? You can use the /help command to get a list of all supported commands I provide and the channels they can be used in!");
            _randomMessages.Add("Don't forget to /register with me the XRP address you'd like to use! I have some really cool features like the daily faucet (free EMBRS!), DEX swaps (between EMBRS, XRP, and GateHub USD), and sending/receiving EMBRS tips!");
            _randomMessages.Add("Want to learn more about how to play Emberlight? You can ask the team (and our awesome community) in the #emberlight channel. Also check out the playthroughs made by Mike at: https://www.youtube.com/channel/UC9CAwUdVLJYVGpxNO8L2ZAQ");
            _randomMessages.Add("Did you know that Mike has started streaming Emberlight a few times per week? Follow him at https://www.twitch.tv/mbhahn to get updates when he goes live!");
            _randomMessages.Add("Want to stay up to date with everything we're doing? Come follow us on Twitter at: https://twitter.com/quarteronion");
            _randomMessages.Add("Did you know we have EMBRS merch? Created and curated by XRPLMerch, you can find it at https://embrs.xrplmerch.com/product-category/embrs/ and we highly recommend you follow Codeward (https://twitter.com/Codeward1) and XRPLMerch (https://twitter.com/XRPLMerch) on Twitter!");
            _randomMessages.Add("EMBRS Forged is an XRP community effort, and we recently discussed the concept of governance in #embrs. All EMBRS holders will have a say in my progression via governance topics and voting!");
            _randomMessages.Add("We recently launched the EMBRS Forged platform website breaking down our plans as well as the general roadmap! You can check it out at: https://emberlight.quarteroniongames.com/platform/");
            _randomMessages.Add("What are EMBRS and why do I want them, oh wonderful bot? Glad you asked, hypothetical person. A breakdown of the EMBRS token can be found at: https://emberlight.quarteroniongames.com/embers/");
            _randomMessages.Add("The EMBRS pool, powered by the awesome StaykX platform, is going live soon! Have a bunch of EMBRS sitting around from all your tournament winnings, and want to earn from them? Please make sure you're all setup if you want to stayk your EMBRS by visiting: https://staykx.com/");
            _randomMessages.Add("We still don't know how the duel process works (anchor beats credit card????), but we have a really fun bot called Epic RPG in our #gameroom channel. Looking to pass time between the faucet and weekly tournament? Come join the devs and community in there!");
            _randomMessages.Add("Want an early access slot to Emberlight: Rekindled? Come join the tournament and we have providing a handful of slots each week along with a handful of other awesome prizes!");
            _randomMessages.Add("That !rank thing we all see in #bot-commands through MEE6? This currently unlocks roles within our server. Higher roles provide more access, with the coveted Elite role giving you an early access slot in Emberlight: Rekindled!");
            _randomMessages.Add("Posted an awesome tweet about me or everything else we're doing? Let us know here and you just might earn yourself some EMBRS in the process!");
            _randomMessages.Add("Calling all of our tournament participants (and future participants)! If you know of anyone that would be interested in joining in, please invite them to our Discord server! The more participants, the more awesome sponsors we'll be able to find to reward you! Let us know if you found someone and you just might earn yourself some EMBRS as well!");
            _randomMessages.Add("Looking for the EMBRS trustline? Here you go: https://xrpl.services/?issuer=rPbKMFvHbdEGBog98UjZXRdUx37MFKMfxB&currency=454D425253000000000000000000000000000000&limit=100000000");
            _randomMessages.Add("Where can I get EMBRS? We are currently an XRPL DEX exclusive at: https://sologenic.org/trade?network=mainnet&market=454D425253000000000000000000000000000000%2BrPbKMFvHbdEGBog98UjZXRdUx37MFKMfxB%2FXRP");
            _randomMessages.Add("Did you know that EMBRS is blackholed, KYCed through XUMM, and we have completed a self assessment through the XRP Ledger Foundation? You can find the self assessment at: https://foundation.xrpl.org/token-self-assessments/454d425253000000000000000000000000000000-rpbkmfvhbdegbog98ujzxrdux37mfkmfxb-r1/");
            _randomMessages.Add("What is Play-To-Earn and why are we bothering to tackle it within EMBRS Forged? Play-To-Earn allows you to earn while playing your favorite games, and we are taking it a step further by establishing 'Gaming-As-An-Income' within our platform! You can read more in our whitepaper at: https://emberlight.quarteroniongames.com/wp-content/uploads/sites/5/2022/02/EmbersWhitepaperV1-1.pdf");
            _randomMessages.Add("Am I an NFT project? No. Our first two phases of EMBRS Forged are strictly Play-To-Earn (via EMBRS and partner tokens) and do not involve NFTs at all! However, we do have plans to utilize NFTs in awesome ways as we expand the platform.");
            _randomMessages.Add("Registered with me? Don't forget to run the daily /faucet command to earn some EMBRS!");
        }

        private async Task MainAsync()
        {
            await Database.Initialize();
            _discordClient.Ready += HandleClientReadyAsync;
            _discordClient.SlashCommandExecuted += _commands.HandleSlashCommandAsync;
            await _discordClient.LoginAsync(TokenType.Bot, Settings.BotToken);
            await _discordClient.StartAsync();

            var loopTask = Task.Run(async () =>
            {
                while (_running)
                {
                    if(_ready) await LoopTasks();
                    await Task.Delay(1000);
                }
            });

            await Task.Delay(Timeout.Infinite);
        }

        private async Task LoopTasks()
        {
            if (Database.IsDirty)
            {
                await Database.Write();
                Database.IsDirty = false;
            }

            try
            {
                switch (DateTime.UtcNow.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        {
                            break;
                        }
                    case DayOfWeek.Monday:
                        {
                            break;
                        }
                    case DayOfWeek.Tuesday:
                        {
                            if (!_tuesdayTournamentMessage)
                            {
                                var guild = _discordClient.GetGuild(ulong.Parse(Settings.GuildID));
                                var emberlightChannel = guild.TextChannels.FirstOrDefault(x => x.Name == "emberlight");
                                await emberlightChannel.SendMessageAsync("**Emberlight tournament sign-ups have started! Use the /tournament command to sign-up. This week's tournament begins Friday night. If you have not done so already, you can register with me using the /register command!**");
                                _tuesdayTournamentMessage = true;
                            }
                            break;
                        }
                    case DayOfWeek.Wednesday:
                        {
                            if (!_wednesdayTournamentMessage)
                            {
                                _tuesdayTournamentMessage = false;
                                var guild = _discordClient.GetGuild(ulong.Parse(Settings.GuildID));
                                var emberlightChannel = guild.TextChannels.FirstOrDefault(x => x.Name == "emberlight");
                                await emberlightChannel.SendMessageAsync("**Emberlight tournament sign-ups are still going on! Who else will be joining in this week?!**");
                                _wednesdayTournamentMessage = true;
                            }
                            break;
                        }
                    case DayOfWeek.Thursday:
                        {
                            if (!_thursdayTournamentMessage)
                            {
                                _wednesdayTournamentMessage = false;
                                var guild = _discordClient.GetGuild(ulong.Parse(Settings.GuildID));
                                var emberlightChannel = guild.TextChannels.FirstOrDefault(x => x.Name == "emberlight");
                                await emberlightChannel.SendMessageAsync("**Last day for Emberlight tournament sign-ups this week! Don't miss out to win EMBRS, early access to Emberlight: Rekindled, and special prizes provided by our sponsors!**");
                                _thursdayTournamentMessage = true;
                            }
                            break;
                        }
                    case DayOfWeek.Friday:
                        {
                            if (!_fridayTournamentMessage)
                            {
                                _thursdayTournamentMessage = false;
                                var guild = _discordClient.GetGuild(ulong.Parse(Settings.GuildID));
                                var emberlightChannel = guild.TextChannels.FirstOrDefault(x => x.Name == "emberlight");
                                await emberlightChannel.SendMessageAsync("**Emberlight tournament sign-ups have ended. If you missed out, no worries! You can earn EMBRS via the /faucet command while you wait!**");
                                _fridayTournamentMessage = true;
                            }
                            break;
                        }
                    case DayOfWeek.Saturday:
                        {
                            _fridayTournamentMessage = false;
                            break;
                        }
                }

                if ((DateTime.UtcNow - _timeSinceLastMessage).TotalHours >= _timeBetweenMessagesInHours)
                {
                    var rng = new System.Random();
                    var guild = _discordClient.GetGuild(ulong.Parse(Settings.GuildID));
                    var generalChannel = guild.TextChannels.FirstOrDefault(x => x.Name == "general");
                    var randomIndex = rng.Next(0, _randomMessages.Count);
                    await generalChannel.SendMessageAsync(_randomMessages[randomIndex]);
                    _timeSinceLastMessage = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                await Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        public static async Task Shutdown()
        {
            await Database.Write();
            Database.IsDirty = false;
            _running = false;
            _ready = false;
            await _discordClient.StopAsync();
        }

        public static Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }

            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
            Console.ResetColor();

            return Task.CompletedTask;
        }        

        private async Task HandleClientReadyAsync()
        {
            try
            {
                var guild = _discordClient.GetGuild(ulong.Parse(Settings.GuildID));
                var commands = await guild.GetApplicationCommandsAsync();

                if(!_updateCommands && commands.Any(r => r.Name == "end")) await Log(new LogMessage(LogSeverity.Info, "Command Loaded", "End"));
                else
                {
                    var endCommand = new SlashCommandBuilder()
                        .WithName("end")
                        .WithDescription("End currently running Emberlight tournament.")
                        .AddOption("week", ApplicationCommandOptionType.String, "Current week", isRequired: true);
                    await guild.CreateApplicationCommandAsync(endCommand.Build());
                }

                if (!_updateCommands && commands.Any(r => r.Name == "faucet")) await Log(new LogMessage(LogSeverity.Info, "Command Loaded", "Faucet"));
                else
                {
                    var faucetCommand = new SlashCommandBuilder()
                        .WithName("faucet")
                        .WithDescription("Earn EMBRS from daily faucet.");
                    await guild.CreateApplicationCommandAsync(faucetCommand.Build());
                }

                if (!_updateCommands && commands.Any(r => r.Name == "help")) await Log(new LogMessage(LogSeverity.Info, "Command Loaded", "Help"));
                else
                {
                    var helpCommand = new SlashCommandBuilder()
                        .WithName("help")
                        .WithDescription("Get EMBRS bot commands.");
                    await guild.CreateApplicationCommandAsync(helpCommand.Build());
                }

                if (!_updateCommands && commands.Any(r => r.Name == "maintenance")) await Log(new LogMessage(LogSeverity.Info, "Command Loaded", "Maintenance"));
                else
                {
                    var maintenanceCommand = new SlashCommandBuilder()
                        .WithName("maintenance")
                        .WithDescription("Shutdown EMBRS Forged bot for maintenance.");
                    await guild.CreateApplicationCommandAsync(maintenanceCommand.Build());
                }

                if (!_updateCommands && commands.Any(r => r.Name == "register")) await Log(new LogMessage(LogSeverity.Info, "Command Loaded", "Register"));
                else
                {
                    var registerCommand = new SlashCommandBuilder()
                        .WithName("register")
                        .WithDescription("Register with EMBRS bot.")
                        .AddOption("xrpaddress", ApplicationCommandOptionType.String, "The XRP address", isRequired: true);
                    await guild.CreateApplicationCommandAsync(registerCommand.Build());
                }

                //if (!_updateCommands && commands.Any(r => r.Name == "reward")) await Log(new LogMessage(LogSeverity.Info, "Command Loaded", "Reward"));
                //else
                //{
                //    var rewardCommand = new SlashCommandBuilder()
                //        .WithName("reward")
                //        .WithDescription("Receive Emberlight tournament reward.");
                //    await guild.CreateApplicationCommandAsync(rewardCommand.Build());
                //}

                if (!_updateCommands && commands.Any(r => r.Name == "select")) await Log(new LogMessage(LogSeverity.Info, "Command Loaded", "Select"));
                else
                {
                    var selectCommand = new SlashCommandBuilder()
                        .WithName("select")
                        .WithDescription("Select random Emberlight tournament winners.")
                        .AddOption("amount", ApplicationCommandOptionType.Integer, "The random winner amount", isRequired: true);
                    await guild.CreateApplicationCommandAsync(selectCommand.Build());
                }

                if (!_updateCommands && commands.Any(r => r.Name == "setwinner")) await Log(new LogMessage(LogSeverity.Info, "Command Loaded", "SetWinner"));
                else
                {
                    var setWinnerCommand = new SlashCommandBuilder()
                        .WithName("setwinner")
                        .WithDescription("Set Emberlight tournament winner.")
                        .AddOption("user", ApplicationCommandOptionType.User, "The user to set as winner", isRequired: true);
                    await guild.CreateApplicationCommandAsync(setWinnerCommand.Build());
                }

                if (!_updateCommands && commands.Any(r => r.Name == "start")) await Log(new LogMessage(LogSeverity.Info, "Command Loaded", "Start"));
                else
                {
                    var startCommand = new SlashCommandBuilder()
                        .WithName("start")
                        .WithDescription("Start currently running Emberlight tournament.")
                        .AddOption("achievement", ApplicationCommandOptionType.String, "The week's tournament goal", isRequired: true)
                        .AddOption("week", ApplicationCommandOptionType.String, "Current week", isRequired: true);
                    await guild.CreateApplicationCommandAsync(startCommand.Build());
                }

                if (!_updateCommands && commands.Any(r => r.Name == "status")) await Log(new LogMessage(LogSeverity.Info, "Command Loaded", "Status"));
                else
                {
                    var statusCommand = new SlashCommandBuilder()
                        .WithName("status")
                        .WithDescription("Check your status within EMBRS bot.");
                    await guild.CreateApplicationCommandAsync(statusCommand.Build());
                }

                if (!_updateCommands && commands.Any(r => r.Name == "swap")) await Log(new LogMessage(LogSeverity.Info, "Command Loaded", "Swap"));
                else
                {
                    var swapCommand = new SlashCommandBuilder()
                        .WithName("swap")
                        .WithDescription("Swap between EMBRS/XRP, EMBRS/USD, and USD/XRP.")
                        .AddOption("from", ApplicationCommandOptionType.String, "EMBRS/USD/XRP", isRequired: true)
                        .AddOption("to", ApplicationCommandOptionType.String, "EMBRS/USD/XRP", isRequired: true)
                        .AddOption("amount", ApplicationCommandOptionType.Number, "The swap amount", isRequired: true);
                    await guild.CreateApplicationCommandAsync(swapCommand.Build());
                }

                if (!_updateCommands && commands.Any(r => r.Name == "tip")) await Log(new LogMessage(LogSeverity.Info, "Command Loaded", "Tip"));
                else
                {
                    var tipCommand = new SlashCommandBuilder()
                        .WithName("tip")
                        .WithDescription("Tip another community member.")
                        .AddOption("user", ApplicationCommandOptionType.User, "The user to tip", isRequired: true)
                        .AddOption("amount", ApplicationCommandOptionType.Number, "The tip amount", isRequired: true);
                    await guild.CreateApplicationCommandAsync(tipCommand.Build());
                }

                if (!_updateCommands && commands.Any(r => r.Name == "tournament")) await Log(new LogMessage(LogSeverity.Info, "Command Loaded", "Tournament"));
                else
                {
                    var tournamentCommand = new SlashCommandBuilder()
                       .WithName("tournament")
                       .WithDescription("Join Emberlight tournament during sign-up period.");
                    await guild.CreateApplicationCommandAsync(tournamentCommand.Build());
                }

                if (!_updateCommands && commands.Any(r => r.Name == "unregister")) await Log(new LogMessage(LogSeverity.Info, "Command Loaded", "Unregister"));
                else
                {
                    var unregisterCommand = new SlashCommandBuilder()
                        .WithName("unregister")
                        .WithDescription("Unregister from EMBRS bot.");
                    await guild.CreateApplicationCommandAsync(unregisterCommand.Build());
                }

                var updateChannel = guild.TextChannels.FirstOrDefault(x => x.Name == "updates");
                await updateChannel.SendMessageAsync("**EMBRS Forged bot is now online! Please check above GitHub check-in for latest changes!**");
                _ready = true;
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                await Log(new LogMessage(LogSeverity.Error, exception.Source, json, exception));
            }
        }
    }
}
