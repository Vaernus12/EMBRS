using Discord;
using Discord.Net;
using Discord.WebSocket;
using EMBRS_Discord;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XUMM.NET.SDK.EMBRS;

namespace EMBRS
{
    public class Program
    {
        private readonly Commands _commands;
        private readonly DiscordSocketClient _discordClient;
        private readonly XummWebSocket _webSocketClient;
        private readonly XummMiscAppStorageClient _appStorageClient;
        private readonly XummMiscClient _miscClient;
        private readonly XummPayloadClient _payloadClient;
        private readonly XummHttpClient _httpClient;

        private readonly bool _updateCommands = false;

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
        }

        private async Task MainAsync()
        {
            await Database.Initialize();
            _discordClient.Ready += HandleClientReadyAsync;
            _discordClient.SlashCommandExecuted += _commands.HandleSlashCommandAsync;
            await _discordClient.LoginAsync(TokenType.Bot, Settings.BotToken);
            await _discordClient.StartAsync();
            await Task.Delay(Timeout.Infinite);
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

                if (!_updateCommands && commands.Any(r => r.Name == "register")) await Log(new LogMessage(LogSeverity.Info, "Command Loaded", "Register"));
                else
                {
                    var registerCommand = new SlashCommandBuilder()
                        .WithName("register")
                        .WithDescription("Register with EMBRS bot.")
                        .AddOption("xrpaddress", ApplicationCommandOptionType.String, "The XRP address", isRequired: true);
                    await guild.CreateApplicationCommandAsync(registerCommand.Build());
                }

                if (!_updateCommands && commands.Any(r => r.Name == "reward")) await Log(new LogMessage(LogSeverity.Info, "Command Loaded", "Reward"));
                else
                {
                    var rewardCommand = new SlashCommandBuilder()
                        .WithName("reward")
                        .WithDescription("Receive Emberlight tournament reward.");
                    await guild.CreateApplicationCommandAsync(rewardCommand.Build());
                }

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
                        .AddOption("achievement", ApplicationCommandOptionType.String, "The week's tournament goal", isRequired: true);
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
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                await Log(new LogMessage(LogSeverity.Error, exception.Source, json, exception));
            }
        }
    }
}
