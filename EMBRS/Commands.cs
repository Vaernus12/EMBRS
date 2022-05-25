using Discord;
using Discord.WebSocket;
using EMBRS;
using Newtonsoft.Json;
using RippleDotNet;
using RippleDotNet.Json.Converters;
using RippleDotNet.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUMM.NET.SDK.EMBRS;
using XUMM.NET.SDK.Models.Payload;

namespace EMBRS_Discord
{
    public class Commands
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly XummWebSocket _webSocketClient;
        private readonly XummMiscAppStorageClient _appStorageClient;
        private readonly XummMiscClient _miscClient;
        private readonly XummPayloadClient _payloadClient;
        private readonly XummHttpClient _httpClient;

        private readonly bool _sponsorRewards = false;

        public Commands(DiscordSocketClient discordClient, XummWebSocket webSocketClient, XummMiscAppStorageClient appStorageClient,
                        XummMiscClient miscClient, XummPayloadClient payloadClient, XummHttpClient httpClient)
        {
            _discordClient = discordClient;
            _webSocketClient = webSocketClient;
            _appStorageClient = appStorageClient;
            _miscClient = miscClient;
            _payloadClient = payloadClient;
            _httpClient = httpClient;
        }

        public async Task HandleSlashCommandAsync(SocketSlashCommand command)
        {
            try
            {
                await command.DeferAsync(ephemeral: true);

                switch (command.Data.Name)
                {
                    case "addtopic":
                        {
                            await Program.Log(new LogMessage(LogSeverity.Info, "Command", "addtopic"));
                            var addTopicTask = Task.Run(async () =>
                            {
                                await HandleAddTopicCommand(command);
                            });
                            break;
                        }
                    case "end":
                        {
                            await Program.Log(new LogMessage(LogSeverity.Info, "Command", "end"));
                            var endTask = Task.Run(async () =>
                            {
                                await HandleEndCommand(command);
                            });
                            break;
                        }
                    case "faucet":
                        {
                            await Program.Log(new LogMessage(LogSeverity.Info, "Command", "faucet"));
                            var faucetTask = Task.Run(async () =>
                            {
                                await HandleFaucetCommand(command);
                            });
                            break;
                        }
                    case "help":
                        {
                            await Program.Log(new LogMessage(LogSeverity.Info, "Command", "help"));
                            var helpTask = Task.Run(async () =>
                            {
                                await HandleHelpCommand(command);
                            });
                            break;
                        }
                    case "maintenance":
                        {
                            await Program.Log(new LogMessage(LogSeverity.Info, "Command", "maintenance"));
                            var maintenanceTask = Task.Run(async () =>
                            {
                                await HandleMaintenanceCommand(command);
                            });
                            break;
                        }
                    case "register":
                        {
                            await Program.Log(new LogMessage(LogSeverity.Info, "Command", "register"));
                            var registerTask = Task.Run(async () =>
                            {
                                await HandleRegisterCommand(command);
                            });
                            break;
                        }
                    case "select":
                        {
                            await Program.Log(new LogMessage(LogSeverity.Info, "Command", "select"));
                            var selectTask = Task.Run(async () =>
                            {
                                await HandleSelectCommand(command);
                            });
                            break;
                        }
                    case "setwinner":
                        {
                            await Program.Log(new LogMessage(LogSeverity.Info, "Command", "setwinner"));
                            var setwinnerTask = Task.Run(async () =>
                            {
                                await HandleSetWinnerCommand(command);
                            });
                            break;
                        }
                    case "start":
                        {
                            await Program.Log(new LogMessage(LogSeverity.Info, "Command", "start"));
                            var startTask = Task.Run(async () =>
                            {
                                await HandleStartCommand(command);
                            });
                            break;
                        }
                    case "status":
                        {
                            await Program.Log(new LogMessage(LogSeverity.Info, "Command", "status"));
                            var statusTask = Task.Run(async () =>
                            {
                                await HandleStatusCommand(command);
                            });
                            break;
                        }
                    case "swap":
                        {
                            await Program.Log(new LogMessage(LogSeverity.Info, "Command", "swap"));
                            var swapTask = Task.Run(async () =>
                            {
                                await HandleSwapCommand(command);
                            });
                            break;
                        }
                    case "tip":
                        {
                            await Program.Log(new LogMessage(LogSeverity.Info, "Command", "tip"));
                            var tipTask = Task.Run(async () =>
                            {
                                await HandleTipCommand(command);
                            });
                            break;
                        }
                    case "tournament":
                        {
                            await Program.Log(new LogMessage(LogSeverity.Info, "Command", "tournament"));
                            var tournamentTask = Task.Run(async () =>
                            {
                                await HandleTournamentCommand(command);
                            });
                            break;
                        }
                    case "unregister":
                        {
                            await Program.Log(new LogMessage(LogSeverity.Info, "Command", "unregister"));
                            var unregisterTask = Task.Run(async () =>
                            {
                                await HandleUnregisterCommand(command);
                            });
                            break;
                        }
                    case "vote":
                        {
                            await Program.Log(new LogMessage(LogSeverity.Info, "Command", "vote"));
                            var voteTask = Task.Run(async () =>
                            {
                                await HandleVoteCommand(command);
                            });
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private async Task HandleAddTopicCommand(SocketSlashCommand command)
        {
            try
            {
                if (!await CheckForTimeBetweenCommands(command)) return;
                if (!await CheckIfRegistered(command)) return;
                if (!await CheckIfCorrectChannel(command, "bot-commands")) return;

                var userInfo = command.User as SocketGuildUser;
                
                var header = (string)command.Data.Options.SingleOrDefault(r => r.Name == "header").Value;
                var content = (string)command.Data.Options.SingleOrDefault(r => r.Name == "content").Value;

                var thread = await Database.GetDatabase<DatabaseThreads>(DatabaseType.Threads).CreateThread(userInfo, header, content, _discordClient);
                await command.FollowupAsync("New thread created in channel #" + thread.GetThreadChannelName() + " under Governance category!", ephemeral: true);
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private async Task HandleEndCommand(SocketSlashCommand command)
        {
            try
            {
                if (!await CheckForTimeBetweenCommands(command)) return;
                if (!await CheckIfAdmin(command)) return;
                if (!await CheckIfCorrectChannel(command, "tournament")) return;

                var userInfo = command.User as SocketGuildUser;
                var guild = _discordClient.GetGuild(ulong.Parse(Settings.GuildID));
                var tournamentRole = guild.Roles.FirstOrDefault(x => x.Name == "Tournament");
                var winnerRole = guild.Roles.FirstOrDefault(x => x.Name == "Tournament Winner");
                var roles = new List<SocketRole>() { tournamentRole, winnerRole };

                var users = await command.Channel.GetUsersAsync().FlattenAsync<IUser>();
                foreach (IGuildUser user in users)
                {
                    await user.RemoveRolesAsync(roles);
                }

                foreach (var user in Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccounts())
                {
                    user.TournamentWinner = false;
                    user.ReceivedTournamentReward = false;
                    user.InTournament = false;
                }

                var message = (string)command.Data.Options.First().Value;
                await command.FollowupAsync("Thank you to everyone that participated in the week " + message + " Emberlight tournament! Sign-ups for week " + (int.Parse(message) + 1).ToString() + " will start tomorrow. Check #announcements for more details.");

                Database.IsDirty = true;
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private async Task HandleFaucetCommand(SocketSlashCommand command)
        {
            try
            {
                if (!await CheckForTimeBetweenFaucet(command)) return;
                if (!await CheckIfRegistered(command)) return;
                if (!await CheckIfCorrectChannel(command, "bot-commands")) return;

                var userInfo = command.User as SocketGuildUser;
                await XRPL.SendRewardAsync(command, null, userInfo, Settings.FaucetTokenAmt, false, false, true);
                Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).EMBRSEarned += float.Parse(Settings.FaucetTokenAmt);
                Database.IsDirty = true;

            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private async Task HandleHelpCommand(SocketSlashCommand command)
        {
            try
            {
                if (!await CheckForTimeBetweenCommands(command)) return;
                if (!await CheckIfCorrectChannel(command, "bot-commands")) return;

                var userInfo = command.User as SocketGuildUser;

                var stringBuilder = new StringBuilder();
                stringBuilder.Append("To use slash commands on **mobile**, please type out the /command until it shows listed above. Tap the command, and then tap each parameter (if required) and it will allow you to fill them out individually.");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                stringBuilder.Append("To use slash commands on **desktop**, please type out the /command until it shows listed above. Click it and it should automatically jump to the first parameter (if required). Click the box next to each parameter to fill them out individually.");
                stringBuilder.AppendLine();
                stringBuilder.Append("-----");

                var swapStringBuilder = new StringBuilder();
                swapStringBuilder.Append("Swap between trading pairs EMBRS/XRP, EMBRS/USD, and USD/XRP. Sign transaction in XUMM Wallet for safety and security (#bot-commands)");
                swapStringBuilder.AppendLine();
                swapStringBuilder.AppendLine();
                swapStringBuilder.Append("Example 1: **/swap EMBRS XRP 10** will swap 10 EMBRS into equal in value amount of XRP (exchange rate shown in XUMM)");
                swapStringBuilder.AppendLine();
                swapStringBuilder.AppendLine();
                swapStringBuilder.Append("Example 2: **/swap USD EMBRS 10** will swap an equal in value amount of USD into 10 EMBRS (exchange rate shown in XUMM)");

                var tipStringBuilder = new StringBuilder();
                tipStringBuilder.Append("Tip a registered EMBRS user, and sign transaction in XUMM Wallet for safety and security (#lounge)");
                tipStringBuilder.AppendLine();
                tipStringBuilder.AppendLine();
                tipStringBuilder.Append("Example: **/tip @Vaernus 10** will tip 10 EMBRS as a payment to @Vaernus (who will most likely tip you back because he's pretty cool)");

                var embedBuiler = new EmbedBuilder()
                    .WithAuthor(userInfo.ToString(), userInfo.GetAvatarUrl() ?? userInfo.GetDefaultAvatarUrl())
                    .WithDescription(stringBuilder.ToString())
                    .WithColor(Color.Orange)
                    .AddField("/faucet", "Receive daily EMBRS from faucet (#bot-commands)")
                    .AddField("/help", "Show EMBRS bot command list (#bot-commands)")
                    .AddField("/register <xrpaddress>", "Register your Discord username and XRP address for faucet, rewards, DEX swaps, and tips (#bot-commands)")
                    .AddField("/status", "Check status of your XRP address, balances, earned EMBRS, etc. in EMBRS bot (#bot-commands)")
                    .AddField("/swap <from> <to> <amount>", swapStringBuilder.ToString())
                    .AddField("/tip <recipient> <amount>", tipStringBuilder.ToString())
                    .AddField("/tournament", "Sign-up for the current week's Emberlight tournament (#bot-commands)")
                    .AddField("/unregister", "Unregister from the EMBRS bot (#bot-commands)");

                await command.FollowupAsync(embed: embedBuiler.Build(), ephemeral: true);
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private async Task HandleMaintenanceCommand(SocketSlashCommand command)
        {
            try
            {
                if (!await CheckForTimeBetweenCommands(command)) return;
                if (!await CheckIfAdmin(command)) return;
                if (!await CheckIfCorrectChannel(command, "bot-commands")) return;

                var userInfo = command.User as SocketGuildUser;
                var guild = _discordClient.GetGuild(ulong.Parse(Settings.GuildID));
                var updateChannel = guild.TextChannels.FirstOrDefault(x => x.Name == "updates");
                await updateChannel.SendMessageAsync("**EMBRS Forged bot is shutting down for maintenance!**");
                await command.FollowupAsync("EMBRS maintenance ready", ephemeral: true);
                await Program.Shutdown();

            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private async Task HandleRegisterCommand(SocketSlashCommand command)
        {
            try
            {
                if (!await CheckForTimeBetweenCommands(command)) return;
                if (!await CheckIfCorrectChannel(command, "bot-commands")) return;

                var userInfo = command.User as SocketGuildUser;
                var xrpAddress = (string)command.Data.Options.First().Value;
                if (!Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).IsRegistered)
                {
                    Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).RegisterAccount(userInfo.Id, xrpAddress);
                    await command.FollowupAsync("You are registered with EMBRS bot!", ephemeral: true);
                    Database.IsDirty = true;
                }
                else
                {
                    Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).RegisterAccount(userInfo.Id, xrpAddress);
                    await command.FollowupAsync("You updated your XRP address in EMBRS bot!", ephemeral: true);
                    Database.IsDirty = true;
                }
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private async Task HandleSelectCommand(SocketSlashCommand command)
        {
            try
            {
                if (!await CheckForTimeBetweenCommands(command)) return;
                if (!await CheckIfAdmin(command)) return;
                if (!await CheckIfCorrectChannel(command, "winners")) return;

                var userInfo = command.User as SocketGuildUser;
                var amount = (Int64)command.Data.Options.First().Value;
                var users = await command.Channel.GetUsersAsync().FlattenAsync<IUser>();
                var usersList = new List<IUser>(users);
                var removeList = new List<IUser>();

                for (int i = 0; i < usersList.Count; i++)
                {
                    var user = usersList[i] as SocketGuildUser;
                    if (!user.Roles.Any(r => r.Name == "Tournament Winner"))
                    {
                        removeList.Add(user);
                    }
                }

                for (int i = 0; i < removeList.Count; i++)
                {
                    usersList.Remove(removeList[i]);
                }

                var rng = new System.Random();
                var guild = _discordClient.GetGuild(ulong.Parse(Settings.GuildID));
                var earlyAccessRole = guild.Roles.FirstOrDefault(x => x.Name == "Early Supporters");

                var stringBuilder = new StringBuilder();

                if (usersList.Count > 0)
                {
                    for (int i = 0; i < amount; i++)
                    {
                        var index = rng.Next(0, usersList.Count);
                        var user = usersList[index] as SocketGuildUser;
                        if (usersList.Count > 1) usersList.RemoveAt(index);

                        stringBuilder.Append($"@{user.Username}#{user.Discriminator}");
                        if (i == 0)
                        {
                            stringBuilder.Append($" - 1100 EMBRS and early access slot to Emberlight: Rekindled!");
                            if (Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).ReceivedTournamentReward)
                            {
                                await XRPL.SendRewardAsync(command, null, user, "1000");
                                Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).EMBRSEarned += 1000;
                            }
                            else
                            {
                                await XRPL.SendRewardAsync(command, null, user, "1100");
                                Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).EMBRSEarned += 1100;
                            }

                            await user.AddRoleAsync(earlyAccessRole);
                            Database.IsDirty = true;
                        }
                        else
                        {
                            stringBuilder.Append($" - 600 EMBRS and early access slot to Emberlight: Rekindled!");
                            if (Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).ReceivedTournamentReward)
                            {
                                await XRPL.SendRewardAsync(command, null, user, "500");
                                Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).EMBRSEarned += 500;
                            }
                            else
                            {
                                await XRPL.SendRewardAsync(command, null, user, "600");
                                Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).EMBRSEarned += 600;
                            }

                            Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).ReceivedTournamentReward = true;

                            await user.AddRoleAsync(earlyAccessRole);
                            Database.IsDirty = true;
                        }
                        stringBuilder.AppendLine();
                    }

                    if (usersList.Count > 0)
                    {
                        for (int i = 0; i < usersList.Count; i++)
                        {
                            var user = usersList[i] as SocketGuildUser;
                            stringBuilder.Append($"@{user.Username}#{user.Discriminator}");
                            stringBuilder.Append($" - 100 EMBRS!");

                            if (!Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).ReceivedTournamentReward)
                            {
                                await XRPL.SendRewardAsync(command, null, user, "100");
                                Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).EMBRSEarned += 100;
                                Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).ReceivedTournamentReward = true;
                            }

                            stringBuilder.AppendLine();
                        }
                    }
                }

                var embedBuiler = new EmbedBuilder()
                    .WithTitle("Tournament Results")
                    .WithColor(Color.Orange)
                    .AddField("Winners", stringBuilder.ToString())
                    .AddField("Congratulations!", "We will be ending the Emberlight tournament shortly! If you have any questions, please let us know here!");

                await command.FollowupAsync(embed: embedBuiler.Build());
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private async Task HandleSetWinnerCommand(SocketSlashCommand command)
        {
            try
            {
                if (!await CheckForTimeBetweenCommands(command)) return;
                if (!await CheckIfAdmin(command)) return;
                if (!await CheckIfCorrectChannel(command, "verify")) return;

                var userInfo = command.User as SocketGuildUser;
                var guildUser = (SocketGuildUser)command.Data.Options.First().Value;
                if (!Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).ContainsAccount(guildUser.Id) || !Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(guildUser.Id).IsRegistered)
                {
                    await command.FollowupAsync($"{guildUser.Username}#{guildUser.Discriminator} is not registered with EMBRS!");
                }
                else
                {
                    var guild = _discordClient.GetGuild(ulong.Parse(Settings.GuildID));
                    var winnerRole = guild.Roles.FirstOrDefault(x => x.Name == "Tournament Winner");
                    await guildUser.AddRoleAsync(winnerRole);
                    await command.FollowupAsync($"A winner is {guildUser.Username}#{guildUser.Discriminator}!");
                    Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(guildUser.Id).TournamentWinner = true;
                    Database.IsDirty = true;
                }
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private async Task HandleStartCommand(SocketSlashCommand command)
        {
            try
            {
                if (!await CheckForTimeBetweenCommands(command)) return;
                if (!await CheckIfAdmin(command)) return;
                if (!await CheckIfCorrectChannel(command, "tournament")) return;

                var userInfo = command.User as SocketGuildUser;
                var achievement = (string)command.Data.Options.SingleOrDefault(r => r.Name == "achievement").Value;
                var week = (string)command.Data.Options.SingleOrDefault(r => r.Name == "week").Value;


                var stringBuilder = new StringBuilder();
                stringBuilder.Append("**The Emberlight tournament has started! Week " + week + "'s achievement is: **");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                stringBuilder.Append("**" + achievement + "**");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                stringBuilder.Append("**Good luck!**");
                await command.FollowupAsync(stringBuilder.ToString());
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private async Task HandleStatusCommand(SocketSlashCommand command)
        {
            try
            {
                if (!await CheckForTimeBetweenCommands(command)) return;
                if (!await CheckIfCorrectChannel(command, "bot-commands")) return;

                var userInfo = command.User as SocketGuildUser;

                if (Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).IsRegistered)
                {
                    var xrp = 0.0m;
                    var embrs = 0.0m;
                    var stx = 0.0m;
                    var usd = 0.0m;

                    IRippleClient client = new RippleClient(Settings.WebSocketUrl);
                    client.Connect();
                    {
                        xrp = await XRPL.ReturnAccountBalance(client, Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).XrpAddress);
                        System.Threading.Thread.Sleep(Settings.AccountLinesThrottle * 1000);

                        string marker = "";
                        do
                        {
                            var returnObj = await XRPL.ReturnTrustLines(client, Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).XrpAddress, marker);
                            if (embrs == 0.0m) embrs = returnObj.EMBRSBalance;
                            if (stx == 0.0m) stx = returnObj.STXBalance;
                            if (usd == 0.0m) usd = returnObj.USDBalance;
                            marker = returnObj.Marker;
                            System.Threading.Thread.Sleep(Settings.AccountLinesThrottle * 1000);
                        } while (marker != "" && marker != null);
                    }
                    client.Disconnect();

                    var stringBuilder = new StringBuilder();
                    stringBuilder.Append("**XRP**: " + xrp.ToString());
                    stringBuilder.AppendLine();
                    stringBuilder.Append("**EMBRS**: " + embrs.ToString());
                    stringBuilder.AppendLine();
                    stringBuilder.Append("**STX**: " + stx.ToString());
                    stringBuilder.AppendLine();
                    stringBuilder.Append("**USD**: " + usd.ToString());

                    var embedBuiler = new EmbedBuilder()
                        .WithAuthor(userInfo.ToString(), userInfo.GetAvatarUrl() ?? userInfo.GetDefaultAvatarUrl())
                        .WithTitle(Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).XrpAddress)
                        .WithColor(Color.Orange)
                        .AddField("Balances", stringBuilder.ToString())
                        .AddField("EMBRS Earned", Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).EMBRSEarned)
                        .AddField("In Tournament", Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).InTournament)
                        .AddField("Won Tournament", Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).TournamentWinner)
                        .AddField("Received Tournament Reward", Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).ReceivedTournamentReward);

                    await command.FollowupAsync(embed: embedBuiler.Build(), ephemeral: true);
                }
                else
                {
                    var embedBuiler = new EmbedBuilder()
                        .WithAuthor(userInfo.ToString(), userInfo.GetAvatarUrl() ?? userInfo.GetDefaultAvatarUrl())
                        .WithTitle("NOT REGISTERED")
                        .WithDescription("Use /register command to link your XRP address!")
                        .WithColor(Color.Orange);
                    await command.FollowupAsync(embed: embedBuiler.Build(), ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private async Task HandleSwapCommand(SocketSlashCommand command)
        {
            try
            {
                if (!await CheckForTimeBetweenSwapOrTip(command)) return;
                if (!await CheckIfRegistered(command)) return;
                if (!await CheckIfCorrectChannel(command, "bot-commands")) return;

                var userInfo = command.User as SocketGuildUser;
                var from = (string)command.Data.Options.SingleOrDefault(r => r.Name == "from").Value;
                var to = (string)command.Data.Options.SingleOrDefault(r => r.Name == "to").Value;
                var amount = (double)command.Data.Options.SingleOrDefault(r => r.Name == "amount").Value;

                XummPayloadResponse createdPayload = null;

                if ((from.ToLower() == "embrs" || from.ToLower() == "embers" || from.ToLower() == "usd" || from.ToLower() == "xrp") &&
                   (to.ToLower() == "embrs" || to.ToLower() == "embers" || to.ToLower() == "usd" || to.ToLower() == "xrp") &&
                   from.ToLower() != to.ToLower())
                {
                    IRippleClient client = new RippleClient(Settings.WebSocketUrl);
                    client.Connect();
                    var initialMidPrice = await XRPL.GetBookOffers(client, from, to);
                    var value = initialMidPrice.Midprice;
                    client.Disconnect();

                    Currency fromCurrency = null;
                    Currency toCurrency = null;
                    var toAmount = decimal.Round(Convert.ToDecimal(amount) * value.Value, 6);

                    if ((from.ToLower() == "embrs" || from.ToLower() == "embers") && to.ToLower() == "xrp")
                    {
                        fromCurrency = new Currency
                        {
                            CurrencyCode = Settings.CurrencyCode,
                            Issuer = Settings.IssuerAddress,
                            Value = amount.ToString()
                        };

                        toCurrency = new Currency()
                        {
                            ValueAsXrp = toAmount
                        };
                    }
                    else if ((to.ToLower() == "embrs" || to.ToLower() == "embers") && from.ToLower() == "xrp")
                    {
                        fromCurrency = new Currency()
                        {
                            ValueAsXrp = toAmount
                        };

                        toCurrency = new Currency
                        {
                            CurrencyCode = Settings.CurrencyCode,
                            Issuer = Settings.IssuerAddress,
                            Value = amount.ToString()
                        };
                    }
                    else if ((from.ToLower() == "embrs" || from.ToLower() == "embers") && to.ToLower() == "usd")
                    {
                        fromCurrency = new Currency
                        {
                            CurrencyCode = Settings.CurrencyCode,
                            Issuer = Settings.IssuerAddress,
                            Value = amount.ToString()
                        };

                        toCurrency = new Currency
                        {
                            CurrencyCode = Settings.USDCurrencyCode,
                            Issuer = Settings.USDIssuerAddress,
                            Value = toAmount.ToString()
                        };
                    }
                    else if ((to.ToLower() == "embrs" || to.ToLower() == "embers") && from.ToLower() == "usd")
                    {
                        fromCurrency = new Currency
                        {
                            CurrencyCode = Settings.USDCurrencyCode,
                            Issuer = Settings.USDIssuerAddress,
                            Value = toAmount.ToString()
                        };

                        toCurrency = new Currency
                        {
                            CurrencyCode = Settings.CurrencyCode,
                            Issuer = Settings.IssuerAddress,
                            Value = amount.ToString()
                        };
                    }
                    else if (from.ToLower() == "usd" && to.ToLower() == "xrp")
                    {
                        fromCurrency = new Currency
                        {
                            CurrencyCode = Settings.USDCurrencyCode,
                            Issuer = Settings.USDIssuerAddress,
                            Value = amount.ToString()
                        };

                        toCurrency = new Currency()
                        {
                            ValueAsXrp = toAmount
                        };
                    }
                    else if (to.ToLower() == "usd" && from.ToLower() == "xrp")
                    {
                        fromCurrency = new Currency()
                        {
                            ValueAsXrp = toAmount
                        };

                        toCurrency = new Currency
                        {
                            CurrencyCode = Settings.USDCurrencyCode,
                            Issuer = Settings.USDIssuerAddress,
                            Value = amount.ToString()
                        };
                    }
                    else
                    {
                        await command.FollowupAsync("Invalid pairs or parameters!", ephemeral: true);
                        return;
                    }

                    await command.FollowupAsync($"Swapping " + amount.ToString() + " " + from.ToUpper() + "/" + to.ToUpper(), ephemeral: true);

                    var takerGetsConverter = new CurrencyConverter();
                    var takerGets = fromCurrency;
                    var takerGetsResult = JsonConvert.SerializeObject(takerGets, takerGetsConverter);

                    var takerPaysConverter = new CurrencyConverter();
                    var takerPays = toCurrency;
                    var takerPaysResult = JsonConvert.SerializeObject(takerPays, takerPaysConverter);

                    var flags = OfferCreateFlags.tfImmediateOrCancel;
                    var flagsResults = JsonConvert.SerializeObject(flags);

                    var payload = new XummPostJsonPayload("{ \"TransactionType\": \"OfferCreate\", " +
                                                            "\"TakerGets\": " + takerGetsResult + ", " +
                                                            "\"TakerPays\": " + takerPaysResult + ", " +
                                                            "\"Flags\": " + flagsResults + " }");

                    payload.Options = new XummPayloadOptions();
                    payload.Options.Expire = 5;
                    payload.Options.Submit = true;

                    payload.CustomMeta = new XummPayloadCustomMeta();
                    payload.CustomMeta.Instruction = "Swapping " + amount.ToString() + " " + from.ToUpper() + "/" + to.ToUpper();

                    createdPayload = await _payloadClient.CreateAsync(payload);

                    // IF MOBILE, PUSH TO XUMM APP
                    if (userInfo.ActiveClients.Any(r => r == ClientType.Mobile))
                    {
                        var embedBuiler = new EmbedBuilder()
                                            .WithUrl(createdPayload.Next.Always)
                                            .WithDescription("Open In Xumm Wallet")
                                            .WithAuthor(userInfo.ToString(), userInfo.GetAvatarUrl() ?? userInfo.GetDefaultAvatarUrl())
                                            .WithTitle("EMBRS Sign Request");
                        await command.FollowupAsync(embed: embedBuiler.Build(), ephemeral: true);

                        var getPayload = await _payloadClient.GetAsync(createdPayload);
                        while (!getPayload.Meta.Expired)
                        {
                            if (getPayload.Meta.Resolved && getPayload.Meta.Signed)
                            {
                                await command.FollowupAsync($"Swap was resolved and signed!", ephemeral: true);
                                Database.IsDirty = true;
                                break;
                            }
                            else if (getPayload.Meta.Resolved && !getPayload.Meta.Signed)
                            {
                                await command.FollowupAsync($"Swap was cancelled by user", ephemeral: true);
                                break;
                            }

                            System.Threading.Thread.Sleep(Settings.TxnThrottle * 3000);
                            getPayload = await _payloadClient.GetAsync(createdPayload);
                        }

                        if (getPayload.Meta.Expired)
                        {
                            await command.FollowupAsync($"Swap sign request expired", ephemeral: true);
                        }
                    }
                    else // IF NOT MOBILE, PUSH FOLLOWUP WITH PNG TO QR SCAN AND SIGN
                    {
                        var qrPNG = createdPayload.Refs.QrPng;
                        var embedBuiler = new EmbedBuilder()
                                            .WithImageUrl(qrPNG)
                                            .WithDescription("Scan In Xumm Wallet")
                                            .WithAuthor(userInfo.ToString(), userInfo.GetAvatarUrl() ?? userInfo.GetDefaultAvatarUrl())
                                            .WithTitle("EMBRS Sign Request");
                        await command.FollowupAsync(embed: embedBuiler.Build(), ephemeral: true);

                        var getPayload = await _payloadClient.GetAsync(createdPayload);
                        while (!getPayload.Meta.Expired)
                        {
                            if (getPayload.Meta.Resolved && getPayload.Meta.Signed)
                            {
                                await command.FollowupAsync($"Swap was resolved and signed!", ephemeral: true);
                                Database.IsDirty = true;
                                break;
                            }
                            else if (getPayload.Meta.Resolved && !getPayload.Meta.Signed)
                            {
                                await command.FollowupAsync($"Swap was cancelled by user", ephemeral: true);
                                break;
                            }

                            System.Threading.Thread.Sleep(Settings.TxnThrottle * 3000);
                            getPayload = await _payloadClient.GetAsync(createdPayload);
                        }

                        if (getPayload.Meta.Expired)
                        {
                            await command.FollowupAsync($"Swap sign request expired", ephemeral: true);
                        }
                    }
                }
                else
                {
                    await command.FollowupAsync("Invalid pairs or parameters!", ephemeral: true);
                    return;
                }
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private async Task HandleTipCommand(SocketSlashCommand command)
        {
            try
            {
                if (!await CheckForTimeBetweenSwapOrTip(command)) return;
                if (!await CheckIfRegistered(command)) return;

                var userInfo = command.User as SocketGuildUser;
                var user = (SocketGuildUser)command.Data.Options.SingleOrDefault(r => r.Name == "user").Value;
                var amount = (double)command.Data.Options.SingleOrDefault(r => r.Name == "amount").Value;

                if (Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).ContainsAccount(user.Id) && Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).IsRegistered)
                {
                    var tipAmount = string.Empty;
                    if (userInfo.Roles.Any(r => r.Name == "Leads"))
                    {
                        await command.FollowupAsync("Beginning server tip", ephemeral: true);
                        tipAmount = Math.Min(amount, float.Parse(Settings.MaxTipTokenAmt)).ToString();
                        await XRPL.SendRewardAsync(command, userInfo, user, tipAmount, false, true, false);
                        Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).EMBRSEarned += float.Parse(tipAmount);
                        Database.IsDirty = true;
                    }
                    else
                    { 
                        var destination = Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).XrpAddress;
                        var currencyAmount = new Currency { CurrencyCode = Settings.CurrencyCode, Issuer = Settings.IssuerAddress, Value = amount.ToString() };

                        var converter = new CurrencyConverter();
                        var result = JsonConvert.SerializeObject(currencyAmount, converter);

                        var payload = new XummPostJsonPayload("{ \"TransactionType\": \"Payment\", " +
                                                                "\"Destination\": \"" + destination + "\", " +
                                                                "\"Amount\": " + result + " }");

                        payload.Options = new XummPayloadOptions();
                        payload.Options.Expire = 5;
                        payload.Options.Submit = true;

                        payload.CustomMeta = new XummPayloadCustomMeta();
                        payload.CustomMeta.Instruction = "Tipping " + amount.ToString() + " EMBRS to " + destination + " (" + user.Username + "#" + user.Discriminator + ")";

                        var createdPayload = await _payloadClient.CreateAsync(payload);

                        // IF MOBILE, PUSH TO XUMM APP
                        if (user.ActiveClients.Any(r => r == ClientType.Mobile))
                        {
                            var embedBuiler = new EmbedBuilder()
                                                .WithUrl(createdPayload.Next.Always)
                                                .WithDescription("Open In Xumm Wallet")
                                                .WithAuthor(userInfo.ToString(), userInfo.GetAvatarUrl() ?? userInfo.GetDefaultAvatarUrl())
                                                .WithTitle("EMBRS Sign Request");
                            await command.FollowupAsync(embed: embedBuiler.Build(), ephemeral: true);

                            var getPayload = await _payloadClient.GetAsync(createdPayload);
                            while (!getPayload.Meta.Expired)
                            {
                                if (getPayload.Meta.Resolved && getPayload.Meta.Signed)
                                {
                                    await command.FollowupAsync($"**{userInfo.Username}#{userInfo.Discriminator} sent {user.Username}#{user.Discriminator} a tip of {amount} EMBRS!**");
                                    Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).EMBRSEarned += (float)amount;
                                    Database.IsDirty = true;
                                    break;
                                }
                                else if (getPayload.Meta.Resolved && !getPayload.Meta.Signed)
                                {
                                    await command.FollowupAsync($"Tip was cancelled by user", ephemeral: true);
                                    break;
                                }

                                System.Threading.Thread.Sleep(Settings.TxnThrottle * 3000);
                                getPayload = await _payloadClient.GetAsync(createdPayload);
                            }

                            if (getPayload.Meta.Expired)
                            {
                                await command.FollowupAsync($"Tip sign request expired", ephemeral: true);
                            }
                        }
                        else // IF NOT MOBILE, PUSH FOLLOWUP WITH PNG TO QR SCAN AND SIGN
                        {
                            var qrPNG = createdPayload.Refs.QrPng;
                            var embedBuiler = new EmbedBuilder()
                                                .WithImageUrl(qrPNG)
                                                .WithDescription("Scan In Xumm Wallet")
                                                .WithAuthor(userInfo.ToString(), userInfo.GetAvatarUrl() ?? userInfo.GetDefaultAvatarUrl())
                                                .WithTitle("EMBRS Sign Request");
                            await command.FollowupAsync(embed: embedBuiler.Build(), ephemeral: true);

                            var getPayload = await _payloadClient.GetAsync(createdPayload);
                            while (!getPayload.Meta.Expired)
                            {
                                if (getPayload.Meta.Resolved && getPayload.Meta.Signed)
                                {
                                    await command.FollowupAsync($"**{userInfo.Username}#{userInfo.Discriminator} sent {user.Username}#{user.Discriminator} a tip of {amount} EMBRS!**");
                                    Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).EMBRSEarned += (float)amount;
                                    Database.IsDirty = true;
                                    break;
                                }
                                else if (getPayload.Meta.Resolved && !getPayload.Meta.Signed)
                                {
                                    await command.FollowupAsync($"Tip was cancelled by user", ephemeral: true);
                                    break;
                                }

                                System.Threading.Thread.Sleep(Settings.TxnThrottle * 3000);
                                getPayload = await _payloadClient.GetAsync(createdPayload);
                            }

                            if (getPayload.Meta.Expired)
                            {
                                await command.FollowupAsync($"Tip sign request expired", ephemeral: true);
                            }
                        }
                    }
                }
                else
                {
                    await command.FollowupAsync("Recipient is not registered for tips!", ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private async Task HandleTournamentCommand(SocketSlashCommand command)
        {
            try
            {
                if (!await CheckForTimeBetweenCommands(command)) return;
                if (!await CheckIfRegistered(command)) return;
                if (!await CheckIfCorrectChannel(command, "bot-commands")) return;

                var userInfo = command.User as SocketGuildUser;

                if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Tuesday || DateTime.UtcNow.DayOfWeek == DayOfWeek.Wednesday || DateTime.UtcNow.DayOfWeek == DayOfWeek.Thursday)
                {
                    var guild = _discordClient.GetGuild(ulong.Parse(Settings.GuildID));
                    var tournamentRole = guild.Roles.FirstOrDefault(x => x.Name == "Tournament");
                    await userInfo.AddRoleAsync(tournamentRole);
                    await command.FollowupAsync("You are signed-up for this week's tournament! Check #tournament for more details.", ephemeral: true);
                    Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).InTournament = true;
                    Database.IsDirty = true;
                }
                else
                {
                    await command.FollowupAsync("Tournament sign-ups for this week are closed. Next week's sign-ups will start on Tuesday!", ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private async Task HandleUnregisterCommand(SocketSlashCommand command)
        {
            try
            {
                if (!await CheckForTimeBetweenCommands(command)) return;
                if (!await CheckIfRegistered(command)) return;
                if (!await CheckIfCorrectChannel(command, "bot-commands")) return;

                var userInfo = command.User as SocketGuildUser;

                Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).UnregisterAccount(userInfo.Id);
                var guild = _discordClient.GetGuild(ulong.Parse(Settings.GuildID));
                var tournamentRole = guild.Roles.FirstOrDefault(x => x.Name == "Tournament");
                var winnerRole = guild.Roles.FirstOrDefault(x => x.Name == "Tournament Winner");
                var roles = new List<SocketRole>() { tournamentRole, winnerRole };
                await userInfo.RemoveRolesAsync(roles);
                Database.IsDirty = true;

                await command.FollowupAsync("You are no longer registered with EMBRS bot!", ephemeral: true);
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private async Task HandleVoteCommand(SocketSlashCommand command)
        {
            try
            {
                if (!await CheckForTimeBetweenCommands(command)) return;
                if (!await CheckIfRegistered(command)) return;

                var userInfo = command.User as SocketGuildUser;
                var result = (string)command.Data.Options.SingleOrDefault(r => r.Name == "result").Value;

                if (result.ToLower() == "yes" || result.ToLower() == "no")
                {
                    var resultBool = (result.ToLower() == "yes") ? true : (result.ToLower() == "no") ? false : false;
                    var channelId = command.Channel.Id;
                    if (await Database.GetDatabase<DatabaseThreads>(DatabaseType.Threads).ContainsThreadByChannelId(channelId))
                    {
                        var thread = await Database.GetDatabase<DatabaseThreads>(DatabaseType.Threads).GetThreadByChannelId(channelId);
                        await thread.SetVote(userInfo.Id, resultBool);
                        await Database.GetDatabase<DatabaseThreads>(DatabaseType.Threads).UpdateThreadPositionInChannel(thread, _discordClient);
                        await command.FollowupAsync("Your vote has been applied!", ephemeral: true);
                    }
                    else
                    {
                        await command.FollowupAsync("Did not find a governance thread to vote on. Please make sure to use /vote command in a thread channel!", ephemeral: true);
                    }
                }
                else
                {
                    await command.FollowupAsync("Parameter incorrect in vote command! Must be yes or no.", ephemeral: true);
                    return;
                }
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private async Task<bool> CheckIfAdmin(SocketSlashCommand command)
        {
            try
            {
                var userInfo = command.User as SocketGuildUser;
                if (userInfo.Roles.Any(r => r.Name == "Leads")) return true;
                await command.FollowupAsync("Admin-only command!", ephemeral: true);
                return false;
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
                return false;
            }
        }

        private async Task<bool> CheckIfRegistered(SocketSlashCommand command)
        {
            try
            {
                var userInfo = command.User as SocketGuildUser;
                if (!Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).IsRegistered)
                {
                    await command.FollowupAsync("Please /register to use this command!", ephemeral: true);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
                return false;
            }
        }

        private async Task<bool> CheckIfCorrectChannel(SocketSlashCommand command, string channelName)
        {
            try
            {
                if (command.Channel.Name == channelName || command.Channel.Name == "testing") return true;
                await command.FollowupAsync("Use in #" + channelName + " channel only!", ephemeral: true);
                return false;
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
                return false;
            }
        }

        private async Task<bool> CheckForTimeBetweenCommands(SocketSlashCommand command)
        {
            try
            {
                var userInfo = command.User as SocketGuildUser;
                if (Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).ContainsAccount(userInfo.Id))
                {
                    if ((DateTime.UtcNow - Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).LastCommandTime).TotalSeconds < Settings.MinCommandTime)
                    {
                        await command.FollowupAsync("Not enough time between commands. Try again!", ephemeral: true);
                        return false;
                    }

                    Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).LastCommandTime = DateTime.UtcNow;
                    Database.IsDirty = true;
                }
                else
                {
                    var account = Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).AddAccount(userInfo.Id);
                    account.LastCommandTime = DateTime.UtcNow;
                    Database.IsDirty = true;
                }

                return true;
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
                return false;
            }
        }

        private async Task<bool> CheckForTimeBetweenFaucet(SocketSlashCommand command)
        {
            try
            {
                var userInfo = command.User as SocketGuildUser;
                if (Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).ContainsAccount(userInfo.Id))
                {
                    if ((DateTime.UtcNow - Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).LastFaucetTime).TotalHours < Settings.MinFaucetTime)
                    {
                        var nextFaucetTime = Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).LastFaucetTime.AddHours(Settings.MinFaucetTime) - DateTime.UtcNow;
                        string formattedTimeSpan = nextFaucetTime.ToString(@"hh\:mm\:ss");
                        string timeSeparator = CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator;
                        formattedTimeSpan = formattedTimeSpan.Replace(":", timeSeparator);
                        await command.FollowupAsync("Faucet is available once every 24 hours. Please try again in " + formattedTimeSpan + "!", ephemeral: true);
                        return false;
                    }

                    Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).LastCommandTime = DateTime.UtcNow;
                    Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).LastFaucetTime = DateTime.UtcNow;
                    Database.IsDirty = true;
                }
                else
                {
                    var account = Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).AddAccount(userInfo.Id);
                    account.LastCommandTime = DateTime.UtcNow;
                    account.LastFaucetTime = DateTime.UtcNow;
                    Database.IsDirty = true;
                }

                return true;
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
                return false;
            }
        }

        private async Task<bool> CheckForTimeBetweenSwapOrTip(SocketSlashCommand command)
        {
            try
            {
                var userInfo = command.User as SocketGuildUser;
                if (Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).ContainsAccount(userInfo.Id))
                {
                    if ((DateTime.UtcNow - Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).LastTipTime).TotalSeconds < Settings.MinTipTime)
                    {
                        await command.FollowupAsync("Swapping/tipping is available once every minute. Please try again later!", ephemeral: true);
                        return false;
                    }

                    Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).LastCommandTime = DateTime.UtcNow;
                    Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(userInfo.Id).LastTipTime = DateTime.UtcNow;
                    Database.IsDirty = true;
                }
                else
                {
                    var account = Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).AddAccount(userInfo.Id);
                    account.LastCommandTime = DateTime.UtcNow;
                    account.LastTipTime = DateTime.UtcNow;
                    Database.IsDirty = true;
                }

                return true;
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
                return false;
            }
        }
    }
}
