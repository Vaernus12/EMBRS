using System;

namespace EMBRS
{
    public class ThreadMessage
    {
        private ulong _threadMessageId;
        private ulong _threadMessageChannelId;
        private DateTime _threadMessageCreation;
        private ulong _threadMessageAuthor;
        private string _threadMessageContent;

        public ThreadMessage(ulong id, ulong author, string content)
        {
            _threadMessageId = id;
            _threadMessageChannelId = 0;
            _threadMessageCreation = DateTime.UtcNow;
            _threadMessageAuthor = author;
            _threadMessageContent = content;
        }

        public ulong GetThreadMessageChannelId()
        {
            return _threadMessageChannelId;
        }

        public void SetThreadMessageChannelId(ulong threadMessageChannelId)
        {
            _threadMessageChannelId = threadMessageChannelId;
        }
    }
}
