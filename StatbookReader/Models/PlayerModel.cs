using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatbookReader.Models
{
    public class PlayerModel
    {
        public string Name { get; set; }
        public string Number { get; set; }
        public bool Captain { get; set; }
        public bool Alternate { get; set; }
    }
}
