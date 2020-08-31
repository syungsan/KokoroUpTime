using CsvReadWrite;
using Expansion;
using FileIOUtils;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WMPLib;
using WpfAnimatedGif;



namespace KokoroUpTime
{
    /// <summary>
    /// GameWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class Chapter2 : Page
    {

        // 気持ちのリスト
        private string[] GOOD_FEELINGS = { "うれしい", "しあわせ", "たのしい", "ホッとした", "きもちいい", "まんぞく", "すき", "やる気マンマン", "かんしゃ", "わくわく", "うきうき", "ほこらしい" };
        private string[] BAD_FEELINGS = { "心配", "こまった", "不安", "こわい", "おちこみ", "がっかり", "いかり", "イライラ", "はずかしい", "ふまん", "かなしい", "おびえる" };

        // メッセージスピードはオプションで設定できるようにする
        private float MESSAGE_SPEED = 3000000.0f;

        // ゲームを進行させるシナリオ
        private int scenarioCount = 0;
        private List<List<string>> scenarios = null;

        // 各種コントロールの名前を収める変数
        private string position = "";

        // マウスクリックを可能にするかどうかのフラグ
        private bool isClickable = false;

        // 画面を何回タップしたか
        private int tapCount = 0;

        // 気持ちの大きさ
        private int feelingSize = 0;

        // メッセージ表示関連
        private DispatcherTimer msgTimer;
        private int word_num;

        private int inlineCount;
        private int imageInlineCount;

        private List<Run> runs = new List<Run>();
        private List<InlineUIContainer> imageInlines = new List<InlineUIContainer>();
        

        // 各種コントロールを任意の文字列で呼び出すための辞書
        private Dictionary<string, Image> imageObjects = null;
        private Dictionary<string, TextBlock> textBlockObjects = null;
        private Dictionary<string, Button> buttonObjects = null;
        private Dictionary<string, Grid> gridObjects = null;

        private Dictionary<string, Ellipse> GoodEventObject = null;

        private string[] EDIT_BUTTON = { "えんぴつ", "けしごむ", "すべてけす", "かんせい" };
        private string[] IMAGE_TEXTS = { "name" };
        private string[] WORD_TEXTS = { "marker", "bold" };

        private Dictionary<string, SolidColorBrush> CharacterColor = null;


        // 音関連
        private WindowsMediaPlayer mediaPlayer;
        private SoundPlayer sePlayer = null;

        // データベースに収めるデータモデルのインスタン
        private DataChapter2 dataChapter2;

        // データベースのパスを通す
        private string dbPath;

        // ゲームの切り替えシーン
        private string scene;

        private string tag;

        // 仮のユーザネームを設定
        public string userName = "なまえ";

        // なったことのある自分の気持ちの一時記録用
        public List<string> mySelectGoodEvents = new List<string>();

        public int aosukesSizeOfFeelingOfSleeping;
        public string aosukesKindOfFeelingOfSleeping;
        public string aosukesDifficultyOfSleeping;

        public string aosukesSizeOfFeelingOfEating;
        public string aosukesKindOfFeelingOfEating;
        public string aosukesDifficultyOfEating;

        public string aosukesSizeOfFeelingOfGettingHighScore;
        public string aosukesKindOfFeelingOfGettingHighScore;
        public string aosukesDifficultyOfGettingHighScore;

        public string aosukesSizeOfFeelingOfTalkingWithFriend;
        public string aosukesKindOfFeelingOfTalkingWithFriend;
        public string aosukesDifficultyOfTalkingWithFriend;

        public Image MyALittlleExcitingEvents ;



        // データベースに収めるデータモデルのインスタンス
        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();
        public DataProgress dataProgress = new DataProgress();


        public Chapter2()
        {
            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            // メディアプレーヤークラスのインスタンスを作成する
            this.mediaPlayer = new WindowsMediaPlayer();

            // マウスイベントの設定
            this.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
            this.MouseUp += new MouseButtonEventHandler(OnMouseUp);
            this.MouseMove += new MouseEventHandler(OnMouseMove);

            // データモデルインスタンス確保
            this.dataChapter2 = new DataChapter2();

            // データベース本体のファイルのパス設定
            string dbName = $"{userName}.sqlite";
            string dirPath = $"./Log/{userName}/";

            // FileUtils.csからディレクトリ作成のメソッド
            // 各ユーザの初回起動のとき実行ファイルの場所下のLogフォルダにユーザネームのフォルダを作る
            DirectoryUtils.SafeCreateDirectory(dirPath);

            this.dbPath = System.IO.Path.Combine(dirPath, dbName);

            // 現在時刻を取得
            this.dataChapter2.CreatedAt = DateTime.Now.ToString();

            // データベースのテーブル作成と現在時刻の書き込みを同時に行う
            using (var connection = new SQLiteConnection(dbPath))
            {
                // 仮（本当は名前を登録するタイミングで）
                connection.CreateTable<DataOption>();
                connection.CreateTable<DataProgress>();
                connection.CreateTable<DataChapter2>();

                // 毎回のアクセス日付を記録
                connection.Insert(this.dataChapter2);
            }

            this.EditingModeItemsControl.ItemsSource = EDIT_BUTTON;

            this.CharacterColor = new Dictionary<string, SolidColorBrush>
            {
                ["白じい"] = new SolidColorBrush(Colors.Purple),
                ["青助"] = new SolidColorBrush(Colors.Aqua),
                ["赤丸"] = new SolidColorBrush(Colors.Red), 
                ["キミ"] =new SolidColorBrush(Colors.Yellow),
            };

            this.InitControls();
        }

        private void InitControls()
        {
            // 各種コントロールに任意の文字列をあてがう

            this.imageObjects = new Dictionary<string, Image>
            {
                ["bg_image"] = this.BackgroundImage,
                ["manga_title_image"] = this.MangaTitleImage,
                ["manga_image"] = this.MangaImage,
                ["item_center_image"] = this.ItemCenterImage,
                ["item_left_image"] = this.ItemLeftImage,
                ["item_left_last_image"] = this.ItemLeftLastImage,
                ["session_title_image"] = this.SessionTitleImage,
                ["kimi_in_plate_image"] = this.KimiInPlateImage,
                ["kimi_plate_outer_image"] = this.KimiPlateOuterImage,
                ["select_heart_image"] = this.SelectHeartImage,
                ["select_needle_image"] = this.SelectNeedleImage,
                ["children_info_image"] = this.ChildrenInfoImage,
                ["shiroji_ending_image"] = this.ShirojiEndingImage,
                ["shiroji_left_image"] = this.ShirojiLeftImage,
                ["shiroji_right_image"] = this.ShirojiRightImage,
                ["shiroji_right_up_image"] = this.ShirojiRightUpImage,
                ["shiroji_small_right_up_image"] = this.ShirojiSmallRightUpImage,
                ["shiroji_small_right_down_image"] = this.ShirojiSmallRightDownImage,
                ["shiroji_right_center_image"] = this.ShirojiRightCenterImage,
                ["shiroji_small_right_center_image"] = this.ShirojiSmallRightCenterImage,
                ["shiroji_very_small_right_image"] = this.ShirojiVerySmallRightImage,
                ["shiroji_center_down_small_image"]=this.ShirojiCenterDownSmallImage,
                ["children_stand_left_image"] = this.ChildrenStandLeftImage,
                ["children_stand_right_image"] = this.ChildrenStandRightImage,
                ["kimi_stand_small_left_image"] = this.KimiStandSmallLeftImage,

         
                ["children_face_image"] = this.ChildrenFaceImage,
                ["children_face_small_left_image"] = this.ChildrenFaceSmallLeftImage,
                ["main_msg_bubble_image"] = this.MainMessageBubbleImage,
                ["item_point_message_bubble_image"]=this.ItemPointMessageBubbleImage,
                ["item_left_last_image"] =this.ItemLeftLastImage,
            };

            this.textBlockObjects = new Dictionary<string, TextBlock>
            {
                ["session_sub_title_text"] = this.SessionSubTitleTextBlock,
                ["session_sentence_text"] = this.SessionSentenceTextBlock,
               
                ["case_of_kimi_text"] = this.CaseOfKimiTextBlock,
                ["kimi_scene1_text"] = this.KimiScene1TextBlock,
                ["kimi_kind_of_feeling_up_text"] = this.KimiKindOfFeelingUpTextBlock,
                ["kimi_size_of_feeling_up_text"] = this.KimiSizeOfFeelingUpTextBlock,
                ["kimi_scene2_text"] = this.KimiScene2TextBlock,
                ["kimi_kind_of_feeling_down_text"] = this.KimiKindOfFeelingDownTextBlock,
                ["kimi_size_of_feeling_down_text"] = this.KimiSizeOfFeelingDownTextBlock,
                ["compare_msg_text"] = this.CompareMessageTextBlock,
                ["kind_of_feeling_akamaru_text"] = this.KindOfFeelingAkamaruTextBlock,
                ["size_of_feeling_akamaru_text"] = this.SizeOfFeelingAkamaruTextBlock,
                ["kind_of_feeling_aosuke_text"] = this.KindOfFeelingAosukeTextBlock,
                ["size_of_feeling_aosuke_text"] = this.SizeOfFeelingAosukeTextBlock,
                ["ending_msg_text"] = this.EndingMessageTextBlock,
                ["epilogue_text"]=this.EpilogueText,
                ["children_face_small_left_msg_text"] = this.ChildrenFaceSmallLeftMessageTextBlock,
                ["main_msg"] = this.MainMessageTextBlock,
                ["thin_msg"] = this.ThinMessageTextBlock,
                ["music_title_text"] = this.MusicTitleTextBlock,
                ["composer_name_text"] = this.ComposerNameTextBlock,
                ["challenge2_bubble_action_text"] =this.Challenge2BubbleActionText,
                ["aosuke_difficulty_of_action_text"] =this.AosukeDifficultyOfActionText,
                ["aosuke_kind_of_feeling_text"] =this.AosukeKindOfFeelingText,
                ["aosuke_size_of_feeling_text"] = this.AosukeSizeOfFeelingText,
                // ["aosuke_size_of_feeling_text"] =this.AosukeSizeOfFeelingText,
                ["item_point_msg_text"] = this.ItemPointMessageText,
                ["challenge2_bubble_action_text"]=this.Challenge2BubbleActionText,
                ["item_book_title_text"]=this.ItemBookTitleTextBlock,
                

                ["GoodEventText1"] = this.GoodEventText1,
                ["GoodEventText2"] = this.GoodEventText2,
                ["GoodEventText3"] = this.GoodEventText3,
                ["GoodEventText4"] = this.GoodEventText4,
                ["GoodEventText5"] = this.GoodEventText5,
                ["GoodEventText6"] = this.GoodEventText6,
                ["GoodEventText7"] = this.GoodEventText7,
                ["GoodEventText8"] = this.GoodEventText8,
                ["GoodEventText9"] = this.GoodEventText9,
                ["GoodEventText10"] = this.GoodEventText10,
                ["GoodEventText11"] = this.GoodEventText11,
                ["GoodEventText12"] = this.GoodEventText12,
                ["GoodEventText13"] = this.GoodEventText13,
                ["GoodEventText14"] = this.GoodEventText14,

            };

            this.buttonObjects = new Dictionary<string, Button>
            {
                ["next_msg_button"] = this.NextMessageButton,
                ["back_msg_button"] = this.BackMessageButton,
                ["thin_msg_button"] = this.ThinMessageButton,
                ["next_page_button"] = this.NextPageButton,
                ["back_page_button"] = this.BackPageButton,
                ["manga_flip_button"] = this.MangaFlipButton,
                ["select_feeling_complete_button"] = this.SelectFeelingCompleteButton,
                ["select_feeling_next_button"] = this.SelectFeelingNextButton,
            };

            this.gridObjects = new Dictionary<string, Grid>
            {
                ["session_grid"] = this.SessionGrid,
                ["challenge1_grid"] = this.Challenge1Grid,
                ["challenge_time_grid"] = this.ChallegeTimeGrid,
                ["challenge_time_title_grid"] =this.ChallengeTimeTitleGrid,
                ["groupe_activity_grid"]=this.GroupeActivityGrid,
                ["item_plate_grid"]=this.ItemPlateGrid,
                ["challenge2_grid"] = this.Challenge2Grid,

                ["summary_grid"] = this.SummaryGrid,
                ["ending_grid"] = this.EndingGrid,
                ["item_name_plate_left_grid"] = this.ItemNamePlateLeftGrid,
                ["item_name_bubble_grid"] = this.ItemNameBubbleGrid,
                ["item_name_plate_center_grid"] = this.ItemNamePlateCenterGrid,
                ["item_info_plate_grid"] = this.ItemInfoPlateGrid,
                ["item_info_sentence_grid"] = this.ItemInfoSentenceGrid,
                ["item_last_info_grid"] = this.ItemLastInfoGrid,
                ["item_review_grid"] = this.ItemReviewGrid,
                ["challenge_msg_grid"] = this.ChallengeMessageGrid,
                ["kimi_plate_inner_up_grid"] = this.KimiPlateInnerUpGrid,
                ["kimi_plate_inner_down_grid"] = this.KimiPlateInnerDownGrid,
                ["item_plate_grid"]=this.ItemPlateGrid,
                
               
                ["children_face_small_left_msg_grid"] = this.ChildrenFaceSmallLeftMessageGrid,
                ["select_heart_grid"] = this.SelectHeartGrid,
                ["akamaru_and_aosuke_compare_grid"] = this.AkamaruAndAosukeCompareGrid,
                ["compare_msg_grid"] = this.CompareMessageGrid,
                ["ending_msg_grid"] = this.EndingMessageGrid,
                ["main_msg_grid"] = this.MainMessageGrid,
                ["item_point_message_grid"] =this.ItemPointMessageGrid,
                ["music_info_grid"] = this.MusicInfoGrid,
                ["branch_select_grid"] = this.BranchSelectGrid,
                ["challenge_time_title_grid"] = this.ChallengeTimeTitleGrid,
                ["view_size_of_feeling_grid"]=this.ViewSizeOfFeelingGrid,
                ["select_heart_grid"]=this.SelectHeartGrid,
                ["difficulty_select_grid"] =this.DifficultySelectGrid,
                ["select_feeling_grid"] =this.SelectFeelingGrid,
                //["exit_back_grid"] = this.ExitBackGrid,
                ["epilogue_grid"] =this.Epiloguegrid,
            };

            GoodEventObject = new Dictionary<string, Ellipse>
            {
                ["GoodEventButton1"] = this.SelectCircle1,
                ["GoodEventButton2"] = this.SelectCircle2,
                ["GoodEventButton3"] = this.SelectCircle3,
                ["GoodEventButton4"] = this.SelectCircle4,
                ["GoodEventButton5"] = this.SelectCircle5,
                ["GoodEventButton6"] = this.SelectCircle6,
                ["GoodEventButton7"] = this.SelectCircle7,
                ["GoodEventButton8"] = this.SelectCircle8,
                ["GoodEventButton9"] = this.SelectCircle9,
                ["GoodEventButton10"] = this.SelectCircle10,
                ["GoodEventButton11"] = this.SelectCircle11,
                ["GoodEventButton12"] = this.SelectCircle12,
                ["GoodEventButton13"] = this.SelectCircle13,
                ["GoodEventButton14"] = this.SelectCircle14,
            };
        }

        private void ResetControls()
        {
            // 各種コントロールを隠すことでフルリセット

            this.SessionGrid.Visibility = Visibility.Hidden;
            this.Challenge1Grid.Visibility = Visibility.Hidden;
            this.Challenge2Grid.Visibility = Visibility.Hidden;
            this.ChallengeTimeTitleGrid.Visibility = Visibility.Hidden;
            this.GroupeActivityGrid.Visibility = Visibility.Hidden;
            this.CanvasGrid.Visibility = Visibility.Hidden;
            this.ItemPlateGrid.Visibility = Visibility.Hidden;

            this.SummaryGrid.Visibility = Visibility.Hidden;
            this.EndingGrid.Visibility = Visibility.Hidden;
            this.ItemNamePlateLeftGrid.Visibility = Visibility.Hidden;
            this.ItemNameBubbleGrid.Visibility = Visibility.Hidden;
            this.ItemNamePlateCenterGrid.Visibility = Visibility.Hidden;
            this.ItemInfoPlateGrid.Visibility = Visibility.Hidden;
            this.ItemLastInfoGrid.Visibility = Visibility.Hidden;
            this.ItemPointMessageGrid.Visibility = Visibility.Hidden;

            this.ItemReviewGrid.Visibility = Visibility.Hidden;
            this.ChallegeTimeGrid.Visibility = Visibility.Hidden;
            this.Epiloguegrid.Visibility = Visibility.Hidden;

          
            this.ChildrenFaceSmallLeftMessageGrid.Visibility = Visibility.Hidden;
            this.ChallengeMessageGrid.Visibility = Visibility.Hidden;
            this.KimiPlateInnerUpGrid.Visibility = Visibility.Hidden;
            this.KimiPlateInnerDownGrid.Visibility = Visibility.Hidden;
            this.SelectHeartGrid.Visibility = Visibility.Hidden;
          
            this.AkamaruAndAosukeCompareGrid.Visibility = Visibility.Hidden;
            this.CompareMessageGrid.Visibility = Visibility.Hidden;
            this.EndingMessageGrid.Visibility = Visibility.Hidden;
            this.MainMessageGrid.Visibility = Visibility.Hidden;
            this.MusicInfoGrid.Visibility = Visibility.Hidden;
            this.ItemBookGrid.Visibility = Visibility.Hidden;

            //this.ExitBackGrid.Visibility = Visibility.Hidden;

            this.BranchSelectGrid.Visibility = Visibility.Hidden;

            this.BackgroundImage.Visibility = Visibility.Hidden;
            this.MangaTitleImage.Visibility = Visibility.Hidden;
            this.MangaImage.Visibility = Visibility.Hidden;
            this.ItemCenterImage.Visibility = Visibility.Hidden;
            this.ItemLeftImage.Visibility = Visibility.Hidden;
            this.ItemLeftLastImage.Visibility = Visibility.Hidden;
            this.SessionTitleImage.Visibility = Visibility.Hidden;
            this.SessionSubTitleTextBlock.Visibility = Visibility.Hidden;
            this.SessionSentenceTextBlock.Visibility = Visibility.Hidden;
            this.SelectFeelingCompleteButton.Visibility = Visibility.Hidden;
            this.SelectFeelingNextButton.Visibility = Visibility.Hidden;
            this.KimiPlateOuterImage.Visibility = Visibility.Hidden;
            this.KimiInPlateImage.Visibility = Visibility.Hidden;
            this.CaseOfKimiTextBlock.Visibility = Visibility.Hidden;
            this.KimiScene1TextBlock.Visibility = Visibility.Hidden;
            this.KimiKindOfFeelingUpTextBlock.Visibility = Visibility.Hidden;
            this.KimiSizeOfFeelingUpTextBlock.Visibility = Visibility.Hidden;
            this.KimiScene2TextBlock.Visibility = Visibility.Hidden;
            this.KimiKindOfFeelingDownTextBlock.Visibility = Visibility.Hidden;
            this.KimiSizeOfFeelingDownTextBlock.Visibility = Visibility.Hidden;
           
            this.ChildrenFaceSmallLeftImage.Visibility = Visibility.Hidden;
            this.ChildrenFaceSmallLeftMessageTextBlock.Visibility = Visibility.Hidden;
            this.CompareMessageTextBlock.Visibility = Visibility.Hidden;
            this.KindOfFeelingAkamaruTextBlock.Visibility = Visibility.Hidden;
            this.SizeOfFeelingAkamaruTextBlock.Visibility = Visibility.Hidden;
            this.KindOfFeelingAosukeTextBlock.Visibility = Visibility.Hidden;
            this.SizeOfFeelingAosukeTextBlock.Visibility = Visibility.Hidden;
            this.ChildrenInfoImage.Visibility = Visibility.Hidden;
            this.EndingMessageTextBlock.Visibility = Visibility.Hidden;
            this.ShirojiRightImage.Visibility = Visibility.Hidden;
            this.ShirojiLeftImage.Visibility = Visibility.Hidden;
            this.ShirojiRightUpImage.Visibility = Visibility.Hidden;
            this.ShirojiSmallRightUpImage.Visibility = Visibility.Hidden;
            this.ShirojiSmallRightDownImage.Visibility = Visibility.Hidden;
            this.ShirojiRightCenterImage.Visibility = Visibility.Hidden;
            this.ShirojiSmallRightCenterImage.Visibility = Visibility.Hidden;
            this.ShirojiVerySmallRightImage.Visibility = Visibility.Hidden;
            this.ShirojiEndingImage.Visibility = Visibility.Hidden;
            this.ChildrenStandLeftImage.Visibility = Visibility.Hidden;
            this.ChildrenStandRightImage.Visibility = Visibility.Hidden;
            this.KimiStandSmallLeftImage.Visibility = Visibility.Hidden;
            this.ShirojiCenterDownSmallImage.Visibility = Visibility.Hidden;


            this.ChildrenFaceImage.Visibility = Visibility.Hidden;
            this.MainMessageTextBlock.Visibility = Visibility.Hidden;
            this.ThinMessageButton.Visibility = Visibility.Hidden;
            this.ThinMessageTextBlock.Visibility = Visibility.Hidden;
            this.NextMessageButton.Visibility = Visibility.Hidden;
            this.BackMessageButton.Visibility = Visibility.Hidden;
            this.NextPageButton.Visibility = Visibility.Hidden;
            this.BackPageButton.Visibility = Visibility.Hidden;
            this.MangaFlipButton.Visibility = Visibility.Hidden;
            this.ItemPointMessageBubbleImage.Visibility = Visibility.Hidden;

            this.CoverLayerImage.Visibility = Visibility.Hidden;

            this.SelectCircle1.Visibility = Visibility.Hidden;
            this.SelectCircle2.Visibility = Visibility.Hidden;
            this.SelectCircle3.Visibility = Visibility.Hidden;
            this.SelectCircle4.Visibility = Visibility.Hidden;
            this.SelectCircle5.Visibility = Visibility.Hidden;
            this.SelectCircle6.Visibility = Visibility.Hidden;
            this.SelectCircle7.Visibility = Visibility.Hidden;
            this.SelectCircle8.Visibility = Visibility.Hidden;
            this.SelectCircle9.Visibility = Visibility.Hidden;
            this.SelectCircle10.Visibility = Visibility.Hidden;
            this.SelectCircle11.Visibility = Visibility.Hidden;
            this.SelectCircle12.Visibility = Visibility.Hidden;
            this.SelectCircle13.Visibility = Visibility.Hidden;
            this.SelectCircle14.Visibility = Visibility.Hidden;

            this.ViewSizeOfFeelingGrid.Visibility = Visibility.Hidden;
            this.DifficultySelectGrid.Visibility = Visibility.Hidden;
            this.SelectFeelingGrid.Visibility = Visibility.Hidden;

            this.ReturnToTitleButton.Visibility = Visibility.Hidden;
            this.CanvasGrid.Visibility = Visibility.Hidden;

            this.SessionSubTitleTextBlock.Text = "";
            this.SessionSentenceTextBlock.Text = "";
           
            this.ChildrenFaceSmallLeftMessageTextBlock.Text = "";
            this.CompareMessageTextBlock.Text = "";
            this.KindOfFeelingAkamaruTextBlock.Text = "";
            this.SizeOfFeelingAkamaruTextBlock.Text = "";
            this.KindOfFeelingAosukeTextBlock.Text = "";
            this.SizeOfFeelingAosukeTextBlock.Text = "";
            this.EndingMessageTextBlock.Text = "";
            //this.FrexibleMainMessageText.Text = "";
            this.ThinMessageTextBlock.Text = "";
            this.MusicTitleTextBlock.Text = "";
            this.ComposerNameTextBlock.Text = "";
            this.ItemPointMessageText.Text = "";

            this.AosukeDifficultyOfActionText.Text = "";
            this.AosukeKindOfFeelingText.Text = "";
            this.AosukeSizeOfFeelingText.Text = "";
            this.Challenge2BubbleActionText.Text = "";

            this.EpilogueText.Text = "";



        }

        public void SetNextPage(InitConfig _initConfig, DataOption _dataOption, DataItem _dataItem, DataProgress _dataProgress)
        {
            this.initConfig = _initConfig;
            this.dataOption = _dataOption;
            this.dataItem = _dataItem;
            this.dataProgress = _dataProgress;

            // 現在時刻を取得
            this.dataChapter2.CreatedAt = DateTime.Now.ToString();

            // データベースのテーブル作成と現在時刻の書き込みを同時に行う
            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
            {
                // 毎回のアクセス日付を記録
                connection.Insert(this.dataChapter2);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var csv = new CsvReader("./Scenarios/chapter2.csv"))
            {
                this.scenarios = csv.ReadToEnd();
            }
            this.ScenarioPlay();
        }

        public void SetInitConfig(InitConfig _initConfig)
        {
            this.initConfig = _initConfig;

            // データベース本体のファイルのパス設定
            string dbName = $"{initConfig.userName}.sqlite";
            string dirPath = $"./Log/{initConfig.userName}_{initConfig.userTitle}/";

            // FileUtils.csからディレクトリ作成のメソッド
            // 各ユーザの初回起動のとき実行ファイルの場所下のLogフォルダにユーザネームのフォルダを作る
            DirectoryUtils.SafeCreateDirectory(dirPath);

            this.dbPath = System.IO.Path.Combine(dirPath, dbName);

            // 現在時刻を取得
            this.dataChapter2.CreatedAt = DateTime.Now.ToString();

            // データベースのテーブル作成と現在時刻の書き込みを同時に行う
            using (var connection = new SQLiteConnection(this.dbPath))
            {
                // 仮（本当は名前を登録するタイミングで）
                connection.CreateTable<DataOption>();
                connection.CreateTable<DataProgress>();
                connection.CreateTable<DataChapter2>();

                // 毎回のアクセス日付を記録
                connection.Insert(this.dataChapter2);
            }
        }

        public void SetDataOption(DataOption _dataOption)
        {
            this.dataOption = _dataOption;
        }
        // ゲーム進行の中核
        private void ScenarioPlay()
        {
            // デバッグのためシナリオのインデックスを出力
            Debug.Print((this.scenarioCount + 1).ToString());

            // 処理分岐のフラグ
            var tag = this.scenarios[this.scenarioCount][0];

            // メッセージ表示関連
            this.word_num = 0;

            switch (tag)
            {
                case "end":

                    // 画面のフェードアウト処理とか入れる（別関数を呼び出す）

                    this.dataProgress.HasCompletedChapter1 = true;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET HasCompletedChapter1 = '{Convert.ToInt32(this.dataProgress.HasCompletedChapter1)}' WHERE Id = 1;");
                    }
                    this.ReturnToTitleButton.Visibility = Visibility.Visible;

                    break;
                // フルリセット
                case "reset":

                    this.ResetControls();

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                // シーン名を取得
                case "scene":

                    this.scene = this.scenarios[this.scenarioCount][1]; 

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "tag":
                    this.Tag = this.scenarios[this.scenarioCount][1];

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                    break;

                // グリッドに対しての処理
                case "grid":

                    // グリッドコントロールを任意の名前により取得
                    this.position = this.scenarios[this.scenarioCount][1];

                    var gridObject = this.gridObjects[this.position];

                    gridObject.Visibility = Visibility.Visible;

                    // アニメ処理をシナリオの流れと同期するか（待ち処理入れる）か非同期にするか（同時進行するか）
                    string gridAnimeIsSync = "sync";

                    // if文は必要のない処理をコンマ以降で指定しなくてよくするため
                    if (this.scenarios[this.scenarioCount].Count > 3 && this.scenarios[this.scenarioCount][3] != "")
                    {
                        gridAnimeIsSync = this.scenarios[this.scenarioCount][3];
                    }

                    // アニメを実現するストーリーボードの指定
                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var gridStoryBoard = this.scenarios[this.scenarioCount][2];

                        // ストーリーボードの名前にコントロールの名前を付け足す
                        gridStoryBoard += $"_{this.position}";

                        this.ShowAnime(storyBoard: gridStoryBoard, isSync: gridAnimeIsSync);
                    }
                    else
                    {
                        // アニメの処理をしない場合はそのまま次に進む
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    break;

                // イメージに対しての処理
                case "image":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var imageObject = this.imageObjects[this.position];

                    string imageFile = "";

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        imageFile = this.scenarios[this.scenarioCount][2];

                        // フォルダの画像でなくリソース内の画像を表示することでスピードアップ
                        imageObject.Source = new BitmapImage(new Uri($"Images/{imageFile}", UriKind.Relative));
                    }

                    if (this.scenarios[this.scenarioCount].Count > 5 && this.scenarios[this.scenarioCount][5] != "")
                    {
                        if (this.scenarios[this.scenarioCount][5] == "gray")
                        {
                            // BitmapSourceを表示する
                            imageObject.Source = this.Image2Gray(imageObject.Source);
                        }
                    }

                    imageObject.Visibility = Visibility.Visible;

                    string imageAnimeIsSync = "sync";

                    if (this.scenarios[this.scenarioCount].Count > 4 && this.scenarios[this.scenarioCount][4] != "")
                    {
                        imageAnimeIsSync = this.scenarios[this.scenarioCount][4];
                    }

                    if (this.scenarios[this.scenarioCount].Count > 3 && this.scenarios[this.scenarioCount][3] != "")
                    {
                        var imageStoryBoard = this.scenarios[this.scenarioCount][3];

                        imageStoryBoard += $"_{this.position}";

                        this.ShowAnime(storyBoard: imageStoryBoard, isSync: imageAnimeIsSync);
                    }
                    else
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    break;

                // ボタンに対する処理
                case "button":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var buttonObject = this.buttonObjects[this.position];

                    buttonObject.Visibility = Visibility.Visible;

                    string buttonAnimeIsSync = "sync";

                    if (this.scenarios[this.scenarioCount].Count > 3 && this.scenarios[this.scenarioCount][3] != "")
                    {
                        buttonAnimeIsSync = this.scenarios[this.scenarioCount][3];
                    }

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var buttonStoryBoard = this.scenarios[this.scenarioCount][2];

                        buttonStoryBoard += $"_{this.position}";

                        this.ShowAnime(storyBoard: buttonStoryBoard, isSync: buttonAnimeIsSync);
                    }
                    else
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    break;

