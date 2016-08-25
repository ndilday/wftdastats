namespace DerbyDataModels
{
    public class Penalty
    {
        public int PlayerID { get; set; }
        public int JamID { get; set; }
        public string PenaltyCode { get; set; }
        public int PenaltyNumber { get; set; }
        public char? MatchingKey { get; set; }
        public bool CanGetInBox()
        {
            return PenaltyCode == "I" || PenaltyCode == "N" || PenaltyCode == "Z" || PenaltyCode == "G";
        }
        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Penalty penalty = obj as Penalty;
            if ((System.Object)penalty == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (JamID == penalty.JamID) &&
                   (PenaltyCode == penalty.PenaltyCode) &&
                   (PenaltyNumber == penalty.PenaltyNumber) &&
                   (PlayerID == penalty.PlayerID);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + JamID.GetHashCode();
            hash = hash * 23 + PenaltyCode.GetHashCode();
            hash = hash * 23 + PenaltyNumber.GetHashCode();
            hash = hash * 23 + PlayerID.GetHashCode();
            return hash;
        }
    }
}
