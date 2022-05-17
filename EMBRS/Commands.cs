using Discord;
using Discord.WebSocket;
using EMBRS;
using Newtonsoft.Json;
using RippleDotNet;
using RippleDotNet.Json.Converters;
using RippleDotNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            switch (command.Data.Name)
            {
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
                case "register":
                    {
                        await Program.Log(new LogMessage(LogSeverity.Info, "Command", "register"));
                        var registerTask = Task.Run(async () =>
                        {
                            await HandleRegisterCommand(command);
                        });
                        break;
                    }
                case "reward":
                    {
                        await Program.Log(new LogMessage(LogSeverity.Info, "Command", "reward"));
                        var rewardTask = Task.Run(async () =>
                        {
                            await HandleRewardCommand(command);
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
            }
        }

        private async Task HandleEndCommand(SocketSlashCommand command)
        {
            try
            {
                var userInfo = command.User;
                if (Database.RegisteredUsers.ContainsKey(userInfo.Id))
                {
                    if ((DateTime.UtcNow - Database.RegisteredUsers[userInfo.Id].LastCommandTime).TotalSeconds < Settings.MinCommandTime)
                    {
                        await command.RespondAsync("Not enough time between commands. Try again!", ephemeral: true);
                        return;
                    }

                    Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                    await Database.Write();
                }
                else
                {
                    var newAccount = new Account(userInfo.Id, string.Empty);
                    if (Database.RegisteredUsers.TryAdd(userInfo.Id, newAccount))
                    {
                        Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                        await Database.Write();
                    }
                    else
                    {
                        await command.RespondAsync("End command failed. Try again!", ephemeral: true);
                        return;
                    }
                }

                if ((userInfo as SocketGuildUser).Roles.Any(r => r.Name == "Leads"))
                {
                    if (command.Channel.Name == "tournament" || command.Channel.Name == "testing")
                    {
                        await command.RespondAsync("Tournament ending.", ephemeral: true);

                        var guild = _discordClient.GetGuild(ulong.Parse(Settings.GuildID));
                        var tournamentRole = guild.Roles.FirstOrDefault(x => x.Name == "Tournament");
                        var winnerRole = guild.Roles.FirstOrDefault(x => x.Name == "Tournament Winner");
                        var roles = new List<SocketRole>() { tournamentRole, winnerRole };

                        var users = await command.Channel.GetUsersAsync().FlattenAsync<IUser>();
                        foreach (IGuildUser user in users)
                        {
                            await user.RemoveRolesAsync(roles);
                        }

                        await command.FollowupAsync("Tournament ended.", ephemeral: true);

                        foreach (var user in Database.RegisteredUsers)
                        {
                            user.Value.TournamentWinner = false;
                            user.Value.ReceivedTournamentReward = false;
                            user.Value.InTournament = false;
                        }

                        var message = (string)command.Data.Options.First().Value;
                        await command.FollowupAsync("Thank you to everyone that participated in the week " + message + " tournament! Sign-ups for week " + (int.Parse(message) + 1).ToString() + " will start momentarily. Check #announcements for details in a few.");

                        await Database.Write();
                    }
                    else
                    {
                        await command.RespondAsync("Use in #tournament channel only!", ephemeral: true);
                    }
                }
                else
                {
                    await command.RespondAsync("Admin-only command!", ephemeral: true);
                }
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
                var userInfo = command.User as SocketGuildUser;
                if (Database.RegisteredUsers.ContainsKey(userInfo.Id))
                {
                    if ((DateTime.UtcNow - Database.RegisteredUsers[userInfo.Id].LastFaucetTime).TotalHours < Settings.MinFaucetTime)
                    {
                        await command.RespondAsync("Faucet is available once every 24 hours. Please try again later!", ephemeral: true);
                        return;
                    }

                    Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                    Database.RegisteredUsers[userInfo.Id].LastFaucetTime = DateTime.UtcNow;
                    await Database.Write();
                }
                else
                {
                    var newAccount = new Account(userInfo.Id, string.Empty);
                    if (Database.RegisteredUsers.TryAdd(userInfo.Id, newAccount))
                    {
                        Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                        Database.RegisteredUsers[userInfo.Id].LastFaucetTime = DateTime.UtcNow;
                        await Database.Write();
                    }
                    else
                    {
                        await command.RespondAsync("Reward command failed. Try again!", ephemeral: true);
                        return;
                    }
                }

                if (command.Channel.Name == "bot-commands" || command.Channel.Name == "testing")
                {
                    await command.RespondAsync("Beginning faucet process!", ephemeral: true);
                    await XRPL.SendRewardAsync(command, null, userInfo, Settings.FaucetTokenAmt, false, false, true);
                    Database.RegisteredUsers[userInfo.Id].EMBRSEarned += float.Parse(Settings.FaucetTokenAmt);
                    await Database.Write();
                }
                else
                {
                    await command.RespondAsync("Use in #bot-commands channel only!", ephemeral: true);
                }
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
                var userInfo = command.User;
                if (Database.RegisteredUsers.ContainsKey(userInfo.Id))
                {
                    if ((DateTime.UtcNow - Database.RegisteredUsers[userInfo.Id].LastCommandTime).TotalSeconds < Settings.MinCommandTime)
                    {
                        await command.RespondAsync("Not enough time between commands. Try again!", ephemeral: true);
                        return;
                    }

                    Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                    await Database.Write();
                }
                else
                {
                    var newAccount = new Account(userInfo.Id, string.Empty);
                    if (Database.RegisteredUsers.TryAdd(userInfo.Id, newAccount))
                    {
                        Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                        await Database.Write();
                    }
                    else
                    {
                        await command.RespondAsync("Help command failed. Try again!", ephemeral: true);
                        return;
                    }
                }

                if (command.Channel.Name == "bot-commands" || command.Channel.Name == "testing")
                {
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
                        .AddField("/reward", "When winning Emberlight tournament, get your EMBRS reward (#winners)")
                        .AddField("/status", "Check status of your XRP address, balances, earned EMBRS, etc. in EMBRS bot (#bot-commands)")
                        .AddField("/swap <from> <to> <amount>", swapStringBuilder.ToString())
                        .AddField("/tip <recipient> <amount>", tipStringBuilder.ToString())
                        .AddField("/tournament", "Sign-up for the current week's Emberlight tournament (#bot-commands)")
                        .AddField("/unregister", "Unregister from the EMBRS bot (#bot-commands)");

                    await command.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
                }
                else
                {
                    await command.RespondAsync("Use in #bot-commands channel only!", ephemeral: true);
                }
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
                var userInfo = command.User;
                if (Database.RegisteredUsers.ContainsKey(userInfo.Id))
                {
                    if ((DateTime.UtcNow - Database.RegisteredUsers[userInfo.Id].LastCommandTime).TotalSeconds < Settings.MinCommandTime)
                    {
                        await command.RespondAsync("Not enough time between commands. Try again!", ephemeral: true);
                        return;
                    }

                    Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                    await Database.Write();
                }
                else
                {
                    var newAccount = new Account(userInfo.Id, string.Empty);
                    if (Database.RegisteredUsers.TryAdd(userInfo.Id, newAccount))
                    {
                        Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                        await Database.Write();
                    }
                    else
                    {
                        await command.RespondAsync("Register command failed. Try again!", ephemeral: true);
                        return;
                    }
                }

                if (command.Channel.Name == "bot-commands" || command.Channel.Name == "testing")
                {
                    var xrpAddress = (string)command.Data.Options.First().Value;
                    if (!Database.RegisteredUsers[userInfo.Id].IsRegistered)
                    {
                        Database.RegisteredUsers[userInfo.Id].XrpAddress = xrpAddress;
                        Database.RegisteredUsers[userInfo.Id].IsRegistered = true;
                        await command.RespondAsync("You are registered with EMBRS bot!", ephemeral: true);
                        await Database.Write();
                    }
                    else
                    {
                        Database.RegisteredUsers[userInfo.Id].XrpAddress = xrpAddress;
                        await command.RespondAsync("You updated your XRP address in EMBRS bot!", ephemeral: true);
                        await Database.Write();
                    }
                }
                else
                {
                    await command.RespondAsync("Use in #bot-commands channel only!", ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private async Task HandleRewardCommand(SocketSlashCommand command)
        {
            try
            {
                var userInfo = command.User as SocketGuildUser;
                if (Database.RegisteredUsers.ContainsKey(userInfo.Id))
                {
                    if ((DateTime.UtcNow - Database.RegisteredUsers[userInfo.Id].LastCommandTime).TotalSeconds < Settings.MinCommandTime)
                    {
                        await command.RespondAsync("Not enough time between commands. Try again!", ephemeral: true);
                        return;
                    }

                    Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                    await Database.Write();
                }
                else
                {
                    var newAccount = new Account(userInfo.Id, string.Empty);
                    if (Database.RegisteredUsers.TryAdd(userInfo.Id, newAccount))
                    {
                        Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                        await Database.Write();
                    }
                    else
                    {
                        await command.RespondAsync("Reward command failed. Try again!", ephemeral: true);
                        return;
                    }
                }

                if (command.Channel.Name == "winners" || command.Channel.Name == "testing")
                {
                    if (userInfo.Roles.Any(r => r.Name == "Tournament Winner"))
                    {
                        if (!Database.RegisteredUsers[userInfo.Id].ReceivedTournamentReward)
                        {
                            Database.RegisteredUsers[userInfo.Id].ReceivedTournamentReward = true;
                            await command.RespondAsync("Beginning rewards process!", ephemeral: true);
                            await XRPL.SendRewardAsync(command, null, userInfo, Settings.RewardTokenAmt, true, false, false);
                            Database.RegisteredUsers[userInfo.Id].EMBRSEarned += float.Parse(Settings.RewardTokenAmt);
                            await Database.Write();
                        }
                        else
                        {
                            await command.RespondAsync("You have already received reward!", ephemeral: true);
                        }
                    }
                    else
                    {
                        await command.RespondAsync("You do not have the Tournament Winner role!", ephemeral: true);
                    }
                }
                else
                {
                    await command.RespondAsync("Use in #winners channel only!", ephemeral: true);
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
                var userInfo = command.User;
                if (Database.RegisteredUsers.ContainsKey(userInfo.Id))
                {
                    if ((DateTime.UtcNow - Database.RegisteredUsers[userInfo.Id].LastCommandTime).TotalSeconds < Settings.MinCommandTime)
                    {
                        await command.RespondAsync("Not enough time between commands. Try again!", ephemeral: true);
                        return;
                    }

                    Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                    await Database.Write();
                }
                else
                {
                    var newAccount = new Account(userInfo.Id, string.Empty);
                    if (Database.RegisteredUsers.TryAdd(userInfo.Id, newAccount))
                    {
                        Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                        await Database.Write();
                    }
                    else
                    {
                        await command.RespondAsync("Select command failed. Try again!", ephemeral: true);
                        return;
                    }
                }

                if ((userInfo as SocketGuildUser).Roles.Any(r => r.Name == "Leads"))
                {
                    if (command.Channel.Name == "winners" || command.Channel.Name == "testing")
                    {
                        await command.RespondAsync("Handling select winner command!", ephemeral: true);

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
                        stringBuilder.Append("Random tournament winners selected are:");
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine();

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
                                    stringBuilder.Append($" - 1000 EMBRS and early access slot to Emberlight: Rekindled!");
                                    await XRPL.SendRewardAsync(command, null, user, "1000");
                                    Database.RegisteredUsers[user.Id].EMBRSEarned += 100;
                                    await user.AddRoleAsync(earlyAccessRole);
                                    await Database.Write();
                                }
                                else
                                {
                                    stringBuilder.Append($" - Early access slot to Emberlight: Rekindled!");
                                    await user.AddRoleAsync(earlyAccessRole);
                                }
                                stringBuilder.AppendLine();
                            }
                        }

                        stringBuilder.AppendLine();
                        stringBuilder.Append("Congratulations!");
                        await command.FollowupAsync(stringBuilder.ToString());

                    }
                    else
                    {
                        await command.RespondAsync("Use in #winners channel only!", ephemeral: true);
                    }
                }
                else
                {
                    await command.RespondAsync("Admin-only command!", ephemeral: true);
                }
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
                var userInfo = command.User;
                if (Database.RegisteredUsers.ContainsKey(userInfo.Id))
                {
                    if ((DateTime.UtcNow - Database.RegisteredUsers[userInfo.Id].LastCommandTime).TotalSeconds < Settings.MinCommandTime)
                    {
                        await command.RespondAsync("Not enough time between commands. Try again!", ephemeral: true);
                        return;
                    }

                    Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                    await Database.Write();
                }
                else
                {
                    var newAccount = new Account(userInfo.Id, string.Empty);
                    if (Database.RegisteredUsers.TryAdd(userInfo.Id, newAccount))
                    {
                        Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                        await Database.Write();
                    }
                    else
                    {
                        await command.RespondAsync("Set winner command failed. Try again!", ephemeral: true);
                        return;
                    }
                }

                if ((userInfo as SocketGuildUser).Roles.Any(r => r.Name == "Leads"))
                {
                    if (command.Channel.Name == "verify" || command.Channel.Name == "testing")
                    {
                        var guildUser = (SocketGuildUser)command.Data.Options.First().Value;
                        if (!Database.RegisteredUsers.ContainsKey(guildUser.Id) || !Database.RegisteredUsers[guildUser.Id].IsRegistered)
                        {
                            await command.RespondAsync($"{guildUser.Username}#{guildUser.Discriminator} is not registered with EMBRS!");
                        }
                        else
                        {
                            var guild = _discordClient.GetGuild(ulong.Parse(Settings.GuildID));
                            var winnerRole = guild.Roles.FirstOrDefault(x => x.Name == "Tournament Winner");
                            await guildUser.AddRoleAsync(winnerRole);
                            await command.RespondAsync($"A winner is {guildUser.Username}#{guildUser.Discriminator}!");
                            Database.RegisteredUsers[guildUser.Id].TournamentWinner = true;
                            await Database.Write();
                        }
                    }
                    else
                    {
                        await command.RespondAsync("Use in #verify channel only!", ephemeral: true);
                    }
                }
                else
                {
                    await command.RespondAsync("Admin-only command!", ephemeral: true);
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
                var userInfo = command.User;
                if (Database.RegisteredUsers.ContainsKey(userInfo.Id))
                {
                    if ((DateTime.UtcNow - Database.RegisteredUsers[userInfo.Id].LastCommandTime).TotalSeconds < Settings.MinCommandTime)
                    {
                        await command.RespondAsync("Not enough time between commands. Try again!", ephemeral: true);
                        return;
                    }

                    Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                    await Database.Write();
                }
                else
                {
                    var newAccount = new Account(userInfo.Id, string.Empty);
                    if (Database.RegisteredUsers.TryAdd(userInfo.Id, newAccount))
                    {
                        Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                        await Database.Write();
                    }
                    else
                    {
                        await command.RespondAsync("Start command failed. Try again!", ephemeral: true);
                        return;
                    }
                }

                if ((userInfo as SocketGuildUser).Roles.Any(r => r.Name == "Leads"))
                {
                    if (command.Channel.Name == "tournament" || command.Channel.Name == "testing")
                    {
                        var achievement = (string)command.Data.Options.First().Value;

                        var stringBuilder = new StringBuilder();
                        stringBuilder.Append("The tournament has started! This week's achievement is: ");
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine();
                        stringBuilder.Append("**" + achievement + "**");
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine();
                        stringBuilder.Append("Good luck!");
                        await command.RespondAsync(stringBuilder.ToString());
                    }
                    else
                    {
                        await command.RespondAsync($"Use in #tournament channel only!", ephemeral: true);
                    }
                }
                else
                {
                    await command.RespondAsync($"Admin-only command!", ephemeral: true);
                }
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
                var userInfo = command.User as SocketGuildUser;
                if (Database.RegisteredUsers.ContainsKey(userInfo.Id))
                {
                    if ((DateTime.UtcNow - Database.RegisteredUsers[userInfo.Id].LastCommandTime).TotalSeconds < Settings.MinCommandTime)
                    {
                        await command.RespondAsync("Not enough time between commands. Try again!", ephemeral: true);
                        return;
                    }

                    Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                    await Database.Write();
                }
                else
                {
                    var newAccount = new Account(userInfo.Id, string.Empty);
                    if (Database.RegisteredUsers.TryAdd(userInfo.Id, newAccount))
                    {
                        Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                        await Database.Write();
                    }
                    else
                    {
                        await command.RespondAsync("Status command failed. Try again!", ephemeral: true);
                        return;
                    }
                }

                if (command.Channel.Name == "bot-commands" || command.Channel.Name == "testing")
                {
                    await command.RespondAsync("Getting EMBRS status...", ephemeral: true);

                    if (Database.RegisteredUsers[userInfo.Id].IsRegistered)
                    {
                        var xrp = 0.0m;
                        var embrs = 0.0m;
                        var stx = 0.0m;
                        var usd = 0.0m;

                        IRippleClient client = new RippleClient(Settings.WebSocketUrl);
                        client.Connect();
                        {
                            xrp = await XRPL.ReturnAccountBalance(client, Database.RegisteredUsers[userInfo.Id].XrpAddress);
                            Thread.Sleep(Settings.AccountLinesThrottle * 1000);

                            string marker = "";
                            do
                            {
                                var returnObj = await XRPL.ReturnTrustLines(client, Database.RegisteredUsers[userInfo.Id].XrpAddress, marker);
                                if (embrs == 0.0m) embrs = returnObj.EMBRSBalance;
                                if (stx == 0.0m) stx = returnObj.STXBalance;
                                if (usd == 0.0m) usd = returnObj.USDBalance;
                                marker = returnObj.Marker;
                                Thread.Sleep(Settings.AccountLinesThrottle * 1000);
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
                            .WithTitle(Database.RegisteredUsers[userInfo.Id].XrpAddress)
                            .WithColor(Color.Orange)
                            .AddField("Balances", stringBuilder.ToString())
                            .AddField("EMBRS Earned", Database.RegisteredUsers[userInfo.Id].EMBRSEarned)
                            .AddField("In Tournament", Database.RegisteredUsers[userInfo.Id].InTournament)
                            .AddField("Won Tournament", Database.RegisteredUsers[userInfo.Id].TournamentWinner)
                            .AddField("Received Tournament Reward", Database.RegisteredUsers[userInfo.Id].ReceivedTournamentReward);

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
                else
                {
                    await command.RespondAsync($"Use in #bot-commands channel only!", ephemeral: true);
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
                var userInfo = command.User;
                if (Database.RegisteredUsers.ContainsKey(userInfo.Id))
                {
                    if ((DateTime.UtcNow - Database.RegisteredUsers[userInfo.Id].LastTipTime).TotalSeconds < Settings.MinTipTime)
                    {
                        await command.RespondAsync("Swapping is available once every minute. Please try again later!", ephemeral: true);
                        return;
                    }

                    Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                    Database.RegisteredUsers[userInfo.Id].LastTipTime = DateTime.UtcNow;
                    await Database.Write();
                }
                else
                {
                    var newAccount = new Account(userInfo.Id, string.Empty);
                    if (Database.RegisteredUsers.TryAdd(userInfo.Id, newAccount))
                    {
                        Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                        Database.RegisteredUsers[userInfo.Id].LastTipTime = DateTime.UtcNow;
                        await Database.Write();
                    }
                    else
                    {
                        await command.RespondAsync("Swap command failed. Try again!", ephemeral: true);
                        return;
                    }
                }

                if (command.Channel.Name == "bot-commands" || command.Channel.Name == "testing")
                {
                    var from = (string)command.Data.Options.SingleOrDefault(r => r.Name == "from").Value;
                    var to = (string)command.Data.Options.SingleOrDefault(r => r.Name == "to").Value;
                    var amount = (double)command.Data.Options.SingleOrDefault(r => r.Name == "amount").Value;

                    if (Database.RegisteredUsers[userInfo.Id].IsRegistered)
                    {
                        await command.RespondAsync($"Starting swap...", ephemeral: true);

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
                                        await Database.Write();
                                        break;
                                    }
                                    else if (getPayload.Meta.Resolved && !getPayload.Meta.Signed)
                                    {
                                        await command.FollowupAsync($"Swap was cancelled by user", ephemeral: true);
                                        break;
                                    }

                                    Thread.Sleep(Settings.TxnThrottle * 3000);
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
                                        await Database.Write();
                                        break;
                                    }
                                    else if (getPayload.Meta.Resolved && !getPayload.Meta.Signed)
                                    {
                                        await command.FollowupAsync($"Swap was cancelled by user", ephemeral: true);
                                        break;
                                    }

                                    Thread.Sleep(Settings.TxnThrottle * 3000);
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
                    else
                    {
                        await command.RespondAsync("You are not registered for swaps!", ephemeral: true);
                    }
                }
                else
                {
                    await command.RespondAsync("Use in #bot-commands channel only!", ephemeral: true);
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
                var userInfo = command.User;
                if (Database.RegisteredUsers.ContainsKey(userInfo.Id))
                {
                    if ((DateTime.UtcNow - Database.RegisteredUsers[userInfo.Id].LastTipTime).TotalSeconds < Settings.MinTipTime)
                    {
                        await command.RespondAsync("Tipping is available once every minute. Please try again later!", ephemeral: true);
                        return;
                    }

                    Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                    Database.RegisteredUsers[userInfo.Id].LastTipTime = DateTime.UtcNow;
                    await Database.Write();
                }
                else
                {
                    var newAccount = new Account(userInfo.Id, string.Empty);
                    if (Database.RegisteredUsers.TryAdd(userInfo.Id, newAccount))
                    {
                        Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                        Database.RegisteredUsers[userInfo.Id].LastTipTime = DateTime.UtcNow;
                        await Database.Write();
                    }
                    else
                    {
                        await command.RespondAsync("Tip command failed. Try again!", ephemeral: true);
                        return;
                    }
                }

                if (command.Channel.Name == "lounge" || command.Channel.Name == "testing")
                {
                    var user = (SocketGuildUser)command.Data.Options.SingleOrDefault(r => r.Name == "user").Value;
                    var amount = (double)command.Data.Options.SingleOrDefault(r => r.Name == "amount").Value;

                    if (Database.RegisteredUsers[userInfo.Id].IsRegistered && Database.RegisteredUsers[user.Id].IsRegistered)
                    {
                        var tipAmount = string.Empty;
                        if ((userInfo as SocketGuildUser).Roles.Any(r => r.Name == "Leads"))
                        {
                            tipAmount = Math.Min(amount, float.Parse(Settings.MaxTipTokenAmt)).ToString();
                            await command.RespondAsync($"You are tipping {tipAmount} EMBRS to {user.Username}#{user.Discriminator}!", ephemeral: true);
                            await XRPL.SendRewardAsync(command, userInfo, user, tipAmount, false, true, false);
                            Database.RegisteredUsers[user.Id].EMBRSEarned += float.Parse(tipAmount);
                            await Database.Write();
                        }
                        else
                        {
                            await command.RespondAsync($"You are tipping {amount} EMBRS to {user.Username}#{user.Discriminator}!", ephemeral: true);

                            var destination = Database.RegisteredUsers[user.Id].XrpAddress;
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
                                        Database.RegisteredUsers[user.Id].EMBRSEarned += (float)amount;
                                        await Database.Write();
                                        break;
                                    }
                                    else if (getPayload.Meta.Resolved && !getPayload.Meta.Signed)
                                    {
                                        await command.FollowupAsync($"Tip was cancelled by user", ephemeral: true);
                                        break;
                                    }

                                    Thread.Sleep(Settings.TxnThrottle * 3000);
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
                                        Database.RegisteredUsers[user.Id].EMBRSEarned += (float)amount;
                                        await Database.Write();
                                        break;
                                    }
                                    else if (getPayload.Meta.Resolved && !getPayload.Meta.Signed)
                                    {
                                        await command.FollowupAsync($"Tip was cancelled by user", ephemeral: true);
                                        break;
                                    }

                                    Thread.Sleep(Settings.TxnThrottle * 3000);
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
                        await command.RespondAsync("User(s) are not registered for tips!", ephemeral: true);
                    }
                }
                else
                {
                    await command.RespondAsync("Use in #lounge channel only!", ephemeral: true);
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
                var userInfo = command.User as SocketGuildUser;
                if (Database.RegisteredUsers.ContainsKey(userInfo.Id))
                {
                    if ((DateTime.UtcNow - Database.RegisteredUsers[userInfo.Id].LastCommandTime).TotalSeconds < Settings.MinCommandTime)
                    {
                        await command.RespondAsync("Not enough time between commands. Try again!", ephemeral: true);
                        return;
                    }

                    Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                    await Database.Write();
                }
                else
                {
                    var newAccount = new Account(userInfo.Id, string.Empty);
                    if (Database.RegisteredUsers.TryAdd(userInfo.Id, newAccount))
                    {
                        Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                        await Database.Write();
                    }
                    else
                    {
                        await command.RespondAsync("Tournament command failed. Try again!", ephemeral: true);
                        return;
                    }
                }

                if (command.Channel.Name == "bot-commands" || command.Channel.Name == "testing")
                {
                    if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Tuesday || DateTime.UtcNow.DayOfWeek == DayOfWeek.Wednesday || DateTime.UtcNow.DayOfWeek == DayOfWeek.Thursday)
                    {
                        if (!Database.RegisteredUsers[userInfo.Id].IsRegistered)
                        {
                            await command.RespondAsync("You need to /register with EMBRS bot to join tournament!", ephemeral: true);
                        }
                        else
                        {
                            var guild = _discordClient.GetGuild(ulong.Parse(Settings.GuildID));
                            var tournamentRole = guild.Roles.FirstOrDefault(x => x.Name == "Tournament");
                            await (userInfo as SocketGuildUser).AddRoleAsync(tournamentRole);
                            await command.RespondAsync("You are signed-up for this week's tournament! Check #tournament for more details.", ephemeral: true);
                            Database.RegisteredUsers[userInfo.Id].InTournament = true;
                            await Database.Write();
                        }
                    }
                    else
                    {
                        await command.RespondAsync("Tournament sign-ups for this week are closed. Next week's sign-ups will start on Tuesday!", ephemeral: true);
                    }
                }
                else
                {
                    await command.RespondAsync("Use in #bot-commands channel only!", ephemeral: true);
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
                var userInfo = command.User as SocketGuildUser;
                if (Database.RegisteredUsers.ContainsKey(userInfo.Id))
                {
                    if ((DateTime.UtcNow - Database.RegisteredUsers[userInfo.Id].LastCommandTime).TotalSeconds < Settings.MinCommandTime)
                    {
                        await command.RespondAsync("Not enough time between commands. Try again!", ephemeral: true);
                        return;
                    }

                    Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                    await Database.Write();
                }
                else
                {
                    var newAccount = new Account(userInfo.Id, string.Empty);
                    if (Database.RegisteredUsers.TryAdd(userInfo.Id, newAccount))
                    {
                        Database.RegisteredUsers[userInfo.Id].LastCommandTime = DateTime.UtcNow;
                        await Database.Write();
                    }
                    else
                    {
                        await command.RespondAsync("Unregister command failed. Try again!", ephemeral: true);
                        return;
                    }
                }

                if (command.Channel.Name == "bot-commands" || command.Channel.Name == "testing")
                {
                    if (!Database.RegisteredUsers[userInfo.Id].IsRegistered)
                    {
                        await command.RespondAsync("You are currently not registered with EMBRS bot!", ephemeral: true);
                    }
                    else
                    {
                        await command.RespondAsync("You are no longer registered with EMBRS bot!", ephemeral: true);

                        Database.RegisteredUsers[userInfo.Id].IsRegistered = false;
                        Database.RegisteredUsers[userInfo.Id].XrpAddress = string.Empty;
                        Database.RegisteredUsers[userInfo.Id].EMBRSEarned = 0;

                        var guild = _discordClient.GetGuild(ulong.Parse(Settings.GuildID));
                        var tournamentRole = guild.Roles.FirstOrDefault(x => x.Name == "Tournament");
                        var winnerRole = guild.Roles.FirstOrDefault(x => x.Name == "Tournament Winner");
                        var roles = new List<SocketRole>() { tournamentRole, winnerRole };
                        await userInfo.RemoveRolesAsync(roles);
                        Database.RegisteredUsers[userInfo.Id].TournamentWinner = false;
                        Database.RegisteredUsers[userInfo.Id].ReceivedTournamentReward = false;
                        Database.RegisteredUsers[userInfo.Id].InTournament = false;

                        await Database.Write();
                    }
                }
                else
                {
                    await command.RespondAsync("Use in #bot-commands channel only!", ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }
    }
}