                // 流れる文字をTextBlockで表現するための処理
                case "msg":

                    this.NextMessageButton.Visibility = Visibility.Hidden;
                    this.BackMessageButton.Visibility = Visibility.Hidden;

                    this.position = this.scenarios[this.scenarioCount][1];

                    var _textObject = this.textBlockObjects[this.position];

                    _textObject.Visibility = Visibility.Hidden;

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var _message = this.scenarios[this.scenarioCount][2];

                        var _messages = this.SequenceCheck(_message);

                        this.ShowMessage(textObject: _textObject, messages: _messages);
                    }
                    else
                    {
                        // xamlに直接書いたStaticな文章を表示する場合
                       // this.ShowMessage(textObject: _textObject, messages: _textObject.Text);
                    }
                    break;

                // 流れない文字に対するTextBlock処理
                case "text":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var textObject = this.textBlockObjects[this.position];

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var _text = this.scenarios[this.scenarioCount][2];

                        textObject.Text = _text;
                    }

                    // 色を変えれるようにする
                    if (this.scenarios[this.scenarioCount].Count > 3 && this.scenarios[this.scenarioCount][3] != "")
                    {
                        var textColor = this.scenarios[this.scenarioCount][3];

                        SolidColorBrush textColorBrush = new SolidColorBrush(Colors.Black);

                        switch (textColor)
                        {
                            case "white":
                                textColorBrush = new SolidColorBrush(Colors.White);
                                break;

                            case "red":
                                textColorBrush = new SolidColorBrush(Colors.Red);
                                break;
                        }
                        textObject.Foreground = textColorBrush;
                    }
                    textObject.Visibility = Visibility.Visible;

                    string textAnimeIsSync = "sync";

                    // テキストに対するアニメも一応用意
                    if (this.scenarios[this.scenarioCount].Count > 5 && this.scenarios[this.scenarioCount][5] != "")
                    {
                        textAnimeIsSync = this.scenarios[this.scenarioCount][5];
                    }

                    if (this.scenarios[this.scenarioCount].Count > 4 && this.scenarios[this.scenarioCount][4] != "")
                    {
                        var textStoryBoard = this.scenarios[this.scenarioCount][4];

                        textStoryBoard += $"_{this.position}";

                        this.ShowAnime(storyBoard: textStoryBoard, isSync: textAnimeIsSync);
                    }
                    else
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    break;

                // メッセージに対する待ち（メッセージボタンの表示切り替え）
                case "wait":


                    

                    bool msgButtonVisible = true;

                    if (this.scenarios[this.scenarioCount].Count > 1 && this.scenarios[this.scenarioCount][1] != "")
                    {
                        var _msgButtonVisible = this.scenarios[this.scenarioCount][1];

                        if (_msgButtonVisible == "no_button")
                        {
                            msgButtonVisible = false;
                        }
                    }

                    if (msgButtonVisible)
                    {
                        this.NextMessageButton.Visibility = Visibility.Visible;
                        this.BackMessageButton.Visibility = Visibility.Visible;
                    }
                    this.isClickable = true;

                    break;

                // 各場面に対する待ち（ページめくりボタン）
                case "next":

                    

                    this.NextPageButton.Visibility = Visibility.Visible;
                    this.BackPageButton.Visibility = Visibility.Visible;

                    this.isClickable = true;

                    break;

                // 漫画めくり
                case "flip":

                    this.MangaFlipButton.Visibility = Visibility.Visible;
                    this.isClickable = true;

                    break;

                // 各種コントロールを個別に隠す処理
                case "hide":

                    // オブジェクトを消すときは後々ほとんどアニメで処理するようにする
                    var hideTarget = this.scenarios[this.scenarioCount][1];

                    switch (hideTarget)
                    {
                        case "image":

                            this.position = this.scenarios[this.scenarioCount][2];
                            this.imageObjects[this.position].Visibility = Visibility.Hidden;

                            this.scenarioCount += 1;
                            this.ScenarioPlay();

                            break;

                        case "text":

                            this.position = this.scenarios[this.scenarioCount][2];
                            this.textBlockObjects[this.position].Visibility = Visibility.Hidden;
                            // this.textBlockObjects[this.position].Text = "";

                            this.scenarioCount += 1;
                            this.ScenarioPlay();

                            break;

                        case "button":

                            this.position = this.scenarios[this.scenarioCount][2];
                            this.buttonObjects[this.position].Visibility = Visibility.Hidden;

                            this.scenarioCount += 1;
                            this.ScenarioPlay();

                            break;

                        case "grid":

                            this.NextMessageButton.Visibility = Visibility.Hidden;
                            this.BackMessageButton.Visibility = Visibility.Hidden;

                            this.position = this.scenarios[this.scenarioCount][2];
                            this.gridObjects[this.position].Visibility = Visibility.Hidden;

                            this.scenarioCount += 1;
                            this.ScenarioPlay();

                            break;
                    }
                    break;

                // テキストコントロール一般の文字を消す処理
                case "clear":

                    var clearTarget = this.scenarios[this.scenarioCount][1];

                    switch (clearTarget)
                    {
                        case "text":

                            this.position = this.scenarios[this.scenarioCount][2];
                            this.textBlockObjects[this.position].Text = "";

                            this.scenarioCount += 1;
                            this.ScenarioPlay();

                            break;
                    }
                    break;



                case "wait_tap":

                    this.isClickable = false;
                
                    //if()
                   
                    //if()
                   

                    break;

                // BGM
                case "bgm":

                    var bgmStatus = this.scenarios[this.scenarioCount][1];

                    switch (bgmStatus)
                    {
                        case "set":

                            var bgmFile = this.scenarios[this.scenarioCount][2];

                            bool _isLoop = false;

                            if (this.scenarios[this.scenarioCount].Count > 3 && this.scenarios[this.scenarioCount][3] != "")
                            {
                                var loopStr = this.scenarios[this.scenarioCount][3];

                                if (loopStr == "loop")
                                {
                                    _isLoop = true;
                                }
                            }

                            int bgmVolume = 100;

                            if (this.scenarios[this.scenarioCount].Count > 4 && this.scenarios[this.scenarioCount][4] != "")
                            {
                                bgmVolume = int.Parse(this.scenarios[this.scenarioCount][4]);
                            }

                            this.SetBGM(soundFile: bgmFile, isLoop: _isLoop, volume: bgmVolume);

                            break;

                        case "play":

                            this.PlayBGM();
                            break;

                        case "stop":

                            this.StopBGM();
                            break;

                        case "pause":

                            this.PauseBGM();
                            break;
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                // 効果音
                case "se":

                    var seStatus = this.scenarios[this.scenarioCount][1];

                    switch (seStatus)
                    {
                        case "play":

                            var seFile = this.scenarios[this.scenarioCount][2];

                            string exePath = Environment.GetCommandLineArgs()[0];
                            string exeFullPath = System.IO.Path.GetFullPath(exePath);
                            string startupPath = System.IO.Path.GetDirectoryName(exeFullPath);

                            this.PlaySE(soundFile: $@"{startupPath}/Sounds/{seFile}");

                            break;

                        case "stop":

                            this.StopSE();
                            break;
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;
                case "gauge":

                    this.ViewSizeOfFeelingTextBlock.Text = "50";
                    this.Angle = 0;
                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;
                case "jump":
                    string _jumptag = this.scenarios[this.scenarioCount][1];
                    this.JumpScenario(jumptag: _jumptag);
                        break;

                /*case "jump":
                     for (int i = 1;i< 100; i++)
                     {
                         string targettag;
                         targettag = this.scenarios[i][0];
                         if(targettag == "scene")
                         {
                             targettag = this.scenarios[i][1];
                             if
                             {
                                 //
                             }
                         }

                     }
                     break;
                */

                /*
                var kindOfFeeling = this.scenarios[this.scenarioCount][1];

                // 後々これを計算で得る
                var feelings = new Dictionary<string, float>() { { "good", 60.0f }, { "bad", 80.0f } };

                var gaugeRotation = new RotateTransform
                {
                    CenterX = 0.0,
                    CenterY = this.NeedleImage.Height * 0.8f,
                    Angle = -40.0f
                };

                this.NeedleImage.RenderTransform = gaugeRotation;

                var feeling = feelings[kindOfFeeling] / 2.0f;

                this.FeelingScaleText.Text = feeling.ToString();

                GaugeUpdate(targetAngle: feeling);

                void GaugeUpdate(float targetAngle)
                {
                    var timer = new DispatcherTimer();

                    timer.Interval = TimeSpan.FromSeconds(0.01f);

                    timer.Tick += (sender, e) =>
                    {
                        if (gaugeRotation.Angle < targetAngle)
                        {
                            gaugeRotation.Angle += 1.0f;

                            this.NeedleImage.RenderTransform = gaugeRotation;
                        }
                        else
                        {
                            timer.Stop();
                            timer = null;
                        }
                    };
                    timer.Start();
                }
                */
                case "gif":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var gifObject = this.imageObjects[this.position];

                    var gifFile = this.scenarios[this.scenarioCount][2];

                    var gifImage = new BitmapImage();

                    gifImage.BeginInit();

                    gifImage.UriSource = new Uri($"Images/{gifFile}", UriKind.Relative);

                    gifImage.EndInit();

                   ImageBehavior.SetAnimatedSource(gifObject,gifImage);

                    gifObject.Visibility = Visibility.Visible;

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "item_book":

                    Image[] itemMainImages = { this.Item01MainImage, this.Item03MainImage, this.Item04MainImage, this.Item05MainImage, this.Item06MainImage, this.Item07MainImage, this.Item08MainImage, this.Item09MainImage, this.Item10MainImage, this.Item11MainImage };

                    Image[] itemNoneImages = { this.Item01NoneImage, this.Item03NoneImage, this.Item04NoneImage, this.Item05NoneImage, this.Item06NoneImage, this.Item07NoneImage, this.Item08NoneImage, this.Item09NoneImage, this.Item10NoneImage, this.Item11NoneImage };

                    var hasGotItems = new bool[] { this.dataItem.HasGotItem02, this.dataItem.HasGotItem03, this.dataItem.HasGotItem04, this.dataItem.HasGotItem05, this.dataItem.HasGotItem06, this.dataItem.HasGotItem07, this.dataItem.HasGotItem08, this.dataItem.HasGotItem09, this.dataItem.HasGotItem10, this.dataItem.HasGotItem11 };

                    for (int i = 0; i < hasGotItems.Length; i++)
                    {
                        if (hasGotItems[i] == true)
                        {
                            itemNoneImages[i].Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            itemMainImages[i].Visibility = Visibility.Hidden;
                        }
                    }
                    this.GetItemGrid.Visibility = Visibility.Visible;
                    this.ItemBookMainGrid.Visibility = Visibility.Visible;
                    this.ItemBookNoneGrid.Visibility = Visibility.Visible;

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;
            }
        }

        private List<List<string>> SequenceCheck(string text)
        {
            // 正規表現によって$と$の間の文字列を抜き出す（無駄処理）
            var Matches = new Regex(@"\$(.+?)\$").Matches(text);

            for (int i = 0; i < Matches.Count; i++)
            {
                var sequence = Matches[i].Value;

                switch (sequence)
                {
                    case "$aosukes_kind_of_feeling$":

                        //     text = text.Replace("$aosukes_kind_of_feeling$", this.dataChapter2.AosukesKindOfFeelings.Split(",")[0]);

                        break;


                    case "$aosukes_size_of_feeling$":

                        //   text = text.Replace("$aosukes_size_of_feeling$", this.dataChapter2.AosukesSizeOfFeeling.ToString());

                        break;
                }
            }

            // 苦悶の改行処理（文章中の「鬱」を疑似改行コードとする）
            text = text.Replace("鬱", "\u2028");

            text = text.Replace("】", "【");
            //セリフをテキストと記号の１次元配列に分解
            var texts = text.Split("【");

            List<List<string>> text2ds = new List<List<string>>();


            foreach (var imageText in IMAGE_TEXTS)
            {
                int[] matchIndexs = { };

                foreach (var (txt, index) in texts.Indexed())
                {
                    if (txt == imageText)
                    {
                        matchIndexs.Append(index);
                    }
                }

                foreach (var tex in texts)
                {
                    List<string> tex1ds = new List<string> { tex };
                    text2ds.Add(tex1ds);
                }

                foreach (var matchIndex in matchIndexs)
                {
                    text2ds[matchIndex].Add(imageText);
                }
            }

            foreach (var wordText in WORD_TEXTS)
            {
                int[] matchIndexs = { };

                foreach (var (txt, index) in texts.Indexed())
                {
                    if (txt == wordText)
                    {
                        matchIndexs.Append(index);
                    }
                }

                foreach (var matchIndex in matchIndexs)
                {
                    text2ds[matchIndex].Add(wordText);
                }
            }
            return text2ds;
        }
       
        void ShowMessage(TextBlock textObject, List<List<string>> messages)
        {
            textObject.Text = "";
            textObject.Visibility = Visibility.Visible;

            this.word_num = 0;
            // メッセージ表示処理
            this.msgTimer = new DispatcherTimer();
            this.msgTimer.Tick += ViewMsg;
            this.msgTimer.Interval = TimeSpan.FromSeconds(1.0f / MESSAGE_SPEED);
            this.msgTimer.Start();

            this.inlineCount = 0;
            this.imageInlineCount = 0;
            

            foreach(var run in this.runs)
            {
                run.Text="";
            }

            this.runs.Clear();

            this.imageInlines.Clear();

   　      textObject.Inlines.Clear();

            // 画像インラインと文字インラインの合体
            foreach (var msgs in messages)
            {
                string namePngPath = "./temp/temp_name.png";

                if (msgs[0] == "name" && File.Exists(namePngPath))
                {
                    var imageInline = new InlineUIContainer { Child = new Image { Source = null, Height = 48 } };

                    textObject.Inlines.Add(imageInline);

                    this.imageInlines.Add(imageInline);
                }

                var run = new Run { Text = "", Foreground = new SolidColorBrush(Colors.Black) };

                textObject.Inlines.Add(run);

                this.runs.Add(run);

                if (msgs[0] == "marker")
                {
                    var markerInline = new Run { Text = "", Background = this.CharacterColor[this.scenarios[this.scenarioCount][3]] };

                    textObject.Inlines.Add(markerInline);

                    this.runs.Add(markerInline);
                }
                if (msgs[0] == "bold")
                {
                    var boldInline = new Run { Text = "", FontWeight = FontWeights.UltraBold};

                    textObject.Inlines.Add(boldInline);

                    this.runs.Add(boldInline);
                }
                
                    
                
            }

            // 一文字ずつメッセージ表示（Inner Func）
            void ViewMsg(object sender, EventArgs e)
            {
                if (this.inlineCount < messages.Count)
                {
                    var msgs = messages[this.inlineCount];

                    string namePngPath = "./temp/temp_name.png";

                    if (msgs[0] == "name" && File.Exists(namePngPath))
                    {
                        // msgs[0].Replace("name", "");

                        // 実行ファイルの場所を絶対パスで取得
                        var startupPath = FileUtils.GetStartupPath();

                        var image = new BitmapImage();

                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        image.UriSource = new Uri($@"{startupPath}/{namePngPath}", UriKind.Absolute);
                        image.EndInit();

                        image.Freeze();

                        (this.imageInlines[imageInlineCount].Child as Image).Source = image;

                        this.imageInlineCount++;

                        this.inlineCount++;
                        this.word_num = 0;

                        return;
                    }
                    if (msgs[0]=="bold")
                    {
                        msgs[0] = "";
                        this.word_num = 0;
                    }
                    if (msgs[0] == "marker")
                    {
                        msgs[0] = "";
                        this.word_num = 0;
                    }
                    this.runs[inlineCount].Text = msgs[0].Substring(0, this.word_num);
                    if (this.word_num < msgs[0].Length)
                    {
                        this.word_num++;
                    }
                    else
                    {
                        this.inlineCount++;
                        this.word_num = 0;
                    }
                }
                else
                {
                    this.msgTimer.Stop();
                    this.msgTimer = null;

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }
            }
        }

        // アニメーション（ストーリーボード）の処理
        private void ShowAnime(string storyBoard, string isSync)
        {
            Storyboard sb = this.FindResource(storyBoard) as Storyboard;

            if (sb != null)
            {
                // 二重終了防止策
                bool isDuplicate = false;

                if (isSync == "sync")
                {
                    sb.Completed += (s, e) =>
                    {
                        if (!isDuplicate)
                        {
                            this.scenarioCount += 1;
                            this.ScenarioPlay();

                            isDuplicate = true;
                        }
                    };
                    sb.Begin(this);
                }
                else if (isSync == "no_sync")
                {
                    sb.Begin(this);

                    if (!isDuplicate)
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();

                        isDuplicate = true;
                    }
                }
            }
        }

        // 黒板ルール処理のためだけの追加
        private void MessageCallBack(object obj)
        {
            switch (obj)
            {
                case CheckBox checkBox:

                    checkBox.Visibility = Visibility.Visible;

                    break;

                case CheckBox[] checkBoxs:

                    foreach (CheckBox checkBox in checkBoxs)
                    {
                        checkBox.IsEnabled = true;
                    }
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 各種ボタンが押されたときの処理

            Button button = sender as Button;

            /*
            if (button.Name.Contains("Back"))
            {
                this.scenarioCount -= 1;
                this.ScenarioPlay();
                // 連続Backの実現にはもっと複雑な処理がいる
            }
            */

            // FullScreen時のデバッグ用に作っておく
            if (button.Name == "ExitButton")
            {
                this.CoverLayerImage.Visibility = Visibility.Visible;
                this.ExitBackGrid.Visibility = Visibility.Visible;
            }

            if (button.Name == "ExitBackYesButton")
            {
                Application.Current.Shutdown();
            }

            if (button.Name == "ExitBackNoButton")
            {

                this.ExitBackGrid.Visibility = Visibility.Hidden;
                this.CoverLayerImage.Visibility = Visibility.Hidden;
            }
            if (button.Name == "NextMessageButton")
            {
                
                this.scenarioCount += 1;
                this.ScenarioPlay();
            }
            if (button.Name == "NextPageButton")
            {
                this.scenarioCount += 1;
                this.ScenarioPlay();
            }
            if (button.Name == "MangaFlipButton")
            {
                this.scenarioCount += 1;
                this.ScenarioPlay();
            }
            if (button.Name == "BranchButton1")
            {
                string _jumptag = "漫画シーン";
                this.JumpScenario(_jumptag);
            }
            if (button.Name == "BranchButton2")
            {
                string _jumptag = "きもちセンサーの復習";
                this.JumpScenario(_jumptag);
            }
            if(button.Name.Substring(0,9) == "GoodEvent") 
            {
                var GoodEventObject = this.GoodEventObject[button.Name];
                if(GoodEventObject.Visibility == Visibility.Visible)
                {
                    GoodEventObject.Visibility = Visibility.Hidden;
                }
                else
                {
                    GoodEventObject.Visibility = Visibility.Visible;
                }
                for(int i = 1; i < 15; i++)
                {
                    string targetbuttonname = "GoodEventButton"+i.ToString();
                    var targetbuttonObject = this.GoodEventObject[targetbuttonname];

                    if (targetbuttonObject.Visibility == Visibility.Visible)
                    {
                        this.SelectFeelingNextButton.Visibility = Visibility.Visible;
                        break;
                    }
                    else if (targetbuttonObject.Visibility == Visibility.Hidden)
                    {
                        this.SelectFeelingNextButton.Visibility = Visibility.Hidden;
                        
                    }
                }

                this.ChallengeMessageGrid.Visibility = Visibility.Hidden;
                
            }
            if (button.Name == "SelectFeelingNextButton")
            {
                if (scene == "チャレンジタイムパート①")
                {
                    for (int i = 1; i < 15; i++)
                    {
                        string targettextname = "GoodEventButton" + i.ToString();
                        var targetObject = this.GoodEventObject[targettextname];
                        if (targetObject.Visibility == Visibility.Visible)
                        {
                            this.mySelectGoodEvents.Add(this.textBlockObjects["GoodEventText" + i.ToString()].Text);
                        }
                    }
                    this.dataChapter2.MySelectGoodEvents = string.Join(",", this.mySelectGoodEvents);
                    using (var connection = new SQLiteConnection(this.dbPath))
                    {
                        connection.Execute($@"UPDATE DataChapter2 SET MySelectGoodEvents = '{this.dataChapter2.MySelectGoodEvents}'WHERE CreatedAt = '{this.dataChapter2.CreatedAt}';");
                    }

                }
               
                this.scenarioCount += 1;
                this.ScenarioPlay();

                if(scene== "「おいしいものを食べる」ときは？")
                {
                    this.aosukesDifficultyOfEating = this.AosukeDifficultyOfActionText.Text;
                    this.aosukesSizeOfFeelingOfEating = this.AosukeSizeOfFeelingText.Text;

                    this.dataChapter2.AosukesDifficultyOfEating = this.aosukesDifficultyOfEating;
                    this.dataChapter2.AosukesSizeOfFeelingOfEating = this.aosukesSizeOfFeelingOfEating;
                    using (var connection = new SQLiteConnection(this.dbPath))
                    {
                        connection.Execute($@"UPDATE DataChapter2 SET AosukesSizeOfFeelingOfEating = '{this.dataChapter2.AosukesSizeOfFeelingOfEating}'WHERE CreatedAt = '{this.dataChapter2.CreatedAt}';");
                        connection.Execute($@"UPDATE DataChapter2 SET AosukesDifficultyOfEating = '{this.dataChapter2.AosukesDifficultyOfEating}'WHERE CreatedAt = '{this.dataChapter2.CreatedAt}';");
                    }
                }
                if (scene == "「全部のテストで100点をとる」ときは？")
                {
                    this.aosukesDifficultyOfGettingHighScore = this.AosukeDifficultyOfActionText.Text;
                    this.aosukesSizeOfFeelingOfGettingHighScore = this.AosukeSizeOfFeelingText.Text;

                    this.dataChapter2.AosukesDifficultyOfGettingHighScore = this.aosukesDifficultyOfGettingHighScore;
                    this.dataChapter2.AosukesSizeOfFeelingOfGettingHighScore = this.aosukesSizeOfFeelingOfGettingHighScore;
                    using (var connection = new SQLiteConnection(this.dbPath))
                    {
                        connection.Execute($@"UPDATE DataChapter2 SET AosukesSizeOfFeelingOfGettingHighScore = '{this.dataChapter2.AosukesSizeOfFeelingOfGettingHighScore}'WHERE CreatedAt = '{this.dataChapter2.CreatedAt}';");
                        connection.Execute($@"UPDATE DataChapter2 SET AosukesDifficultyOfGettingHighScore = '{this.dataChapter2.AosukesDifficultyOfGettingHighScore}'WHERE CreatedAt = '{this.dataChapter2.CreatedAt}';");

                    }
                }
                if (scene == "「休み時間に友だちとおしゃべりする」")
                {
                    this.aosukesDifficultyOfTalkingWithFriend = this.AosukeDifficultyOfActionText.Text;
                    this.aosukesSizeOfFeelingOfTalkingWithFriend = this.AosukeSizeOfFeelingText.Text;

                    this.dataChapter2.AosukesSizeOfFeelingOfTalkingWithFriend = this.aosukesDifficultyOfEating;
                    this.dataChapter2.AosukesDifficultyOfTalkingWithFriend = this.aosukesSizeOfFeelingOfEating;
                    using (var connection = new SQLiteConnection(this.dbPath))
                    {
                        connection.Execute($@"UPDATE DataChapter2 SET AosukesSizeOfFeelingOfTalkingWithFriend = '{this.dataChapter2.AosukesSizeOfFeelingOfTalkingWithFriend}'WHERE CreatedAt = '{this.dataChapter2.CreatedAt}';");
                        connection.Execute($@"UPDATE DataChapter2 SET AosukesDifficultyOfTalkingWithFriend = '{this.dataChapter2.AosukesDifficultyOfTalkingWithFriend}'WHERE CreatedAt = '{this.dataChapter2.CreatedAt}';");
                    }
                }
            }
            if(button.Name== "GroupActivityWritingButton")
            {
                this.CanvasGrid.Visibility = Visibility.Visible;
            }
            if (button.Content == "えんぴつ")
            {
                NameCanvas.EditingMode = InkCanvasEditingMode.Ink;
            }
            if (button.Content == "けしごむ")
            {
                NameCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
            }
            if (button.Content == "すべてけす")
            {
                NameCanvas.Strokes.Clear();
            }
            if (button.Content == "かんせい")
            {

                // ストロークが描画されている境界を取得
                Rect rectBounds = new Rect(0, 0, this.NameCanvas.ActualWidth, this.NameCanvas.ActualHeight);

                // 描画先を作成
                DrawingVisual dv = new DrawingVisual();
                DrawingContext dc = dv.RenderOpen();

                // 描画エリアの位置補正（補正しないと黒い部分ができてしまう）
                dc.PushTransform(new TranslateTransform(-rectBounds.X, -rectBounds.Y));

                // 描画エリア(dc)に四角形を作成
                // 四角形の大きさはストロークが描画されている枠サイズとし、
                // 背景色はInkCanvasコントロールと同じにする
                dc.DrawRectangle(NameCanvas.Background, null, rectBounds);

                // 上記で作成した描画エリア(dc)にInkCanvasのストロークを描画
                NameCanvas.Strokes.Draw(dc);
                dc.Close();

                // ビジュアルオブジェクトをビットマップに変換する
                RenderTargetBitmap rtb = new RenderTargetBitmap(
                    (int)rectBounds.Width, (int)rectBounds.Height,
                    96, 96,
                    PixelFormats.Default);
                rtb.Render(dv);

                //仮置き
                string nameBmp = "GroupActivity2.bmp";
                string dirPath = $"./Log/{userName}";

                string nameBmpPath = System.IO.Path.Combine(dirPath, nameBmp);
                var startupPath = FileUtils.GetStartupPath();

                // ビットマップエンコーダー変数の宣言
                BitmapEncoder enc = null;
                enc = new BmpBitmapEncoder();
                // ビットマップフレームを作成してエンコーダーにフレームを追加する
                enc.Frames.Add(BitmapFrame.Create(rtb));
                // ファイルのパスは仮
                using (var stream = System.IO.File.Create($@"{startupPath}/{nameBmpPath}"))
                {
                    enc.Save(stream);
                }

                this.GroupeActivityGrid.Visibility = Visibility.Visible;
                this.CanvasGrid.Visibility = Visibility.Hidden;

                this.GroupActivityWritingImage.Source = new BitmapImage(new Uri($@"{startupPath}/{nameBmpPath}", UriKind.RelativeOrAbsolute));

                if (scene == "グループアクティビティ")
                {
                    if (this.SelectFeelingNextButton.Visibility == Visibility.Hidden)
                    {
                        this.SelectFeelingNextButton.Visibility = Visibility.Visible;
                    }
                }
            }
            if (button.Name == "SizeOfFeelingButton")
            {
                this.Challenge2Grid.Visibility = Visibility.Hidden;
                this.SelectFeelingNextButton.Visibility = Visibility.Hidden;
                string _jumptag = "きもちのおおきさ";
                this.JumpScenario(_jumptag);
            }
            if (button.Name == "DifficultyOfActionButton")
            {
                this.Challenge2Grid.Visibility = Visibility.Hidden;
                this.SelectFeelingNextButton.Visibility = Visibility.Hidden;
                string _jumptag = "かんたんにできるか";
                this.JumpScenario(_jumptag);
            }
            if (button.Name == "GoodButton")
            {
                this.DifficultySelectGrid.Visibility = Visibility.Hidden;
                this.AosukeDifficultyOfActionText.Text = "〇";
                this.JumpScenario(scene);
            }
            if (button.Name == "BadButton")
            {
                this.DifficultySelectGrid.Visibility = Visibility.Hidden;
                this.AosukeDifficultyOfActionText.Text = "×";
                this.JumpScenario(scene);
            }
            if (button.Name == "NormalButton")
            {
                this.DifficultySelectGrid.Visibility = Visibility.Hidden;
                this.AosukeDifficultyOfActionText.Text = "△";
                this.JumpScenario(scene);
            }
            if (button.Name == "ReturnToTitleButton")
            {
                TitlePage titlePage = new TitlePage();

                titlePage.SetIsFirstBootFlag(false);

                titlePage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                this.NavigationService.Navigate(titlePage);
            }
        }

        private void SetBGM(string soundFile, bool isLoop, int volume)
        {
            var startupPath = FileUtils.GetStartupPath();

            // ループ再生を指定
            this.mediaPlayer.settings.setMode("loop", isLoop);

            // 通常は自動再生にファイルを指定すればループ再生がはじまる
            this.mediaPlayer.URL = $@"{startupPath}/Sounds/{soundFile}";

            this.mediaPlayer.settings.volume = volume; // 0から100
        }

        private void PlayBGM()
        {
            this.mediaPlayer.controls.play(); // 再生
        }

        private void StopBGM()
        {
            this.mediaPlayer.controls.stop(); // 停止(再生中停止すればplayBGM()で頭から再生)
        }

        private void PauseBGM()
        {
            mediaPlayer.controls.pause(); // ポーズ(playBGM()で再開)
        }

        private void StopSE()
        {
            if (sePlayer != null)
            {
                sePlayer.Stop();
                sePlayer.Dispose();
                sePlayer = null;
            }
        }

        private void PlaySE(string soundFile)
        {
            sePlayer = new SoundPlayer(soundFile);
            sePlayer.Play();
        }

        // ジェスチャー認識キャンバスのロード
        void GestureCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            var gestureCanvas = (InkCanvas)sender;

            gestureCanvas.SetEnabledGestures(new[]
            {
                ApplicationGesture.Circle,
                ApplicationGesture.DoubleCircle,
                ApplicationGesture.Triangle,
                ApplicationGesture.Check,
                ApplicationGesture.ArrowDown,
                ApplicationGesture.ChevronDown,
                ApplicationGesture.DownUp,
                ApplicationGesture.Up,
                ApplicationGesture.Down,
                ApplicationGesture.Left,
                ApplicationGesture.Right,
                ApplicationGesture.Curlicue,
                ApplicationGesture.DoubleCurlicue,
            });
        }

        // ジェスチャーキャンバスの処理
        void GestureCanvas_Gesture(object sender, InkCanvasGestureEventArgs e)
        {
            // 信頼性 (RecognitionConfidence) を無視したほうが、Circle と Triangle の認識率は上がるようです。
            var gestureResult = e.GetGestureRecognitionResults()
                .FirstOrDefault(r => r.ApplicationGesture != ApplicationGesture.NoGesture);

            var startupPath = FileUtils.GetStartupPath();

            if (gestureResult == null)
            {
                PlaySE($@"{startupPath}/Sounds/None.wav");
                return;
            }

            AnswerResult answerResult;

            switch (gestureResult.ApplicationGesture)
            {
                case ApplicationGesture.Circle:
                case ApplicationGesture.DoubleCircle:
                    answerResult = AnswerResult.Correct;
                    break;
                case ApplicationGesture.Triangle:
                    answerResult = AnswerResult.Intermediate;
                    break;
                case ApplicationGesture.Check:
                case ApplicationGesture.ArrowDown:
                case ApplicationGesture.ChevronDown:
                case ApplicationGesture.DownUp:
                case ApplicationGesture.Up:
                case ApplicationGesture.Down:
                case ApplicationGesture.Left:
                case ApplicationGesture.Right:
                case ApplicationGesture.Curlicue:
                case ApplicationGesture.DoubleCurlicue:
                    answerResult = AnswerResult.Incorrect;
                    break;

                default:
                    throw new InvalidOperationException();
            }

            PlaySE($@"{startupPath}/Sounds/{answerResult}.wav");

            var gestureCanvas = (InkCanvas)sender;
        }



        private enum AnswerResult
        {
            None,
            Incorrect,
            Intermediate,
            Correct,
        }



        // ハートゲージの角度をデータバインド
        private static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(Chapter2), new UIPropertyMetadata(0.0));

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        // マウスのドラッグ処理（マウスの左ボタンを押下したとき）
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this);

            if (this.SelectHeartGrid.Visibility == Visibility.Visible)
            {
                this.CalcAngle();

                this.ViewSizeOfFeelingTextBlock.Text = this.feelingSize.ToString();

                
            }
        }

        // マウスのドラッグ処理（マウスの左ボタンを離したとき）
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);

            if (this.SelectHeartGrid.Visibility == Visibility.Visible)
            {
                this.SelectHeartGrid.Visibility = Visibility.Hidden;
                this.ViewSizeOfFeelingGrid.Visibility = Visibility.Hidden;
                this.AosukeSizeOfFeelingText.Text = "(  " + this.ViewSizeOfFeelingTextBlock.Text + "  )";

                this.JumpScenario(scene);

            }

        }

        // マウスのドラッグ処理（マウスを動かしたとき）
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.Captured == this)
            {
                if (this.SelectHeartGrid.Visibility == Visibility.Visible)
                {
                    this.CalcAngle();

                    this.ViewSizeOfFeelingTextBlock.Text = this.feelingSize.ToString();

                    
                }
            }
        }

        // ハートゲージの針の角度に関する計算
        private void CalcAngle()
        {
            Point currentLocation = this.PointToScreen(Mouse.GetPosition(this));

            Point knobCenter = this.SelectHeartImage.PointToScreen(new Point(this.SelectHeartImage.ActualWidth * 0.5, this.SelectHeartImage.ActualHeight * 0.8));

            double radians = Math.Atan((currentLocation.Y - knobCenter.Y) / (currentLocation.X - knobCenter.X));

            this.Angle = radians * 180 / Math.PI + 90;

            this.feelingSize = (int)(this.Angle + 50.0f);

            if (currentLocation.X - knobCenter.X < 0)
            {
                this.Angle += 180;

                this.feelingSize = (int)(this.Angle - 310.0f);
            }
            if (this.feelingSize <= 0)
            {
                this.feelingSize = 0;
                this.Angle = (double)this.feelingSize - 50.0f;
            }
            if (this.feelingSize >= 100)
            {
                this.feelingSize = 100;
                this.Angle = (double)this.feelingSize + 310.0f;
            }
        }

        private void JumpScenario(string jumptag)
        {
            for (int i = 0; i < this.scenarios.Count; i++)
            {
                if (this.scenarios[i][0] == "scene" || this.scenarios[i][0]=="tag")
                {
                    if (jumptag == this.scenarios[i][1])
                    {
                        this.scenarioCount = i;
                        this.ScenarioPlay();
                    }
                }
            }
        }

        private BitmapSource Image2Gray(ImageSource originalImageSource)
        {
            // BitmapImageのPixelFormatをPbgra32に変換する
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap((BitmapSource)originalImageSource, PixelFormats.Pbgra32, null, 0);

            // 画像の大きさに従った配列を作る
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            byte[] originalPixcels = new byte[width * height * 4];
            byte[] grayPixcels = new byte[width * height * 4];

            // BitmapSourceから配列にコピー
            int stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(originalPixcels, stride, 0);

            // 色を白黒にする
            for (int x = 0; x < originalPixcels.Length; x = x + 4)
            {
                var grayAverage = (originalPixcels[x] + originalPixcels[x + 1] + originalPixcels[x + 2]) / 3;

                grayPixcels[x] = (byte)grayAverage;
                grayPixcels[x + 1] = (byte)grayAverage;
                grayPixcels[x + 2] = (byte)grayAverage;
                grayPixcels[x + 3] = originalPixcels[x + 3]; // アルファ値の維持
            }

            // 配列からBitmaopSourceを作る
            BitmapSource grayBitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, grayPixcels, stride);

            return grayBitmap;
        }

    }
}
