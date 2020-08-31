using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using CsvReadWrite;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Linq;
using System.Security.AccessControl;
using WMPLib;
using System.Windows.Ink;
using System.Media;
using SQLite;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using WpfAnimatedGif;
using FileIOUtils;
using Expansion;

namespace KokoroUpTime
{
    /// <summary>
    /// GameWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class Chapter3 : Page
    {
        // 気持ちのリスト
        private string[] GOOD_FEELINGS = {"うれしい", "しあわせ", "たのしい", "ホッとした", "きもちいい", "まんぞく", "すき", "やる気マンマン", "かんしゃ", "わくわく", "うきうき", "ほこらしい"};
        private string[] BAD_FEELINGS = { "心配", "こまった", "不安", "こわい", "おちこみ", "がっかり", "いかり", "イライラ", "はずかしい", "ふまん", "かなしい", "おびえる"};

        // ゲームを進行させるシナリオ
        private int scenarioCount = 0; //
        private List<List<string>> scenarios = null; //

        // 各種コントロールの名前を収める変数
        private string position = ""; //

        // マウスクリックを可能にするかどうかのフラグ
        private bool isClickable = false;

        // 気持ちの大きさ
        private int feelingSize = 0;

        // メッセージ表示関連
        private DispatcherTimer msgTimer; //
        private int word_num; //

        // 各種コントロールを任意の文字列で呼び出すための辞書
        private Dictionary<string, Image> imageObjects = null; //
        private Dictionary<string, TextBlock> textBlockObjects = null; //
        private Dictionary<string, Button> buttonObjects = null; //
        private Dictionary<string, Grid> gridObjects = null; //
        private Dictionary<string, Border> borderObjects = null; //

        // 音関連
        private WindowsMediaPlayer mediaPlayer; //
        private SoundPlayer sePlayer = null;

        // マウス押下中フラグ
        private bool isMouseDown = false;

        // ゲームの切り替えシーン
        private string scene;

        // なったことのある自分の気持ちの一時記録用
        private List<string> myKindOfGoodFeelings = new List<string>();
        private List<string> myKindOfBadFeelings = new List<string>();

        private bool hasKimisKindOfFeelingsRecorded = false;
        private bool hasAkamarusKindOfFeelingsRecorded = false;
        private bool hasAosukesKindOfFeelingsRecorded = false;
        private bool hasAkamarusSizeOfFeelingRecorded = false;
        private bool hasAosukesSizeOfFeelingRecorded = false;

        // データベースに収めるデータモデルのインスタンス
        private DataChapter1 dataChapter1;

        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();
        public DataProgress dataProgress = new DataProgress();

        public Chapter3()
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
            this.dataChapter1 = new DataChapter1();

            // xamlのItemControlに気持ちリストをデータバインド
            this.ChallengeGoodFeelingItemControl.ItemsSource = GOOD_FEELINGS;
            this.ChallengeBadFeelingItemControl.ItemsSource = BAD_FEELINGS;
            this.SelectGoodFeelingItemControl.ItemsSource = GOOD_FEELINGS;
            this.SelectBadFeelingItemControl.ItemsSource = BAD_FEELINGS;

            this.InitControls();
        }
        
