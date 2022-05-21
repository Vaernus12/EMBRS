using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EMBRS
{
    public static class Database
    {
        public static Dictionary<ulong, Account> RegisteredUsers;
        public static bool IsDirty;

        public static async Task Initialize()
        {
            RegisteredUsers = new Dictionary<ulong, Account>();
            await Read();
            IsDirty = false;
        }

        public static async Task Write()
        {
            try
            {
                var result = JsonConvert.SerializeObject(RegisteredUsers);
                await File.WriteAllTextAsync("EMBRSDatabase.dat", result);
                await Program.Log(new LogMessage(LogSeverity.Info, "Database Write", "Complete"));
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }

        private static async Task Read()
        {
            try
            {
                if (File.Exists("EMBRSDatabase.dat"))
                {
                    string value = await File.ReadAllTextAsync("EMBRSDatabase.dat");
                    RegisteredUsers = JsonConvert.DeserializeObject<Dictionary<ulong, Account>>(value);
                    await Program.Log(new LogMessage(LogSeverity.Info, "Database Read", "Complete"));
                }
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }
    }
}
