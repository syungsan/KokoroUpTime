using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace KokoroUpTime
{
    public class DataCapter1
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string MyKindOfFeelings { get; set; }

        public string KimisKindOfFeeling { get; set; }

        public string AkamarusKindOfFeeling { get; set; }

        public int AkamarusSizeOfFeeling { get; set; }

        public string AosukesKindOfFeeling { get; set; }

        public int AosukesSizeOfFeeling { get; set; }

        public string CreatedAt{ get; set; }
    }
}
