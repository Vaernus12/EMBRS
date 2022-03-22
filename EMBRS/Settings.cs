using Newtonsoft.Json.Linq;
using System.IO;

namespace EMBRS
{
    public static class Settings
    {
        public static uint SteamID { get; set; }
        public static string TestPlayer { get; set; }
        public static string TestAddress { get; set; }
        public static string WebAPIKey { get; set; }
        public static string WebSocketUrl { get; set; }
        public static string RewardAddress { get; set; }
        public static string RewardSecret { get; set; }
        public static string CurrencyCode { get; set; }
        public static string IssuerAddress { get; set; }
        public static string RewardTokenAmt { get; set; }
        public static int AccountLinesThrottle { get; set; }
        public static int TxnThrottle { get; set; }
        public static decimal FeeMultiplier { get; set; }
        public static int MaximumFee { get; set; }
        public static decimal TransferFee { get; set; }
        
        public static void Initialize()
        {
            string jsonConfig = File.ReadAllText(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config/settings.json"));
            dynamic d = JObject.Parse(jsonConfig);
            SteamID = d.SteamID;
            TestPlayer = d.TestPlayer;
            TestAddress = d.TestAddress;
            WebAPIKey = d.WebAPIKey;
            WebSocketUrl = d.WebSocketURL;
            RewardAddress = d.RewardAddress;
            RewardSecret = d.RewardSecret;
            string currencyCodeVal = d.CurrencyCode.Value;
            if(currencyCodeVal.Length == 3) CurrencyCode = d.CurrencyCode.Value;
            else CurrencyCode = Utils.AddZeros(Utils.ConvertHex(d.CurrencyCode.Value));
            IssuerAddress = d.IssuerAddress;
            TransferFee = d.TransferFee;
            RewardTokenAmt = d.RewardTokenAmt;
            AccountLinesThrottle = d.AccountLinesThrottle;
            TxnThrottle = d.TxnThrottle;
            FeeMultiplier = d.FeeMultiplier;
            MaximumFee = d.MaximumFee;
        }
    }
}
