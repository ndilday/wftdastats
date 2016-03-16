
namespace DerbyDataModels
{
    public class JamEffectiveness
    {
        public int JamID { get; set; }
        public double BaseEffectiveness { get; set; }
        public double PenaltyCost { get; set; }
    }

    public class JamPlayerEffectiveness
    {
        public int PlayerID { get; set; }
        public int TeamID { get; set; }
        public bool IsJammer { get; set; }
        public int JamID { get; set; }
        public double JamPortion { get; set; }
        public double BaseQuality { get; set; }
        public double PenaltyCost { get; set; }
    }
}
