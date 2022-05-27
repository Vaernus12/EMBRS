using Newtonsoft.Json;
using System;

namespace EMBRS
{
    [Serializable]
    public class Account
    {
        [JsonProperty("Id")] private ulong _id;
        [JsonProperty("XrpAddress")] private string _xrpAddress;
        [JsonProperty("EMBRSEarned")] private float _embrsEarned;
        [JsonProperty("IsRegistered")] private bool _isRegistered;
        [JsonProperty("InTournament")] private bool _inTournament;
        [JsonProperty("TournamentWinner")] private bool _tournamentWinner;
        [JsonProperty("LastCommandTime")] private DateTime _lastCommandTime;
        [JsonProperty("LastTipTime")] private DateTime _lastTipTime;
        [JsonProperty("ReceivedFaucetReward")] private bool _receivedFaucetReward;

        public Account(ulong id, string xrpAddress)
        {
            _id = id;
            _xrpAddress = xrpAddress;
            _embrsEarned = 0;
            _isRegistered = false;
            _inTournament = false;
            _tournamentWinner = false;
            _lastCommandTime = DateTime.UtcNow;
            _lastTipTime = DateTime.MinValue;
            _receivedFaucetReward = false;
        }

        public ulong GetId()
        {
            return _id;
        }

        public string GetXRPAddress()
        {
            return _xrpAddress;
        }

        public void SetXRPAddress(string xrpAddress)
        {
            _xrpAddress = xrpAddress;
        }

        public bool GetIsRegistered()
        {
            return _isRegistered;
        }

        public void SetIsRegistered(bool registered)
        {
            _isRegistered = registered;
        }

        public float GetEMBRSEarned()
        {
            return _embrsEarned;
        }

        public void ModEMBRSEarned(float amount)
        {
            _embrsEarned += amount;
        }

        public bool GetInTournament()
        {
            return _inTournament;
        }

        public void SetInTournament(bool inTournament)
        {
            _inTournament = inTournament;
        }

        public bool GetTournamentWinner()
        {
            return _tournamentWinner;
        }

        public void SetTournamentWinner(bool tournamentWinner)
        {
            _tournamentWinner = tournamentWinner;
        }

        public bool ReceivedFaucetReward()
        {
            return _receivedFaucetReward;
        }

        public void SetReceivedFaucetReward(bool faucetReward)
        {
            _receivedFaucetReward = faucetReward;
        }

        public void ResetTournament()
        {
            _inTournament = false;
            _tournamentWinner = false;
        }

        public DateTime GetLastCommandTime()
        {
            return _lastCommandTime;
        }

        public void SetLastCommandTime(DateTime commandTime)
        {
            _lastCommandTime = commandTime;
        }

        public DateTime GetLastTipTime()
        {
            return _lastTipTime;
        }

        public void SetLastTipTime(DateTime tipTime)
        {
            _lastTipTime = tipTime;
        }
    }
}