        private void InitControls()
        {
            // 各種コントロールに任意の文字列をあてがう

            this.imageObjects = new Dictionary<string, Image>
            {
                ["bg_image"] = this.BackgroundImage, //
                ["manga_title_image"] = this.MangaTitleImage, //
                ["manga_image"] = this.MangaImage, //
                ["item_center_up_image"] = this.ItemCenterUpImage, //
                ["item_center_image"] = this.ItemCenterImage, //
                ["item_left_image"] = this.ItemLeftImage, //
                ["item_detail_info_image"] = this.ItemDetailInfoImage, //
                ["children_face_left_center_image"] = this.ChildrenFaceLeftCenterImage, //
                ["children_feeling_comment_image"] = this.ChildrenFeelingCommentImage, //
                ["shiroji_very_small_right_up_image"] = this.ShirojiVerySmallRightUpImage, //


                ["item_left_last_image"] = this.ItemLeftLastImage,
                ["session_title_image"] = this.SessionTitleImage, //
                ["kimi_in_plate_image"] = this.KimiInPlateImage,
                ["kimi_plate_outer_image"] = this.KimiPlateOuterImage,
                ["select_heart_image"] = this.SelectHeartImage,
                ["select_needle_image"] = this.SelectNeedleImage,
                ["children_info_image"] = this.ChildrenInfoImage,
                ["shiroji_ending_image"] = this.ShirojiEndingImage,
                ["shiroji_right_image"] = this.ShirojiRightImage, //
                ["shiroji_right_up_image"] = this.ShirojiRightUpImage, //
                ["shiroji_left_image"] = this.ShirojiLeftImage, //
                ["shiroji_small_right_up_image"] = this.ShirojiSmallRightUpImage,
                ["shiroji_small_right_down_image"] = this.ShirojiSmallRightDownImage,
                ["shiroji_right_center_image"] = this.ShirojiRightCenterImage,
                ["shiroji_small_right_center_image"] = this.ShirojiSmallRightCenterImage,
                ["shiroji_very_small_right_image"] = this.ShirojiVerySmallRightImage,
                ["children_stand_left_image"] = this.ChildrenStandLeftImage,
                ["children_stand_right_image"] = this.ChildrenStandRightImage,
                ["children_stand_left_onomatopoeia_image"] = this.ChildrenStandLeftOnomatopoeiaImage,
                ["kimi_stand_small_left_image"] = this.KimiStandSmallLeftImage,
                ["intro_akamaru_face_image"] = this.IntroAkamaruFaceImage,
                ["intro_aosuke_face_image"] = this.IntroAosukeFaceImage,
                ["intro_kimi_face_image"] = this.IntroKimiFaceImage,
                ["children_face_left_image"] = this.ChildrenFaceLeftImage, //
                ["children_face_right_image"] = this.ChildrenFaceRightImage, //
                ["teacher_image"] = this.TeacherImage,
                ["main_msg_bubble_image"] = this.MainMessageBubbleImage, //
                ["kind_of_feeling_input_image"] = this.KindOfFeelingInputImage, //
                ["shiroji_small_left_down_image"] = this.ShirojiSmallLeftDownImage, //
            };

            this.textBlockObjects = new Dictionary<string, TextBlock>
            {
                ["session_sub_title_text"] = this.SessionSubTitleTextBlock, //
                ["session_sentence_text"] = this.SessionSentenceTextBlock, //
                ["view_kind_of_feeling_person_text"] = this.ViewKindOfFeelingPersonTextBlock,
                ["view_kind_of_feeling_text"] = this.ViewKindOfFeelingTextBlock,
                ["view_size_of_feeling_text"] = this.ViewSizeOfFeelingTextBlock,
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
                ["children_stand_left_onomatopoeia_text"] = this.ChildrenStandLeftOnomatopoeiaTextBlock,
                ["children_face_small_left_msg_text"] = this.ChildrenFaceSmallLeftMessageTextBlock,
                ["main_msg"] = this.MainMessageTextBlock, //
                ["thin_msg"] = this.ThinMessageTextBlock,
                ["music_title_text"] = this.MusicTitleTextBlock,
                ["composer_name_text"] = this.ComposerNameTextBlock,
                ["item_book_title_text"] = this.ItemBookTitleTextBlock,
                ["challenge_time_title_text"] = this.ChallengeTimeTitleTextBlock, //

                ["let_s_try_title_text"] = this.Let_sTryTitleTextBlock, //
                

                ["children_feeling_title_text"] = ChildrenFeelingTitleTextBlock, //
                ["children_feeling_comment_text"] = this.ChildrenFeelingCommentTextBlock, //
                ["kind_of_feeling_title_text"] = this.KindOfFeelingTitleTextBlock, //
                ["size_of_feeling_title_text"] = this.SizeOfFeelingTitleTextBlock, //
            };

            this.buttonObjects = new Dictionary<string, Button>
            {
                ["next_msg_button"] = this.NextMessageButton, //
                ["back_msg_button"] = this.BackMessageButton, //
                ["thin_msg_button"] = this.ThinMessageButton,
                ["next_page_button"] = this.NextPageButton, //
                ["back_page_button"] = this.BackPageButton, //
                ["manga_flip_button"] = this.MangaFlipButton, //
                ["select_feeling_complete_button"] = this.SelectFeelingCompleteButton,
                ["select_feeling_next_button"] = this.SelectFeelingNextButton,

                ["check_manga_button"] = this.CheckMangaButton, //
                ["feeling_next_go_button"] = this.FeelingNextGoButton, //
                ["feeling_prev_back_button"] = this.FeelingPrevBackButton, //
                ["kind_of_feeling_input_button"] = this.KindOfFeelingInputButton, //
                ["size_of_feeling_input_button"] = this.SizeOfFeelingInputButton, //
                ["manga_prev_back_button"] = this.MangaPrevBackButton, //
            };

            this.gridObjects = new Dictionary<string, Grid>
            {
                ["memory_check_grid"] = this.MemoryCheckGrid, //
                ["session_grid"] = this.SessionGrid, //
                // ["challenge_time_grid"] = this.ChallengeTimeGrid, //
                ["select_feeling_grid"] = this.SelectFeelingGrid,
                ["summary_grid"] = this.SummaryGrid,
                ["ending_grid"] = this.EndingGrid,
                ["item_name_plate_left_grid"] = this.ItemNamePlateLeftGrid, 
                ["item_name_bubble_grid"] = this.ItemNameBubbleGrid,
                ["item_name_plate_center_grid"] = this.ItemNamePlateCenterGrid,
                ["item_info_plate_grid"] = this.ItemInfoPlateGrid,
                ["item_info_sentence_grid"] = this.ItemInfoSentenceGrid,
                ["item_last_info_grid"] = this.ItemLastInfoGrid,

                ["item_check_grid"] = this.ItemCheckGrid, //
                ["children_feeling_comment_grid"] = this.ChildrenFeelingCommentGrid, //

                ["challenge_msg_grid"] = this.ChallengeMessageGrid,
                ["kimi_plate_inner_up_grid"] = this.KimiPlateInnerUpGrid,
                ["kimi_plate_inner_down_grid"] = this.KimiPlateInnerDownGrid,
                ["view_kind_of_feeling_grid"] = this.ViewKindOfFeelingGrid,
                ["view_size_of_feeling_grid"] = this.ViewSizeOfFeelingGrid,
                ["children_face_small_left_msg_grid"] = this.ChildrenFaceSmallLeftMessageGrid,
                ["select_heart_grid"] = this.SelectHeartGrid,
                ["compare_akamaru_heart_grid"] = this.CompareAkamaruHeartGrid,
                ["compare_aosuke_heart_grid"] = this.CompareAosukeHeartGrid,
                ["akamaru_and_aosuke_compare_grid"] = this.AkamaruAndAosukeCompareGrid,
                ["compare_msg_grid"] = this.CompareMessageGrid,
                ["ending_msg_grid"] = this.EndingMessageGrid,
                ["main_msg_grid"] = this.MainMessageGrid, //
                ["music_info_grid"] = this.MusicInfoGrid, //
                ["exit_back_grid"] = this.ExitBackGrid, //

                ["hot_word_button_grid"] = this.HotWordButtonGrid, //
                ["hot_word_comment_grid"] = this.HotWordCommentGrid, //
            };

            this.borderObjects = new Dictionary<string, Border>
            {
                ["challenge_time_title_border"] = this.ChallengeTimeTitleBorder, //
                ["children_feeling_title_border"] = this.ChildrenFeelingTitleBorder, //
                ["let_s_try_title_border"] = this.Let_sTryTitleBorder, //
            };
        }

