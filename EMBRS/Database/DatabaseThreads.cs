using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMBRS
{
    [Serializable]
    public class DatabaseThreads : DatabaseBase
    {
        [JsonProperty] private Dictionary<ulong, Thread> _registeredThreads;
        [JsonProperty] private ulong _nextThreadId = 1;
        [JsonProperty] private ulong _categoryId;
        [JsonProperty] private List<ulong> _threadChannels;

        public DatabaseThreads()
        {
            Type = DatabaseType.Threads;
            _registeredThreads = new Dictionary<ulong, Thread>();
            _threadChannels = new List<ulong>();
        }

        public void SetCategoryId(ulong categoryId)
        {
            _categoryId = categoryId;      
        }

        public async Task<Thread> CreateThread(SocketGuildUser author, string header, string content, DiscordSocketClient client = null)
        {
            var newThread = new Thread(_nextThreadId, author.Id, header, content);
            _nextThreadId++;

            _registeredThreads.Add(_nextThreadId, newThread);
            if (client != null)
            {
                await LinkThreadToChannel(author, newThread, client);
                await UpdateThreadPositionInChannel(newThread, client);
            }

            return newThread;
        }

        private async Task LinkThreadToChannel(SocketGuildUser author, Thread linkedThread, DiscordSocketClient client = null)
        {
            var guild = client.GetGuild(ulong.Parse(Settings.GuildID));    
            
            var channel = await guild.CreateTextChannelAsync(linkedThread.GetThreadChannelName(), prop => prop.SlowModeInterval = 10);
            await channel.ModifyAsync(prop => prop.CategoryId = _categoryId);
            linkedThread.SetThreadChannelId(channel.Id);

            var embedBuiler = new EmbedBuilder()
                .WithAuthor(author.ToString(), author.GetAvatarUrl() ?? author.GetDefaultAvatarUrl())
                .WithTitle(linkedThread.GetThreadHeader())
                .WithDescription(linkedThread.GetThreadContent())
                .AddField("Yes Votes", "0")
                .AddField("No Votes", "0", true)
                .WithCurrentTimestamp()
                .WithColor(Color.Orange);

            var message = await channel.SendMessageAsync(null, false, embedBuiler.Build());
            linkedThread.SetThreadEmbedId(message.Id);
        }

        public async Task UpdateThreadPositionInChannel(Thread thread, DiscordSocketClient client = null)
        {
            var guild = client.GetGuild(ulong.Parse(Settings.GuildID));
            var threadChannel = guild.TextChannels.FirstOrDefault(x => x.Id == thread.GetThreadChannelId());
            await threadChannel.ModifyAsync(x =>
            {
                x.Position = 0;
            });
        }

        public async Task<bool> ContainsThreadByChannelId(ulong channelId)
        {
            return _registeredThreads.Any(thread => thread.Value.GetThreadChannelId() == channelId);
        }

        public async Task<Thread> GetThreadByChannelId(ulong channelId)
        {
            if (await ContainsThreadByChannelId(channelId)) return _registeredThreads.FirstOrDefault(thread => thread.Value.GetThreadChannelId() == channelId).Value;
            return null;
        }

        public async Task DeleteThread(ulong channelId)
        {
            var thread = await GetThreadByChannelId(channelId);
            if(thread != null && _registeredThreads.ContainsKey(thread.GetThreadId())) _registeredThreads.Remove(thread.GetThreadId());
        }

        public async Task TestAllThreads(DiscordSocketClient client)
        {
            var threadsToDelete = new List<Thread>();
            foreach(var thread in _registeredThreads)
            {
                if((DateTime.UtcNow - thread.Value.GetThreadUpdateTime()).TotalDays >= Settings.IdleThreadTimeToDelete && thread.Value.GetThreadExpirationTime() > DateTime.UtcNow)
                {
                    threadsToDelete.Add(thread.Value);
                }
            }

            for(int i = 0; i < threadsToDelete.Count; i++)
            {
                var guild = client.GetGuild(ulong.Parse(Settings.GuildID));
                var channel = guild.TextChannels.FirstOrDefault(x => x.Id == threadsToDelete[i].GetThreadChannelId());
                await channel.DeleteAsync();
            }
        }
    }
}
