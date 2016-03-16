namespace DerbyDataModels
{
    public class Player
    {
        public int ID { get; set; }
        public int TeamID { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public int? RinxterID { get; set; }
        public override string ToString()
        {
            return Number + ": " + Name;
        }
    }
}
