using System;

namespace PersistantSystems.Software.FirebaseModels
{
    [Serializable] public class LeaderboardEntryData
    {
        public string username = "";
        public int score = 0;
        public bool defused = false;
        public string stationId = "";

        public LeaderboardEntryData() { }

        public LeaderboardEntryData(string username, int score, bool defused, string stationId)
        {
            this.username = username;
            this.score = score;
            this.defused = defused;
            this.stationId = stationId;
        }
    }
}