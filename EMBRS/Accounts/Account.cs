using System;

namespace EMBRS
{
    public class Account
    {
        public ulong Id;
        public string XrpAddress;
        public float EMBRSEarned;
        public bool IsRegistered;
        public bool InTournament;
        public bool TournamentWinner;
        public bool ReceivedTournamentReward;
        public DateTime LastCommandTime;
        public DateTime LastTipTime;
        public DateTime LastFaucetTime;

        public Account(ulong id, string xrpAddress)
        {
            Id = id;
            XrpAddress = xrpAddress;
            EMBRSEarned = 0;
            IsRegistered = false;
            InTournament = false;
            TournamentWinner = false;
            ReceivedTournamentReward = false;
            LastCommandTime = DateTime.UtcNow;
            LastTipTime = DateTime.MinValue;
            LastFaucetTime = DateTime.MinValue;
        }
    }
}