        private void ResetControls()
        {
            // 各種コントロールを隠すことでフルリセット

            this.MemoryCheckGrid.Visibility = Visibility.Hidden; //
            this.SessionGrid.Visibility = Visibility.Hidden; //
            this.ChallengeGrid.Visibility = Visibility.Hidden;
            this.SelectFeelingGrid.Visibility = Visibility.Hidden;
            this.SummaryGrid.Visibility = Visibility.Hidden;
            this.EndingGrid.Visibility = Visibility.Hidden;
            this.ItemNamePlateLeftGrid.Visibility = Visibility.Hidden;
            this.ItemNameBubbleGrid.Visibility = Visibility.Hidden;
            this.ItemNamePlateCenterGrid.Visibility = Visibility.Hidden;
            this.ItemInfoPlateGrid.Visibility = Visibility.Hidden;
            this.ItemLastInfoGrid.Visibility = Visibility.Hidden;
            this.ViewKindOfFeelingGrid.Visibility = Visibility.Hidden;
            this.ViewSizeOfFeelingGrid.Visibility = Visibility.Hidden;
            this.ChildrenFaceSmallLeftMessageGrid.Visibility = Visibility.Hidden;
            this.ChallengeMessageGrid.Visibility = Visibility.Hidden;
            this.KimiPlateInnerUpGrid.Visibility = Visibility.Hidden;
            this.KimiPlateInnerDownGrid.Visibility = Visibility.Hidden;
            this.SelectHeartGrid.Visibility = Visibility.Hidden;
            this.CompareAkamaruHeartGrid.Visibility = Visibility.Hidden;
            this.CompareAosukeHeartGrid.Visibility = Visibility.Hidden;
            this.AkamaruAndAosukeCompareGrid.Visibility = Visibility.Hidden;
            this.CompareMessageGrid.Visibility = Visibility.Hidden;
            this.EndingMessageGrid.Visibility = Visibility.Hidden;
            this.MainMessageGrid.Visibility = Visibility.Hidden; //
            this.MusicInfoGrid.Visibility = Visibility.Hidden; //
            this.ExitBackGrid.Visibility = Visibility.Hidden; //
            this.BackgroundImage.Visibility = Visibility.Hidden; //
            this.MangaTitleImage.Visibility = Visibility.Hidden; //
            this.MangaImage.Visibility = Visibility.Hidden; //
            this.ItemCenterUpImage.Visibility = Visibility.Hidden; //
            this.ItemCenterImage.Visibility = Visibility.Hidden; //
            this.ItemLeftImage.Visibility = Visibility.Hidden; //
            this.ItemDetailInfoImage.Visibility = Visibility.Hidden; //
            this.ItemCheckGrid.Visibility = Visibility.Hidden; //
            this.ItemLeftLastImage.Visibility = Visibility.Hidden;
            this.SessionTitleImage.Visibility = Visibility.Hidden; //
            this.SessionSubTitleTextBlock.Visibility = Visibility.Hidden; //
            this.SessionSentenceTextBlock.Visibility = Visibility.Hidden; //
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
            this.ViewKindOfFeelingPersonTextBlock.Visibility = Visibility.Hidden;
            this.ViewKindOfFeelingTextBlock.Visibility = Visibility.Hidden;
            this.ViewSizeOfFeelingTextBlock.Visibility = Visibility.Hidden;
            this.ChildrenFaceLeftImage.Visibility = Visibility.Hidden; //
            this.ChildrenFaceRightImage.Visibility = Visibility.Hidden; //
            this.ChildrenFaceSmallLeftMessageTextBlock.Visibility = Visibility.Hidden;
            this.CompareMessageTextBlock.Visibility = Visibility.Hidden;
            this.KindOfFeelingAkamaruTextBlock.Visibility = Visibility.Hidden;
            this.SizeOfFeelingAkamaruTextBlock.Visibility = Visibility.Hidden;
            this.KindOfFeelingAosukeTextBlock.Visibility = Visibility.Hidden;
            this.SizeOfFeelingAosukeTextBlock.Visibility = Visibility.Hidden;
            this.ChildrenInfoImage.Visibility = Visibility.Hidden;
            this.EndingMessageTextBlock.Visibility = Visibility.Hidden;
            this.ShirojiRightImage.Visibility = Visibility.Hidden; //
            this.ShirojiRightUpImage.Visibility = Visibility.Hidden; //
            this.ShirojiLeftImage.Visibility = Visibility.Hidden; //
            this.ShirojiSmallRightUpImage.Visibility = Visibility.Hidden;
            this.ShirojiSmallRightDownImage.Visibility = Visibility.Hidden;
            this.ShirojiRightCenterImage.Visibility = Visibility.Hidden;
            this.ShirojiSmallRightCenterImage.Visibility = Visibility.Hidden;
            this.ShirojiVerySmallRightImage.Visibility = Visibility.Hidden;
            this.ShirojiEndingImage.Visibility = Visibility.Hidden;
            this.ChildrenStandLeftImage.Visibility = Visibility.Hidden;
            this.ChildrenStandRightImage.Visibility = Visibility.Hidden;
            this.KimiStandSmallLeftImage.Visibility = Visibility.Hidden;
            this.ChildrenStandLeftOnomatopoeiaImage.Visibility = Visibility.Hidden;
            this.ChildrenStandLeftOnomatopoeiaTextBlock.Visibility = Visibility.Hidden;
            this.IntroAkamaruFaceImage.Visibility = Visibility.Hidden;
            this.IntroAosukeFaceImage.Visibility = Visibility.Hidden;
            this.IntroKimiFaceImage.Visibility = Visibility.Hidden;
            this.TeacherImage.Visibility = Visibility.Hidden;
            this.MainMessageTextBlock.Visibility = Visibility.Hidden; //
            this.ThinMessageButton.Visibility = Visibility.Hidden;
            this.ThinMessageTextBlock.Visibility = Visibility.Hidden;
            this.NextMessageButton.Visibility = Visibility.Hidden;
            this.BackMessageButton.Visibility = Visibility.Hidden;
            this.NextPageButton.Visibility = Visibility.Hidden;
            this.BackPageButton.Visibility = Visibility.Hidden;
            this.MangaFlipButton.Visibility = Visibility.Hidden; //
            this.CoverLayerImage.Visibility = Visibility.Hidden;
            this.ChallengeTimeTitleBorder.Visibility = Visibility.Hidden; //
            this.ChallengeTimeTitleTextBlock.Visibility = Visibility.Hidden; //

            this.HotWordButtonGrid.Visibility = Visibility.Hidden; //
            this.HotWordCommentGrid.Visibility = Visibility.Hidden; //
            this.ShirojiSmallLeftDownImage.Visibility = Visibility.Hidden; //

            this.Let_sTryTitleBorder.Visibility = Visibility.Hidden; //

            this.ChildrenFeelingTitleBorder.Visibility = Visibility.Hidden; //
            this.ChildrenFaceLeftCenterImage.Visibility = Visibility.Hidden; //
            this.ChildrenFeelingCommentGrid.Visibility = Visibility.Hidden; //
            // this.ChildrenFeelingCommentImage.Visibility = Visibility.Hidden; //
            this.ChildrenFeelingCommentTextBlock.Visibility = Visibility.Hidden; //
            this.ShirojiVerySmallRightUpImage.Visibility = Visibility.Hidden; //
            this.CheckMangaButton.Visibility = Visibility.Hidden; //
            this.FeelingNextGoButton.Visibility = Visibility.Hidden; //
            this.FeelingPrevBackButton.Visibility = Visibility.Hidden; //
            this.KindOfFeelingTitleTextBlock.Visibility = Visibility.Hidden; //
            this.SizeOfFeelingTitleTextBlock.Visibility = Visibility.Hidden; //
            this.KindOfFeelingInputButton.Visibility = Visibility.Hidden; //
            this.KindOfFeelingInputImage.Visibility = Visibility.Hidden; //
            this.SizeOfFeelingInputButton.Visibility = Visibility.Hidden; //
            this.ChildrenFeelingTitleTextBlock.Visibility = Visibility.Hidden; //

            this.MangaPrevBackButton.Visibility = Visibility.Hidden; //

            this.SessionSubTitleTextBlock.Text = "";
            this.SessionSentenceTextBlock.Text = "";
            this.ViewKindOfFeelingPersonTextBlock.Text = "";
            this.ViewKindOfFeelingTextBlock.Text = "";
            this.ViewSizeOfFeelingTextBlock.Text = "";
            this.ChildrenFaceSmallLeftMessageTextBlock.Text = "";
            this.CompareMessageTextBlock.Text = "";
            this.KindOfFeelingAkamaruTextBlock.Text = "";
            this.SizeOfFeelingAkamaruTextBlock.Text = "";
            this.KindOfFeelingAosukeTextBlock.Text = "";
            this.SizeOfFeelingAosukeTextBlock.Text = "";
            this.EndingMessageTextBlock.Text = "";

            this.MainMessageTextBlock.Text = ""; //

            this.ThinMessageTextBlock.Text = "";
            this.MusicTitleTextBlock.Text = ""; //
            this.ComposerNameTextBlock.Text = ""; //

            this.ChildrenFeelingCommentTextBlock.Text = ""; //
            this.ChildrenFeelingTitleTextBlock.Text = ""; //

            this.ItemBookTitleTextBlock.Visibility = Visibility.Hidden;
            this.ItemBookMainGrid.Visibility = Visibility.Hidden;
            this.ItemBookNoneGrid.Visibility = Visibility.Hidden;

            this.ReturnToTitleButton.Visibility = Visibility.Hidden;
        }

