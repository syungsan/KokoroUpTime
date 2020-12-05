using CsvReadWrite;
using Expansion;
using FileIOUtils;
using Osklib;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WMPLib;
using WpfAnimatedGif;




namespace KokoroUpTime


{
    /// <summary>
    /// GameWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class Chapter6 : Page
    {
        // すてきなせいかくのリスト
        private string[] NICE_PERSONALITY = { "●　親切にしてもらった", "●　アドバイスをくれた", "●　みんなをひっぱってくれた", "●　きりんと順番をまもってくれた", "●　みんなを笑顔にしてくれた", "●　（何か悪いことや失敗をゆるしてもらった）", "●　自分の気持ちを分かってもらった", "●　仲良くしてもらった", "●　ありがとうと言ってくれた" };

        private float THREE_SECOND_RULE_TIME = 3.0f;

        // ゲームを進行させるシナリオ
        private int scenarioCount = 0;
        private List<List<string>> scenarios = null;

        // 各種コントロールの名前を収める変数
        private string position = "";

        // マウスクリックを可能にするかどうかのフラグ
        private bool isClickable = false;

        // 画面を何回タップしたか
        private int tapCount = 0;

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

        //「すてきな性格」確認用のチェックボックス
        private CheckBox[] checkBoxs;

        private string[] EDIT_BUTTON = { "えんぴつ", "けしごむ", "すべてけす", "かんせい" };

        // 音関連
        private WindowsMediaPlayer mediaPlayer;
        private SoundPlayer sePlayer = null;

        // データベースに収めるデータモデルのインスタン
        private DataChapter6 dataChapter6;

        // ゲームの切り替えシーン
        private string scene;

        // データベースに収めるデータモデルのインスタンス
        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();
        public DataProgress dataProgress = new DataProgress();

        public InputMethodStyleSelector styleSelector = new InputMethodStyleSelector();


        public Chapter6()
        {
            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            // メディアプレーヤークラスのインスタンスを作成する
            this.mediaPlayer = new WindowsMediaPlayer();


            // データモデルインスタンス確保
            this.dataChapter6 = new DataChapter6();


            //this.GoupeActivityItemsControl.ItemsSource = HowtoInputDatas;
            this.SelectNicePersonalityListBox.ItemsSource = NICE_PERSONALITY;

            this.checkBoxs = new CheckBox[] { this.PointCheckBox1, this.PointCheckBox2, this.PointCheckBox3 };

            if (this.dataOption.InputMethod == 0)
            {
                //手書き
            }
            else if (this.dataOption.InputMethod == 1)
            {
                //キーボード
            }

            //this.styleSelector.SelectStyle(this.NicePesonalityListView, ListViewItem,this.dataOption.InputMethod);

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
                ["shiroji_small_right_center_image"] = this.ShirojiSmallRightCenterImage,
                ["session_title_image"] = this.SessionTitleImage,

                ["main_msg_bubble_image"] = this.MainMessageBubbleImage,

            };

            this.textBlockObjects = new Dictionary<string, TextBlock>
            {
                ["session_sub_title_text"] = this.SessionSubTitleTextBlock,
                ["session_sentence_text"] = this.SessionSentenceTextBlock,

                ["ending_msg_text"] = this.EndingMessageTextBlock,
                ["main_msg"] = this.MainMessageTextBlock,
                ["music_title_text"] = this.MusicTitleTextBlock,
                ["composer_name_text"] = this.ComposerNameTextBlock,
                ["item_book_title_text"] = this.ItemBookTitleTextBlock,
                ["groupe_activity_msg"] = this.GroupeActivityMessageTextBlock,
                ["session_frame_text"] = this.SessionFrameText,
                ["large_plate_title_text"] = this.LargePlateTitleText,
                ["small_plate_title_text"] = this.SmallPlateTitleText,


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
                ["ok_button"] = this.OkButton,
                ["return_button"] = this.ReturnButton,
            };

            this.gridObjects = new Dictionary<string, Grid>
            {
                ["session_grid"] = this.SessionGrid,
                ["session_frame_grid"] = this.SessionFrameGrid,

                ["summary_grid"] = this.SummaryGrid,
                ["ending_grid"] = this.EndingGrid,
                ["item_name_plate_left_grid"] = this.ItemNamePlateLeftGrid,
                ["item_name_bubble_grid"] = this.ItemNameBubbleGrid,
                ["item_name_plate_center_grid"] = this.ItemNamePlateCenterGrid,
                ["item_info_plate_grid"] = this.ItemInfoPlateGrid,
                ["item_info_sentence_grid"] = this.ItemInfoSentenceGrid,
                ["item_last_info_grid"] = this.ItemLastInfoGrid,
                ["item_review_grid"] = this.ItemReviewGrid,



                ["ending_msg_grid"] = this.EndingMessageGrid,
                ["main_msg_grid"] = this.MainMessageGrid,
                ["music_info_grid"] = this.MusicInfoGrid,
                ["branch_select_grid"] = this.BranchSelectGrid,
                ["exit_back_grid"] = this.ExitBackGrid,
                ["groupe_activity_message_grid"] = this.GroupeActivityMessageGrid,

                ["large_plate_title_grid"] = this.LargePlateTitleGrid,
                ["small_plate_title_grid"] = this.SmallPlateTitleGrid,

                ["aosukes_uncomfortable_situation_grid"] = this.AosukesUncomfortableSituationGrid,
                ["aosukes_perspnality_grid"] = this.AosukesPerspnalityGrid,
                ["akamarus_perspnality_grid"] = this.AkamarusPersonalityGrid,
                ["kimis_perspnality_grid"] = this.KimisPersonalityGrid,

                ["challenge_time_title_grid"] = this.ChallengeTimeTitleGrid,
                ["challenge_time_description_grid"] = this.ChallengeTimeDescriptionGrid,
                ["challenge_time_grid"] = this.ChallengeTimeGrid,
                ["challenge_time_message_grid"] = this.ChallengeTimeMessageGrid,
                ["groupe_activity_grid"] = this.GroupeActivityGrid,
                ["groupe_activity_message_grid"] = this.GroupeActivityMessageGrid,
                ["hint_check_grid"] = this.HintCheckGrid,
                ["canvas_edit_grid"] = this.CanvasEditGrid,

                ["point_check_box_grid"] = this.PointCheckBoxGrid,
                ["point_check_box_01_grid"] = this.PointCheckBox1Grid,
                ["point_check_box_02_grid"] = this.PointCheckBox2Grid,
                ["point_check_box_03_grid"] = this.PointCheckBox3Grid,
            };


        }

