using Newtonsoft.Json.Linq;
using System.IO;

namespace EMBRS
{
    public static class Settings
    {
        public static string JsonUrl { get; set; }
        public static uint SteamID { get; set; }
        public static string TestPlayer { get; set; }
        public static string TestAddress { get; set; }
        public static string WebAPIKey { get; set; }
        public static string WebsockUrl { get; set; }
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
            JsonUrl = d.JSON_URL;
            SteamID = d.Steam_ID;
            TestPlayer = d.Test_Player;
            TestAddress = d.Test_Address;
            WebAPIKey = d.Web_API_Key;
            WebsockUrl = d.WebSocket_URL;
            RewardAddress = d.Reward_Address;
            RewardSecret = d.Reward_Address_Secret;
            string currencyCodeVal = d.Currency_Code.Value;
            if(currencyCodeVal.Length == 3) CurrencyCode = d.Currency_Code.Value;
            else CurrencyCode = Utils.AddZeros(Utils.ConvertHex(d.Currency_Code.Value));
            IssuerAddress = d.Issuer_Address;
            TransferFee = d.TransferFee;
            RewardTokenAmt = d.Reward_Token_Amt;
            AccountLinesThrottle = d.AccountLinesThrottle;
            TxnThrottle = d.TxnThrottle;
            FeeMultiplier = d.FeeMultiplier;
            MaximumFee = d.MaximumFee;
        }
    }
}
