namespace DerbyDataModels
{
    public class Jam
    {
        public int ID { get; set; }
        public int BoutID { get; set; }
        public bool IsFirstHalf { get; set; }
        public int JamNumber { get; set; }
        public int RinxterNumber { get; set; }

        public override string ToString()
        {
            return (IsFirstHalf ? "P1 J" : "P2 J") + JamNumber.ToString();
        }

        public bool Matches(Jam jam)
        {
            return IsFirstHalf == jam.IsFirstHalf && JamNumber == jam.JamNumber;
        }

        public bool IsOneJamBefore(Jam jam, int lastP1Jam)
        {
            if (IsFirstHalf == jam.IsFirstHalf)
            {
                return JamNumber == jam.JamNumber - 1;
            }
            else if (IsFirstHalf && JamNumber == lastP1Jam)
            {
                return !jam.IsFirstHalf && jam.JamNumber == 1;
            }
            return false;
        }

        public bool IsBetweenJams(Jam startJam, Jam endJam)
        {
            if (endJam == null)
            {
                return (startJam.IsFirstHalf && !IsFirstHalf) || (startJam.IsFirstHalf == IsFirstHalf && startJam.JamNumber <= JamNumber);
            }
            else
            {
                return
                    (IsFirstHalf && startJam.IsFirstHalf && JamNumber >= startJam.JamNumber && (!endJam.IsFirstHalf || endJam.JamNumber >= JamNumber)) ||
                    (!IsFirstHalf && !endJam.IsFirstHalf && endJam.JamNumber >= JamNumber && (startJam.IsFirstHalf || startJam.JamNumber <= JamNumber));
            }
        }

        public bool IsBefore(Jam jam)
        {
            return IsFirstHalf && !jam.IsFirstHalf || (IsFirstHalf == jam.IsFirstHalf && JamNumber < jam.JamNumber);
        }
    }
}
