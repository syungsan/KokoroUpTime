using CsvReadWrite;
using Expansion;
using FileIOUtils;
using Osklib;
using OutlineTextMaker;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
        //楽しいことのリスト
        private string[] GOOD_EVENTS = {"●野球でホームランを打つ", "●友だちと遊ぶ", "●新しいゲームを買う", "●のんびりする", "●遊園地に行く", "●コンサートに行く", "●テストで１００点をとる",
                                        "●遠足に行く","●友だちとおしゃべりする","●動物園に行く","●おいしいものを食べる","●好きなスポーツをする","●好きな教科の勉強をする","●レストランに食べに行く"};

        private float THREE_SECOND_RULE_TIME = 3.0f;

        private int RETURN_COUNT = 1;

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
        private DispatcherTimer buttonTimer;
        private int word_num;

        private int inlineCount;
        private int imageInlineCount;

        private Dictionary<string, List<Run>> runs = new Dictionary<string, List<Run>>();
        private Dictionary<string, List<InlineUIContainer>> imageInlines = new Dictionary<string, List<InlineUIContainer>>();


        // 各種コントロールを任意の文字列で呼び出すための辞書
        private Dictionary<string, OutlineText> outLineTextObjects = null;
        private Dictionary<string, Image> imageObjects = null;
        private Dictionary<string, TextBlock> textBlockObjects = null;
        private Dictionary<string, Button> buttonObjects = null;
        private Dictionary<string, Grid> gridObjects = null;

        private Dictionary<string, Ellipse> GoodEventObject = null;

        private string[] EDIT_BUTTON = { "えんぴつ", "けしごむ", "すべてけす", "かんせい" };

        private Dictionary<string, SolidColorBrush> CharacterColor = null;


        // 音関連
        private WindowsMediaPlayer mediaPlayer;
        private SoundPlayer sePlayer = null;

        // データベースに収めるデータモデルのインスタン
        private DataChapter2 dataChapter2;

        // ゲームの切り替えシーン
        private string scene;

        private string tag;

        // なったことのある自分の気持ちの一時記録用
        public List<string> mySelectGoodEvents = new List<string>();

        public string aosukesSizeOfFeelingOfEating;
        public string aosukesKindOfFeelingOfEating;
        public string aosukesDifficultyOfEating;

        public string aosukesSizeOfFeelingOfGettingHighScore;
        public string aosukesKindOfFeelingOfGettingHighScore;
        public string aosukesDifficultyOfGettingHighScore;

        public string aosukesSizeOfFeelingOfTalkingWithFriend;
        public string aosukesKindOfFeelingOfTalkingWithFriend;
        public string aosukesDifficultyOfTalkingWithFriend;

        public string myALittlleExcitingEvents ;





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

            this.EditingModeItemsControl.ItemsSource = EDIT_BUTTON;

            this.CharacterColor = new Dictionary<string, SolidColorBrush>
            {
                ["白じい"] = new SolidColorBrush(Colors.Orange),
                ["青助"] = new SolidColorBrush(Colors.Aqua),
                ["赤丸"] = new SolidColorBrush(Colors.Red),
                ["キミ"] = new SolidColorBrush(Colors.Yellow),
            };

            this.InitControls();

            this.SelectGoodEventListBox.ItemsSource = this.GOOD_EVENTS;
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
                ["item_center_up_image"] = this.ItemCenterUpImage, 
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
                ["challenge2_action_bubble_image"] =this.Challenge2ActionBubbleImage,
               
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
                ["children_face_small_left_msg_text"] = this.ChildrenFaceSmallLeftMessageTextBlock,
                ["main_msg"] = this.MainMessageTextBlock,
                ["thin_msg"] = this.ThinMessageTextBlock,
                ["music_title_text"] = this.MusicTitleTextBlock,
                ["composer_name_text"] = this.ComposerNameTextBlock,
                ["challenge2_bubble_action_text"] =this.Challenge2BubbleActionText,
                ["aosuke_difficulty_of_action_text"] =this.AosukeDifficultyOfActionText,
                ["aosuke_kind_of_feeling_text"] =this.AosukeKindOfFeelingText,
                ["aosuke_size_of_feeling_text"] = this.AosukeSizeOfFeelingText,
                ["item_point_msg_text"] = this.ItemPointMessageText,
                ["challenge2_bubble_action_text"]=this.Challenge2BubbleActionText,
                ["item_book_title_text"]=this.ItemBookTitleTextBlock,
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

            this.outLineTextObjects = new Dictionary<string, OutlineText>
            {
                ["title_outline_text"] = this.TitleOutlineText,
            };

            this.gridObjects = new Dictionary<string, Grid>
            {
                ["session_grid"] = this.SessionGrid,
                ["challenge1_grid"] = this.ChallengePart1Grid,
                ["group_activity_grid"]=this.GroupeActivityGrid,
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
                ["view_size_of_feeling_grid"]=this.ViewSizeOfFeelingGrid,
                ["select_heart_grid"]=this.SelectHeartGrid,
                ["difficulty_select_grid"] =this.DifficultySelectGrid,
                
                ["exit_back_grid"] = this.ExitBackGrid,
                ["challenge_time_result_grid"] =this.ChallengeTimeResultGrid,
                ["challenge_time_result_msg_grid"] = this.ChallengeTimeResultMessageGrid,
                ["challenge2_cover_grid"]=this.Challenge2CoverGrid,
                ["goupe_activity_message_grid"]=this.GroupeActivityMessageGrid,
            };

            
        }

        private void ResetControls()
        {
            // 各種コントロールを隠すことでフルリセット

            this.SessionGrid.Visibility = Visibility.Hidden;
            this.ChallengePart1Grid.Visibility = Visibility.Hidden;
            this.Challenge2Grid.Visibility = Visibility.Hidden;
            this.GroupeActivityGrid.Visibility = Visibility.Hidden;
            this.CanvasGrid.Visibility = Visibility.Hidden;
            this.ItemPlateGrid.Visibility = Visibility.Hidden;
            this.Challenge2CoverGrid.Visibility = Visibility.Hidden;

            this.SummaryGrid.Visibility = Visibility.Hidden;
            this.EndingGrid.Visibility = Visibility.Hidden;
            this.ItemNamePlateLeftGrid.Visibility = Visibility.Hidden;
            this.ItemNameBubbleGrid.Visibility = Visibility.Hidden;
            this.ItemNamePlateCenterGrid.Visibility = Visibility.Hidden;
            this.ItemInfoPlateGrid.Visibility = Visibility.Hidden;
            this.ItemLastInfoGrid.Visibility = Visibility.Hidden;
            this.ItemPointMessageGrid.Visibility = Visibility.Hidden;

            this.ItemReviewGrid.Visibility = Visibility.Hidden;
            this.ChallengeTimeResultGrid.Visibility = Visibility.Hidden;
            this.ChallengeTimeResultMessageGrid.Visibility = Visibility.Hidden;


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
            this.GetItemGrid.Visibility = Visibility.Hidden;
            this.ItemBookMainGrid.Visibility = Visibility.Hidden;
            this.ItemBookNoneGrid.Visibility = Visibility.Hidden;
            this.ItemBookTitleTextBlock.Visibility = Visibility.Hidden;

            this.ExitBackGrid.Visibility = Visibility.Hidden;

            this.BranchSelectGrid.Visibility = Visibility.Hidden;

            this.BackgroundImage.Visibility = Visibility.Hidden;
            this.MangaTitleImage.Visibility = Visibility.Hidden;
            this.MangaImage.Visibility = Visibility.Hidden;
            this.ItemCenterImage.Visibility = Visibility.Hidden;
            this.ItemCenterUpImage.Visibility = Visibility.Hidden;
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

            this.ViewSizeOfFeelingGrid.Visibility = Visibility.Hidden;
            this.DifficultySelectGrid.Visibility = Visibility.Hidden;
           
            this.ReturnToTitleButton.Visibility = Visibility.Hidden;
            this.CanvasGrid.Visibility = Visibility.Hidden;

            this.TitleOutlineText.Visibility = Visibility.Hidden;
            this.GroupeActivityMessageGrid.Visibility = Visibility.Hidden;

            this.InputTextGrid.Visibility = Visibility.Hidden;


            this.tag = "";

            this.SessionSubTitleTextBlock.Text = "";
            this.SessionSentenceTextBlock.Text = "";
           
            this.ChildrenFaceSmallLeftMessageTextBlock.Text = "";
            this.CompareMessageTextBlock.Text = "";
            this.KindOfFeelingAkamaruTextBlock.Text = "";
            this.SizeOfFeelingAkamaruTextBlock.Text = "";
            this.KindOfFeelingAosukeTextBlock.Text = "";
            this.SizeOfFeelingAosukeTextBlock.Text = "";
            this.EndingMessageTextBlock.Text = "";
            
            this.ThinMessageTextBlock.Text = "";
            this.MusicTitleTextBlock.Text = "";
            this.ComposerNameTextBlock.Text = "";
            this.ItemPointMessageText.Text = "";

            this.AosukeDifficultyOfActionText.Text = "";
            this.AosukeKindOfFeelingText.Text = "";
            this.AosukeSizeOfFeelingText.Text = "";
            this.Challenge2BubbleActionText.Text = "";
            //this.GroupeActivityInputText.Text = "";
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
                case "start":

                    // 画面のフェードイン処理とか入れる（別関数を呼び出す）

                    this.dataProgress.CurrentChapter = 1;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET CurrentChapter = '{this.dataProgress.CurrentChapter}' WHERE Id = 1;");
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

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

                    this.dataProgress.CurrentScene = this.scene;
                    this.dataProgress.LatestChapter1Scene = this.scene;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET CurrentScene = '{this.dataProgress.CurrentScene}', LatestChapter1Scene = '{this.dataProgress.LatestChapter1Scene}' WHERE Id = 1;");
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                // グリッドに対しての処理
                case "grid":

                    // グリッドコントロールを任意の名前により取得
                    this.position = this.scenarios[this.scenarioCount][1];

                    if (this.position == "music_info_grid" && !this.dataOption.IsPlayBGM)
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();

                        break;
                    }

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

                        var gridObjectName = gridObject.Name;

                        this.ShowAnime(storyBoard: gridStoryBoard, objectName: gridObjectName, isSync: gridAnimeIsSync);
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

                        var imageObjectName = imageObject.Name;

                        this.ShowAnime(storyBoard: imageStoryBoard, objectName: imageObjectName, isSync: imageAnimeIsSync);
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

                        var buttonObjectName = buttonObject.Name;

                        this.ShowAnime(storyBoard: buttonStoryBoard, objectName: buttonObjectName, isSync: buttonAnimeIsSync);
                    }
                    else
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    break;

                // 流れる文字をTextBlockで表現するための処理
                case "msg":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var _textObject = this.textBlockObjects[this.position];

                    _textObject.Visibility = Visibility.Hidden;

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var _message = this.scenarios[this.scenarioCount][2];

                        var _messages = this.SequenceCheck(_message);

                        this.ShowSentence(textObject: _textObject, sentences: _messages, mode: "msg");
                    }
                    else
                    {
                        var _messages = this.SequenceCheck(_textObject.Text);

                        // xamlに直接書いたStaticな文章を表示する場合
                        this.ShowSentence(textObject: _textObject, sentences: _messages, mode: "msg");
                    }
                    break;

                // 流れない文字に対するTextBlock処理
                case "text":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var __textObject = this.textBlockObjects[this.position];

                    __textObject.Visibility = Visibility.Visible;

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var _text = this.scenarios[this.scenarioCount][2];

                        var _texts = this.SequenceCheck(_text);

                        this.ShowSentence(textObject: __textObject, sentences: _texts, mode: "text");
                    }
                    else
                    {
                        if (__textObject.Text != "")
                        {
                            var _texts = this.SequenceCheck(__textObject.Text);

                            // xamlに直接書いたStaticな文章を表示する場合
                            this.ShowSentence(textObject: __textObject, sentences: _texts, mode: "text");
                        }
                    }

                    string textAnimeIsSync = "sync";

                    // テキストに対するアニメも一応用意
                    if (this.scenarios[this.scenarioCount].Count > 5 && this.scenarios[this.scenarioCount][5] != "")
                    {
                        textAnimeIsSync = this.scenarios[this.scenarioCount][5];
                    }

                    if (this.scenarios[this.scenarioCount].Count > 4 && this.scenarios[this.scenarioCount][4] != "")
                    {
                        var textStoryBoard = this.scenarios[this.scenarioCount][4];

                        var textObjectName = __textObject.Name;

                        this.ShowAnime(storyBoard: textStoryBoard, objectName: textObjectName, isSync: textAnimeIsSync);
                    }
                    else
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    break;

                case "wait":

                    // 時間のオプション指定がない場合は無限待ち
                    if (this.scenarios[this.scenarioCount].Count > 1 && this.scenarios[this.scenarioCount][1] != "")
                    {
                        var spanTime = float.Parse(this.scenarios[this.scenarioCount][1]);

                        // 数秒後に処理を実行
                        DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(spanTime) };
                        timer.Start();
                        timer.Tick += (s, args) =>
                        {
                            // タイマーの停止
                            timer.Stop();

                            // 以下に待機後の処理を書く
                            this.scenarioCount += 1;
                            this.ScenarioPlay();
                        };
                    }

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        if (this.scenarios[this.scenarioCount][2] == "disable_click")
                        {
                            this.isClickable = false;
                        }
                    }
                    else
                    {
                        this.isClickable = true;
                    }
                    break;


                // ボタン押下待ち
                case "click":

                    if (this.scenarios[this.scenarioCount].Count > 1 && this.scenarios[this.scenarioCount][1] != "")
                    {
                        var clickButton = this.scenarios[this.scenarioCount][1];

                        string clickMethod = "";

                        if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                        {
                            clickMethod = this.scenarios[this.scenarioCount][2];
                        }

                        if (this.dataOption.Is3SecondRule)
                        {
                            DispatcherTimer waitTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(THREE_SECOND_RULE_TIME) };

                            if (clickMethod == "next_only")
                            {
                                waitTimer.Start();

                                waitTimer.Tick += (s, args) =>
                                {
                                    waitTimer.Stop();
                                    waitTimer = null;

                                    if (clickButton == "msg")
                                    {
                                        this.NextMessageButton.Visibility = Visibility.Visible;
                                    }
                                    else if (clickButton == "page")
                                    {
                                        this.NextPageButton.Visibility = Visibility.Visible;
                                    }
                                };
                            }
                            else
                            {
                                waitTimer.Start();

                                waitTimer.Tick += (s, args) =>
                                {
                                    waitTimer.Stop();
                                    waitTimer = null;

                                    if (clickButton == "msg")
                                    {
                                        this.NextMessageButton.Visibility = Visibility.Visible;
                                        this.BackMessageButton.Visibility = Visibility.Visible;
                                    }
                                    else if (clickButton == "page")
                                    {
                                        this.NextPageButton.Visibility = Visibility.Visible;
                                        this.BackPageButton.Visibility = Visibility.Visible;
                                    }
                                };
                            }
                        }
                        else
                        {
                            if (clickMethod == "next_only")
                            {
                                if (clickButton == "msg")
                                {
                                    this.NextMessageButton.Visibility = Visibility.Visible;
                                }
                                else if (clickButton == "page")
                                {
                                    this.NextPageButton.Visibility = Visibility.Visible;
                                }
                            }
                            else
                            {
                                if (clickButton == "msg")
                                {
                                    this.NextMessageButton.Visibility = Visibility.Visible;
                                    this.BackMessageButton.Visibility = Visibility.Visible;
                                }
                                else if (clickButton == "page")
                                {
                                    this.NextPageButton.Visibility = Visibility.Visible;
                                    this.BackPageButton.Visibility = Visibility.Visible;
                                }
                            }
                        }
                    }
                    this.isClickable = true;

                    break;

                // 漫画めくり
                case "flip":

                    this.MangaFlipButton.Visibility = Visibility.Visible;

                    Storyboard sb = this.FindResource("wipe_flip_manga_button_image") as Storyboard;

                    if (sb != null)
                    {
                        // 二重終了防止策
                        bool isDuplicate = false;

                        sb.Completed += (s, e) =>
                        {
                            if (!isDuplicate)
                            {
                                this.isClickable = true;

                                isDuplicate = true;
                            }
                        };
                        sb.Begin(this);
                    }
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
                    };
                    break;

                // BGM
                case "bgm":

                    if (this.dataOption.IsPlayBGM)
                    {
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
                    }
                    else
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    break;

                // 効果音
                case "se":

                    if (this.dataOption.IsPlaySE)
                    {
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
                    }
                    else
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    break;

                // ハートゲージに対する処理
                case "gauge":

                    this.ViewSizeOfFeelingTextBlock.Text = "50";

                    this.SelectHeartImage.Source = null;
                    this.SelectNeedleImage.Source = null;

                   
                    break;

                case "sub":

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "goto":

                    if (this.scenarios[this.scenarioCount].Count > 1 && this.scenarios[this.scenarioCount][1] != "")
                    {
                        var GoToLabel = this.scenarios[this.scenarioCount][1];
                        if (GoToLabel == "current_scene")
                        {
                            this.GoTo(this.scene);
                        }
                        else
                        {
                            this.GoTo(GoToLabel);
                        }
                    }
                    break;

                case "get_item":

                    this.dataItem.HasGotItem01 = true;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataItem SET HasGotItem01 = 1 WHERE Id = 1;");
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                // イメージに対しての処理
                case "gif":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var gifObject = this.imageObjects[this.position];

                    var gifFile = this.scenarios[this.scenarioCount][2];

                    var gifImage = new BitmapImage();

                    gifImage.BeginInit();

                    gifImage.UriSource = new Uri($"Images/{gifFile}", UriKind.Relative);

                    gifImage.EndInit();

                    ImageBehavior.SetAnimatedSource(gifObject, gifImage);

                    gifObject.Visibility = Visibility.Visible;

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "item_book":

                    Image[] itemMainImages = { this.Item02MainImage, this.Item03MainImage, this.Item04MainImage, this.Item05MainImage, this.Item06MainImage, this.Item07MainImage, this.Item08MainImage, this.Item09MainImage, this.Item10MainImage, this.Item11MainImage };

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

                case "#":

                    // しれっとメモリ開放
                    if (this.imageInlines?.Count > 0)
                    {
                        this.imageInlines.Clear();
                    }
                    if (this.runs?.Count > 0)
                    {
                        this.runs.Clear();
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "outline_text":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var outLineTextObject = this.outLineTextObjects[this.position];

                    var outLineTextStyle = this.scenarios[this.scenarioCount][2];

                    outLineTextObject.Visibility = Visibility.Visible;

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var _text = this.scenarios[this.scenarioCount][2];

                        var _texts = this.SequenceCheck(_text);

                        this.ShowSentence(textObject: (TextBlock)outLineTextObject, sentences: _texts, mode: "text");
                    }
                    else
                    {
                        if (__textObject.Text != "")
                        {
                            var _texts = this.SequenceCheck(__textObject.Text);

                            // xamlに直接書いたStaticな文章を表示する場合
                            this.ShowSentence(textObject: __textObject, sentences: _texts, mode: "text");
                        }
                    }

                    string outLineTextAnimeIsSync = "sync";

                    // テキストに対するアニメも一応用意
                    if (this.scenarios[this.scenarioCount].Count > 5 && this.scenarios[this.scenarioCount][5] != "")
                    {
                        textAnimeIsSync = this.scenarios[this.scenarioCount][5];
                    }

                    if (this.scenarios[this.scenarioCount].Count > 4 && this.scenarios[this.scenarioCount][4] != "")
                    {
                        var textStoryBoard = this.scenarios[this.scenarioCount][4];

                        var textObjectName = __textObject.Name;

                        this.ShowAnime(storyBoard: textStoryBoard, objectName: textObjectName, isSync: outLineTextAnimeIsSync);
                    }
                    else
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
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
                }
            }
            
            Dictionary<string, string> imageOrTextDic = new Dictionary<string, string>()
            {
                {"name", this.initConfig.userName},
                {"dumy", "dumyText"}
            };

            text = text.Replace("【くん／ちゃん／さん】", this.initConfig.userTitle);

            // 苦悶の改行処理（文章中の「鬱」を疑似改行コードとする）
            text = text.Replace("鬱", "\u2028");

            foreach (string imageOrTextKey in imageOrTextDic.Keys)
            {
                switch (imageOrTextKey)
                {
                    case "name":

                        var imageNameTags = new Regex(@"\<image=name\>(.*?)\<\/image\>").Matches(text);
                        var imageNamePath = $"./Log/{initConfig.userName}/name.png";

                        if (imageNameTags.Count > 0)
                        {
                            if (!File.Exists(imageNamePath))
                            {
                                text = text.Replace(imageNameTags[0].Value, imageOrTextDic[imageOrTextKey]);
                            }
                        }
                        break;

                    default: { break; }
                }
            }

            var matchTexts = new Regex(@"\<(.+?\=.+?)\>(.*?)\<(\/.+?)\>").Matches(text);

            var tempText = text;

            List<string> text1ds;
            List<List<string>> text2ds = new List<List<string>>();

            foreach (Match matchText in matchTexts)
            {
                var startTagRaw = new Regex(@"\<(.+?\=.+?)\>").Matches(matchText.Value)[0].ToString();
                var endTagRaw = new Regex(@"\<(\/.+?)\>").Matches(matchText.Value)[0].ToString();

                var trimTag = startTagRaw.Replace("<", "").Replace(">", "");
                var tagRaws = trimTag.Split("=");

                var tag = tagRaws[0];
                var option = string.Join(",", tagRaws[1].Split("#"));

                tempText = tempText.Replace(startTagRaw, "$").Replace(endTagRaw, "$");
                var texts = tempText.Split("$");

                text1ds = new List<string> { };

                if (texts[0] != "")
                {
                    text1ds.Add(texts[0]);
                    tempText = tempText.Remove(0, texts[0].Length);
                    text2ds.Add(text1ds);
                }

                text1ds = new List<string> { };

                text1ds.Add(texts[1]);
                text1ds.Add(tag);
                text1ds.Add(option);

                tempText = tempText.Remove(0, texts[1].Length + 2);

                text2ds.Add(text1ds);
            }

            if (tempText.Length > 0)
            {
                text1ds = new List<string> { };

                text1ds.Add(tempText);
                text2ds.Add(text1ds);
            }
            return text2ds;
        }

        // 純正のメッセージ表示関数
        private void PureShowMessage(TextBlock textObject, string message, object obj = null)
        {
            this.word_num = 0;

            // メッセージ表示処理
            this.msgTimer = new DispatcherTimer();
            this.msgTimer.Tick += ViewMsg;
            this.msgTimer.Interval = TimeSpan.FromSeconds(1.0f / this.dataOption.MessageSpeed);
            this.msgTimer.Start();

            // 一文字ずつメッセージ表示（Inner Func）
            void ViewMsg(object sender, EventArgs e)
            {
                textObject.Text = message.Substring(0, this.word_num);

                if (this.word_num == 0)
                {
                    textObject.Visibility = Visibility.Visible;
                }

                if (this.word_num < message.Length)
                {
                    this.word_num++;
                }
                else
                {
                    this.msgTimer.Stop();
                    this.msgTimer = null;

                    if (obj != null)
                    {
                        this.MessageCallBack(obj);
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }
            }
        }

        private void ShowSentence(TextBlock textObject, List<List<string>> sentences, string mode, object obj = null)
        {
            if (this.imageInlines.ContainsKey(textObject.Name))
            {
                this.imageInlines.Remove(textObject.Name);
            }
            if (this.runs.ContainsKey(textObject.Name))
            {
                this.runs.Remove(textObject.Name);
            }
            this.imageInlines.Add(textObject.Name, new List<InlineUIContainer>());
            this.runs.Add(textObject.Name, new List<Run>());

            textObject.Text = "";

            this.runs[textObject.Name].Clear();
            this.imageInlines[textObject.Name].Clear();

            this.inlineCount = 0;
            this.imageInlineCount = 0;

            if (mode == "msg")
            {
                textObject.Visibility = Visibility.Visible;

                this.word_num = 0;

                // メッセージ表示処理
                this.msgTimer = new DispatcherTimer();
                this.msgTimer.Tick += ViewWordCharacter;
                this.msgTimer.Interval = TimeSpan.FromSeconds(1.0f / this.dataOption.MessageSpeed);
                this.msgTimer.Start();
            }

            textObject.Inlines.Clear();

            // 画像インラインと文字インラインの合体
            foreach (var stns in sentences)
            {
                string namePngPath = $"./Log/{this.initConfig.userName}/name.png";

                if (stns.Count > 2 && stns[1] == "image" && stns[2] == "name" && File.Exists(namePngPath))
                {
                    var imageInline = new InlineUIContainer { Child = new Image { Source = null, Height = textObject.FontSize } };

                    textObject.Inlines.Add(imageInline);

                    this.imageInlines[textObject.Name].Add(imageInline);
                }
                if (stns.Count > 2 && stns[1] == "image"&& Regex.IsMatch(stns[2],".+png"))
                {
                    var imageInline = new InlineUIContainer { Child = new Image { Source = null, Height = textObject.FontSize } };

                    textObject.Inlines.Add(imageInline);

                    this.imageInlines[textObject.Name].Add(imageInline);
                }
                var run = new Run { };

                if (stns.Count > 2 && stns[1] == "font")
                {
                    var options = stns[2].Split(",");

                    var foreground = new SolidColorBrush(Colors.Black);
                    double fontSize = textObject.FontSize;

                    var background = new SolidColorBrush(Colors.White);
                    background.Opacity = 0;

                    var fontWeights = FontWeights.Normal;

                    TextDecoration textDecoration = new TextDecoration();
                    TextDecorationCollection textDecorations = new TextDecorationCollection();

                    if (options.Length > 0 && options[0] != "")
                    {
                        switch (options[0])
                        {
                            case "red": { foreground = new SolidColorBrush(Colors.Red); break; };
                            case "green": { foreground = new SolidColorBrush(Colors.Green); break; };
                            case "blue": { foreground = new SolidColorBrush(Colors.Blue); break; };

                            default: { break; }
                        }
                    }

                    if (options.Length > 1 && options[1] != "")
                    {
                        fontSize = double.Parse(options[1]);
                    }

                    if (options.Length > 2 && options[2] != "")
                    {
                        switch (options[2])
                        {
                            case "red": { background = new SolidColorBrush(Colors.Red); background.Opacity = 1; break; };
                            case "green": { background = new SolidColorBrush(Colors.Green); background.Opacity = 1; break; };
                            case "blue": { background = new SolidColorBrush(Colors.Blue); background.Opacity = 1; break; };
                            case "yellow": { background = new SolidColorBrush(Colors.Yellow); background.Opacity = 1; break; };

                            default: { break; }
                        }
                    }

                    if (options.Length > 3 && options[3] != "")
                    {
                        if (options[3] == "true")
                        {
                            fontWeights = FontWeights.UltraBold;
                        }
                    }

                    if (options.Length > 4 && options[4] != "")
                    {
                        switch (options[4])
                        {
                            case "red": { textDecoration.Pen = new Pen(Brushes.Red, 1); break; };
                            case "green": { textDecoration.Pen = new Pen(Brushes.Green, 1); break; };
                            case "blue": { textDecoration.Pen = new Pen(Brushes.Blue, 1); break; };
                            case "black": { textDecoration.Pen = new Pen(Brushes.Black, 1); break; };

                            default: { break; }
                        }
                        textDecoration.PenThicknessUnit = TextDecorationUnit.FontRecommended;
                        textDecorations.Add(textDecoration);
                    }
                    run = new Run { Text = "", Foreground = foreground, FontSize = fontSize, Background = background, FontWeight = fontWeights, TextDecorations = textDecorations };
                }
                else
                {
                    run = new Run { Text = "" };
                }

                textObject.Inlines.Add(run);

                this.runs[textObject.Name].Add(run);

                if (mode == "text")
                {
                    ViewTextAtOnes();
                }
            }

            // 一文字ずつメッセージ表示（Inner Func）
            void ViewWordCharacter(object sender, EventArgs e)
            {
                if (this.inlineCount < sentences.Count)
                {
                    var stns = sentences[this.inlineCount];

                    string namePngPath = $"./Log/{this.initConfig.userName}/name.png";

                    if (stns.Count > 2 && stns[1] == "image" && stns[2] == "name" && File.Exists(namePngPath))
                    {
                        // 実行ファイルの場所を絶対パスで取得
                        var startupPath = FileUtils.GetStartupPath();

                        var image = new BitmapImage();

                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        image.UriSource = new Uri($@"{startupPath}/{namePngPath}", UriKind.Absolute);
                        image.EndInit();

                        image.Freeze();

                        (this.imageInlines[textObject.Name][imageInlineCount].Child as Image).Source = image;

                        this.imageInlineCount++;

                        this.inlineCount++;
                        this.word_num = 0;

                        return;
                    }
                    if (stns.Count > 2 && stns[1] == "image" && Regex.IsMatch(stns[2], ".+png") )
                    {
                        /*
                        var startupPath = FileUtils.GetStartupPath();

                        var image = new BitmapImage();

                        string imageFile = stns[2];

                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        image.UriSource = new Uri($"Images/{imageFile}", UriKind.Relative);
                        image.EndInit();

                        image.Freeze();

                        (this.imageInlines[textObject.Name][imageInlineCount].Child as Image).Source = image;

                       // this.WipeInImage(this.imageInlines[textObject.Name][imageInlineCount].Child as Image, image.Width,TimeSpan.FromSeconds(1.0));
                       */
                        this.imageInlineCount++;

                        this.inlineCount++;
                        this.word_num = 0;
                        

                        return;
                    }
                    this.runs[textObject.Name][inlineCount].Text = stns[0].Substring(0, this.word_num);

                    if (this.word_num < stns[0].Length)
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

                    if (obj != null)
                    {
                        this.MessageCallBack(obj);
                    }

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }
            }

            // 一気にテキストを表示（Inner Func）
            void ViewTextAtOnes()
            {
                if (this.inlineCount < sentences.Count)
                {
                    var stns = sentences[this.inlineCount];

                    string namePngPath = $"./Log/{this.initConfig.userName}/name.png";

                    if (stns.Count > 2 && stns[1] == "image" && stns[2] == "name" && File.Exists(namePngPath))
                    {
                        // 実行ファイルの場所を絶対パスで取得
                        var startupPath = FileUtils.GetStartupPath();

                        var image = new BitmapImage();

                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        image.UriSource = new Uri($@"{startupPath}/{namePngPath}", UriKind.Absolute);
                        image.EndInit();

                        image.Freeze();

                        (this.imageInlines[textObject.Name][imageInlineCount].Child as Image).Source = image;

                        this.imageInlineCount++;
                        this.inlineCount++;

                        return;
                    }
                    if (stns.Count > 2 && stns[1] == "image" && Regex.IsMatch(stns[2], ".+png"))
                    {
                        var image = new BitmapImage();

                        string imageFile = stns[2];

                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        image.UriSource = new Uri($"Images/{imageFile}", UriKind.Relative);
                        image.EndInit();

                        image.Freeze();

                        (this.imageInlines[textObject.Name][imageInlineCount].Child as Image).Source = image;

                        // this.WipeInImage(this.imageInlines[textObject.Name][imageInlineCount].Child as Image, image.Width,TimeSpan.FromSeconds(1.0));

                        this.imageInlineCount++;

                        this.inlineCount++;
                        this.word_num = 0;

                        return;
                    }
                    this.runs[textObject.Name][inlineCount].Text = stns[0];
                    this.inlineCount++;
                }
            }
        }

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

        // アニメーション（ストーリーボード）の処理
        private void ShowAnime(string storyBoard,string objectName, string isSync)
        {
            Storyboard sb = this.FindResource(storyBoard) as Storyboard;

            foreach (var child in sb.Children)
                Storyboard.SetTargetName(child, objectName);

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
            if (button.Name == "BackMessageButton")
            {
                this.BackMessageButton.Visibility = Visibility.Hidden;
                this.NextMessageButton.Visibility = Visibility.Hidden;

                BackScenario();
            }

            if (button.Name == "BackPageButton")
            {
                this.BackPageButton.Visibility = Visibility.Hidden;
                this.NextPageButton.Visibility = Visibility.Hidden;

                BackScenario();
            }

            void BackScenario()
            {
                var index = this.scenarioCount;
                int returnCount = 0;

                while (index > 0)
                {
                    if (this.scenarios[index][0] == "#")
                    {
                        if (returnCount >= RETURN_COUNT)
                        {
                            this.scenarioCount = index;
                            this.ScenarioPlay();

                            break;
                        }
                        returnCount += 1;
                    }
                    index -= 1;
                }
            }
            if (button.Name == "BranchButton1")
            {
                this.GoTo("manga");
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
                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataChapter2 SET MySelectGoodEvents = '{this.dataChapter2.MySelectGoodEvents}'WHERE CreatedAt = '{this.dataChapter2.CreatedAt}';");
                    }

                }
               
                

                if(scene== "「おいしいものを食べる」ときは？")
                {
                    this.aosukesDifficultyOfEating = this.AosukeDifficultyOfActionText.Text;
                    this.aosukesSizeOfFeelingOfEating = this.AosukeSizeOfFeelingText.Text;

                    this.dataChapter2.AosukesDifficultyOfEating = this.aosukesDifficultyOfEating;
                    this.dataChapter2.AosukesSizeOfFeelingOfEating = this.aosukesSizeOfFeelingOfEating;
                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
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
                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataChapter2 SET AosukesSizeOfFeelingOfGettingHighScore = '{this.dataChapter2.AosukesSizeOfFeelingOfGettingHighScore}'WHERE CreatedAt = '{this.dataChapter2.CreatedAt}';");
                        connection.Execute($@"UPDATE DataChapter2 SET AosukesDifficultyOfGettingHighScore = '{this.dataChapter2.AosukesDifficultyOfGettingHighScore}'WHERE CreatedAt = '{this.dataChapter2.CreatedAt}';");

                    }
                }
                if (scene == "「休み時間に友だちとおしゃべりする」ときは？")
                {
                    this.aosukesDifficultyOfTalkingWithFriend = this.AosukeDifficultyOfActionText.Text;
                    this.aosukesSizeOfFeelingOfTalkingWithFriend = this.AosukeSizeOfFeelingText.Text;

                    this.dataChapter2.AosukesSizeOfFeelingOfTalkingWithFriend = this.aosukesDifficultyOfEating;
                    this.dataChapter2.AosukesDifficultyOfTalkingWithFriend = this.aosukesSizeOfFeelingOfEating;
                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataChapter2 SET AosukesSizeOfFeelingOfTalkingWithFriend = '{this.dataChapter2.AosukesSizeOfFeelingOfTalkingWithFriend}'WHERE CreatedAt = '{this.dataChapter2.CreatedAt}';");
                        connection.Execute($@"UPDATE DataChapter2 SET AosukesDifficultyOfTalkingWithFriend = '{this.dataChapter2.AosukesDifficultyOfTalkingWithFriend}'WHERE CreatedAt = '{this.dataChapter2.CreatedAt}';");
                    }
                }

                if (scene == "グループアクティビティ")
                {
                    if (this.dataOption.InputMethod == 1)
                    {
                        this.myALittlleExcitingEvents = this.InputText.Text;
                        this.dataChapter2.MyALittlleExcitingEvents = this.myALittlleExcitingEvents;
                        
                        using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                        {
                            connection.Execute($@"UPDATE DataChapter2 SET MyALittlleExcitingEvents = '{this.dataChapter2.MyALittlleExcitingEvents}'WHERE CreatedAt = '{this.dataChapter2.CreatedAt}';");
                        }
                    }
                }

                this.scenarioCount += 1;
                this.ScenarioPlay();
            }
            
            if(button.Name== "GroupeActivityWritingButton")
            {
                
                    if (this.dataOption.InputMethod == 1)
                    {
                        this.InputTextGrid.Visibility = Visibility.Visible;

                        this.ReadyKeyboard();
                        this.InputText.Focus();

                        if (this.GroupeActivityWritingButton.Content == null)
                        {
                            this.GroupeActivityWritingButton.Content = new ScrollViewer { Height = 350, Width = 1300, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };
                            ScrollViewer scroll = this.GroupeActivityWritingButton.Content as ScrollViewer;
                            TextBlock text = new TextBlock { Name = "GroupeActivityInputText", FontSize = 40, FontFamily = new FontFamily("Yu Gothic"), TextWrapping = TextWrapping.Wrap};
                            text.PreviewMouseDown += new MouseButtonEventHandler(TextBoxMouseDown);

                            scroll.Content = text;
                            this.InputText.SelectAll();
                        }
                        else
                        {
                            ScrollViewer scroll = this.GroupeActivityWritingButton.Content as ScrollViewer;
                            TextBlock text = scroll.Content as TextBlock;
                            if (text.Text != "")
                            {
                                 this.InputText.Text = text.Text;
                            }
                    }
                    }
                    if (this.dataOption.InputMethod == 0)
                    {
                        if (this.GroupeActivityWritingButton.Content == null)
                        {
                            this.GroupeActivityWritingButton.Content = new Image { Name = "GroupeActivityWritingImage", Margin = new Thickness(0, 60, 0, 0) };
                        }

                        this.CanvasGrid.Visibility = Visibility.Visible;
                        
                    }
                
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
                System.Windows.Rect rectBounds = new System.Windows.Rect(0, 0,this.NameCanvas.ActualWidth,this.NameCanvas.ActualHeight );

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
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)rectBounds.Width, (int)rectBounds.Height,96, 96,PixelFormats.Pbgra32);
                rtb.Render(dv);

                //仮置き
                string nameBmp = "GroupeActivity02_MyALittlleExcitingEvents.bmp";
                string dirPath = $"./Log/{this.initConfig.userName}";

                string nameBmpPath = System.IO.Path.Combine(dirPath, nameBmp);
                var startupPath = FileUtils.GetStartupPath();

                PngBitmapEncoder png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(rtb));

                // ファイルのパスは仮
                using (var stream = File.Create($@"{startupPath}/{nameBmpPath}"))
                {
                    png.Save(stream);
                }

                var pngmap = new BitmapImage();

                pngmap.BeginInit();
                pngmap.CacheOption = BitmapCacheOption.OnLoad;    //ココ
                pngmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;  //ココ
                pngmap.UriSource = new Uri($@"{startupPath}/{nameBmpPath}", UriKind.Absolute);
                pngmap.EndInit();

                pngmap.Freeze();

                this.GroupeActivityGrid.Visibility = Visibility.Visible;
                this.CanvasGrid.Visibility = Visibility.Hidden;

               (this.GroupeActivityWritingButton.Content as Image).Source  = new BitmapImage(new Uri($@"{startupPath}/{nameBmpPath}", UriKind.Absolute));

            }
            if (this.tag == "かんたんにできるか")
            {
                this.DifficultySelectGrid.Visibility = Visibility.Hidden;

                if (button.Name == "GoodButton")
                {
                    this.AosukeDifficultyOfActionText.Text = "〇";
                }
                if (button.Name == "BadButton")
                {
                    this.AosukeDifficultyOfActionText.Text = "×";
                }
                if (button.Name == "NormalButton")
                {
                    this.AosukeDifficultyOfActionText.Text = "△";
                }

                this.GoTo(scene);

                if (this.AosukeDifficultyOfActionText.Text != "" && this.AosukeSizeOfFeelingText.Text != "")
                {
                    this.SelectFeelingNextButton.Visibility = Visibility.Visible;
                }
            }
            if (button.Name == "SizeOfFeelingButton")
            {
                this.Challenge2Grid.Visibility = Visibility.Hidden;
                this.SelectFeelingNextButton.Visibility = Visibility.Hidden;
                string _jumptag = "きもちのおおきさ";
                this.GoTo(_jumptag);
            }
            if (button.Name == "DifficultyOfActionButton")
            {
                this.Challenge2Grid.Visibility = Visibility.Hidden;
                this.SelectFeelingNextButton.Visibility = Visibility.Hidden;
                string _jumptag = "かんたんにできるか";
                this.GoTo(_jumptag);
            }

            if(button.Name== "InputTextCompleteButton")
            {
                if (this.InputText.Text != "")
                {
                    this.InputTextGrid.Visibility = Visibility.Hidden;
                    ScrollViewer scroll = this.GroupeActivityWritingButton.Content as ScrollViewer;
                    TextBlock text = scroll.Content as TextBlock;
                    text.Text = this.InputText.Text;

                    if (OnScreenKeyboard.IsOpened())
                    {
                        try
                        {
                            OnScreenKeyboard.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("なにも書かれてないよ");
                }
                
              
            }
            if (this.isClickable && (button.Name == "NextMessageButton" || button.Name == "NextPageButton" || button.Name == "SelectFeelingCompleteButton" || button.Name =="MangaFlipButton"||button.Name=="BranchButton2"))
            {
                this.isClickable = false;

                if (button.Name == "NextMessageButton")
                {
                    this.BackMessageButton.Visibility = Visibility.Hidden;
                    this.NextMessageButton.Visibility = Visibility.Hidden;
                }

                if (button.Name == "NextPageButton")
                {
                    this.BackPageButton.Visibility = Visibility.Hidden;
                    this.NextPageButton.Visibility = Visibility.Hidden;
                }

                this.scenarioCount += 1;
                this.ScenarioPlay();
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

        private enum AnswerResult
        {
            None,
            Incorrect,
            Intermediate,
            Correct,
            Decision,
            Cancel,
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

                if (this.AosukeDifficultyOfActionText.Text != "" && this.AosukeSizeOfFeelingText.Text != "")
                {
                    this.SelectFeelingNextButton.Visibility = Visibility.Visible;
                }

                this.GoTo(scene);

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

        private void GoTo(string tag)
        {
            foreach (var (scenario, index) in this.scenarios.Indexed())
            {
                if (scenario[0] == "sub" && scenario[1] == tag)
                {
                    this.scenarioCount = index + 1;
                    this.ScenarioPlay();
                }
                if (this.scene == tag && (scenario[0] == "scene" && scenario[1] == tag))
                {
                    this.scenarioCount = index + 1;
                    this.ScenarioPlay();
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

        //セリフ内の画像をワイプインアニメーションで表示させるための処理
        private void WipeInImage(Image wordArtImage, double newWidth, TimeSpan duration)
        {
            this.msgTimer.Stop();

            DoubleAnimation animation = new DoubleAnimation(newWidth,duration);

            animation.Completed += (s, e) =>
            {
                // 二重終了防止策
                bool isDuplicate = false;

                if (!isDuplicate)
                {
                    this.msgTimer.Start();

                    isDuplicate = true;
                }
            };

            wordArtImage.BeginAnimation(Image.WidthProperty, animation);
        }

        private void ReadyKeyboard()
        {
            if (!OnScreenKeyboard.IsOpened())
            {
                try
                {
                    OnScreenKeyboard.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void TextBoxMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Button_Click(this.GroupeActivityWritingButton, e);
        }

        private void SelectGoodEvent(object sender, MouseButtonEventArgs e)
        {
            //XAML上で記載したListBoxのテンプレートにEllipseコントロールを追加
            Grid feelingGrid = sender as Grid;

            Color feelingColor = Colors.Black ;

            Brush colorBrush = new SolidColorBrush { Color = feelingColor };
            Ellipse feelingColorEllipse = new Ellipse { Stroke = colorBrush, StrokeThickness = 3, Margin = new Thickness(25, 5, 25, 0) };

            AnswerResult selectResult = AnswerResult.None;

            if (feelingGrid.Children.Count < 2)
            {
                feelingGrid.Children.Add(feelingColorEllipse);
                selectResult = AnswerResult.Decision;

                if (this.SelectGoodEventListBox.SelectedItems.Count != 1)
                    this.SelectFeelingNextButton.Visibility = Visibility.Visible;
            }
            else
            {
                feelingGrid.Children.RemoveAt(1);
                selectResult = AnswerResult.Cancel;

                if (this.SelectGoodEventListBox.SelectedItems.Count == 1)
                    this.SelectFeelingNextButton.Visibility = Visibility.Hidden;
            }

            var startupPath = FileUtils.GetStartupPath();

            PlaySE($@"{startupPath}/Sounds/{selectResult}.wav");

           
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

       
    }
}