        private void ResetControls()
        {
            // 各種コントロールを隠すことでフルリセット

            this.SessionGrid.Visibility = Visibility.Hidden;

            this.SummaryGrid.Visibility = Visibility.Hidden;
            this.EndingGrid.Visibility = Visibility.Hidden;
            this.ItemNamePlateLeftGrid.Visibility = Visibility.Hidden;
            this.ItemNameBubbleGrid.Visibility = Visibility.Hidden;
            this.ItemNamePlateCenterGrid.Visibility = Visibility.Hidden;
            this.ItemInfoPlateGrid.Visibility = Visibility.Hidden;
            this.ItemLastInfoGrid.Visibility = Visibility.Hidden;
            this.ItemLeftLastImage.Visibility = Visibility.Hidden;

            this.ItemReviewGrid.Visibility = Visibility.Hidden;
            this.GroupeActivityMessageGrid.Visibility = Visibility.Hidden;
            this.SessionFrameGrid.Visibility = Visibility.Hidden;
            this.SessionFrameText.Visibility = Visibility.Hidden;

            this.LargePlateTitleGrid.Visibility = Visibility.Hidden;
            this.SmallPlateTitleGrid.Visibility = Visibility.Hidden;
            this.AosukesUncomfortableSituationGrid.Visibility = Visibility.Hidden;
            this.AosukesPerspnalityGrid.Visibility = Visibility.Hidden;
            this.AkamarusPersonalityGrid.Visibility = Visibility.Hidden;
            this.KimisPersonalityGrid.Visibility = Visibility.Hidden;
            this.ChallengeTimeGrid.Visibility = Visibility.Hidden;
            this.ChallengeTimeTitleGrid.Visibility = Visibility.Hidden;
            this.ChallengeTimeDescriptionGrid.Visibility = Visibility.Hidden;
            this.ChallengeTimeMessageGrid.Visibility = Visibility.Hidden;
            this.GroupeActivityGrid.Visibility = Visibility.Hidden;
            this.GroupeActivityMessageGrid.Visibility = Visibility.Hidden;
            this.HintCheckGrid.Visibility = Visibility.Hidden;
            this.PointCheckBoxGrid.Visibility = Visibility.Hidden;
            this.PointCheckBox1Grid.Visibility = Visibility.Hidden;
            this.PointCheckBox2Grid.Visibility = Visibility.Hidden;
            this.PointCheckBox3Grid.Visibility = Visibility.Hidden;

            this.ShirojiSmallRightCenterImage.Visibility = Visibility.Hidden;

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
            this.SessionTitleImage.Visibility = Visibility.Hidden;
            this.SessionSubTitleTextBlock.Visibility = Visibility.Hidden;
            this.SessionSentenceTextBlock.Visibility = Visibility.Hidden;
            this.SelectFeelingCompleteButton.Visibility = Visibility.Hidden;
            this.SelectFeelingNextButton.Visibility = Visibility.Hidden;
            this.ReturnButton.Visibility = Visibility.Hidden;


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
            this.GroupeActivityNextMessageButton.Visibility = Visibility.Hidden;
            this.GroupeActivityBackMessageButton.Visibility = Visibility.Hidden;
            this.CanvasEditGrid.Visibility = Visibility.Hidden;

            this.CoverLayerImage.Visibility = Visibility.Hidden;


            this.ReturnToTitleButton.Visibility = Visibility.Hidden;


            this.OkButton.Visibility = Visibility.Hidden;

            this.SelectFeelingBackButton.Visibility = Visibility.Hidden;
            this.CompleteRolePlayButton.Visibility = Visibility.Hidden;

            this.SessionSubTitleTextBlock.Text = "";
            this.SessionSentenceTextBlock.Text = "";

            this.EndingMessageTextBlock.Text = "";

            this.MusicTitleTextBlock.Text = "";
            this.ComposerNameTextBlock.Text = "";


        }

