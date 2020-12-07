using CsvReadWrite;
using Expansion;
using FileIOUtils;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Osklib;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
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
    public partial class Chapter4 : Page
    {
        // 気持ちのリスト
        private string[] GOOD_FEELINGS = { "●　うれしい", "●　しあわせ", "●　たのしい", "●　ホッとした", "●　きもちいい", "●　まんぞく", "●　すき", "●　やる気マンマン", "●　かんしゃ", "●　わくわく", "●　うきうき", "●　ほこらしい" };
        private string[] BAD_FEELINGS = { "●　心配", "●　こまった", "●　不安", "●　こわい", "●　おちこみ", "●　がっかり", "●　いかり", "●　イライラ", "●　はずかしい", "●　ふまん", "●　かなしい", "●　おびえる" };

        //選択肢のリスト

        private float THREE_SECOND_RULE_TIME = 3.0f;

        // ゲームを進行させるシナリオ
        private int scenarioCount = 0;
        private List<List<string>> scenarios = null;

        // 各種コントロールの名前を収める変数
        private string position = "";

        // マウスクリックを可能にするかどうかのフラグ
        private bool isClickable = true;


        // 気持ちの大きさ
        private int feelingSize = 0;

        //グループアクティビティで選択した言葉が正解かどうかのフラグ
        private bool isCorrect = false;

        // メッセージ表示関連
        private DispatcherTimer msgTimer;
       
        private int word_num;

        private int inlineCount;
        private int imageInlineCount;

        private Dictionary<string, List<Run>> runs = new Dictionary<string, List<Run>>();
        private Dictionary<string, List<InlineUIContainer>> imageInlines = new Dictionary<string, List<InlineUIContainer>>();


        // 各種コントロールを任意の文字列で呼び出すための辞書
        private Dictionary<string, Image> imageObjects = null;
        private Dictionary<string, TextBlock> textBlockObjects = null;
        private Dictionary<string, Button> buttonObjects = null;
        private Dictionary<string, Grid> gridObjects = null;
        private Dictionary<string, ListBox> itemsControlObjects = null; 

       

        private Dictionary<string, List<string>> SITUATIONS_SELECTION = null;
        private string[] EDIT_BUTTON = { "えんぴつ", "けしごむ", "すべてけす", "かんせい" };
        private string[] IMAGE_TEXTS = { "name" ,"word_art_01", "word_art_02" };
        private string[] WORD_TEXTS = { "marker", "bold" ,"under_line"};

        private TextBlock  selectedWordButtonText= new TextBlock();


        // 音関連
        private WindowsMediaPlayer mediaPlayer;
        private SoundPlayer sePlayer = null;

        // データベースに収めるデータモデルのインスタン
        private DataChapter4 dataChapter4;

        // ゲームの切り替えシーン
        private string scene;

        // データベースに収めるデータモデルのインスタンス
        private InitConfig initConfig = new InitConfig();
        private DataOption dataOption = new DataOption();
        private DataItem dataItem = new DataItem();
        private DataProgress dataProgress = new DataProgress();


        public Chapter4()
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
            this.dataChapter4 = new DataChapter4();

            this.SITUATIONS_SELECTION = new Dictionary<string, List<string>>
            {
                ["select_branch"] = new List<string>(){ "おぼえているよ", "かくにんしたいな" },
                ["select_situation"] = new List<string>() { "場面①の赤丸くん", "場面②のキミちゃん", "場面③の青助くん" },
                ["choice_words_akamaru"] = new List<string>() { "一緒に見せてくれる？", "ありがとう、助かるよ", "教科書を家に忘れたから" },
                ["choice_words_kimi"] = new List<string>() { "日曜は家の用事があるの", "また別の日に遊びましょう", "ごめんね", "だから、その日は遊べないわ" },
                ["choice_words_aosuke"] = new List<string>() { "だから貸せないんだ", "ごめんよ", "僕、まだ読み終わっていなくて", "読み終わったら貸してあげるよ" },
               
                ["situation_akamaru"] = new List<string>() { "①理由を言う", "②してほしいことを言う","③お礼を言う" },
                ["situation_kimi"] = new List<string>() { "①あやまる", "②理由を言う", "③ことわる", "④代わりの言葉" },
                ["situation_aosuke"] = new List<string>() { "①あやまる", "②理由を言う", "③ことわる","④代わりの言葉" },

                ["correct_words_akamaru"] = new List<string>() { "教科書を家に忘れたから", "一緒に見せてくれる？", "ありがとう、助かるよ" },
                ["correct_words_kimi"] = new List<string>() { "ごめんね", "日曜は家の用事があるの", "だから、その日は遊べないわ", "また別の日に遊びましょう" },
                ["correct_words_aosuke"] = new List<string>() { "ごめんよ", "僕、まだ読み終わっていなくて", "だから貸せないんだ", "読み終わったら貸してあげるよ" },

            };
            

            this.SelectGoodFeelingListBox.ItemsSource = GOOD_FEELINGS;
            this.SelectBadFeelingListBox.ItemsSource = BAD_FEELINGS;

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
                ["item_center_up_image"] = this.ItemCenterUpImage, //
                ["item_left_image"] = this.ItemLeftImage,
                ["item_left_last_image"] = this.ItemLeftLastImage,
                ["session_title_image"] = this.SessionTitleImage,
                ["select_heart_image"] = this.SelectHeartImage,
                ["select_needle_image"] = this.SelectNeedleImage,
                ["children_face_left_image"] = this.ChildrenFaceLeftImage,
                ["children_face_right_image"] = this.ChildrenFaceRightImage,
                ["shiroji_ending_image"] = this.ShirojiEndingImage,
                ["shiroji_left_image"] = this.ShirojiLeftImage,
                ["shiroji_right_image"] = this.ShirojiRightImage,
                ["shiroji_right_up_image"] = this.ShirojiRightUpImage,
                ["shiroji_small_right_up_image"] = this.ShirojiSmallRightUpImage,
                ["shiroji_small_right_down_image"] = this.ShirojiSmallRightDownImage,
                ["children_stand_left_image"] = this.ChildrenStandLeftImage,
                ["children_stand_right_image"] = this.ChildrenStandRightImage,
                ["manga_flip_arrow_go_image"] = this.MangaFlipArrowGoImage,
                ["session_title_image"] = this.SessionTitleImage,



                ["main_msg_bubble_image"] = this.MainMessageBubbleImage,
                ["activity_title_image"] = this.ActivityTitleImage,
                ["situation_character_image"] = this.SituationCharacterImage,
                ["groupe_activity_character"] =this.GroupeActivityCharacter,
                ["select_word_bubble"] =this.SelectWordBubble

            };

            this.textBlockObjects = new Dictionary<string, TextBlock>
            {
                ["session_sub_title_text"] = this.SessionSubTitleTextBlock,
                ["session_sentence_text"] = this.SessionSentenceTextBlock,

                ["ending_msg_text"] = this.EndingMessageTextBlock,
                ["main_msg"] = this.MainMessageTextBlock,
                ["music_title_text"] = this.MusicTitleTextBlock,
                ["composer_name_text"] = this.ComposerNameTextBlock,
                ["kind_of_feeling_text"] = this.KindOfFeelingText,
                ["size_of_feeling_text"] = this.SizeOfFeelingText,
                ["children_text_bubble_text"] = this.ChildrenTextBubbleText,
                ["activity_title_text"] = this.ActivityTitleText,
                ["item_check_right_text"] = this.ItemCheckRightText,
                ["item_check_center_text"] = this.ItemCheckCenterText,
                ["item_book_title_text"] = this.ItemBookTitleTextBlock,
                ["result_kind_of_feeling_text"] = this.ResultKindOfFeelingText,
                ["result_size_of_feeling_text"] = this.ResultSizeOfFeelingText,
                ["situation_title_text"] = this.SituationTitleText,
                ["situation_text"] = this.SituationText,
                ["select_word_title_text"] = this.SelectWordTitleText,
                ["complete_role_play_text"] =this.CompleteRolePlayText,
                ["small_situation_title_text"] =this.SmallSituationTitleText,
                ["small_situation_text"] =this.SmallSituationText,
                ["groupe_activity_message_text"] =this.GroupeActivityMessageTextBlock,
                ["session_frame_text"] =this.SessionFrameText,

            };

            this.buttonObjects = new Dictionary<string, Button>
            {
                ["next_msg_button"] = this.NextMessageButton,
                ["back_msg_button"] = this.BackMessageButton,
                ["next_page_button"] = this.NextPageButton,
                ["back_page_button"] = this.BackPageButton,
                ["manga_flip_button"] = this.MangaFlipButton,
                ["manga_prev_back_button"] = this.MangaPrevBackButton,
                ["select_feeling_complete_button"] = this.SelectFeelingCompleteButton,
                ["select_feeling_next_button"] = this.SelectFeelingNextButton,
                ["select_feeling_back_button"] = this.SelectFeelingBackButton,
                ["complete_role_play_button"] = this.CompleteRolePlayButton,
                ["ok_button"] =this.OkButton,
            };

            this.gridObjects = new Dictionary<string, Grid>
            {
                ["session_grid"] = this.SessionGrid,
                ["challenge_time_grid"] = this.ChallengeTimeGrid,
                ["activity_title_grid"] = this.ActivityTitleGrid,
                ["session_frame_grid"] =this.SessionFrameGrid,

                ["summary_grid"] = this.SummaryGrid,
                ["ending_grid"] = this.EndingGrid,
                ["item_name_plate_left_grid"] = this.ItemNamePlateLeftGrid,
                ["item_name_bubble_grid"] = this.ItemNameBubbleGrid,
                ["item_name_plate_center_grid"] = this.ItemNamePlateCenterGrid,
                ["item_info_plate_grid"] = this.ItemInfoPlateGrid,
                ["item_info_sentence_grid"] = this.ItemInfoSentenceGrid,
                ["item_last_info_grid"] = this.ItemLastInfoGrid,
                ["item_review_grid"] = this.ItemReviewGrid,



                ["select_heart_grid"] = this.SelectHeartGrid,
                ["ending_msg_grid"] = this.EndingMessageGrid,
                ["main_msg_grid"] = this.MainMessageGrid,
                ["music_info_grid"] = this.MusicInfoGrid,
                ["branch_select_grid"] = this.BranchSelectGrid,
                ["view_size_of_feeling_grid"] = this.ViewSizeOfFeelingGrid,
                ["select_heart_grid"] = this.SelectHeartGrid,
                ["select_feeling_grid"] = this.SelectFeelingGrid,
                ["exit_back_grid"] = this.ExitBackGrid,
                ["challenge_time_result_grid"] = this.ChallengeTimeResultGrid,
                ["challenge_time_result_msg_grid"] = this.ChallengeTimeResultMessageGrid,
                ["item_check_right_grid"] = this.ItemCheckRightGrid,
                ["item_check_center_grid"] = this.ItemCheckCentertGrid,
                ["children_text_grid"] = this.ChildrenTextGrid,
                ["seesaw_grid"] = this.SeesawGrid,
                ["result_feeling_grid"] = this.ResultFeelingGrid,
                ["situation_plate_grid"] = this.SituationPlateGrid,
                ["groupe_activity_grid"] = this.GroupeActivityGrid,
                ["groupe_activity_message_grid"] = this.GroupeActivityMessageGrid,
                ["small_situation_title_plate_grid"] =this.SmallSituationTitlePlateGrid,


            };

            this.itemsControlObjects = new Dictionary<string, ListBox>
            {
                ["select_some_words_list_box"] =this.SelectSomeWordsListBox,
                ["some_choices_list_box"] =this.SomeChoicesListBox,
            };
        }

        private void ResetControls()
        {
            // 各種コントロールを隠すことでフルリセット

            this.SessionGrid.Visibility = Visibility.Hidden;
            this.ChallengeTimeGrid.Visibility = Visibility.Hidden;

            this.SummaryGrid.Visibility = Visibility.Hidden;
            this.EndingGrid.Visibility = Visibility.Hidden;
            this.ItemNamePlateLeftGrid.Visibility = Visibility.Hidden;
            this.ItemNameBubbleGrid.Visibility = Visibility.Hidden;
            this.ItemNamePlateCenterGrid.Visibility = Visibility.Hidden;
            this.ItemInfoPlateGrid.Visibility = Visibility.Hidden;
            this.ItemLastInfoGrid.Visibility = Visibility.Hidden;

            this.ItemReviewGrid.Visibility = Visibility.Hidden;
            this.ActivityTitleImage.Visibility = Visibility.Hidden;
            this.ChallengeTimeResultGrid.Visibility = Visibility.Hidden;
            this.ChallengeTimeResultMessageGrid.Visibility = Visibility.Hidden;
            this.ActivityTitleGrid.Visibility = Visibility.Hidden;
            this.GroupeActivityGrid.Visibility = Visibility.Hidden;
            this.GroupeActivityMessageGrid.Visibility = Visibility.Hidden;
            this.SmallSituationTitlePlateGrid.Visibility = Visibility.Hidden;
            this.SessionFrameGrid.Visibility = Visibility.Hidden;
            this.SessionFrameText.Visibility = Visibility.Hidden;

            this.SelectHeartGrid.Visibility = Visibility.Hidden;
            this.SeesawGrid.Visibility = Visibility.Hidden;
          
            this.EndingMessageGrid.Visibility = Visibility.Hidden;
            this.MainMessageGrid.Visibility = Visibility.Hidden;
            this.MusicInfoGrid.Visibility = Visibility.Hidden;
            this.GetItemGrid.Visibility = Visibility.Hidden;
            this.ItemBookMainGrid.Visibility = Visibility.Hidden;
            this.ItemBookNoneGrid.Visibility = Visibility.Hidden;
            this.ItemBookTitleTextBlock.Visibility = Visibility.Hidden;

            this.ExitBackGrid.Visibility = Visibility.Hidden;
            this.BranchSelectGrid.Visibility = Visibility.Hidden;

            //
            this.SomeChoicesListBox.Visibility = Visibility.Hidden;
            this.SelectSomeWordsListBox.Visibility = Visibility.Hidden;
           
            this.BackgroundImage.Visibility = Visibility.Hidden;
            this.MangaTitleImage.Visibility = Visibility.Hidden;
            this.MangaImage.Visibility = Visibility.Hidden;
            this.ItemCenterImage.Visibility = Visibility.Hidden;
            this.ItemCenterUpImage.Visibility = Visibility.Hidden;
            this.ItemLeftImage.Visibility = Visibility.Hidden;
            this.SessionTitleImage.Visibility = Visibility.Hidden;
            this.SessionSubTitleTextBlock.Visibility = Visibility.Hidden;
            this.SessionSentenceTextBlock.Visibility = Visibility.Hidden;
            this.SelectFeelingCompleteButton.Visibility = Visibility.Hidden;
            this.SelectFeelingNextButton.Visibility = Visibility.Hidden;
            

            this.EndingMessageTextBlock.Visibility = Visibility.Hidden;
            this.ShirojiRightImage.Visibility = Visibility.Hidden;
            this.ShirojiLeftImage.Visibility = Visibility.Hidden;
            this.ShirojiRightUpImage.Visibility = Visibility.Hidden;
            this.ShirojiSmallRightUpImage.Visibility = Visibility.Hidden;
            this.ShirojiSmallRightDownImage.Visibility = Visibility.Hidden;
            this.ShirojiEndingImage.Visibility = Visibility.Hidden;
            this.ChildrenStandLeftImage.Visibility = Visibility.Hidden;
            this.ChildrenStandRightImage.Visibility = Visibility.Hidden;


            this.ChildrenFaceLeftImage.Visibility = Visibility.Hidden;
            this.ChildrenFaceRightImage.Visibility = Visibility.Hidden;
            this.MainMessageTextBlock.Visibility = Visibility.Hidden;
            this.NextMessageButton.Visibility = Visibility.Hidden;
            this.BackMessageButton.Visibility = Visibility.Hidden;
            this.NextPageButton.Visibility = Visibility.Hidden;
            this.BackPageButton.Visibility = Visibility.Hidden;
            this.MangaFlipButton.Visibility = Visibility.Hidden;
            this.MangaPrevBackButton.Visibility = Visibility.Hidden;
            this.MangaFlipArrowGoImage.Visibility = Visibility.Hidden;

            this.ItemCheckCentertGrid.Visibility = Visibility.Hidden;
            this.ItemCheckRightGrid.Visibility = Visibility.Hidden;
            this.SituationPlateGrid.Visibility = Visibility.Hidden;

            this.CoverLayerImage.Visibility = Visibility.Hidden;

            this.ViewSizeOfFeelingGrid.Visibility = Visibility.Hidden;
            this.ResultFeelingGrid.Visibility = Visibility.Hidden;
            this.SelectFeelingGrid.Visibility = Visibility.Hidden;

            this.ReturnToTitleButton.Visibility = Visibility.Hidden;
          

            this.OkButton.Visibility = Visibility.Hidden;

            this.SelectFeelingBackButton.Visibility = Visibility.Hidden;
            this.CompleteRolePlayButton.Visibility = Visibility.Hidden;
            this.CompleteRolePlayText.Visibility = Visibility.Hidden;

            this.SessionSubTitleTextBlock.Text = "";
            this.SessionSentenceTextBlock.Text = "";
           
            this.EndingMessageTextBlock.Text = "";
            
            this.MusicTitleTextBlock.Text = "";
            this.ComposerNameTextBlock.Text = "";

            this.ResultKindOfFeelingText.Text = "";
            this.ResultSizeOfFeelingText.Text = "";

        }

        public void SetNextPage(InitConfig _initConfig, DataOption _dataOption, DataItem _dataItem, DataProgress _dataProgress)
        {
            this.initConfig = _initConfig;
            this.dataOption = _dataOption;
            this.dataItem = _dataItem;
            this.dataProgress = _dataProgress;

            // 現在時刻を取得
            this.dataChapter4.CreatedAt = DateTime.Now.ToString();

            // データベースのテーブル作成と現在時刻の書き込みを同時に行う
            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
            {
                // 毎回のアクセス日付を記録
                connection.Insert(this.dataChapter4);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var csv = new CsvReader("./Scenarios/chapter4.csv"))
            {
                this.scenarios = csv.ReadToEnd();
            }
            this.ScenarioPlay();
        }


        private void ScenarioPlay()
        {
            // デバッグのためシナリオのインデックスを出力
            Debug.Print((this.scenarioCount + 1).ToString());

            // 処理分岐のフラグ
            var tag = this.scenarios[this.scenarioCount][0];

            switch (tag)
            {
                case "start":

                    // 画面のフェードイン処理とか入れる（別関数を呼び出す）

                    this.dataProgress.CurrentChapter = 4;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET CurrentChapter = '{this.dataProgress.CurrentChapter}' WHERE Id = 1;");
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "end":

                    // 画面のフェードアウト処理とか入れる（別関数を呼び出す）

                    this.StopBGM();

                    this.dataProgress.HasCompletedChapter3 = true;

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

                //ボーダーに対しての処理
                /*case "border":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var borderObject = this.borderObjects[this.position];

                    borderObject.Visibility = Visibility.Visible;

                    string borderAnimeIsSync = "sync";

                    if (this.scenarios[this.scenarioCount].Count > 3 && this.scenarios[this.scenarioCount][3] != "")
                    {
                        borderAnimeIsSync = this.scenarios[this.scenarioCount][4];
                    }

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var borderStoryBoard = this.scenarios[this.scenarioCount][2];

                        borderStoryBoard += $"_{this.position}";

                        this.ShowAnime(storyBoard: borderStoryBoard, isSync: borderAnimeIsSync);
                    }
                    else
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    break;
                */
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

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var _text = this.scenarios[this.scenarioCount][2];

                        var _texts = this.SequenceCheck(_text);

                        this.ShowSentence(textObject: __textObject, sentences: _texts, mode: "text");
                    }
                    __textObject.Visibility = Visibility.Visible;

                    string textAnimeIsSync = "sync";

                    // テキストに対するアニメも一応用意
                    if (this.scenarios[this.scenarioCount].Count > 4 && this.scenarios[this.scenarioCount][4] != "")
                    {
                        textAnimeIsSync = this.scenarios[this.scenarioCount][4];
                    }

                    if (this.scenarios[this.scenarioCount].Count > 3 && this.scenarios[this.scenarioCount][3] != "")
                    {
                        var textStoryBoard = this.scenarios[this.scenarioCount][3];

                        textStoryBoard += $"_{this.position}";

                        this.ShowAnime(storyBoard: textStoryBoard, isSync: textAnimeIsSync);
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
                    this.isClickable = true;

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
                            else if(clickMethod == "back_only")
                            {
                                waitTimer.Start();

                                waitTimer.Tick += (s, args) =>
                                {
                                    waitTimer.Stop();
                                    waitTimer = null;

                                    if (clickButton == "msg")
                                    {
                                        this.BackMessageButton.Visibility = Visibility.Visible;
                                    }
                                    else if (clickButton == "page")
                                    {
                                        this.BackPageButton.Visibility = Visibility.Visible;
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
                    this.isClickable = true;

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

                        /*case "border":

                            this.position = this.scenarios[this.scenarioCount][2];
                            this.borderObjects[this.position].Visibility = Visibility.Hidden;

                            this.scenarioCount += 1;
                            this.ScenarioPlay();

                            break;
                            */
                    }
                    break;

                // テキストコントロール一般の文字を消す処理
                case "clear":

                    var clearTarget = this.scenarios[this.scenarioCount][1];

                    switch (clearTarget)
                    {
                        case "text":

                            this.position = this.scenarios[this.scenarioCount][2];
                            if(this.position== "size_of_feeling_text")
                            {
                                this.textBlockObjects[this.position].Text = "(        )";
                            }
                            else if(this.position == "kind_of_feeling_text")
                            {
                                this.ClearSelectFeelingEllipse();

                                this.textBlockObjects[this.position].Text = "";
                            }
                            else
                            {
                                this.textBlockObjects[this.position].Text = "";
                            }

                            this.scenarioCount += 1;
                            this.ScenarioPlay();

                            break;

                        case "select_feeling":

                            this.ClearSelectFeelingEllipse();

                            break;

                    }
                    break;

                case "sub":

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "goto":

                    if (this.scenarios[this.scenarioCount].Count > 1 && this.scenarios[this.scenarioCount][1] != "")
                    {
                        var GoToLabel = this.scenarios[this.scenarioCount][1];
                        if (GoToLabel =="current_scene")
                        {
                            this.GoTo(this.scene);
                        }
                        else
                        {
                            this.GoTo(GoToLabel);
                        }
                    }
                    break;

                case "wait_tap":

                    this.isClickable = false;
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

                    if(this.ViewSizeOfFeelingTextBlock.Text == "")
                    {
                        this.ViewSizeOfFeelingTextBlock.Text = "50";
                    }

                    this.SelectHeartImage.Source = null;
                    this.SelectNeedleImage.Source = null;

                    if (this.scene == "日直を任された場面のきもち" || this.scene == "赤丸くんにたのまれた場面のきもち")
                    {
                        if (this.SelectGoodFeelingListBox.SelectedItem != null)
                        {
                            this.SelectHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_red.png", UriKind.Relative));
                            this.SelectNeedleImage.Source = new BitmapImage(new Uri(@"./Images/red_needle.png", UriKind.Relative));
                        }
                        else if (this.SelectBadFeelingListBox.SelectedItem != null)
                        {
                            this.SelectHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_blue.png", UriKind.Relative));
                            this.SelectNeedleImage.Source = new BitmapImage(new Uri(@"./Images/blue_needle.png", UriKind.Relative));
                        }
                    }

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "get_item":

                    this.dataItem.HasGotItem04 = true;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataItem SET HasGotItem04 = 1 WHERE Id = 1;");
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

                    Image[] itemMainImages = { this.Item01MainImage, this.Item02MainImage, this.Item03MainImage, this.Item05MainImage, this.Item06MainImage, this.Item07MainImage, this.Item08MainImage, this.Item09MainImage, this.Item10MainImage, this.Item11MainImage };

                    Image[] itemNoneImages = { this.Item01NoneImage, this.Item02NoneImage, this.Item03NoneImage, this.Item05NoneImage, this.Item06NoneImage, this.Item07NoneImage, this.Item08NoneImage, this.Item09NoneImage, this.Item10NoneImage, this.Item11NoneImage };

                    var hasGotItems = new bool[] { this.dataItem.HasGotItem01, this.dataItem.HasGotItem02, this.dataItem.HasGotItem03, this.dataItem.HasGotItem05, this.dataItem.HasGotItem06, this.dataItem.HasGotItem07, this.dataItem.HasGotItem08, this.dataItem.HasGotItem09, this.dataItem.HasGotItem10, this.dataItem.HasGotItem11 };

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

                case "list_box":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var _itemSource = this.scenarios[this.scenarioCount][2];

                    var _listBox = this.itemsControlObjects[this.position];


                    _listBox.ItemsSource =this.SITUATIONS_SELECTION[_itemSource];

                    _listBox.Visibility = Visibility.Visible;

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
                    
                    case "$select_character$":

                        if (this.scene == "赤丸くんの場面")
                        {
                            text = text.Replace("$select_character$", "赤丸くん");
                        }
                        else if(this.scene == "キミちゃんの場面")
                        {
                            text = text.Replace("$select_character$", "キミちゃん");
                        }
                        else if (this.scene == "青助くんの場面")
                        {
                            text = text.Replace("$select_character$", "青助くん");
                        }

                            break;


                    case "$number_of_selects$":
                        
                        if(this.scene == "赤丸くんの場面")
                        {
                            text = text.Replace("$number_of_selects$", "①、②、③");
                        }
                        else if(this.scene == "キミちゃんの場面" || this.scene == "青助くんの場面")
                        {
                            text = text.Replace("$number_of_selects$", "①、②、③、④");
                        }


                        break;
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

            MatchCollection imageOrTextTags = null;
            string imagePath = "";

            foreach (string imageOrTextKey in imageOrTextDic.Keys)
            {
                switch (imageOrTextKey)
                {
                    case "name":

                        imageOrTextTags = new Regex(@"\<image=name\>(.*?)\<\/image\>").Matches(text);
                        imagePath = $"./Log/{initConfig.userName}/name.png";
                        break;

                    default: { break; }
                }

                if (imageOrTextTags.Count > 0)
                {
                    if (!File.Exists(imagePath))
                    {
                        text = text.Replace(imageOrTextTags[0].Value, imageOrTextDic[imageOrTextKey]);
                    }
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
                            case "aqua": { background = new SolidColorBrush(Colors.Aqua); background.Opacity = 1; break; };
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
                    this.runs[textObject.Name][inlineCount].Text = stns[0];
                    this.inlineCount++;
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

                this.isClickable = false;

                if (isSync == "sync")
                {
                    sb.Completed += (s, e) =>
                    {
                        if (!isDuplicate)
                        {
                            this.scenarioCount += 1;
                            this.ScenarioPlay();

                            isDuplicate = true;
                            isClickable = true;
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
                        isClickable = true;
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
            if (this.isClickable)
            {
                if (button.Name == "SelectWordButton")
                {
                    Grid btnContent = (Grid)button.Content;
                    foreach (var Text in btnContent.Children)
                    {
                        if (Text is TextBlock && ((TextBlock)Text).Name == "SelectWordButtonText")
                        {
                            selectedWordButtonText = (TextBlock)Text;

                        }
                    }

                    if (this.scene == "赤丸くんの場面")
                    {
                        this.GoTo("select_word_akamaru");
                    }
                    if (this.scene == "キミちゃんの場面")
                    {
                        this.GoTo("select_word_kimi");
                    }
                    if (this.scene == "青助くんの場面")
                    {
                        this.GoTo("select_word_aosuke");
                    }
                }
                if ((button.Name == "NextMessageButton" || button.Name == "NextPageButton" || button.Name == "MangaFlipButton" || button.Name == "SelectFeelingCompleteButton" || button.Name == "BranchButton2" || button.Name == "MangaPrevBackButton"))
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
                    if (button.Name == "MangaPrevBackButton")
                    {
                        if (this.KindOfFeelingText.Text != "" && this.SizeOfFeelingText.Text != "(        )")
                        {
                            this.SelectFeelingNextButton.Visibility = Visibility.Visible;

                        }
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }
                if (button.Name == "CompleteRolePlayButton")
                {
                    this.GoTo("complete_groupe_activity");
                }
            }
            // FullScreen時のデバッグ用に作っておく
            if (button.Name == "ExitButton")
            {

                this.CoverLayerImage.Visibility = Visibility.Visible;
                this.ExitBackGrid.Visibility = Visibility.Visible;
            }

            if (button.Name == "ExitBackYesButton")
            {
                this.StopBGM();

                TitlePage titlePage = new TitlePage();

                titlePage.SetIsFirstBootFlag(false);

                titlePage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                this.NavigationService.Navigate(titlePage);
            }

            if (button.Name == "ExitBackNoButton")
            {
                this.ExitBackGrid.Visibility = Visibility.Hidden;
                this.CoverLayerImage.Visibility = Visibility.Hidden;
            }
            if (button.Name == "BackMessageButton" || button.Name == "BackPageButton")
            {
                this.BackMessageButton.Visibility = Visibility.Hidden;
                this.NextMessageButton.Visibility = Visibility.Hidden;

                this.BackPageButton.Visibility = Visibility.Hidden;
                this.NextPageButton.Visibility = Visibility.Hidden;

                this.ScenarioBack();
            }
            if (button.Name == "SelectFeelingBackButton")
            {
                this.SelectFeelingBackButton.Visibility = Visibility.Hidden;

                if (this.ChallengeTimeGrid.Visibility == Visibility.Visible)
                {
                    this.ScenarioBack();
                }

                var challengeflag = false;

                if (this.SelectFeelingGrid.Visibility == Visibility.Visible)
                {
                    if (this.SelectGoodFeelingListBox.SelectedItem != null)
                    {
                        this.KindOfFeelingText.Text = this.SelectGoodFeelingListBox.SelectedItem.ToString().Replace("●　", "");
                    }
                    if (this.SelectBadFeelingListBox.SelectedItem != null)
                    {
                        this.KindOfFeelingText.Text = this.SelectBadFeelingListBox.SelectedItem.ToString().Replace("●　", "");
                    }

                    challengeflag = true;
                }
                if (this.SelectHeartGrid.Visibility == Visibility.Visible && this.ViewFeelingGrid.Visibility == Visibility.Visible)
                {
                    this.SizeOfFeelingText.Text = "(   " + this.ViewSizeOfFeelingTextBlock.Text + "  )";
                    challengeflag = true;
                }
                if (this.KindOfFeelingText.Text != "" && this.SizeOfFeelingText.Text != "(        )")
                {
                    this.SelectFeelingNextButton.Visibility = Visibility.Visible;

                }
                if (challengeflag)
                {
                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }



            }
            if (button.Name == "BranchButton1")
            {
                this.GoTo("manga");
            }
            if (button.Name == "SelectFeelingNextButton")
            {
                this.SelectFeelingNextButton.Visibility = Visibility.Hidden;
                this.scenarioCount += 1;
                this.ScenarioPlay();
            }
            if (button.Name == "MangaCheckButton")
            {
                if (this.scene == "日直を任された場面のきもち")
                {
                    this.GoTo("check_manga1");
                }
                if (this.scene == "赤丸くんにたのまれた場面のきもち")
                {
                    this.GoTo("check_manga2");
                }
            }
            if (button.Name == "SizeOfFeelingButton")
            {
                if (this.KindOfFeelingText.Text == "")
                {
                    MessageBox.Show("先にきもちの種類をえらんでみよう！");
                }
                else
                {
                    this.GoTo("size_of_feeling");
                }
            }
            if (button.Name == "KindOfFeelingButton")
            {
                this.GoTo("kind_of_feeling");
            }
            if (button.Name == "ChoicesButton")
            {
                if (this.scene == "グループアクティビティ")
                {
                    Grid btnContent = (Grid)button.Content;
                    foreach (var Text in btnContent.Children)
                    {
                        if (Text is TextBlock)
                        {
                            string situationText = ((TextBlock)Text).Text;

                            if (situationText == "場面①の赤丸くん")
                            {
                                this.GoTo("situation_akamaru");
                            }
                            if (situationText == "場面②のキミちゃん")
                            {
                                this.GoTo("situation_kimi");
                            }
                            if (situationText == "場面③の青助くん")
                            {
                                this.GoTo("situation_aosuke");
                            }
                        }
                    }
                }
                else if ((this.scene == "赤丸くんの場面" || this.scene == "キミちゃんの場面" || this.scene == "青助くんの場面"))
                {
                    Grid btnContent = (Grid)button.Content;
                    foreach (var Text in btnContent.Children)
                    {
                        if (Text is TextBlock)
                        {
                            selectedWordButtonText.Text = ((TextBlock)Text).Text;
                        }
                    }
                    this.GoTo(this.scene);

                    int checkCount = 0;
                    int isCorrectCount = 0;

                    foreach (Object itemControl in this.SelectSomeWordsListBox.Items)
                    {
                        this.isCorrect = false;

                        ListBoxItem content = (ListBoxItem)(this.SelectSomeWordsListBox.ItemContainerGenerator.ContainerFromItem(itemControl));
                        ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(content);
                        DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
                        StackPanel panel = (StackPanel)myDataTemplate.FindName("SelectWordPanel", myContentPresenter);
                        
                       //ItemsControlの子要素を取得
                       foreach(var btn in panel.Children)
                       {
                            if(btn is Button)
                            {
                                Grid grid = (Grid)((Button)btn).Content;

                                foreach (TextBlock checkText in grid.Children)
                                {
                                    //要素ごとに選択肢が選択肢が選択されたか否かを確認
                                    if (checkText.Name == "SelectWordButtonText" && checkText.Text != "")
                                    {
                                        string checktag = "";
                                        if(this.scene== "赤丸くんの場面")
                                        {
                                            checktag = "correct_words_akamaru";
                                        }
                                        else if (this.scene == "キミちゃんの場面")
                                        {
                                            checktag = "correct_words_kimi";
                                        }
                                        else if (this.scene == "青助くんの場面")
                                        {
                                            checktag = "correct_words_aosuke";
                                        }
                                        if (checkText.Text == (this.SITUATIONS_SELECTION[checktag])[checkCount])
                                        {
                                            isCorrectCount++;
                                        }
                                        checkCount++;
                                    }
                                    //　
                                    else if(checkText.Text == "")
                                    {
                                        break;
                                    }
                                }
                            }
                            if (this.SelectSomeWordsListBox.Items.Count == checkCount)
                            {
                                this.OkButton.Visibility = Visibility.Visible;

                                if(this.SelectSomeWordsListBox.Items.Count == isCorrectCount)
                                {
                                    this.isCorrect = true;
                                }
                            }
                            
                       }
                    }
                }
            }
            
            if (button.Name == "ReturnToTitleButton")
            {
                TitlePage titlePage = new TitlePage();

                titlePage.SetIsFirstBootFlag(false);

                titlePage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                this.NavigationService.Navigate(titlePage);
            }
            if(button.Name == "OkButton")
            {
                if (this.isCorrect)
                {
                    this.GoTo("select_correct_words");
                }
                else
                {
                    this.GoTo("select_uncorrect_words");
                }
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
            Cancel
        }



        // ハートゲージの角度をデータバインド
        private static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(Chapter4), new UIPropertyMetadata(0.0));

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
            /*
            if(this.scene == "日直を任された場面のきもち" || this.scene == "赤丸くんにたのまれた場面のきもち")
            {
                this.SelectHeartGrid.Visibility = Visibility.Hidden;
                this.ViewSizeOfFeelingGrid.Visibility = Visibility.Hidden;
                this.SizeOfFeelingText.Text ="(  "+ this.ViewSizeOfFeelingTextBlock.Text +"   )";
            }
            */
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

        private void WipeInWordArtMessage(Image wordArtImage, double newWidth, TimeSpan duration)
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

        private void selectFeeling(object sender, MouseButtonEventArgs e)
        {
            this.ClearSelectFeelingEllipse();

            //XAML上で記載したListBoxのテンプレートにEllipseコントロールを追加
            Grid feelingGrid = sender as Grid;

            Color feelingColor;

            if (feelingGrid.Name == "SelectGoodFeelingGrid" || feelingGrid.Name == "ChallengeGoodFeelingGrid")
            {
                feelingColor = (Color)ColorConverter.ConvertFromString("#FFEE2222");
            }
            else if (feelingGrid.Name == "SelectBadFeelingGrid" || feelingGrid.Name == "ChallengeBadFeelingGrid")
            {
                feelingColor = (Color)ColorConverter.ConvertFromString("#FF1E90FF");
            }

            Brush colorBrush = new SolidColorBrush { Color = feelingColor };
            Ellipse feelingColorEllipse = new Ellipse { Stroke = colorBrush, StrokeThickness = 3, Margin = new Thickness(25, 5, 25, 0) };

            AnswerResult selectResult = AnswerResult.None;

            if (feelingGrid.Children.Count < 3)
            {
                feelingGrid.Children.Add(feelingColorEllipse);
                selectResult = AnswerResult.Decision;
            }
            else
            {
                feelingGrid.Children.RemoveAt(2);
                selectResult = AnswerResult.Cancel;
            }

            var startupPath = FileUtils.GetStartupPath();

            this.PlaySE($@"{startupPath}/Sounds/{selectResult}.wav");
        }

        private void ClearSelectFeelingEllipse()
        {
            string selectFeelingName = "";

                ListBoxItem myListBoxItem = null;

                if (this.SelectGoodFeelingListBox.SelectedItem != null)
                {
                    myListBoxItem = (ListBoxItem)(this.SelectGoodFeelingListBox.ItemContainerGenerator.ContainerFromItem(this.SelectGoodFeelingListBox.SelectedItem));
                    selectFeelingName = "SelectGoodFeelingGrid";


                }
                if (this.SelectBadFeelingListBox.SelectedItem != null)
                {
                    myListBoxItem = (ListBoxItem)(this.SelectBadFeelingListBox.ItemContainerGenerator.ContainerFromItem(this.SelectBadFeelingListBox.SelectedItem));
                    selectFeelingName = "SelectBadFeelingGrid";


                }
                if (myListBoxItem != null)
                {
                    ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(myListBoxItem);
                    DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
                    Grid grid = (Grid)myDataTemplate.FindName(selectFeelingName, myContentPresenter);
                    if (grid.Children.Count == 3)
                    {
                        grid.Children.RemoveAt(2);
                    }

                }
                this.SelectGoodFeelingListBox.SelectedIndex = -1;
                this.SelectBadFeelingListBox.SelectedIndex = -1;
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

      
        private void TextBoxMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ScenarioBack()
        {
            var currentScenarioCount = this.scenarioCount;

            int returnCount = 0;

            for (int i = currentScenarioCount; i <= currentScenarioCount; i--)
            {
                if (this.scenarios[i][0] == "#")
                {
                    returnCount += 1;

                    if (returnCount == 2)
                    {
                        this.scenarioCount = i;
                        this.ScenarioPlay();

                        break;
                    }
                }
            }
        }

    }
}

