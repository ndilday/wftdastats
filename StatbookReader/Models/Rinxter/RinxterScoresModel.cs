using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatbookReader.Models.Rinxter
{
    public class RinxterScoresModel
    {
        public RinxterScoreRowModel[] rows;
    }

    public class RinxterScoreRowModel
    {
        public int id;
        public object[] data;
    }
}
