using System;

namespace EMBRS
{
    [Serializable]
    public class DatabaseSettings : DatabaseBase
    {
        public bool SundayMessage = false;
        public bool MondayMessage = false;
        public bool TuesdayTournamentMessage = false;
        public bool WednesdayTournamentMessage = false;
        public bool ThursdayTournamentMessage = false;
        public bool FridayTournamentMessage = false;
        public bool SaturdayMessage = false;

        public DateTime TimeSinceLastMessage = DateTime.MinValue;

        public DatabaseSettings()
        {
            Type = DatabaseType.Settings;
        }
    }
}