        public void SetNextPage(InitConfig _initConfig, DataOption _dataOption, DataItem _dataItem, DataProgress _dataProgress)
        {
            this.initConfig = _initConfig;
            this.dataOption = _dataOption;
            this.dataItem = _dataItem;
            this.dataProgress = _dataProgress;

            // 現在時刻を取得
            this.dataChapter6.CreatedAt = DateTime.Now.ToString();

            // データベースのテーブル作成と現在時刻の書き込みを同時に行う
            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
            {
                // 毎回のアクセス日付を記録
                connection.Insert(this.dataChapter6);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var csv = new CsvReader("./Scenarios/chapter6.csv"))
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

                        var gridgridObjectName = gridObject.Name;

                        string _objectsName = this.position;

                        this.ShowAnime(storyBoard: gridStoryBoard, objectName: gridgridObjectName, objectsName: _objectsName, isSync: gridAnimeIsSync);
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

                        string _objectsName = this.position;

                        this.ShowAnime(storyBoard: imageStoryBoard, objectName: imageObjectName, objectsName: _objectsName, isSync: imageAnimeIsSync);
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
                        var borderObjectName = borderObject.Name;
                        this.ShowAnime(storyBoard: borderStoryBoard,objectName: borderObjectName, isSync: borderAnimeIsSync);
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

                        var buttonObjectName = buttonObject.Name;

                        string _objectsName = this.position;

                        this.ShowAnime(storyBoard: buttonStoryBoard, objectName: buttonObjectName, objectsName: _objectsName, isSync: buttonAnimeIsSync);
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

                        var textObjectName = __textObject.Name;

                        string _objectsName = this.position;

                        this.ShowAnime(storyBoard: textStoryBoard, objectName: textObjectName, objectsName: _objectsName, isSync: textAnimeIsSync);
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
                            else if (clickMethod == "small_next_only")
                            {
                                waitTimer.Start();

                                waitTimer.Tick += (s, args) =>
                                {
                                    waitTimer.Stop();
                                    waitTimer = null;

                                    if (clickButton == "msg")
                                    {
                                        this.GroupeActivityNextMessageButton.Visibility = Visibility.Visible;
                                    }
                                };
                            }
                            else if (clickMethod == "back_only")
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
                            else if (clickMethod == "small_next_only")
                            {

                                if (clickButton == "msg")
                                {
                                    this.GroupeActivityNextMessageButton.Visibility = Visibility.Visible;
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

                    this.isClickable = false;

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

                            this.textBlockObjects[this.position].Text = "";

                            this.scenarioCount += 1;
                            this.ScenarioPlay();

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


                case "get_item":

                    this.dataItem.HasGotItem06 = true;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataItem SET HasGotItem06 = 1 WHERE Id = 1;");
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

                    Image[] itemMainImages = { this.Item01MainImage, this.Item02MainImage, this.Item03MainImage, this.Item04MainImage, this.Item05MainImage, this.Item07MainImage, this.Item08MainImage, this.Item09MainImage, this.Item10MainImage, this.Item11MainImage };

                    Image[] itemNoneImages = { this.Item01NoneImage, this.Item02NoneImage, this.Item03NoneImage, this.Item04NoneImage, this.Item05NoneImage, this.Item07NoneImage, this.Item08NoneImage, this.Item09NoneImage, this.Item10NoneImage, this.Item11NoneImage };

                    var hasGotItems = new bool[] { this.dataItem.HasGotItem01, this.dataItem.HasGotItem02, this.dataItem.HasGotItem03, this.dataItem.HasGotItem04, this.dataItem.HasGotItem05,  this.dataItem.HasGotItem07, this.dataItem.HasGotItem08, this.dataItem.HasGotItem09, this.dataItem.HasGotItem10, this.dataItem.HasGotItem11 };

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
                        else if (this.scene == "キミちゃんの場面")
                        {
                            text = text.Replace("$select_character$", "キミちゃん");
                        }
                        else if (this.scene == "青助くんの場面")
                        {
                            text = text.Replace("$select_character$", "青助くん");
                        }

                        break;


                    case "$number_of_selects$":

                        if (this.scene == "赤丸くんの場面")
                        {
                            text = text.Replace("$number_of_selects$", "①、②、③");
                        }
                        else if (this.scene == "キミちゃんの場面" || this.scene == "青助くんの場面")
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
                            case "yellow": { foreground = new SolidColorBrush(Colors.Yellow); break; };
                            case "purple": { foreground = new SolidColorBrush(Colors.Purple); break; };

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
        private void ShowAnime(string storyBoard, string objectName, string objectsName, string isSync)
        {
            Storyboard sb;
            try
            {
                sb = this.FindResource(storyBoard) as Storyboard;
                foreach (var child in sb.Children)
                    Storyboard.SetTargetName(child, objectName);
            }
            catch (ResourceReferenceKeyNotFoundException ex)
            {
                string objectsStroryBoard = $"{storyBoard}_{objectsName}";
                sb = this.FindResource(objectsStroryBoard) as Storyboard;
            }

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
                this.isClickable = false;

                if ((button.Name == "NextMessageButton" || button.Name == "NextPageButton" || button.Name == "MangaFlipButton" || button.Name == "SelectFeelingCompleteButton" || button.Name == "BranchButton2" || button.Name == "MangaPrevBackButton" || button.Name == "GroupeActivityNextMessageButton" || button.Name == "ReturnButton"))
                {

                    if (button.Name == "NextMessageButton")
                    {
                        this.BackMessageButton.Visibility = Visibility.Hidden;
                        this.NextMessageButton.Visibility = Visibility.Hidden;
                    }
                    else if (button.Name == "NextPageButton")
                    {
                        this.BackPageButton.Visibility = Visibility.Hidden;
                        this.NextPageButton.Visibility = Visibility.Hidden;
                    }
                    else if (button.Name == "GroupeActivityNextMessageButton")
                    {
                        this.GroupeActivityBackMessageButton.Visibility = Visibility.Hidden;
                        this.GroupeActivityNextMessageButton.Visibility = Visibility.Hidden;
                    }

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }
                else if (button.Name == "CompleteRolePlayButton")
                {
                    this.GoTo("complete_groupe_activity");
                }
                else if (button.Name == "BackMessageButton" || button.Name == "BackPageButton" || button.Name == "GroupeActivityBackMessageButton" || button.Name == "SelectFeelingBackButton")
                {
                    this.BackMessageButton.Visibility = Visibility.Hidden;
                    this.NextMessageButton.Visibility = Visibility.Hidden;

                    this.BackPageButton.Visibility = Visibility.Hidden;
                    this.NextPageButton.Visibility = Visibility.Hidden;

                    this.SelectFeelingNextButton.Visibility = Visibility.Hidden;
                    this.SelectFeelingBackButton.Visibility = Visibility.Hidden;

                    this.ScenarioBack();
                }

            }
            // FullScreen時のデバッグ用に作っておく
            if (button.Name == "ExitButton")
            {

                this.CoverLayerImage.Visibility = Visibility.Visible;
                this.ExitBackGrid.Visibility = Visibility.Visible;
            }
            else if (button.Name == "ExitBackYesButton")
            {
                this.StopBGM();

                TitlePage titlePage = new TitlePage();

                titlePage.SetReloadPageFlag(false);

                titlePage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                this.NavigationService.Navigate(titlePage);
            }
            else if (button.Name == "ExitBackNoButton")
            {
                this.ExitBackGrid.Visibility = Visibility.Hidden;
                this.CoverLayerImage.Visibility = Visibility.Hidden;
            }
            else if (button.Name == "BranchButton1")
            {
                this.GoTo("manga");
            }
            else if (button.Name == "SelectFeelingNextButton")
            {
                this.SelectFeelingNextButton.Visibility = Visibility.Hidden;
                this.scenarioCount += 1;
                this.ScenarioPlay();
            }
            else if (button.Name == "ReturnToTitleButton")
            {
                TitlePage titlePage = new TitlePage();

                titlePage.SetReloadPageFlag(false);

                titlePage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                this.NavigationService.Navigate(titlePage);
            }
            else if (button.Name == "HintCheckButton")
            {
                this.GoTo("check_hint");
            }

            else if (button.Name == "GroupeActivityButton")
            {
                this.GoTo("input_nice_personality");
                button.Background = null;
                this.GroupeActivityGrid.Width = 1920;
                this.GroupeActivityGrid.Height = 840;
                this.GroupeActivityGrid.VerticalAlignment = VerticalAlignment.Bottom;

                this.CanvasEditGrid.Visibility = Visibility.Visible;
            }
            else if (button.Name == "CompleteWritingButton")
            {
                this.GroupeActivityButton.Background = Brushes.Transparent;
                this.GroupeActivityGrid.Width = 1520;
                this.GroupeActivityGrid.Height = 665;
                this.GroupeActivityGrid.VerticalAlignment = VerticalAlignment.Center;
                Thickness margin = new Thickness(0, 100, 0, 0);
                this.GroupeActivityGrid.Margin = margin;

                this.scenarioCount += 1;
                this.ScenarioPlay();
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





        private void GoTo(string tag)
        {
            foreach (var (scenario, index) in this.scenarios.Indexed())
            {
                if (scenario[0] == "sub" && scenario[1] == tag)
                {
                    this.scenarioCount = index + 1;
                    this.ScenarioPlay();

                    break;
                }
                if (this.scene == tag && (scenario[0] == "scene" && scenario[1] == tag))
                {
                    this.scenarioCount = index + 1;
                    this.ScenarioPlay();

                    break;
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

            DoubleAnimation animation = new DoubleAnimation(newWidth, duration);

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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            if (this.checkBoxs.Contains(checkBox))
            {
                this.tapCount += 1;

                if (this.tapCount >= this.checkBoxs.Length)
                {

                    foreach (CheckBox _checkBox in this.checkBoxs)
                    {
                        _checkBox.IsEnabled = false;
                    }

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }
            }
        }

        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            if (this.checkBoxs.Contains(checkBox))
            {
                this.tapCount -= 1;
            }
        }



        // TextBoxにフォーカスが当たったときに起動
        private void TriggerKeyboard(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!OnScreenKeyboard.IsOpened())
            {
                try
                {
                    Process.Start("./tabtip.bat");

                    OnScreenKeyboard.Show();
                }
                catch (Exception ex)
                {
                    // MessageBox.Show(ex.Message);
                    Debug.Print(ex.Message);
                }
            }
            else if (OnScreenKeyboard.IsOpened())
            {
                try
                {
                    OnScreenKeyboard.Close();
                }
                catch (Exception ex)
                {
                    // MessageBox.Show(ex.Message);
                    Debug.Print(ex.Message);
                }
            }
        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem listBoxItem = sender as ListBoxItem;
            if (listBoxItem.Name == "PenButton")
            {
                
            }
            else if (listBoxItem.Name == "EraserButton")
            {

            }
        }


    }
}