        public void SetNextPage(InitConfig _initConfig, DataOption _dataOption, DataItem _dataItem, DataProgress _dataProgress)
        {
            this.initConfig = _initConfig;
            this.dataOption = _dataOption;
            this.dataItem = _dataItem;
            this.dataProgress = _dataProgress;

            // ======== 新規ユーザ登録時 ===============================================

            /*
            // データベース本体のファイルのパス設定
            string dbName = $"{initConfig.userName}.sqlite";
            string dirPath = $"./Log/{initConfig.userName}/";

            // FileUtils.csからディレクトリ作成のメソッド
            // 各ユーザの初回起動のとき実行ファイルの場所下のLogフォルダにユーザネームのフォルダを作る
            DirectoryUtils.SafeCreateDirectory(dirPath);

            this.dbPath = System.IO.Path.Combine(dirPath, dbName);
            */

            // =========================================================================

            // 現在時刻を取得
            this.dataChapter1.CreatedAt = DateTime.Now.ToString();

            // データベースのテーブル作成と現在時刻の書き込みを同時に行う
            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
            {
                // 仮（本当は名前を登録するタイミングで）
                connection.CreateTable<DataOption>();
                connection.CreateTable<DataProgress>();
                connection.CreateTable<DataChapter1>();

                // 毎回のアクセス日付を記録
                connection.Insert(this.dataChapter1);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var csv = new CsvReader("./Scenarios/chapter3.csv"))
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

            switch (tag)
            {
                case "start":

                    // 画面のフェードイン処理とか入れる（別関数を呼び出す）

                    this.dataProgress.CurrentChapter = 3;

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

                // ボーダーに対しての処理
                case "border":

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

                    string msgIsSync = "sync";

                    if (this.scenarios[this.scenarioCount].Count > 4 && this.scenarios[this.scenarioCount][4] != "")
                    {
                        msgIsSync = this.scenarios[this.scenarioCount][4];
                    }

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var _message = this.scenarios[this.scenarioCount][2];

                        _message = this.SequenceCheck(_message);

                        this.ShowMessage(textObject: _textObject, message: _message, isSync: msgIsSync);
                    }
                    else
                    {
                        // xamlに直接書いたStaticな文章を表示する場合
                        this.ShowMessage(textObject: _textObject, message: _textObject.Text, isSync: msgIsSync);
                    }
                    break;

                // 流れない文字に対するTextBlock処理
                case "text":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var textObject = this.textBlockObjects[this.position];

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var _text = this.scenarios[this.scenarioCount][2];

                        var text = this.SequenceCheck(_text);

                        textObject.Text = text;
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

                            case "yellow":

                                textColorBrush = new SolidColorBrush(Colors.Yellow);

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

                        case "border":

                            this.position = this.scenarios[this.scenarioCount][2];
                            this.borderObjects[this.position].Visibility = Visibility.Hidden;

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

                case "jump":

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                // 教室のルール黒板の処理
                /*
                case "rule":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var ruleObject = this.textBlockObjects[this.position];

                    var rule = this.scenarios[this.scenarioCount][2];

                    this.checkBoxs = new CheckBox[] { this.RuleBoardCheck1Box, this.RuleBoardCheck2Box, this.RuleBoardCheck3Box };

                    var checkNum = this.scenarios[this.scenarioCount][3];

                    object _obj;

                    if (checkNum == "all")
                    {
                        _obj = checkBoxs;
                    }
                    else
                    {
                        _obj = checkBoxs[int.Parse(checkNum)];
                    }
                    ruleObject.Visibility = Visibility.Visible;

                    this.ShowMessage(textObject: ruleObject, message: rule, obj: _obj);

                    break;
                */

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

                    this.ViewSizeOfFeelingTextBlock.Text = "50";

                    this.SelectHeartImage.Source = null;
                    this.SelectNeedleImage.Source = null;

                    if (this.scene == "赤丸くんのきもちの大きさ")
                    {
                        if (this.dataChapter1.AkamarusKindOfFeelings.Split(",")[1].ToString() == "良い")
                        {
                            this.SelectHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_red.png", UriKind.Relative));
                            this.SelectNeedleImage.Source = new BitmapImage(new Uri(@"./Images/red_needle.png", UriKind.Relative));
                            this.CompareAkamaruHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_red.png", UriKind.Relative));
                            this.CompareAkamaruNeedleImage.Source = new BitmapImage(new Uri(@"./Images/red_needle.png", UriKind.Relative));
                        }
                        else if (this.dataChapter1.AkamarusKindOfFeelings.Split(",")[1].ToString() == "悪い")
                        {
                            this.SelectHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_blue.png", UriKind.Relative));
                            this.SelectNeedleImage.Source = new BitmapImage(new Uri(@"./Images/blue_needle.png", UriKind.Relative));
                            this.CompareAkamaruHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_blue.png", UriKind.Relative));
                            this.CompareAkamaruNeedleImage.Source = new BitmapImage(new Uri(@"./Images/blue_needle.png", UriKind.Relative));
                        }
                    }

                    if (this.scene == "青助くんのきもちの大きさ")
                    {
                        if (this.dataChapter1.AosukesKindOfFeelings.Split(",")[1].ToString() == "良い")
                        {
                            this.SelectHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_red.png", UriKind.Relative));
                            this.SelectNeedleImage.Source = new BitmapImage(new Uri(@"./Images/red_needle.png", UriKind.Relative));
                            this.CompareAosukeHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_red.png", UriKind.Relative));
                            this.CompareAosukeNeedleImage.Source = new BitmapImage(new Uri(@"./Images/red_needle.png", UriKind.Relative));
                        }
                        else if (this.dataChapter1.AosukesKindOfFeelings.Split(",")[1].ToString() == "悪い")
                        {
                            this.SelectHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_blue.png", UriKind.Relative));
                            this.SelectNeedleImage.Source = new BitmapImage(new Uri(@"./Images/blue_needle.png", UriKind.Relative));
                            this.CompareAosukeHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_blue.png", UriKind.Relative));
                            this.CompareAosukeNeedleImage.Source = new BitmapImage(new Uri(@"./Images/blue_needle.png", UriKind.Relative));
                        }
                    }

                    if (this.scene == "ふたりのきもちの比較")
                    {
                        int[] feelings = { int.Parse(this.dataChapter1.AkamarusSizeOfFeeling.ToString()), int.Parse(this.dataChapter1.AosukesSizeOfFeeling.ToString()) };
                       
                        Image[] needles = { this.CompareAkamaruNeedleImage, this.CompareAosukeNeedleImage };

                        for (int i = 0; i < feelings.Length; i++) {
                            {
                                var gaugeRotation = new RotateTransform
                                {
                                    CenterX = 0.0f,
                                    CenterY = 0.0f, //needles[i].Height * 0.1f,
                                    Angle = -45.0f
                                };

                                needles[i].RenderTransform = gaugeRotation;

                                var _targetAngle = feelings[i] - 50.0f;

                                GaugeUpdate(targetAngle: _targetAngle, needleImage: needles[i]);

                                void GaugeUpdate(float targetAngle, Image needleImage)
                                {
                                    var timer = new DispatcherTimer();

                                    timer.Interval = TimeSpan.FromSeconds(0.01f);

                                    timer.Tick += (sender, e) =>
                                    {
                                        if (gaugeRotation.Angle < targetAngle)
                                        {
                                            gaugeRotation.Angle += 1.0f;

                                            needleImage.RenderTransform = gaugeRotation;
                                        }
                                        else
                                        {
                                            timer.Stop();
                                            timer = null;
                                        }
                                    };
                                    timer.Start();
                                }
                            }
                        }
                    }
                    else
                    {
                        this.Angle = 0.0f;
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "get_item":

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

                    Image[] itemNoneImages = { this.Item02NoneImage, this.Item03NoneImage, this.Item04NoneImage, this.Item05NoneImage, this.Item06NoneImage, this.Item07NoneImage, this.Item08NoneImage, this.Item09NoneImage, this.Item10NoneImage, this.Item11NoneImage };

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

        string SequenceCheck(string text)
        {
            // 正規表現によって$と$の間の文字列を抜き出す（無駄処理）
            var Matches = new Regex(@"\$(.+?)\$").Matches(text);

            for (int i = 0; i < Matches.Count; i++)
            {
                var sequence = Matches[i].Value;

                switch (sequence)
                {
                    case "$kimis_kind_of_feeling$":

                        text = text.Replace("$kimis_kind_of_feeling$", this.dataChapter1.KimisKindOfFeelings.Split(",")[0]);

                        break;

                    case "$akamarus_kind_of_feeling$":

                        text = text.Replace("$akamarus_kind_of_feeling$", this.dataChapter1.AkamarusKindOfFeelings.Split(",")[0]);

                        break;

                    case "$aosukes_kind_of_feeling$":

                        text = text.Replace("$aosukes_kind_of_feeling$", this.dataChapter1.AosukesKindOfFeelings.Split(",")[0]);

                        break;

                    case "$akamarus_size_of_feeling$":

                        text = text.Replace("$akamarus_size_of_feeling$", this.dataChapter1.AkamarusSizeOfFeeling.ToString());

                        break;

                    case "$aosukes_size_of_feeling$":

                        text = text.Replace("$aosukes_size_of_feeling$", this.dataChapter1.AosukesSizeOfFeeling.ToString());

                        break;
                }
            }

            // 苦悶の改行処理（文章中の「鬱」を疑似改行コードとする）
            text = text.Replace("鬱", "\u2028");

            if (this.dataOption.InputMethod == 1 || this.dataOption.InputMethod == 2)
            {
                text = text.Replace("【name】", initConfig.userName);
            }
            else if (this.dataOption.InputMethod == 0)
            {
                text = text.Replace("【name】", "n");
            }

            text = text.Replace("【くん／ちゃん／さん】", initConfig.userTitle);

            return text;
        }

        void ShowMessage(TextBlock textObject, string message, string isSync, object obj=null)
        {
            if (isSync == "no_sync")
            {
                this.scenarioCount += 1;
                this.ScenarioPlay();
            }

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

                    if (isSync == "sync")
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                }
            }
        }

