﻿using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMBRS
{
    [Serializable]
    public class DatabaseTournament : DatabaseBase
    {
        [JsonProperty] private TournamentStatus _tournamentStatus;
        [JsonProperty] private uint _tournamentWeek;
        [JsonProperty] private string _tournamentAchievement;
        [JsonProperty] private List<ulong> _tournamentParticipants;
        [JsonProperty] private List<ulong> _tournamentWinners;
        [JsonProperty] private TournamentReward _tournamentReward;
        [JsonProperty] private List<TournamentSponsor> _tournamentSponsors;

        public DatabaseTournament()
        {
            Type = DatabaseType.Tournament;
            _tournamentStatus = TournamentStatus.SignUp;
            _tournamentWeek = 4;
            _tournamentAchievement = "";
            _tournamentParticipants = new List<ulong>();
            _tournamentWinners = new List<ulong>();
            _tournamentReward = null;
            _tournamentSponsors = new List<TournamentSponsor>();
        }

        public TournamentStatus GetTournamentStatus()
        {
            return _tournamentStatus;
        }

        public void SetTournamentStatus(TournamentStatus status)
        {
            _tournamentStatus = status;
        }

        public uint GetTournamentWeek()
        {
            return _tournamentWeek;
        }

        public void IncrementTournamentWeek()
        {
            _tournamentWeek++;
        }

        public string GetTournamentAchievement()
        {
            return _tournamentAchievement == string.Empty ? "None" : _tournamentAchievement;
        }

        public void SetTournamentAchievement(string achievement)
        {
            _tournamentAchievement = achievement;
        }

        public string GetTournamentParticipants(DiscordSocketClient client)
        {
            var stringBuilder = new StringBuilder();
            var guild = client.GetGuild(ulong.Parse(Settings.GuildID));

            foreach (var participant in _tournamentParticipants)
            {
                var guildUser = guild.GetUser(participant);
                stringBuilder.Append($"{guildUser.Username}#{guildUser.Discriminator}");
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        public async Task AddTournamentParticipant(DiscordSocketClient client, Account participant)
        {
            _tournamentParticipants.Add(participant.GetId());
            participant.SetInTournament(true);
            var guild = client.GetGuild(ulong.Parse(Settings.GuildID));
            var guildUser = guild.GetUser(participant.GetId());
            var tournamentRole = guild.Roles.FirstOrDefault(x => x.Name == "Tournament");
            await guildUser.AddRoleAsync(tournamentRole);
            Database.IsDirty = true;
        }

        public string GetTournamentWinners(DiscordSocketClient client)
        {
            var stringBuilder = new StringBuilder();
            var guild = client.GetGuild(ulong.Parse(Settings.GuildID));

            foreach (var winner in _tournamentWinners)
            {
                var guildUser = guild.GetUser(winner);
                stringBuilder.Append($"{guildUser.Username}#{guildUser.Discriminator}");
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        public async Task AddTournamentWinner(DiscordSocketClient client, Account winner)
        {
            _tournamentWinners.Add(winner.GetId());
            winner.SetTournamentWinner(true);
            var guild = client.GetGuild(ulong.Parse(Settings.GuildID));
            var guildUser = guild.GetUser(winner.GetId());
            var winnerRole = guild.Roles.FirstOrDefault(x => x.Name == "Tournament Winner");
            await guildUser.AddRoleAsync(winnerRole);
            Database.IsDirty = true;
        }

        public string GetTournamentReward()
        {
            return _tournamentReward.GetRewardString();
        }

        public void SetTournamentReward(string topReward, string nextReward, string normalReward)
        {
            _tournamentReward = new TournamentReward(topReward, nextReward, normalReward);
        }

        public List<TournamentSponsor> GetTournamentSponsors()
        {
            return _tournamentSponsors;
        }

        public void AddTournamentSponsor(TournamentSponsor sponsor)
        {
            _tournamentSponsors.Add(sponsor);
        }

        public async Task StartTournament(DiscordSocketClient client)
        {
            var guild = client.GetGuild(ulong.Parse(Settings.GuildID));
            var tournamentChannel = guild.TextChannels.FirstOrDefault(x => x.Name == "tournament");
            var embedBuilder = new EmbedBuilder()
                .WithAuthor(client.CurrentUser.ToString(), client.CurrentUser.GetAvatarUrl() ?? client.CurrentUser.GetDefaultAvatarUrl())
                .WithDescription("The Emberlight tournament has started!")
                .WithCurrentTimestamp()
                .WithColor(Color.Orange)
                .AddField("Tournament Achievement", _tournamentAchievement);

            await tournamentChannel.SendMessageAsync(null, false, embedBuilder.Build());
        }

        public async Task HandleRewards(DiscordSocketClient client)
        {
            var guild = client.GetGuild(ulong.Parse(Settings.GuildID));
            var amount = (Int64)3;
            var winnersChannel = guild.TextChannels.FirstOrDefault(x => x.Name == "winners") as ISocketMessageChannel;
            var usersList = _tournamentWinners;
            var rng = new System.Random();
            var earlyAccessRole = guild.Roles.FirstOrDefault(x => x.Name == "Council");

            var stringBuilder = new StringBuilder();

            if (usersList.Count > 0)
            {
                for (int i = 0; i < amount; i++)
                {
                    var index = rng.Next(0, usersList.Count);
                    var user = guild.GetUser(usersList[index]) as SocketGuildUser;
                    if (usersList.Count > 1) usersList.RemoveAt(index);

                    stringBuilder.Append($"@{user.Username}#{user.Discriminator}");
                    if (i == 0)
                    {
                        stringBuilder.Append($" - 1100 EMBRS and early access slot to Emberlight: Rekindled!");
                        await XRPL.SendRewardAsync(client, null, null, user, "1100");
                        Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).ModEMBRSEarned(1100);
                        await user.AddRoleAsync(earlyAccessRole);
                    }
                    else
                    {
                        stringBuilder.Append($" - 600 EMBRS and early access slot to Emberlight: Rekindled!");
                        await XRPL.SendRewardAsync(client, null, null, user, "600");
                        Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).ModEMBRSEarned(600);
                        await user.AddRoleAsync(earlyAccessRole);
                    }
                    stringBuilder.AppendLine();
                }

                if (usersList.Count > 0)
                {
                    for (int i = 0; i < usersList.Count; i++)
                    {
                        var user = guild.GetUser(usersList[i]) as SocketGuildUser;
                        stringBuilder.Append($"@{user.Username}#{user.Discriminator}");
                        stringBuilder.Append($" - 100 EMBRS!");
                        await XRPL.SendRewardAsync(client, null, null, user, "100");
                        Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(user.Id).ModEMBRSEarned(100);
                        stringBuilder.AppendLine();
                    }
                }
            }

            var embedBuiler = new EmbedBuilder()
                .WithTitle("Tournament Results")
                .WithColor(Color.Orange)
                .AddField("Winners", stringBuilder.ToString())
                .AddField("Congratulations!", "The new tournament week will start on Tuesday! If you have any questions, please let us know in here!");

            await winnersChannel.SendMessageAsync(embed: embedBuiler.Build());
            Database.IsDirty = true;
        }

        public async Task ResetTournament(DiscordSocketClient client)
        {
            var guild = client.GetGuild(ulong.Parse(Settings.GuildID));
            var tournamentRole = guild.Roles.FirstOrDefault(x => x.Name == "Tournament");
            var winnerRole = guild.Roles.FirstOrDefault(x => x.Name == "Tournament Winner");
            var roles = new List<SocketRole>() { tournamentRole, winnerRole };
            var tournamentChannel = guild.TextChannels.FirstOrDefault(x => x.Name == "tournament") as ISocketMessageChannel;

            var users = await tournamentChannel.GetUsersAsync().FlattenAsync<IUser>();
            foreach (IGuildUser user in users)
            {
                await user.RemoveRolesAsync(roles);
            }

            foreach (var user in Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccounts())
            {
                user.ResetTournament();
            }

            foreach (var participant in _tournamentParticipants) Database.GetDatabase<DatabaseAccounts>(DatabaseType.Accounts).GetAccount(participant).ResetTournament();
            _tournamentAchievement = "";
            _tournamentParticipants = new List<ulong>();
            _tournamentWinners = new List<ulong>();
            _tournamentReward = null;
            _tournamentSponsors = new List<TournamentSponsor>();

            Database.IsDirty = true;
        }
    }
}