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

        public bool Is3SecondRule { get; set; }
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

        public string KimisKindOfFeeling { get; set; }

        public string AkamarusKindOfFeeling { get; set; }

        public int? AkamarusSizeOfFeeling { get; set; }

        public string AosukesKindOfFeeling { get; set; }

        public int? AosukesSizeOfFeeling { get; set; }

        public string CreatedAt { get; set; }
    }

    // チャプター2のログカラム
    public class DataChapter2
    {
        public DataChapter2()
        {
            MySelectGoodEvents = "";
            AosukesSizeOfFeelingOfEating = -1;
            AosukesDifficultyOfEating = "";
            AosukesSizeOfFeelingOfGettingHighScore = -1;
            AosukesDifficultyOfGettingHighScore = "";
            AosukesSizeOfFeelingOfTalkingWithFriend = -1;
            AosukesDifficultyOfTalkingWithFriend = "";
            MyALittlleExcitingEvents = "";
        }
        // このレコードは定石
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // データベースに記録するレコードの型の宣言をしてください。
        public string MySelectGoodEvents { get; set; }

        public int? AosukesSizeOfFeelingOfEating { get; set; }

        public string AosukesDifficultyOfEating { get; set; }

        public int? AosukesSizeOfFeelingOfGettingHighScore { get; set; }

        public string AosukesDifficultyOfGettingHighScore { get; set; }

        public int? AosukesSizeOfFeelingOfTalkingWithFriend { get; set; }

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

        public  string AosukesKindOfFeelingPreUseItem { get; set; }

        public string KimisKindOfFeelingPreUseItem { get; set; }

        public string AosukesKindOfFeelingAfterUsedItem { get; set; }

        public string KimisKindOfFeelingAfterUsedItem { get; set; }

        public string AkamarusKindOfFeelingAfterUsedItem { get; set; }

        public int? AosukesSizeOfFeelingPreUseItem { get; set; }

        public int? KimisSizeOfFeelingPreUseItem { get; set; }

        public int? AosukesSizeOfFeelingAfterUsedItem { get; set; }

        public int? KimisSizeOfFeelingAfterUsedItem { get; set; }

        public int? AkamarusSizeOfFeelingAfterUsedItem { get; set; }

        public string SelectedPraiseHotWord { get; set; }

        public string SelectedWorryHotWord { get; set; }

        public string SelectedEncourageHotWord { get; set; }

        public string SelectedThanksHotWord { get; set; }

        public string CreatedAt { get; set; }
    }

    // チャプター4のログカラム
    public class DataChapter4
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }


        public string CreatedAt { get; set; }

        public string KimisKindOfFeelingAskedForWork { get; set; }
        public string KimisKindOfFeelingAskedByAkamaru { get; set; }

        public int? KimisSizeOfFeelingAskedForWork { get; set; }
        public int? KimisSizeOfFeelingAskedByAkamaru { get; set; }


    }

    public class DataChapter5
    {
       [PrimaryKey, AutoIncrement]
       public int Id { get; set; }
       
       public string CreatedAt { get; set; }

       public string InputFaceImageOfKimiText{ get; set; }

       public string InputHandImageOfKimiText{ get; set; }

       public string InputStomachImageOfKimiText{ get; set; }

       public string InputOthersImageOfKimiText{ get; set; }

       public string InputHeadImageOfKimiText{ get; set; }

       public string InputShoulderImageOfKimiText{ get; set; }

       public string InputLegImageOfKimiText { get; set; }

       public string KindOfFeelingNotUnderstandProblem { get; set; }
       
       public string KindOfFeelingRecoderProblem { get; set; }

       public int? SizeOfFeelingNotUnderstandProblem { get; set; }

       public int? SizeOfFeelingRecoderProblem { get; set; }

       public string InputMyBodyImageTextNotUnderstandProblem { get; set; }

       public string InputMyBodyImageTextRecoderProblem { get; set; }

    }

    public class DataChapter6
     {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }


        public string CreatedAt { get; set; }
    }

    public class DataChapter7
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }


        public string CreatedAt { get; set; }

        public string ChallengeTimeSelectedScene { get; set; }
        
        public string GroupeActivitySelectedScene { get; set; }

        public string KimisKindOfFeelingInviteFriends { get; set; }

        public string KimisKindOfFeelingAnnouncement { get; set; }

        public string YourKindOfFeelingAnnouncement { get; set; }

        public string YourKindOfFeelingGreetingToFriend { get; set; }

        public string YourFriendsKindOfFeelingAnnouncement { get; set; }

        public string YourFriendsKindOfFeelingGreetingToAnotherFriend { get; set; }

        public int? KimisSizeOfFeelingInviteFriends { get; set; }

        public int? KimisSizeOfFeelingAnnouncement { get; set; }

        public int? YourSizeOfFeelingAnnouncement { get; set; }

        public int? YourSizeOfFeelingGreetingToFriend { get; set; }

        public int? YourFriendsSizeOfFeelingAnnouncement { get; set; }

        public int? YourFriendsSizeOfFeelingGreetingToAnotherFriend { get; set; }

        public string InputAkamaruThoughtText { get; set; }

        //public byte[] InputAkamaruThoughtStroke { get; set; }

        public string InputAosukeThoughtText { get; set; }

        //public byte[] InputAosukeThoughtStroke { get; set; }

        public string InputYourToughtText1 { get; set; }

        //public byte[] InputYourToughtStroke1 { get; set; }

        public string InputYourToughtText2 { get; set; }

        //public byte[] InputYourToughtStroke2 { get; set; }

        public string InputFriendToughtText1 { get; set; }

        //public byte[] InputFriendToughtStroke1 { get; set; }

        public string InputFriendToughtText2 { get; set; }

        //public byte[] InputFriendToughtStroke2 { get; set; }

    }

    public class DataChapter8
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string CreatedAt { get; set; }

        public string AosukesKindOfFeelingPreUseItem { get; set; }

        public string KimisKindOfFeelingPreUseItem { get; set; }

        public string AkamarusKindOfFeelingPreUseItem { get; set; }

        public string AosukesKindOfFeelingAfterUsedItem { get; set; }

        public string KimisKindOfFeelingAfterUsedItem { get; set; }

        public string AkamarusKindOfFeelingAfterUsedItem { get; set; }

        public string Let_sCheckKindOfFeeling { get; set; }

        public string PositiveThinkingKindOfFeeling { get; set; }

        public string ThoughtsOfOthersKindOfFeeling { get; set; }

        public int? AosukesSizeOfFeelingPreUseItem { get; set; }

        public int? KimisSizeOfFeelingPreUseItem { get; set; }

        public int? AkamarusSizeOfFeelingPreUseItem { get; set; }

        public int? AosukesSizeOfFeelingAfterUsedItem { get; set; }

        public int? KimisSizeOfFeelingAfterUsedItem { get; set; }

        public int? AkamarusSizeOfFeelingAfterUsedItem { get; set; }

        public int? Let_sCheckSizeOfFeeling { get; set; }

        public int? PositiveThinkingSizeOfFeeling { get; set; }

        public int? ThoughtsOfOthersSizeOfFeeling { get; set; }

        public string GroupeActivityInputText1 { get; set; }

        public string GroupeActivityInputText2 { get; set; }

        public string GroupeActivityInputText3 { get; set; }
    }

    public class DataChapter9
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }


        public string CreatedAt { get; set; }

        public string AosukesKindOfFeelingTalkToFriends { get; set; }

        public string AosukesKindOfFeelingAfter10minutes { get; set; }

        public string AosukesKindOfFeelingNextDay { get; set; }

        public string AosukesKindOfFeelingDayAfterTomorrow { get; set; }

        public int? AosukesSizeOfFeelingTalkToFriends { get; set; }

        public int? AosukesSizeOfFeelingAfter10minutes { get; set; }

        public int? AosukesSizeOfFeelingNextDay { get; set; }

        public int? AosukesSizeOfFeelingDayAfterTomorrow { get; set; }

        public string InputChallengeText { get; set; }

    }

    public class DataChapter10
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }


        public string CreatedAt { get; set; }

        public string AosukeChallengeStep1KindOfFeeling { get; set; }

        public string AosukeChallengeStep2KindOfFeeling { get; set; }

        public string AosukeChallengeStep3KindOfFeeling { get; set; }

        public int? AosukeChallengeStep1SizeOfFeeling { get; set; }

        public int? AosukeChallengeStep2SizeOfFeeling { get; set; }

        public int? AosukeChallengeStep3SizeOfFeeling { get; set; }

        public string ReasonDifferenceSizeOfFeelingInputText { get; set; }

        public string AosukeSmallChallengeInputText {get; set;}

        public string AosukeChallengeStep1InputText {get; set;}

        public string AosukeChallengeStep2InputText {get; set;}

        public string AosukeChallengeStep3InputText {get; set;}

    }

    public class DataChapter11
    {

        public DataChapter11()
        {
            AkamaruOtherSolutionUseItemInputText = "";
            SelectedAkamaruGoalText="";
            Step2AkamaruManyMethodInputText1="";
            Step2AkamaruManyMethodInputText2="";
            Step2AkamaruManyMethodInputText3="";
            EvaluateAkamaruMethodText1="";
            EvaluateAkamaruMethodText2="";
            EvaluateAkamaruMethodText3="";
        }
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string CreatedAt { get; set; }

        public string AkamaruOtherSolutionUseItemInputText { get; set; }

        public string SelectedAkamaruGoalText { get; set; }

        public string Step2AkamaruManyMethodInputText1 { get; set; }
        public string Step2AkamaruManyMethodInputText2 { get; set; }
        public string Step2AkamaruManyMethodInputText3 { get; set; }

        public string EvaluateAkamaruMethodText1 { get; set; }
        public string EvaluateAkamaruMethodText2 { get; set; }
        public string EvaluateAkamaruMethodText3 { get; set; }
    }

    public class DataChapter12
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }


        public string CreatedAt { get; set; }

        public string ItemMethodInputText1 { get; set; }
        public string ItemMethodInputText2 { get; set; }
        public string ItemMethodInputText3 { get; set; }
    }

    //public class LogData
    //{
    //    [PrimaryKey, AutoIncrement]
    //    public int Id { get; set; }

    //    public string[] Chapter1Log { get; set; }
    //    public string[] Chapter2Log { get; set; }
    //    public string[] Chapter3Log { get; set; }
    //    public string[] Chapter4Log { get; set; }
    //    public string[] Chapter5Log { get; set; }
    //    public string[] Chapter6Log { get; set; }
    //    public string[] Chapter7Log { get; set; }
    //    public string[] Chapter8Log { get; set; }
    //    public string[] Chapter9Log { get; set; }
    //    public string[] Chapter10Log { get; set; }
    //    public string[] Chapter11Log { get; set; }
    //    public string[] Chapter12Log { get; set; }

    //}
}
