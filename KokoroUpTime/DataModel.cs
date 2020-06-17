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

    public class DataCapter2
    {
        // このレコードは定石
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // データベースに記録するレコードの型の宣言をしてください。

        // 何かしらレコードを記録するときは日付も記録する。
        public string CreatedAt { get; set; }
    }
}