        /*
        void ShowMessage_Back(TextBlock textObject, string message, object obj=null)
        {
            this.word_num = 0;

            if (this.dataOption.IsWordRecognition == false && textObject.Name == "MainMessageTextBlock")
            {
                this.MainMessageFrontText.Text = "";
                this.MainMessageBackText.Text = "";
                this.NameImage.Source = null;

                textObject.Visibility = Visibility.Visible;
            }

            // メッセージ表示処理
            this.msgTimer = new DispatcherTimer();
            this.msgTimer.Tick += ViewMsg;
            this.msgTimer.Interval = TimeSpan.FromSeconds(1.0f / this.dataOption.MessageSpeed);
            this.msgTimer.Start();

            // 一文字ずつメッセージ表示（Inner Func）
            void ViewMsg(object sender, EventArgs e)
            {
                if (this.dataOption.IsWordRecognition == false && textObject.Name == "MainMessageTextBlock" && this.scene == "前説")
                {
                    if (this.word_num > 0 && message.Substring(this.word_num - 1, 1) == "n") // && this.MainMessageBackText.Text == null)
                    {
                        message = message.Replace("n", "");

                        // Name.bmpを収める場所の設定
                        string nameBmp = "Name.bmp";
                        string dirPath = $"./Log/{initConfig.userName}/";

                        string nameBmpPath = System.IO.Path.Combine(dirPath, nameBmp);

                        // 実行ファイルの場所を絶対パスで取得
                        var startupPath = FileUtils.GetStartupPath();

                        this.NameImage.Source = new BitmapImage(new Uri($@"{startupPath}/{nameBmpPath}", UriKind.Absolute));
                    }
                    else if (this.word_num > 0 && NameImage.Source != null) // && this.MainMessageFrontText != null)
                    {
                        this.MainMessageBackText.Text = message.Substring(this.MainMessageFrontText.Text.Length, this.word_num - MainMessageFrontText.Text.Length);
                    }
                    else
                    {
                        this.MainMessageFrontText.Text = message.Substring(0, this.word_num);
                    }
                }
                else
                {
                    textObject.Text = message.Substring(0, this.word_num);

                    if (this.word_num == 0)
                    {
                        textObject.Visibility = Visibility.Visible;
                    }
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
        */

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

