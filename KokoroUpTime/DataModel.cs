using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace KokoroUpTime
{
    // 各種初期化変数
    public class InitConfig
    {
        public string userName = null;

        public string userTitle = null;

        public string accessDateTime = null;

        public string dbPath = null;
    }

    // データベースオプションカラム
    public class DataOption
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public bool IsPlaySE { get; set; }

        public bool IsPlayBGM { get; set; }

        public float MessageSpeed { get; set; }

        public bool IsAddRubi { get; set; }

        public int InputMethod { get; set; }
    }

    // アイテム取得フラグカラム
    public class DataItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public bool HasGotItem01 { get; set; }

        public bool HasGotItem02 { get; set; }

        public bool HasGotItem03 { get; set; }

        public bool HasGotItem04 { get; set; }

        public bool HasGotItem05 { get; set; }

        public bool HasGotItem06 { get; set; }

        public bool HasGotItem07 { get; set; }

        public bool HasGotItem08 { get; set; }

        public bool HasGotItem09 { get; set; }

        public bool HasGotItem10 { get; set; }

        public bool HasGotItem11 { get; set; }
    }

    // 各チャプターの進捗具合を収めるカラム
    public class DataProgress
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int CurrentChapter { get; set; }

        public string CurrentScene { get; set; }

        public string LatestChapter1Scene { get; set; }

        public bool HasCompletedChapter1 { get; set; }

        public string LatestChapter2Scene { get; set; }

        public bool HasCompletedChapter2 { get; set; }

        public string LatestChapter3Scene { get; set; }

        public bool HasCompletedChapter3 { get; set; }

        public string LatestChapter4Scene { get; set; }

        public bool HasCompletedChapter4 { get; set; }

        public string LatestChapter5Scene { get; set; }

        public bool HasCompletedChapter5 { get; set; }

        public string LatestChapter6Scene { get; set; }

        public bool HasCompletedChapter6 { get; set; }

        public string LatestChapter7Scene { get; set; }

        public bool HasCompletedChapter7 { get; set; }

        public string LatestChapter8Scene { get; set; }

        public bool HasCompletedChapter8 { get; set; }

        public string LatestChapter9Scene { get; set; }

        public bool HasCompletedChapter9 { get; set; }

        public string LatestChapter10Scene { get; set; }

        public bool HasCompletedChapter10 { get; set; }

        public string LatestChapter11Scene { get; set; }

        public bool HasCompletedChapter11 { get; set; }

        public string LatestChapter12Scene { get; set; }

        public bool HasCompletedChapter12 { get; set; }
    }

    // チャプター1のログカラム
    public class DataChapter1
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

        public string CreatedAt { get; set; }
    }

    // チャプター2のログカラム
    public class DataChapter2
    {
        // このレコードは定石
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // データベースに記録するレコードの型の宣言をしてください。
        public string MySelectGoodEvents { get; set; }

        public string AosukesSizeOfFeelingOfSleeping { get; set; }

        public string AosukesKindOfFeelingOfSleeping { get; set; }

        public string AosukesDifficultyOfSleeping { get; set; }

        public string AosukesSizeOfFeelingOfEating { get; set; }

        public string AosukesKindOfFeelingOfEating { get; set; }

        public string AosukesDifficultyOfEating { get; set; }

        public string AosukesSizeOfFeelingOfGettingHighScore { get; set; }

        public string AosukesKindOfFeelingOfGettingHighScore { get; set; }

        public string AosukesDifficultyOfGettingHighScore { get; set; }

        public string AosukesKindOfFeelingOfTalkingWithFriend { get; set; }

        public string AosukesSizeOfFeelingOfTalkingWithFriend { get; set; }

        public string AosukesDifficultyOfTalkingWithFriend { get; set; }

        public string MyALittlleExcitingEvents { get; set; }

        // 何かしらレコードを記録するときは日付も記録する。
        public string CreatedAt { get; set; }
    }

    // チャプター3のログカラム
    public class DataChapter3
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }


        public string CreatedAt { get; set; }
    }
}
