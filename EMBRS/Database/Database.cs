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
        public static bool IsDirty;

        private static Dictionary<ulong, Account> RegisteredUsers; // Old database
        private static Dictionary<DatabaseType, DatabaseBase> _tables; // New database

        private static bool UseOldDatabase = false;
        private static bool ConvertToNewDatabase = true;

        public static async Task Initialize()
        {
            RegisteredUsers = new Dictionary<ulong, Account>();
            _tables = new Dictionary<DatabaseType, DatabaseBase>();
            _tables.Add(DatabaseType.Accounts, new DatabaseAccounts());
            _tables.Add(DatabaseType.Settings, new DatabaseSettings());
            _tables.Add(DatabaseType.Threads, new DatabaseThreads());
            await Read();
            IsDirty = false;
        }

        public static T GetDatabase<T>(DatabaseType type) where T : DatabaseBase
        {
            if (_tables.ContainsKey(type))
            {
                return (T)_tables[type];
            }

            return null;
        }

        public static async Task Write()
        {
            try
            {
                if (UseOldDatabase)
                {
                    var result = JsonConvert.SerializeObject(RegisteredUsers);
                    await File.WriteAllTextAsync("EMBRSDatabase.dat", result);
                    await Program.Log(new LogMessage(LogSeverity.Info, "Database Write", "Complete"));
                }
                else
                {
                    var result = JsonConvert.SerializeObject(_tables);
                    await File.WriteAllTextAsync("EMBRSDatabase2.dat", result);
                    await Program.Log(new LogMessage(LogSeverity.Info, "Database Write", "Complete"));
                }
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
                if (UseOldDatabase || ConvertToNewDatabase)
                {
                    if (File.Exists("EMBRSDatabase.dat"))
                    {
                        string value = await File.ReadAllTextAsync("EMBRSDatabase.dat");
                        RegisteredUsers = JsonConvert.DeserializeObject<Dictionary<ulong, Account>>(value);
                        await Program.Log(new LogMessage(LogSeverity.Info, "Database Read", "Complete"));
                    }

                    if(ConvertToNewDatabase)
                    {
                        var database = GetDatabase<DatabaseAccounts>(DatabaseType.Accounts);
                        database.UpdateRegisteredUsers(RegisteredUsers);
                        IsDirty = true;
                    }
                }
                else if(!UseOldDatabase)
                {
                    if (File.Exists("EMBRSDatabase2.dat"))
                    {
                        string value = await File.ReadAllTextAsync("EMBRSDatabase2.dat");
                        _tables = JsonConvert.DeserializeObject<Dictionary<DatabaseType, DatabaseBase>>(value);
                        await Program.Log(new LogMessage(LogSeverity.Info, "Database Read", "Complete"));
                    }
                }
            }
            catch (Exception ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex));
            }
        }
    }
}