                    foreach (CheckBox checkBox in checkBoxs) {
                        checkBox.IsEnabled = true;
                    }
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 各種ボタンが押されたときの処理

            Button button = sender as Button;

            if (this.isClickable)
            {
                if (button.Name == "WishCheckButton")
                {
                    this.isClickable = false;

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }

                if (button.Name == "ImRememberButton")
                {
                    this.isClickable = false;

                    JumpTo("manga");
                }

                if (button.Name == "CheckMangaButton")
                {
                    this.isClickable = false;

                    if (this.scene == "キミちゃんのきもちを考える1")
                    {
                        JumpTo("manga_kimi_part");
                    }

                    if (this.scene == "青助くんのきもちを考える1")
                    {
                        JumpTo("manga_aosuke_part");
                    }
                }

                if (button.Name == "MangaPrevBackButton")
                {
                    this.isClickable = false;
                    
                    if (this.scene == "キミちゃんのきもちを考える1")
                    {
                        JumpTo("think_kimi's_feeling_1");
                    }

                    if (this.scene == "青助くんのきもちを考える1")
                    {
                        JumpTo("think_aosuke's_feeling_1");
                    }
                }

                if (button.Name == "FeelingNextGoButton")
                {
                    this.isClickable = false;

                    if (this.scene == "キミちゃんのきもちを考える1")
                    {
                        JumpTo("chiku_chiku_kotoba");
                    }

                    if (this.scene == "青助くんのきもちを考える1")
                    {
                        JumpTo("think_kimi's_feeling_1");
                    }
                }
            }

            


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

