using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EMBRS
{
    public class Thread
    {
        private ulong _threadId;
        private ulong _threadChannelId;
        private ulong _threadEmbedId;
        private DateTime _threadCreation;
        private DateTime _threadExpiration;
        private DateTime _threadUpdated;
        private ulong _threadAuthor;
        private string _threadHeader;
        private string _threadContent;
        private Dictionary<ulong, bool> _threadVotes;
        private List<ThreadMessage> _threadDiscussion;
        private ulong _nextThreadMessageId = 1;

        private readonly Regex regex = new Regex("[^a-zA-Z0-9 _]");

        public Thread(ulong id, ulong author, string header, string content)
        {
            _threadId = id;
            _threadChannelId = 0;
            _threadEmbedId = 0;
            _threadCreation = DateTime.UtcNow;
            _threadExpiration = DateTime.UtcNow.AddDays(Settings.ThreadTimeInDays);
            _threadUpdated = DateTime.UtcNow;
            _threadAuthor = author;
            _threadHeader = header;
            _threadContent = content;
            _threadVotes = new Dictionary<ulong, bool>();
            _threadDiscussion = new List<ThreadMessage>();
        }

        public ulong GetThreadId()
        {
            return _threadId;
        }

        public ulong GetThreadChannelId()
        {
            return _threadChannelId;
        }

        public DateTime GetThreadExpirationTime()
        {
            return _threadExpiration;
        }

        public DateTime GetThreadUpdateTime()
        {
            return _threadUpdated;
        }

        public void SetThreadChannelId(ulong threadChannelId)
        {
            _threadChannelId = threadChannelId;
        }

        public void SetThreadEmbedId(ulong threadEmbedId)
        {
            _threadEmbedId = threadEmbedId;
        }

        public string GetThreadChannelName()
        {
            var channelName = _threadHeader;
            channelName = regex.Replace(channelName, string.Empty);
            channelName = channelName.Replace(' ', '-').ToLower();
            channelName = channelName.Substring(0, 32);
            return channelName;
        }

        public string GetThreadHeader()
        {
            return _threadHeader;
        }

        public string GetThreadContent()
        {
            return _threadContent;
        }

        public async Task SetVote(ulong author, bool vote, DiscordSocketClient client = null)
        {
            if (!_threadVotes.ContainsKey(author)) _threadVotes.Add(author, vote);
            else _threadVotes[author] = vote;
            if (client != null) await UpdateThread(client);
        }

        public async Task<ThreadMessage> AddThreadMessage(ulong author, string description, DiscordSocketClient client = null)
        {
            var newThreadMessage = new ThreadMessage(_nextThreadMessageId, author, description);
            _nextThreadMessageId++;
            _threadDiscussion.Add(newThreadMessage);
            if (client != null) await UpdateThread(client);
            return newThreadMessage;
        }

        public async Task UpdateThread(DiscordSocketClient client = null)
        {
            _threadUpdated = DateTime.UtcNow;

            var guild = client.GetGuild(ulong.Parse(Settings.GuildID));
            var author = guild.GetUser(_threadAuthor);
            var channel = guild.TextChannels.FirstOrDefault(x => x.Id == _threadChannelId);
            var message = await channel.GetMessageAsync(_threadEmbedId) as IUserMessage;

            var yesVotes = 0;
            var noVotes = 0;

            foreach(var vote in _threadVotes)
            {
                if (vote.Value) yesVotes++;
                else noVotes++;
            }

            var embedBuiler = new EmbedBuilder()
                .WithAuthor(author.ToString(), author.GetAvatarUrl() ?? author.GetDefaultAvatarUrl())
                .WithTitle(_threadHeader)
                .WithDescription(_threadContent)
                .AddField("Yes Votes", yesVotes.ToString())
                .AddField("No Votes", noVotes.ToString(), true)
                .WithCurrentTimestamp()
                .WithColor(Color.Orange);

            await message.ModifyAsync(x =>
            {
                x.Embed = embedBuiler.Build();

            });
        }

        public async Task<bool> ContainsThreadMessageByMessageId(ulong messageId)
        {
            return _threadDiscussion.Any(threadMessage => threadMessage.GetThreadMessageChannelId() == messageId);
        }

        public async Task<ThreadMessage> GetThreadMessageByMessageId(ulong messageId)
        {
            if (await ContainsThreadMessageByMessageId(messageId)) return _threadDiscussion.FirstOrDefault(threadMessage => threadMessage.GetThreadMessageChannelId() == messageId);
            return null;
        }

        public async Task DeleteThreadMessage(ulong messageId)
        {
            var threadMessage = await GetThreadMessageByMessageId(messageId);
            if (threadMessage != null && _threadDiscussion.Contains(threadMessage)) _threadDiscussion.Remove(threadMessage);
        }
    }
}
