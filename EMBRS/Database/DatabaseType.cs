using System;

namespace EMBRS
{
    [Serializable]
    public enum DatabaseType : byte
    {
        Accounts,
        Settings,
        Threads,
        Tournament,
    }
}
