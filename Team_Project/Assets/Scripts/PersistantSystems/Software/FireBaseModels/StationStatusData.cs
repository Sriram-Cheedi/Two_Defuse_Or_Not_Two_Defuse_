using System;

namespace PersistantSystems.Software.FirebaseModels
{
    [Serializable]
    public class StationStatusData
    {
        public string teamName = "---";
        public string status = "Waiting";

        public StationStatusData() { }

        public StationStatusData(string teamName, string status)
        {
            this.teamName = teamName;
            this.status = status;
        }
    }
}