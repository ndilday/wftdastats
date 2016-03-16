using System;

namespace DerbyDataModels
{
    public class PlayerTrueSkill
    {
        public int PlayerID { get; set; }
        public bool IsJammer { get; set; }
        public double Mean { get; set; }
        public double StdDev { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