            if (button.Name == "SelectFeelingCompleteButton")
            {
                if (this.scene == "チャレンジきもち選択")

                this.ExitBackGrid.Visibility = Visibility.Hidden;
                this.CoverLayerImage.Visibility = Visibility.Hidden;
            }

            if (button.Name == "NextPageButton")
            {
                if (this.scene == "キミちゃんのきもちの種類" && !hasKimisKindOfFeelingsRecorded)
                {
                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataChapter1 SET KimisKindOfFeelings = '{this.dataChapter1.KimisKindOfFeelings}' WHERE CreatedAt = '{this.dataChapter1.CreatedAt}';");
                    }
                    this.hasKimisKindOfFeelingsRecorded = true;
                    this.AllGestureCanvas_Clear();
                }

                if (this.scene == "赤丸くんのきもちの種類" && !hasAkamarusKindOfFeelingsRecorded)
                {
                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataChapter1 SET AkamarusKindOfFeelings = '{this.dataChapter1.AkamarusKindOfFeelings}' WHERE CreatedAt = '{this.dataChapter1.CreatedAt}';");
                    }
                    this.hasAkamarusKindOfFeelingsRecorded = true;
                    this.AllGestureCanvas_Clear();
                }

                if (this.scene == "青助くんのきもちの種類" && !hasAosukesKindOfFeelingsRecorded)
                {
                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataChapter1 SET AosukesKindOfFeelings = '{this.dataChapter1.AosukesKindOfFeelings}' WHERE CreatedAt = '{this.dataChapter1.CreatedAt}';");
                    }
                    this.hasAosukesKindOfFeelingsRecorded = true;
                    this.AllGestureCanvas_Clear();
                }

                if (this.scene == "赤丸くんのきもちの大きさ" && !hasAkamarusSizeOfFeelingRecorded)
                {
                    this.dataChapter1.AkamarusSizeOfFeeling = this.feelingSize;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataChapter1 SET AkamarusSizeOfFeeling = '{this.dataChapter1.AkamarusSizeOfFeeling}' WHERE CreatedAt = '{this.dataChapter1.CreatedAt}';");
                    }
                    this.hasAkamarusSizeOfFeelingRecorded = true;
                }

                if (this.scene == "青助くんのきもちの大きさ" && !hasAosukesSizeOfFeelingRecorded)
                {
                    this.dataChapter1.AosukesSizeOfFeeling = this.feelingSize;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataChapter1 SET AosukesSizeOfFeeling = '{this.dataChapter1.AosukesSizeOfFeeling}' WHERE CreatedAt = '{this.dataChapter1.CreatedAt}';");
                    }
                    this.hasAosukesSizeOfFeelingRecorded = true;
                }
            }

            if (button.Name == "SelectFeelingCompleteButton")
            {
                if (this.scene == "チャレンジきもち選択")
                {
                    // 配列をコンマで結合して文字列として扱う
                    this.dataChapter1.MyKindOfGoodFeelings = string.Join(",", this.myKindOfGoodFeelings);
                    this.dataChapter1.MyKindOfBadFeelings = string.Join(",", this.myKindOfBadFeelings);

                    // 当回のプレイのチャレンジタイムのログに関するデータベースをアップデート
                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataChapter1 SET MyKindOfGoodFeelings = '{this.dataChapter1.MyKindOfGoodFeelings}', MyKindOfBadFeelings = '{this.dataChapter1.MyKindOfBadFeelings}' WHERE CreatedAt = '{this.dataChapter1.CreatedAt}';");
                    }
                }
                this.AllGestureCanvas_Enabled(false);
            }

            if (button.Name == "SelectFeelingNextButton")
            {
                this.AllGestureCanvas_Clear();
                this.AllGestureCanvas_Enabled(true);
            }

            if (this.isClickable && (button.Name == "NextMessageButton" || button.Name == "NextPageButton" || button.Name == "RuleBoardButton" || button.Name == "ThinMessageButton" || button.Name == "MangaFlipButton" || button.Name == "SelectFeelingCompleteButton" || button.Name == "SelectFeelingNextButton"))
            {
                this.isClickable = false;

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

        private void JumpTo(string tag)
        {
            foreach (var (scenario, index) in this.scenarios.Indexed())
            {
                if (scenario[0] == "jump" && scenario[1] == tag)
                {
                    this.scenarioCount = index + 1;
                    this.ScenarioPlay();
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

            if (gestureResult.ApplicationGesture == ApplicationGesture.Circle || gestureResult.ApplicationGesture == ApplicationGesture.DoubleCircle)
            {
                // 気持ち選択場面でのそれぞれの選択項目に対する処理
                foreach (var textBlock in gestureCanvas.Children.OfType<TextBlock>())
                {
                    switch (textBlock.Name)
                    {
                        case "ChallengeGoodFeelingItemTextBlock":

                            if (this.scene == "チャレンジきもち選択")
                            {
                                this.myKindOfGoodFeelings.Add(textBlock.Text);
                            }
                            break;

                        case "ChallengeBadFeelingItemTextBlock":

                            if (this.scene == "チャレンジきもち選択")
                            {
                                this.myKindOfBadFeelings.Add(textBlock.Text);
                            }
                            break;

                        case "SelectGoodFeelingItemTextBlock":

                            if (this.scene == "キミちゃんのきもちの種類")
                            {
                                this.dataChapter1.KimisKindOfFeelings = $@"{textBlock.Text},良い";
                            }

                            if (this.scene == "赤丸くんのきもちの種類")
                            {
                                this.dataChapter1.AkamarusKindOfFeelings = $@"{textBlock.Text},良い";
                            }

                            if (this.scene == "青助くんのきもちの種類")
                            {
                                this.dataChapter1.AosukesKindOfFeelings = $@"{textBlock.Text},良い";
                            }
                            break;

                        case "SelectBadFeelingItemTextBlock":

                            if (this.scene == "キミちゃんのきもちの種類")
                            {
                                this.dataChapter1.KimisKindOfFeelings = $@"{textBlock.Text},悪い";
                            }

                            if (this.scene == "赤丸くんのきもちの種類")
                            {
                                this.dataChapter1.AkamarusKindOfFeelings = $@"{textBlock.Text},悪い";
                            }

                            if (this.scene == "青助くんのきもちの種類")
                            {
                                this.dataChapter1.AosukesKindOfFeelings = $@"{textBlock.Text},悪い";
                            }
                            break;
                    }
                }
            }

            if (this.scene == "キミちゃんのきもちの種類" || this.scene == "赤丸くんのきもちの種類" || this.scene == "青助くんのきもちの種類")
            {
                this.AllGestureCanvas_Clear();
                gestureCanvas.Strokes.Add(e.Strokes);
            }
            else
            {
                gestureCanvas.Strokes.Clear();
                gestureCanvas.Strokes.Add(e.Strokes);
            }
        }

        private enum AnswerResult
        {
            None,
            Incorrect,
            Intermediate,
            Correct,
        }

        void AllGestureCanvas_Clear()
        {
            // 拡張クラス（Expansion.csの一部）GetChildrenを使ってItemTenplateの子要素（InkCanvas）にアクセス
            // 全てのInkCanvasをクリアしてから新しいストロークを書き込む排他処理
            foreach (var goodInkCanvas in this.ChallengeGoodFeelingItemControl.GetChildren<InkCanvas>().ToList())
            {
                goodInkCanvas.Strokes.Clear();
            }

            foreach (var badInkCanvas in this.ChallengeBadFeelingItemControl.GetChildren<InkCanvas>().ToList())
            {
                badInkCanvas.Strokes.Clear();
            }

            foreach (var goodInkCanvas in this.SelectGoodFeelingItemControl.GetChildren<InkCanvas>().ToList())
            {
                goodInkCanvas.Strokes.Clear();
            }

            foreach (var badInkCanvas in this.SelectBadFeelingItemControl.GetChildren<InkCanvas>().ToList())
            {
                badInkCanvas.Strokes.Clear();
            }
        }

        void AllGestureCanvas_Enabled(bool IsEnable)
        {
            foreach (var goodInkCanvas in this.ChallengeGoodFeelingItemControl.GetChildren<InkCanvas>().ToList())
            {
                goodInkCanvas.IsEnabled = IsEnable;
            }

            foreach (var badInkCanvas in this.ChallengeBadFeelingItemControl.GetChildren<InkCanvas>().ToList())
            {
                badInkCanvas.IsEnabled = IsEnable;
            }

            foreach (var goodInkCanvas in this.SelectGoodFeelingItemControl.GetChildren<InkCanvas>().ToList())
            {
                goodInkCanvas.IsEnabled = IsEnable;
            }

            foreach (var badInkCanvas in this.SelectBadFeelingItemControl.GetChildren<InkCanvas>().ToList())
            {
                badInkCanvas.IsEnabled = IsEnable;
            }
        }

        // ハートゲージの角度をデータバインド
        private static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(Chapter1), new UIPropertyMetadata(0.0));

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        // マウスのドラッグ処理（マウスの左ボタンを押下したとき）
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this);

            if (e.Source as FrameworkElement != null)
            {
                var dragObjName = (e.Source as FrameworkElement).Name;

                if (dragObjName == "SelectNeedleImage")
                {
                    this.isMouseDown = true;
                }
            }
            
            if (this.isMouseDown && this.SelectHeartGrid.Visibility == Visibility.Visible && (this.scene == "赤丸くんのきもちの大きさ" || this.scene == "青助くんのきもちの大きさ"))
            {
                this.CalcAngle();

                this.ViewSizeOfFeelingTextBlock.Text = this.feelingSize.ToString();
            }
        }

        // マウスのドラッグ処理（マウスの左ボタンを離したとき）
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);

            this.isMouseDown = false;
        }

        // マウスのドラッグ処理（マウスを動かしたとき）
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!isMouseDown)
            {
                return;
            }

            if (Mouse.Captured == this)
            {
                if (this.SelectHeartGrid.Visibility == Visibility.Visible && (this.scene == "赤丸くんのきもちの大きさ" || this.scene == "青助くんのきもちの大きさ"))
                {
                    this.CalcAngle();

                    this.ViewSizeOfFeelingTextBlock.Text = this.feelingSize.ToString();
                }   
            }
        }
        
        // ハートゲージの針の角度に関する計算
        private void CalcAngle()
        {
            System.Windows.Point currentLocation = this.PointToScreen(Mouse.GetPosition(this));

            System.Windows.Point knobCenter = this.SelectHeartImage.PointToScreen(new System.Windows.Point(this.SelectHeartImage.ActualWidth * 0.5, this.SelectHeartImage.ActualHeight * 0.7));

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
    }
}
