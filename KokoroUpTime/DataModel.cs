using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace KokoroUpTime
{
    public class DataOption
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public bool IsPlaySE { get; set; }

        public bool IsPlayBGM { get; set; }

        public float MessageSpeed { get; set; }

        public bool IsAddRubi { get; set; }

        public bool IsWordRecognition { get; set; }
    }

    public class DataProgress
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int CurrentCapter { get; set; }

        public int CurrentScene { get; set; }

        public int LatestChapter1Scene { get; set; }

        public bool HasCompletedChapter1 { get; set; }

        public int LatestChapter2Scene { get; set; }

        public bool HasCompletedChapter2 { get; set; }

        public int LatestChapter3Scene { get; set; }

        public bool HasCompletedChapter3 { get; set; }

        public int LatestChapter4Scene { get; set; }

        public bool HasCompletedChapter4 { get; set; }

        public int LatestChapter5Scene { get; set; }

        public bool HasCompletedChapter5 { get; set; }

        public int LatestChapter6Scene { get; set; }

        public bool HasCompletedChapter6 { get; set; }

        public int LatestChapter7Scene { get; set; }

        public bool HasCompletedChapter7 { get; set; }

        public int LatestChapter8Scene { get; set; }

        public bool HasCompletedChapter8 { get; set; }

        public int LatestChapter9Scene { get; set; }

        public bool HasCompletedChapter9 { get; set; }

        public int LatestChapter10Scene { get; set; }

        public bool HasCompletedChapter10 { get; set; }

        public int LatestChapter11Scene { get; set; }

        public bool HasCompletedChapter11 { get; set; }

        public int LatestChapter12Scene { get; set; }

        public bool HasCompletedChapter12 { get; set; }
    }

    public class DataCapter1
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string MyKindOfGoodFeelings { get; set; }

        public string MyKindOfBadFeelings { get; set; }

        public string KimisKindOfFeelings { get; set; }

        public string AkamarusKindOfFeelings { get; set; }

        public int AkamarusSizeOfFeeling { get; set; }

        public string AosukesKindOfFeelings { get; set; }

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
