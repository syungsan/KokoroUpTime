using SQLite;

namespace DataModel
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

    //DataChapterの継承元となる抽象クラス
    public abstract class BaseDataChapter
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string CreatedAt { get; set; }
    }

    // チャプター1のログカラム
    public class DataChapter1
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string MyKindOfGoodFeelings { get; set; } = "";

        public string MyKindOfBadFeelings { get; set; }="";

        public string KimisKindOfFeeling { get; set; }="";

        public string AkamarusKindOfFeeling { get; set; }="";

        public int AkamarusSizeOfFeeling { get; set; } = -1;

        public string AosukesKindOfFeeling { get; set; }="";

        public int AosukesSizeOfFeeling { get; set; }=-1;

        public string CreatedAt { get; set; }
    }

    // チャプター2のログカラム
    public class DataChapter2
    {
        // このレコードは定石
        [PrimaryKey, AutoIncrement]
        // データベースに記録するレコードの型の宣言をしてください。
        public int Id { get; set; }

        public string MySelectGoodEvents { get; set; } = "";

        public int AosukesSizeOfFeelingOfEating { get; set; } = -1;

        public string AosukesDifficultyOfEating { get; set; }="";

        public int AosukesSizeOfFeelingOfGettingHighScore { get; set; }=-1;

        public string AosukesDifficultyOfGettingHighScore { get; set; }="";

        public int AosukesSizeOfFeelingOfTalkingWithFriend { get; set; }=-1;

        public string AosukesDifficultyOfTalkingWithFriend { get; set; }="";

        public string MyALittlleExcitingEvents { get; set; } = "";
        
        public string CreatedAt { get; set; }

    }

    // チャプター3のログカラム
    public class DataChapter3
    {
        [PrimaryKey, AutoIncrement]

        public int Id { get; set; }

        public string AosukesKindOfFeelingPreUseItem { get; set; } = "";

        public string KimisKindOfFeelingPreUseItem { get; set; }="";

        public string AosukesKindOfFeelingAfterUsedItem { get; set; }="";

        public string KimisKindOfFeelingAfterUsedItem { get; set; }="";

        public string AkamarusKindOfFeelingAfterUsedItem { get; set; }="";

        public int AosukesSizeOfFeelingPreUseItem { get; set; } = -1;

        public int KimisSizeOfFeelingPreUseItem { get; set; }=-1;

        public int AosukesSizeOfFeelingAfterUsedItem { get; set; }=-1;

        public int KimisSizeOfFeelingAfterUsedItem { get; set; }=-1;

        public int AkamarusSizeOfFeelingAfterUsedItem { get; set; }=-1;

        public string SelectedPraiseHotWord { get; set; }="";

        public string SelectedWorryHotWord { get; set; }="";

        public string SelectedEncourageHotWord { get; set; }="";

        public string SelectedThanksHotWord { get; set; }="";

        public string CreatedAt { get; set; }
    }

    // チャプター4のログカラム
    public class DataChapter4
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string KimisKindOfFeelingAskedForWork { get; set; } = "";
        public string KimisKindOfFeelingAskedByAkamaru { get; set; }="";

        public int KimisSizeOfFeelingAskedForWork { get; set; } = -1;
        public int KimisSizeOfFeelingAskedByAkamaru { get; set; }=-1;

        public string SelectedScene { get; set; } = "";

        public string CreatedAt { get; set; }
    }

    // チャプター5のログカラム
    public class DataChapter5
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string InputFaceImageOfKimiText { get; set; } = "";

        public string InputHandImageOfKimiText { get; set; }="";

        public string InputStomachImageOfKimiText { get; set; }="";

        public string InputOthersImageOfKimiText { get; set; }="";

        public string InputHeadImageOfKimiText { get; set; }="";

        public string InputShoulderImageOfKimiText { get; set; }="";

        public string InputLegImageOfKimiText { get; set; }="";

        public string KindOfFeelingNotUnderstandProblem { get; set; }="";

        public string KindOfFeelingRecorderProblem { get; set; }="";

        public int? SizeOfFeelingNotUnderstandProblem { get; set; } = -1;

        public int? SizeOfFeelingRecorderProblem { get; set; }=-1;

        public string InputMyBodyImageTextNotUnderstandProblem { get; set; } = "";

        public string InputMyBodyImageTextRecorderProblem { get; set; }="";

        public string InputRelaxMethodText { get; set; }="";

       public string CreatedAt { get; set; }
    }

    // チャプター6のログカラム
    public class DataChapter6
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string SelectedNicePersonality { get; set; } = "";
        public string InputFriendsNicePersonality { get; set; }="";

        public string CreatedAt { get; set; }
    }

    // チャプター7のログカラム
    public class DataChapter7
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string ChallengeTimeSelectedScene { get; set; } = "";
        
        public string GroupeActivitySelectedScene { get; set; }="";

        public string KimisKindOfFeelingInviteFriends { get; set; }="";

        public string KimisKindOfFeelingAnnouncement { get; set; }="";

        public string YourKindOfFeelingAnnouncement { get; set; }="";

        public string YourKindOfFeelingGreetingToFriend { get; set; }="";

        public string YourFriendsKindOfFeelingAnnouncement { get; set; }="";

        public string YourFriendsKindOfFeelingGreetingToAnotherFriend { get; set; }="";

        public int? KimisSizeOfFeelingInviteFriends { get; set; } = -1;

        public int? KimisSizeOfFeelingAnnouncement { get; set; }=-1;

        public int? YourSizeOfFeelingAnnouncement { get; set; }=-1;

        public int? YourSizeOfFeelingGreetingToFriend { get; set; }=-1;

        public int? YourFriendsSizeOfFeelingAnnouncement { get; set; }=-1;

        public int? YourFriendsSizeOfFeelingGreetingToAnotherFriend { get; set; }=-1;

        public string InputAkamaruThoughtText { get; set; } = "";

        public string InputAosukeThoughtText { get; set; }="";

        public string InputYourToughtText1 { get; set; }="";

        public string InputYourToughtText2 { get; set; }="";

        public string InputFriendToughtText1 { get; set; }="";

        public string InputFriendToughtText2 { get; set; }="";

        public string CreatedAt { get; set; }
    }

    // チャプター8のログカラム
    public class DataChapter8
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string AosukesKindOfFeelingPreUseItem { get; set; } = "";

        public string KimisKindOfFeelingPreUseItem { get; set; }="";

        public string AkamarusKindOfFeelingPreUseItem { get; set; }="";

        public string AosukesKindOfFeelingAfterUsedItem { get; set; }="";

        public string KimisKindOfFeelingAfterUsedItem { get; set; }="";

        public string AkamarusKindOfFeelingAfterUsedItem { get; set; }="";

        public string Let_sCheckKindOfFeeling { get; set; }="";

        public string PositiveThinkingKindOfFeeling { get; set; }="";

        public string ThoughtsOfOthersKindOfFeeling { get; set; }="";

        public int AosukesSizeOfFeelingPreUseItem { get; set; } = -1;

        public int KimisSizeOfFeelingPreUseItem { get; set; }=-1;

        public int AkamarusSizeOfFeelingPreUseItem { get; set; }=-1;

        public int AosukesSizeOfFeelingAfterUsedItem { get; set; }=-1;

        public int KimisSizeOfFeelingAfterUsedItem { get; set; }=-1;

        public int AkamarusSizeOfFeelingAfterUsedItem { get; set; }=-1;

        public int Let_sCheckSizeOfFeeling { get; set; }=-1;

        public int PositiveThinkingSizeOfFeeling { get; set; }=-1;

        public int ThoughtsOfOthersSizeOfFeeling { get; set; }=-1;

        public string GroupeActivityInputText1 { get; set; } = "";

        public string GroupeActivityInputText2 { get; set; }="";

        public string GroupeActivityInputText3 { get; set; }="";

        public string CreatedAt { get; set; }
    }

    // チャプター9のログカラム
    public class DataChapter9
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string CheckedNotGoodEvent { get; set; } = "";

        public string AosukesKindOfFeelingTalkToFriends { get; set; }="";

        public string AosukesKindOfFeelingAfter10minutes { get; set; }="";

        public string AosukesKindOfFeelingNextDay { get; set; }="";

        public string AosukesKindOfFeelingDayAfterTomorrow { get; set; }="";

        public int AosukesSizeOfFeelingTalkToFriends { get; set; } = -1;

        public int AosukesSizeOfFeelingAfter10minutes { get; set; }=-1;

        public int AosukesSizeOfFeelingNextDay { get; set; }=-1;

        public int AosukesSizeOfFeelingDayAfterTomorrow { get; set; }=-1;

        public string InputChallengeText { get; set; }="";

        public string CreatedAt { get; set; }

    }

    // チャプター10のログカラム
    public class DataChapter10
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string AosukeChallengeStep1KindOfFeeling { get; set; } = "";

        public string AosukeChallengeStep2KindOfFeeling { get; set; }="";

        public string AosukeChallengeStep3KindOfFeeling { get; set; }="";

        public int AosukeChallengeStep1SizeOfFeeling { get; set; }=-1;

        public int AosukeChallengeStep2SizeOfFeeling { get; set; }=-1;

        public int AosukeChallengeStep3SizeOfFeeling { get; set; }=-1;

        public string ReasonDifferenceSizeOfFeelingInputText { get; set; }="";

        public string AosukeSmallChallengeInputText {get; set;}="";

        public string AosukeChallengeStep1InputText {get; set;}="";

        public string AosukeChallengeStep2InputText {get; set;}="";

        public string AosukeChallengeStep3InputText {get; set;}="";

        public string CreatedAt { get; set; }

    }

    // チャプター11のログカラム
    public class DataChapter11
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string SelectedItem { get; set; } = "";
        public string AkamaruOtherSolutionUseItemInputText { get; set; }="";

        public string SelectedAkamaruGoalText { get; set; }="";

        public string Step2AkamaruManyMethodInputText { get; set; }="";

        public string EvaluationAkamaruMethodText1 { get; set; }="";
        public string EvaluationAkamaruMethodText2 { get; set; }="";
        public string EvaluationAkamaruMethodText3 { get; set; }="";

        public string Step2AosukeManyMethodInputText { get; set; }="";
        public string EvaluationAosukeMethodText { get; set; }="";

        public string CreatedAt { get; set; }

    }

    // チャプター12のログカラム
    public class DataChapter12
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string SelectedSceneText { get; set; } = "";

        public string SelectedItem1 { get; set; } = "";
        public string SelectedItem2 { get; set; } = "";
        public string SelectedItem3 { get; set; } = "";

        public string ItemMethodInputText1 { get; set; } = "";
        public string ItemMethodInputText2 { get; set; } = "";
        public string ItemMethodInputText3 { get; set; } = "";

        public int Item01NameIsCorrect {get;set;} = 0;
        public int Item01REviewIsCorrect { get; set; } = 0;
        public int Item02NameIsCorrect { get; set; } = 0;
        public int Item02REviewIsCorrect { get; set; } = 0; 
        public int Item03NameIsCorrect { get; set; } = 0;
        public int Item03REviewIsCorrect { get; set; } = 0;
        public int Item04NameIsCorrect { get; set; } = 0;
        public int Item04REviewIsCorrect { get; set; } = 0;
        public int Item05NameIsCorrect { get; set; } = 0;
        public int Item05REviewIsCorrect { get; set; } = 0;
        public int Item06NameIsCorrect { get; set; } = 0;
        public int Item06REviewIsCorrect { get; set; } = 0;
        public int Item07NameIsCorrect { get; set; } = 0;
        public int Item07REviewIsCorrect { get; set; } = 0;
        public int Item08NameIsCorrect { get; set; } = 0;
        public int Item08REviewIsCorrect { get; set; } = 0;
        public int Item09NameIsCorrect { get; set; } = 0;
        public int Item09REviewIsCorrect { get; set; } = 0;
        public int Item10NameIsCorrect { get; set; } = 0;
        public int Item10REviewIsCorrect { get; set; } = 0;
        public int Item11NameIsCorrect { get; set; } = 0;
        public int Item11REviewIsCorrect { get; set; } = 0;
        public int Item12NameIsCorrect { get; set; } = 0;
        public int Item12REviewIsCorrect { get; set; } = 0;






        public string CreatedAt { get; set; }
    }
}
