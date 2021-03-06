﻿using CsvReadWrite;
using Expansion;
using FileIOUtils;
using Osklib;
using OutlineTextMaker;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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
using System.Windows.Threading;
using WMPLib;
using XamlAnimatedGif;
using System.Reflection;

namespace KokoroUpTime


{
    /// <summary>
    /// GameWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class Chapter12 : Page
    {
        //待ち時間

        private float THREE_SECOND_RULE_TIME = 3.0f;

        private Dictionary<string, string[]> ITEM_VALUES = new Dictionary<string, string[]>()
        {
            {"きもちセンサー", new string[] { "Images/item01_solo.png", "気持ちの種類と大きさ\nを測ることができます。", "0"/*名前が不一致:0 一致:1*/,"0"/*説明が不一致:0 一致:1*/} },
            {"うきうきシューズ", new string[] { "Images/item02_solo.png", "ちょっぴりうきうきする活動を\n見つけられます。", "0","0"} },
            {"あったかスープ", new string[] { "Images/item03_solo.png", "周りの人に温かい言葉を\nかけられるようになります。", "0","0"} },
            {"はきはきスピーカー", new string[] { "Images/item04_solo.png", "上手に頼んだり、断ったり、\n自分の気持ちを\nはきはき伝えられます。", "0","0"} },
            {"ゆったりんご", new string[] { "Images/item05_solo.png", "自分に合った\nリラックスの方法を\n見つけられます。", "0","0"} },
            {"すてきステッキ", new string[] { "Images/item06_solo.png", "自分やまわりの人の\nすてきなせいかくを\n見つけられます。", "0","0"} },
            {"考えライト", new string[] { "Images/item07_solo.png", "自分の考えを見つけられます。","0" ,"0"} },
            {"おじゃま虫バスター", new string[] { "Images/item08_solo.png", "おじゃま虫\n（いやな気持にさせる考え方）\nをたいじできます。", "0","0"} },
            {"勇者のマント", new string[] { "Images/item09_solo.png", "自分の苦手なことについて\n知ることができます。" ,"0","0"} },
            {"勇者のつるぎ", new string[] { "Images/item10_solo.png", "苦手なことにちょうせんする\nために、苦手なことを小さく\n分けられます。" ,"0","0"} },
            {"3色だんご", new string[] { "Images/item11_solo.png", "問題をかいけつするための\nステップを見つけられます。" ,"0","0"} },
        };


        Dictionary<string, SelectedItemMethodStrokeData> ITEM_METHOD_STROKE;

        Dictionary<string, SelectedItemMethodTextData> ITEM_METHOD_TEXT;

        LogManager logManager;

        // ゲームを進行させるシナリオ
        private int scenarioCount = 0;
        private List<List<string>> scenarios = null;

        // 各種コントロールの名前を収める変数
        private string position = "";

        // マウスクリックを可能にするかどうかのフラグ
        private bool isClickable = false;

        //アニメーションを表示させるか否か
#if DEBUG
        private readonly bool isAnimationSkip = true;
#else
        private bool isAnimationSkip = false;

#endif

        // マウス押下中フラグ
        private bool isMouseDown = false;

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
        private Dictionary<string, Border> borderObjects = null;
        private Dictionary<string, OutlineText> outlineTextObjects = null;


        // 音関連
        private WindowsMediaPlayer mediaPlayer;
        private SoundPlayer sePlayer = null;

        // データベースに収めるデータモデルのインスタンス
        private DataChapter12 dataChapter12;
        
        private string InputDictionaryKey = "item_method1";

        //入力したストロークを保存するためのコレクション


        ObservableCollection<ITEM_VALUESData> iTEM_VALUESDatas = new ObservableCollection<ITEM_VALUESData>();

        // ゲームの切り替えシーン
        private string scene;

        StrokeCollection ItemMethodInputStroke1 = new StrokeCollection();
        StrokeCollection ItemMethodInputStroke2 = new StrokeCollection();
        StrokeCollection ItemMethodInputStroke3 = new StrokeCollection();

        // データベースに収めるデータモデルのインスタンス
        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();
        public DataProgress dataProgress = new DataProgress();

        public Chapter12()
        {
            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            // メディアプレーヤークラスのインスタンスを作成する
            this.mediaPlayer = new WindowsMediaPlayer();


            // データモデルインスタンス確保
            this.dataChapter12 = new DataChapter12();

            // マウスイベントの設定
            this.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
            this.MouseUp += new MouseButtonEventHandler(OnMouseUp);
            this.MouseMove += new MouseEventHandler(OnMouseMove);
            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(OnPreviewMouseLeftButtonDown);

            logManager = new LogManager();

            this.InitControls();
        }

        private void InitControls()
        {
            // 各種コントロールに任意の文字列をあてがう

            this.imageObjects = new Dictionary<string, Image>
            {
                ["bg_image"] = this.BackgroundImage,
                ["manga_image"] = this.MangaImage,
                ["item_center_up_image"] = this.ItemCenterUpImage, //
                ["item_review_image"]=this.ItemReviewImage,
                ["session_title_image"] = this.SessionTitleImage,
                ["children_face_left_image"] = this.ChildrenFaceLeftImage,
                ["children_face_right_image"] = this.ChildrenFaceRightImage,
                ["shiroji_left_image"] = this.ShirojiLeftImage,
                ["shiroji_right_image"] = this.ShirojiRightImage,
                ["shiroji_right_up_image"] = this.ShirojiRightUpImage,
                ["shiroji_small_right_up_image"] = this.ShirojiSmallRightUpImage,
                ["children_stand_left_image"] = this.ChildrenStandLeftImage,
                ["children_stand_right_image"] = this.ChildrenStandRightImage,
                ["shiroji_small_right_center_image"] = this.ShirojiSmallRightCenterImage,
                ["session_title_image"] = this.SessionTitleImage,
                ["session_title_right_image"] = this.SessionTitleRightImage,
                ["cover_layer_image"] = this.CoverLayerImage,

                ["main_msg_bubble_image"] = this.MainMessageBubbleImage,

            };

            this.textBlockObjects = new Dictionary<string, TextBlock>
            {
                ["session_sub_title_text"] = this.SessionSubTitleTextBlock,
                ["session_sentence_text"] = this.SessionSentenceTextBlock,
                ["session_sub_right_title_text"] = this.SessionSubTitleRightTextBlock,
                ["session_sentence_right_text"] = this.SessionSentenceRightTextBlock,

                ["main_msg"] = this.MainMessageTextBlock,
                ["select_scene_msg"]=this.SelectSceneMessageTextBlock,
                ["music_title_text"] = this.MusicTitleTextBlock,
                ["composer_name_text"] = this.ComposerNameTextBlock,
                ["item_book_title_text"] = this.ItemBookTitleTextBlock,

                ["challenge_time_title_text"] = this.ChallengeTImeTitleText,

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
                ["return_button"] = this.ReturnButton,
                ["complete_input_button"] = this.CompleteInputButton,
                ["check_item_book_large_button"] =this.CheckItemBookLargeButton,
                ["to_challenge_time_button"] =this.ToChallengeTimeButton,
                ["to_groupe_activity_button"] = this.ToGroupeActivityButton,
                ["next_item_method_button"] =this.NextItemMethodButton,
                ["back_item_method_button"] = this.BackItemMethodButton,

            };

            this.gridObjects = new Dictionary<string, Grid>
            {
                ["session_grid"] = this.SessionGrid,
                ["session_frame_grid"] = this.SessionFrameGrid,
                ["session_frame_right_grid"] = this.SessionFrameRightGrid,
                ["manga_grid"] = this.MangaGrid,
                ["title_grid"] = this.TitleGrid,
                ["item_book_grid"]=this.ItemBookGrid,

                ["award_grid"]=this.AwardGrid,
                ["ending_grid"] = this.EndingGrid,

                ["main_msg_grid"] = this.MainMessageGrid,
                ["select_scene_msg_grid"] = this.SelectSceneMessageGrid,
                ["select_scene_grid"]=this.SelectSceneGrid,

                ["music_info_grid"] = this.MusicInfoGrid,
                ["branch_select_grid"] = this.BranchSelectGrid,
                ["exit_back_grid"] = this.ExitBackGrid,

                ["challenge_time_grid"] =this.ChallengeTimeGrid,
                ["groupe_activity_grid"] =this.GroupeActivityGrid,

                ["canvas_edit_grid"] = this.CanvasEditGrid,


            };

            this.borderObjects = new Dictionary<string, Border>
            {
                ["title_border"] = this.TitleBorder,
                ["input_canvas_border"] = this.InputCanvasBorder,
                ["input_text_box_border"] = this.InputTextBorder,

            };

            this.outlineTextObjects = new Dictionary<string, OutlineText>
            {
                ["lets_solve_problems_scene_title"] = this.LetsSolveProblemsSceneTitle,
                ["lets_use_item_scene_title"] = this.LetsUseItemSceneTitle,
                ["step_to_solve_problem_scene_title"] = this.StepToSolveProblemSceneTitle,
            };
        }

        private void ResetControls()
        {
            // 各種コントロールを隠すことでフルリセット

            this.SessionGrid.Visibility = Visibility.Hidden;

            this.EndingGrid.Visibility = Visibility.Hidden;

            this.SessionFrameGrid.Visibility = Visibility.Hidden;
            this.SessionFrameRightGrid.Visibility = Visibility.Hidden;
            this.SessionTitleImage.Visibility = Visibility.Hidden;
            this.SessionTitleRightImage.Visibility = Visibility.Hidden;


            this.TitleGrid.Visibility = Visibility.Hidden;
            this.TitleBorder.Visibility = Visibility.Hidden;
            this.LetsSolveProblemsSceneTitle.Visibility = Visibility.Hidden;
            this.StepToSolveProblemSceneTitle.Visibility = Visibility.Hidden;
            this.LetsUseItemSceneTitle.Visibility = Visibility.Hidden;
            this.ChallengeTImeTitleText.Visibility = Visibility.Hidden;

            this.ChallengeTimeGrid.Visibility = Visibility.Hidden;
            this.GroupeActivityGrid.Visibility = Visibility.Hidden;
            this.AwardGrid.Visibility = Visibility.Hidden;
            this.SelectSceneGrid.Visibility = Visibility.Hidden;
            this.ShirojiSmallRightCenterImage.Visibility = Visibility.Hidden;

            this.MainMessageGrid.Visibility = Visibility.Hidden;
            this.MusicInfoGrid.Visibility = Visibility.Hidden;
            this.ItemBookGrid.Visibility = Visibility.Hidden;
            this.ItemBookTitleTextBlock.Visibility = Visibility.Hidden;

            this.ExitBackGrid.Visibility = Visibility.Hidden;
            this.BranchSelectGrid.Visibility = Visibility.Hidden;


            this.BackgroundImage.Visibility = Visibility.Hidden;
            this.MangaGrid.Visibility = Visibility.Hidden;
            this.MangaImage.Visibility = Visibility.Hidden;
            this.ItemCenterUpImage.Visibility = Visibility.Hidden;
            this.ItemReviewImage.Visibility = Visibility.Hidden;
            this.SessionTitleImage.Visibility = Visibility.Hidden;
            this.SessionSubTitleTextBlock.Visibility = Visibility.Hidden;
            this.SessionSentenceTextBlock.Visibility = Visibility.Hidden;
            this.SelectFeelingCompleteButton.Visibility = Visibility.Hidden;
            this.SelectFeelingNextButton.Visibility = Visibility.Hidden;
            this.ReturnButton.Visibility = Visibility.Hidden;
            this.CheckItemBookLargeButton.Visibility = Visibility.Hidden;
            this.SelectSceneMessageGrid.Visibility = Visibility.Hidden;
            this.GroupeActivityMessageGrid.Visibility = Visibility.Hidden;


            this.ShirojiRightImage.Visibility = Visibility.Hidden;
            this.ShirojiLeftImage.Visibility = Visibility.Hidden;
            this.ShirojiRightUpImage.Visibility = Visibility.Hidden;
            this.ShirojiSmallRightUpImage.Visibility = Visibility.Hidden;
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
            this.CanvasEditGrid.Visibility = Visibility.Hidden;
            this.CompleteInputButton.Visibility = Visibility.Hidden;

           
            this.InputCanvasBorder.Visibility = Visibility.Hidden;
            this.InputTextBorder.Visibility = Visibility.Hidden;
            this.CoverLayerImage.Visibility = Visibility.Hidden;
            this.ReturnToTitleButton.Visibility = Visibility.Hidden;

            this.SelectFeelingBackButton.Visibility = Visibility.Hidden;
            this.SelectFeelingCompleteButton.Visibility = Visibility.Hidden;
            this.ToGroupeActivityButton.Visibility = Visibility.Hidden;
            this.ToChallengeTimeButton.Visibility = Visibility.Hidden;
                
                
                

            this.SessionSubTitleTextBlock.Text = "";
            this.SessionSentenceTextBlock.Text = "";

            this.MusicTitleTextBlock.Text = "";
            this.ComposerNameTextBlock.Text = "";

        }

        private void SetInputMethod()
        {
            if (this.dataOption.InputMethod == 0)
            {

                this.ITEM_METHOD_STROKE = new Dictionary<string, SelectedItemMethodStrokeData>()
                {
                    ["item_method1"] = new SelectedItemMethodStrokeData("発明品①", "", new StrokeCollection()),
                    ["item_method2"] = new SelectedItemMethodStrokeData("発明品②", "", new StrokeCollection()),
                    ["item_method3"] = new SelectedItemMethodStrokeData("発明品③", "", new StrokeCollection()),
                };
        
                ObservableCollection<SelectedItemMethodStrokeData> datas = new ObservableCollection<SelectedItemMethodStrokeData>();
                datas.Add(this.ITEM_METHOD_STROKE[this.InputDictionaryKey]);

                this.InputItemMethodItemsControl.ItemsSource = datas;
            }
            else
            {
                this.ITEM_METHOD_TEXT = new Dictionary<string, SelectedItemMethodTextData>()
                {
                    ["item_method1"] = new SelectedItemMethodTextData("発明品①", "", this.dataChapter12.ItemMethodInputText1=""),
                    ["item_method2"] = new SelectedItemMethodTextData("発明品②", "", this.dataChapter12.ItemMethodInputText2=""),
                    ["item_method3"] = new SelectedItemMethodTextData("発明品③", "", this.dataChapter12.ItemMethodInputText3=""),
                };

                ObservableCollection<SelectedItemMethodTextData> datas = new ObservableCollection<SelectedItemMethodTextData>();
                datas.Add(this.ITEM_METHOD_TEXT[this.InputDictionaryKey]);

                this.InputItemMethodItemsControl.ItemsSource = datas;
            }
        }

        public void SetNextPage(InitConfig _initConfig, DataOption _dataOption, DataItem _dataItem, DataProgress _dataProgress, bool isCreateNewTable)
        {
            this.initConfig = _initConfig;
            this.dataOption = _dataOption;
            this.dataItem = _dataItem;
            this.dataProgress = _dataProgress;

            // 現在時刻を取得
            this.dataChapter12.CreatedAt = DateTime.Now.ToString();
            if (isCreateNewTable)
            {
                // データベースのテーブル作成と現在時刻の書き込みを同時に行う
                using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                {
                    // 毎回のアクセス日付を記録
                    connection.Insert(this.dataChapter12);
                }
            }
            else
            {
                string lastCreatedAt = "";

                using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                {
                    var chapter12 = connection.Query<DataChapter12>($"SELECT * FROM DataChapter12 ORDER BY Id DESC LIMIT 1;");

                    foreach (var row in chapter12)
                    {
                        lastCreatedAt = row.CreatedAt;
                    }
                }

                using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                {
                    connection.Execute($@"UPDATE DataChapter12 SET CreatedAt = '{this.dataChapter12.CreatedAt}'WHERE CreatedAt = '{lastCreatedAt}';");
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var csv = new CsvReader("./Scenarios/chapter12.csv"))
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

                    this.dataProgress.CurrentChapter = 12;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET CurrentChapter = '{this.dataProgress.CurrentChapter}' WHERE Id = 1;");
                    }

                    this.SetInputMethod();

                    logManager.StartLog(this.initConfig, this.dataProgress,this.MainGrid);

                    var startupPath1 = FileUtils.GetStartupPath();
                    string namePngPath1 = $"./Log/{this.initConfig.userName}/name.png";

                    if (File.Exists(namePngPath1))
                    {
                        this.AwardName.UnicodeString = "どの";

                        var image = new BitmapImage();

                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        image.UriSource = new Uri($@"{startupPath1}/{namePngPath1}", UriKind.Absolute);
                        image.EndInit();

                        image.Freeze();
                        this.AwardNameImage.Source = image;
                    }
                    else
                    {
                        this.AwardName.UnicodeString = $"{this.initConfig.userName}どの";
                    }
                    string[] text = Regex.Match(this.initConfig.accessDateTime, "[0-9][0-9][0-9][0-9]/[0-9][0-9]/[0-9][0-9]").ToString().Split("/");
                    DateTime dateTime = new DateTime(int.Parse(text[0]), int.Parse(text[1]), int.Parse(text[2]));
                    JapaneseCalendar calendar = new JapaneseCalendar();
                    var cultureJp = new CultureInfo("ja-JP", false);
                    cultureJp.DateTimeFormat.Calendar = calendar;

                    var date = dateTime.ToString("ggyy年MM月dd日", cultureJp);
                    Debug.Print(date);

                    string[] a = new string[] { "", "", "" };
                    int count = 0;
                    
                    string[] juu = { "十", "二十", "三十", "四十", "五十", "六十", "七十", "八十", "九十" };
                    string[] iti = { "一", "二", "三", "四", "五", "六", "七", "八", "九" };

                    foreach (var aaa in Regex.Matches(date, "[0-9][0-9]"))
                    {
                        string rrr = aaa.ToString();

                        if(int.Parse(rrr[0].ToString()) == 0)
                        {

                        }
                        else
                        {
                            a[count] = juu[int.Parse(rrr[0].ToString())-1];
                        }

                        if (int.Parse(rrr[1].ToString()) == 0)
                        {

                        }
                        else
                        {
                            a[count] += iti[int.Parse(rrr[1].ToString())-1];
                        }
                        count++;
                    }
                    this.AwardDate.UnicodeString = dateTime.ToString($"gg{a[0]}年{a[1]}月{a[2]}日", cultureJp);




                    ////前回のつづきからスタート
                    //if (this.dataProgress.CurrentScene != null)
                    //{
                    //    this.GoTo(this.dataProgress.CurrentScene, "scene");
                    //    this.SetData();
                    //}
                    //else
                    //{
                    //    this.scenarioCount += 1;
                    //    this.ScenarioPlay();
                    //}

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "end":

                    // 画面のフェードアウト処理とか入れる（別関数を呼び出す）

                    this.StopBGM();

                    this.dataProgress.HasCompletedChapter12 = true;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET HasCompletedChapter12 = '{Convert.ToInt32(this.dataProgress.HasCompletedChapter12)}' WHERE Id = 1;");
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
                    this.dataProgress.LatestChapter12Scene = this.scene;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET CurrentScene = '{this.dataProgress.CurrentScene}', LatestChapter12Scene = '{this.dataProgress.LatestChapter12Scene}' WHERE Id = 1;");
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
                case "border":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var borderObject = this.borderObjects[this.position];

                    borderObject.Visibility = Visibility.Visible;

                    string borderAnimeIsSync = "sync";

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var background = this.scenarios[this.scenarioCount][2];
                        var converter = new BrushConverter();

                        switch (background)
                        {
                            case "light_orange":
                                borderObject.Background = (Brush)converter.ConvertFromString("#B2FFD966");
                                break;

                            case "light_green":
                                borderObject.Background = (Brush)converter.ConvertFromString("#B2C8FAC8");
                                break;

                            case "blue":
                                borderObject.Background = (Brush)converter.ConvertFromString("#FF4472C4");
                                break;

                            case "orange":
                                borderObject.Background = (Brush)converter.ConvertFromString("#FFFFC000");
                                break;
                        }
                    }
                    if (this.scenarios[this.scenarioCount].Count > 3 && this.scenarios[this.scenarioCount][3] != "")
                    {
                        var borderBrush = this.scenarios[this.scenarioCount][3];
                        var converter = new BrushConverter();

                        switch (borderBrush)
                        {
                            case "red":
                                borderObject.BorderBrush = (Brush)converter.ConvertFromString("#FFFF0000");
                                break;

                            case "blue":
                                borderObject.BorderBrush = (Brush)converter.ConvertFromString("#FF0070C0");
                                break;

                            case "yellow":
                                borderObject.BorderBrush = (Brush)converter.ConvertFromString("#FFFFC000");
                                break;

                            case "black":
                                borderObject.BorderBrush = (Brush)converter.ConvertFromString("#FF000000");
                                break;
                        }
                    }
                    if (this.scenarios[this.scenarioCount].Count > 5 && this.scenarios[this.scenarioCount][5] != "")
                    {
                        borderAnimeIsSync = this.scenarios[this.scenarioCount][5];
                    }
                    if (this.scenarios[this.scenarioCount].Count > 4 && this.scenarios[this.scenarioCount][4] != "")
                    {
                        var borderStoryBoard = this.scenarios[this.scenarioCount][4];
                        var borderObjectName = borderObject.Name;
                        string _objectsName = this.position;

                        this.ShowAnime(storyBoard: borderStoryBoard, objectName: borderObjectName, objectsName: _objectsName, isSync: borderAnimeIsSync);
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

                    if (this.isAnimationSkip)
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
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

                //縁取り文字を表示させるための処理
                case "outline_text":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var outlineTextObject = this.outlineTextObjects[this.position];

                    outlineTextObject.Visibility = Visibility.Visible;

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

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
                            else if (clickMethod == "with_next_msg")
                            {
                                waitTimer.Start();

                                waitTimer.Tick += (s, args) =>
                                {
                                    waitTimer.Stop();
                                    waitTimer = null;
                                    if (clickButton == "page")
                                    {
                                        this.BackPageButton.Visibility = Visibility.Visible;
                                        this.NextMessageButton.Visibility = Visibility.Visible;
                                    }
                                };
                            }
                            else if (clickMethod == "with_back_msg")
                            {
                                waitTimer.Start();

                                waitTimer.Tick += (s, args) =>
                                {
                                    waitTimer.Stop();
                                    waitTimer = null;
                                    if (clickButton == "page")
                                    {
                                        this.NextPageButton.Visibility = Visibility.Visible;
                                        this.BackMessageButton.Visibility = Visibility.Visible;
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
                            else if (clickMethod == "back_only")
                            {
                                if (clickButton == "msg")
                                {
                                    this.BackMessageButton.Visibility = Visibility.Visible;
                                }
                                else if (clickButton == "page")
                                {
                                    this.BackPageButton.Visibility = Visibility.Visible;
                                }
                            }
                            else if (clickMethod == "with_next_msg")
                            {
                                if (clickButton == "page")
                                {
                                    this.BackPageButton.Visibility = Visibility.Visible;
                                    this.NextMessageButton.Visibility = Visibility.Visible;
                                }
                            }
                            else if (clickMethod == "with_back_msg")
                            {
                                if (clickButton == "page")
                                {
                                    this.NextPageButton.Visibility = Visibility.Visible;
                                    this.BackMessageButton.Visibility = Visibility.Visible;
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

                    if (this.dataOption.Is3SecondRule)
                    {
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
                    }
                    else
                    {
                        this.isClickable = true;
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

                        case "border":
                            this.position = this.scenarios[this.scenarioCount][2];
                            this.borderObjects[this.position].Visibility = Visibility.Hidden;
                            this.scenarioCount += 1;
                            this.ScenarioPlay();
                            break;

                        case "outline_text":
                            this.position = this.scenarios[this.scenarioCount][2];
                            this.outlineTextObjects[this.position].Visibility = Visibility.Hidden;
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
                            switch (this.scene)
                            {
                                case "チャレンジタイム！　パート①":
                                    this.GoTo("challenge_time_part1","sub");
                                    break;

                                case "チャレンジタイム！　パート②":
                                    this.GoTo("think_akamaru_method","sub");
                                    break;

                                case "グループアクティビティ":
                                    this.GoTo("groupe_activity","sub");
                                    break;
                            }
                        }
                        else
                        {
                            this.GoTo(GoToLabel,"sub");
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

                // イメージに対しての処理
                case "gif":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var gifObject = this.imageObjects[this.position];

                    var gifFile = this.scenarios[this.scenarioCount][2];

                    var gifUri = new Uri($"Images/{gifFile}", UriKind.Relative);

                    AnimationBehavior.SetSourceUri(gifObject, gifUri);

                    gifObject.Visibility = Visibility.Visible;

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

                case "is_animation_skip":

                    #if DEBUG
                    #else
                        this.isAnimationSkip = Convert.ToBoolean(this.scenarios[this.scenarioCount][1]);
                    #endif


                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "set_item_question":

                    var questiontime = this.scenarios[this.scenarioCount][1];
                    int number = 1;
                    bool flag = true;

                    if (questiontime == "first")
                    {
                        flag = true;
                        this.ItemQuestionGrid06.Visibility = Visibility.Visible;
                        this.ItemNameButton06.Visibility = Visibility.Visible;
                        this.ItemReviewButton06.Visibility = Visibility.Visible;

                        for (int i = 1; i < 7;i++)
                        {
                           
                           
                        }
                    }
                    else if(questiontime == "second")
                    {
                        flag = false;
                        this.ItemQuestionGrid06.Visibility = Visibility.Hidden;
                        this.ItemNameButton06.Visibility = Visibility.Hidden;
                        this.ItemReviewButton06.Visibility = Visibility.Hidden;

                        for (int i = 1;i < 6;i++)
                        {

                        }
                    }
                    foreach (KeyValuePair<string,string[]> pair in this.ITEM_VALUES)
                    {
                        if (pair.Value[0] == "Images/item07_solo.png")
                        {
                            flag = !flag;
                        }
                        if (flag)
                        {
                            Image image = this.FindName($"ItemQuestionImage0{number.ToString()}") as Image;
                            image.Source = new BitmapImage(new Uri(pair.Value[0], UriKind.Relative));

                            Button nameButton = this.FindName($"ItemNameButton0{number.ToString()}") as Button;
                            nameButton.Content = pair.Key;
                            Button reviewButton = this.FindName($"ItemReviewButton0{number.ToString()}") as Button;
                            reviewButton.Content = pair.Value[1];

                            if (pair.Value[2]=="1")
                            {
                                nameButton.Visibility = Visibility.Hidden;

                                Border nameBorder = this.FindName($"ItemNameBorder0{number.ToString()}") as Border;
                                nameBorder.BorderBrush = Brushes.Red;
                                nameBorder.Opacity = 1.0;

                                TextBlock nameTextBlock = this.FindName($"ItemNameText0{number.ToString()}") as TextBlock;
                                nameTextBlock.Text = pair.Key;
                                nameTextBlock.FontWeight = FontWeights.Bold;

                            }
                            else
                            {
                                nameButton.Visibility = Visibility.Visible;

                                Border nameBorder = this.FindName($"ItemNameBorder0{number.ToString()}") as Border;
                                nameBorder.BorderBrush = Brushes.Black;
                                nameBorder.Opacity = 0.8;

                                TextBlock nameTextBlock = this.FindName($"ItemNameText0{number.ToString()}") as TextBlock;
                                nameTextBlock.Text = "？？？";
                                nameTextBlock.FontWeight = FontWeights.Regular;
                            }

                            if (pair.Value[3] == "1")
                            {
                                reviewButton.Visibility = Visibility.Hidden;

                                Border reviewBorder = this.FindName($"ItemReviewBorder0{number.ToString()}") as Border;
                                reviewBorder.BorderBrush = Brushes.Red;
                                reviewBorder.Opacity = 1.0;

                                TextBlock nameTextBlock = this.FindName($"ItemReviewText0{number.ToString()}") as TextBlock;
                                nameTextBlock.Text = pair.Value[1];
                                nameTextBlock.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                reviewButton.Visibility = Visibility.Visible;

                                Border reviewBorder = this.FindName($"ItemReviewBorder0{number.ToString()}") as Border;
                                reviewBorder.BorderBrush = Brushes.Black;
                                reviewBorder.Opacity = 0.8;

                                TextBlock nameTextBlock = this.FindName($"ItemReviewText0{number.ToString()}") as TextBlock;
                                nameTextBlock.Text = "？？？";
                                nameTextBlock.FontWeight = FontWeights.Regular;
                            }

                            number++;
                        }
                    }

                    

                    this.PlaceInitialPoint();

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                    break;

                case "set_groupe_activity":

                    if (this.dataOption.InputMethod == 0)
                    {
                        ObservableCollection<SelectedItemMethodStrokeData> datas = new ObservableCollection<SelectedItemMethodStrokeData>();
                        datas.Add(this.ITEM_METHOD_STROKE["item_method1"]);

                        this.InputItemMethodItemsControl.ItemsSource = datas;
                        this.BackItemMethodButton.Visibility = Visibility.Hidden;

                        foreach(SelectedItemMethodStrokeData data in this.ITEM_METHOD_STROKE.Values)
                        {
                            if(data.SelectedItemImageSource !="" && data.ItemMethodStroke.Count > 0)
                            {
                                this.SelectFeelingNextButton.Visibility = Visibility.Visible;
                                break;
                            }
                            else
                            {
                                this.SelectFeelingNextButton.Visibility = Visibility.Hidden;
                            }
                        }
                    }
                    else
                    {
                        ObservableCollection<SelectedItemMethodTextData> datas = new ObservableCollection<SelectedItemMethodTextData>();
                        datas.Add(this.ITEM_METHOD_TEXT[this.InputDictionaryKey]);

                        this.InputItemMethodItemsControl.ItemsSource = datas;

                        foreach (SelectedItemMethodTextData data in this.ITEM_METHOD_TEXT.Values)
                        {
                            if (data.SelectedItemImageSource != "" && data.ItemMethodText !="")
                            {
                                this.SelectFeelingNextButton.Visibility = Visibility.Visible;
                                break;
                            }
                            else
                            {
                                this.SelectFeelingNextButton.Visibility = Visibility.Hidden;
                            }
                        }
                    }

                    this.NextItemMethodButton.Visibility = Visibility.Visible;
                    this.BackItemMethodButton.Visibility = Visibility.Hidden;

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "check_answer":

                    if(this.scene == "アイテムの総復習①")
                    {
                        if (this.CheckAnswer("first"))
                        {
                            this.scenarioCount += 1;
                            this.ScenarioPlay();
                        }
                        else
                        {
                            this.GoTo("uncorrect_scene","sub");
                        }
                    }
                    else
                    {
                        

                        if (this.CheckAnswer("second"))
                        {
                            this.scenarioCount += 1;
                            this.ScenarioPlay();
                        }
                        else
                        {
                            this.GoTo("uncorrect_scene","sub");
                        }
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

                //switch (sequence)
                //{
                //    //
                //}
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
            if (this.isAnimationSkip)
            {
                mode = "text";

                textObject.Visibility = Visibility.Visible;
            }
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

                    var converter = new BrushConverter();
                    if (options.Length > 0 && options[0] != "")
                    {
                        switch (options[0])
                        {
                            case "red": { foreground = new SolidColorBrush(Colors.Red); break; };
                            case "green": { foreground = new SolidColorBrush(Colors.Green); break; };
                            case "blue": { foreground = new SolidColorBrush(Colors.Blue); break; };
                            case "yellow": { foreground = new SolidColorBrush(Colors.Yellow); break; };
                            case "purple": { foreground = (SolidColorBrush)converter.ConvertFromString("#FF9966FF"); break; };
                            case "gray": { foreground = new SolidColorBrush(Colors.Gray); break; };

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
            if (!this.isAnimationSkip)
            {
                Storyboard sb;
                try
                {
                    sb = this.FindResource(storyBoard) as Storyboard;
                    foreach (var child in sb.Children)
                        Storyboard.SetTargetName(child, objectName);
                }
                catch (ResourceReferenceKeyNotFoundException)
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
            else
            {
                this.scenarioCount += 1;
                this.ScenarioPlay();
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

                return;
            }
            else if (button.Name == "ExitBackYesButton")
            {
                this.StopBGM();

                TitlePage titlePage = new TitlePage();

                titlePage.SetReloadPageFlag(false);

                titlePage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                this.NavigationService.Navigate(titlePage);

                return;
            }
            else if (button.Name == "ExitBackNoButton")
            {
                this.ExitBackGrid.Visibility = Visibility.Hidden;
                this.CoverLayerImage.Visibility = Visibility.Hidden;

                return;
            }


            else if (button.Name == "ReturnToTitleButton")
            {
                TitlePage titlePage = new TitlePage();

                titlePage.SetReloadPageFlag(false);

                titlePage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                this.NavigationService.Navigate(titlePage);

                return;
            }

            if (this.isClickable)
            {
                this.isClickable = false;

                if ((button.Name == "NextMessageButton" || button.Name == "NextPageButton" || button.Name == "MangaFlipButton" || button.Name == "SelectFeelingCompleteButton" || button.Name == "BranchButton2" || button.Name == "MangaPrevBackButton" || button.Name == "GroupeActivityNextMessageButton" || button.Name == "DecideAosukeMethodButton"))
                {

                    if (button.Name == "NextMessageButton")
                    {
                        this.BackMessageButton.Visibility = Visibility.Hidden;
                        this.NextMessageButton.Visibility = Visibility.Hidden;

                        this.BackPageButton.Visibility = Visibility.Hidden;
                        this.NextPageButton.Visibility = Visibility.Hidden;
                    }
                    else if (button.Name == "NextPageButton")
                    {
                        this.BackPageButton.Visibility = Visibility.Hidden;
                        this.NextPageButton.Visibility = Visibility.Hidden;

                        this.BackMessageButton.Visibility = Visibility.Hidden;
                        this.NextMessageButton.Visibility = Visibility.Hidden;


                        if (this.scene == "修了証書授与")
                        {
                            this.dataProgress.HasCompletedChapter12 = true;

                            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                            {
                                connection.Execute($@"UPDATE DataProgress SET HasCompletedChapter12 = '{Convert.ToInt32(this.dataProgress.HasCompletedChapter12)}' WHERE Id = 1;");
                            }
//#if DEBUG
                            this.StopBGM();

                            EndingPage endingPage = new EndingPage();

                            endingPage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                            this.NavigationService.Navigate(endingPage);
//#endif

                        }

                    }
                    else if (button.Name == "MangaFlipButton")
                    {
                        this.MangaFlipButton.Visibility = Visibility.Hidden;
                    }
                    else if(button.Name == "SelectFeelingCompleteButton")
                    {
                        this.SelectFeelingCompleteButton.Visibility = Visibility.Hidden;
                        this.SelectFeelingBackButton.Visibility = Visibility.Hidden;
                    }

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }


                else if (button.Name == "BackMessageButton" || button.Name == "BackPageButton" || button.Name == "GroupeActivityBackMessageButton" || button.Name == "SelectFeelingBackButton")
                {
                    this.BackMessageButton.Visibility = Visibility.Hidden;
                    this.NextMessageButton.Visibility = Visibility.Hidden;

                    this.BackPageButton.Visibility = Visibility.Hidden;
                    this.NextPageButton.Visibility = Visibility.Hidden;

                    this.SelectFeelingNextButton.Visibility = Visibility.Hidden;
                    this.SelectFeelingCompleteButton.Visibility = Visibility.Hidden;
                    this.SelectFeelingBackButton.Visibility = Visibility.Hidden;


                    this.ScenarioBack();



                }
                else if (button.Name == "SelectFeelingNextButton")
                {
                    this.SelectFeelingNextButton.Visibility = Visibility.Hidden;

                    if (this.GroupeActivityGrid.Visibility == Visibility.Visible)
                    {
                        if (this.dataOption.InputMethod == 0)
                        {
                            int count = 1;
                            foreach (SelectedItemMethodStrokeData data in this.ITEM_METHOD_STROKE.Values)
                            {
                                string itemName ="";
                            
                                switch (data.SelectedItemImageSource)
                                {
                                    case "Images/item01_solo.png":
                                        itemName = "きもちセンサー";
                                        break;
                                    case "Images/item02_solo.png":
                                        itemName = "うきうきシューズ";
                                        break;
                                    case "Images/item03_solo.png":
                                        itemName = "あったかスープ";
                                        break;
                                    case "Images/item04_solo.png":
                                        itemName = "はきはきスピーカー";
                                        break;
                                    case "Images/item05_solo.png":
                                        itemName = "ゆったりんご";
                                        break;
                                    case "Images/item06_solo.png":
                                        itemName = "すてきステッキ";
                                        break;
                                    case "Images/item07_solo.png":
                                        itemName = "考えライト";
                                        break;
                                    case "Images/item08_solo.png":
                                        itemName = "おじゃま虫バスター";
                                        break;
                                    case "Images/item09_solo.png":
                                        itemName = "勇者のマント";
                                        break;
                                    case "Images/item10_solo.png":
                                        itemName = "勇者のつるぎ";
                                        break;
                                    case "Images/item11_solo.png":
                                        itemName = "3色だんご";
                                        break;
                                }

                                switch (data.SelectedItemTitle)
                                {
                                    case "発明品①":
                                        this.dataChapter12.SelectedItem1 = itemName;
                                        using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                                        {
                                            connection.Execute($@"UPDATE DataChapter12 SET SelectedItem1 = '{this.dataChapter12.SelectedItem1}' WHERE CreatedAt = '{this.dataChapter12.CreatedAt}';");
                                        }
                                        break;

                                    case "発明品②":
                                        this.dataChapter12.SelectedItem2 = itemName;
                                        using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                                        {
                                            connection.Execute($@"UPDATE DataChapter12 SET SelectedItem2 = '{this.dataChapter12.SelectedItem2}' WHERE CreatedAt = '{this.dataChapter12.CreatedAt}';");
                                        }
                                        break;

                                    case "発明品③":
                                        this.dataChapter12.SelectedItem3 = itemName;
                                        using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                                        {
                                            connection.Execute($@"UPDATE DataChapter12 SET SelectedItem3 = '{this.dataChapter12.SelectedItem3}' WHERE CreatedAt = '{this.dataChapter12.CreatedAt}';");
                                        }
                                        break;
                                }

                                StrokeConverter strokeConverter = new StrokeConverter();
                                strokeConverter.ConvertToBmpImage(this.InputCanvas,data.ItemMethodStroke,$"groupe_activity_item_method_stroke{count}",this.initConfig.userName,this.dataProgress.CurrentChapter);
                                count++;
                            }
                        }
                        else
                        {
                            foreach (SelectedItemMethodTextData data in this.ITEM_METHOD_TEXT.Values)
                            {
                                PropertyInfo itemMethodInputTextProperty;

                                string itemName = "";

                                switch (data.SelectedItemImageSource)
                                {
                                    case "Images/item01_solo.png":
                                        itemName = "きもちセンサー";
                                        break;
                                    case "Images/item02_solo.png":
                                        itemName = "うきうきシューズ";
                                        break;
                                    case "Images/item03_solo.png":
                                        itemName = "あったかスープ";
                                        break;
                                    case "Images/item04_solo.png":
                                        itemName = "はきはきスピーカー";
                                        break;
                                    case "Images/item05_solo.png":
                                        itemName = "ゆったりんご";
                                        break;
                                    case "Images/item06_solo.png":
                                        itemName = "すてきステッキ";
                                        break;
                                    case "Images/item07_solo.png":
                                        itemName = "考えライト";
                                        break;
                                    case "Images/item08_solo.png":
                                        itemName = "おじゃま虫バスター";
                                        break;
                                    case "Images/item09_solo.png":
                                        itemName = "勇者のマント";
                                        break;
                                    case "Images/item10_solo.png":
                                        itemName = "勇者のつるぎ";
                                        break;
                                    case "Images/item11_solo.png":
                                        itemName = "3色だんご";
                                        break;
                                }

                                switch (data.SelectedItemTitle)
                                {
                                    case "発明品①":
                                        this.dataChapter12.SelectedItem1 = itemName;

                                        itemMethodInputTextProperty = typeof(DataChapter12).GetProperty("ItemMethodInputText1");
                                        itemMethodInputTextProperty.SetValue(this.dataChapter12, data.ItemMethodText);

                                        using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                                        {
                                            connection.Execute($@"UPDATE DataChapter12 SET ItemMethodInputText1 = '{this.dataChapter12.ItemMethodInputText1}' WHERE CreatedAt = '{this.dataChapter12.CreatedAt}';");
                                            connection.Execute($@"UPDATE DataChapter12 SET SelectedItem1 = '{this.dataChapter12.SelectedItem1}' WHERE CreatedAt = '{this.dataChapter12.CreatedAt}';");
                                        }
                                        break;

                                    case "発明品②":
                                        this.dataChapter12.SelectedItem2 = itemName;


                                        itemMethodInputTextProperty = typeof(DataChapter12).GetProperty("ItemMethodInputText2");
                                        itemMethodInputTextProperty.SetValue(this.dataChapter12, data.ItemMethodText);

                                        using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                                        {
                                            connection.Execute($@"UPDATE DataChapter12 SET ItemMethodInputText2 = '{this.dataChapter12.ItemMethodInputText2}' WHERE CreatedAt = '{this.dataChapter12.CreatedAt}';");
                                            connection.Execute($@"UPDATE DataChapter12 SET SelectedItem2 = '{this.dataChapter12.SelectedItem2}' WHERE CreatedAt = '{this.dataChapter12.CreatedAt}';");
                                        }
                                        break;

                                    case "発明品③":
                                        this.dataChapter12.SelectedItem3 = itemName;

                                        itemMethodInputTextProperty = typeof(DataChapter12).GetProperty("ItemMethodInputText3");
                                        itemMethodInputTextProperty.SetValue(this.dataChapter12, data.ItemMethodText);

                                        using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                                        {
                                            connection.Execute($@"UPDATE DataChapter12 SET ItemMethodInputText3 = '{this.dataChapter12.ItemMethodInputText3}' WHERE CreatedAt = '{this.dataChapter12.CreatedAt}';");
                                            connection.Execute($@"UPDATE DataChapter12 SET SelectedItem3 = '{this.dataChapter12.SelectedItem3}' WHERE CreatedAt = '{this.dataChapter12.CreatedAt}';");

                                        }
                                        break;
                                }
                            }
                        }
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }

                else if (button.Name == "KindOfFeelingInputButton")
                {
                    this.GoTo("select_kind_of_feeling","sub");
                }
                else if (button.Name == "SizeOfFeelingInputButton")
                {
                    this.GoTo("select_size_of_feeling","sub");
                }
                else if (button.Name == "BranchButton1")
                {
                    this.GoTo("manga","sub");
                }
                else if (button.Name == "CompleteInputButton")
                {
                    if (this.dataOption.InputMethod == 0)
                    {
                        if (this.ITEM_METHOD_STROKE[this.InputDictionaryKey] .ItemMethodStroke!= null)
                        {
                            this.InputCanvas.Strokes = this.ITEM_METHOD_STROKE[this.InputDictionaryKey].ItemMethodStroke;
                        }

                        this.SelectFeelingCompleteButton.Visibility = Visibility.Hidden;
                        foreach (SelectedItemMethodStrokeData data in this.ITEM_METHOD_STROKE.Values)
                        {
                            if (data.ItemMethodStroke.Count >0 && data.SelectedItemImageSource !="")
                            {
                                this.SelectFeelingNextButton.Visibility = Visibility.Visible;
                            }
                        }
                        
                    }
                    else
                    {
                        this.ITEM_METHOD_TEXT[this.InputDictionaryKey].ItemMethodText = this.InputTextBox.Text;
                        //ItemsControlのDatatemplate内のViewTextBlockにテキストを代入
                        ((Grid)(this.InputItemMethodItemsControl.GetChildren<Button>().ToList()[0].Content)).Children.OfType<TextBlock>().ToList()[0].Text = this.ITEM_METHOD_TEXT[this.InputDictionaryKey].ItemMethodText;
                        this.CloseOSK();

                        this.SelectFeelingCompleteButton.Visibility = Visibility.Hidden;
                        foreach (SelectedItemMethodTextData data in this.ITEM_METHOD_TEXT.Values)
                        {
                            if (data.ItemMethodText !="" && data.SelectedItemImageSource.ToString() != "")
                            {
                                this.SelectFeelingNextButton.Visibility = Visibility.Visible;
                            }
                        }
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                }
                else if (button.Name == "HintCheckButton")
                {
                    switch (this.scene)
                    {
                        case "チャレンジタイム！　パート①":
                            this.GoTo("check_akamaru_situation","sub");
                            break;

                        case "チャレンジタイム！　パート②":
                        case "グループアクティビティ":
                            this.GoTo("check_tips_for_thinking","sub");
                            break;
                    }
                }
                else if(Regex.IsMatch(button.Name, "SelectSceneButton."))
                {
                    foreach(var Text in ((Grid)button.Content).Children.OfType<TextBlock>())
                    {
                        this.SelectedSceneTitle.Text = Text.Text;
                        this.dataChapter12.SelectedSceneText = Text.Text;
                        
                    }

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataChapter12 SET SelectedSceneText = '{this.dataChapter12.SelectedSceneText}' WHERE CreatedAt = '{this.dataChapter12.CreatedAt}';");
                    }

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }
                else if(Regex.IsMatch(button.Name, "Item.+MainButton"))
                {
                    Match match = Regex.Match(button.Name, "[0-9][0-9]");
                    this.ItemReviewImage.Source = new BitmapImage(new Uri($"Images/item{match}_review.png", UriKind.Relative));

                    this.ToChallengeTimeButton.Visibility = Visibility.Hidden;
                    this.ToGroupeActivityButton.Visibility = Visibility.Hidden;
                    this.ItemMainGrid.Visibility = Visibility.Hidden;
                    this.ItemBookTitleTextBlock.Visibility = Visibility.Hidden;
                    this.ItemReviewImage.Visibility = Visibility.Visible;
                    this.ReturnButton.Visibility = Visibility.Visible;

                    this.isClickable = true;
                }
                else if (Regex.IsMatch(button.Name, "CheckItemBook.+Button"))
                {
                    if (this.scene =="アイテムの総復習①"|| this.scene == "アイテムの総復習②")
                    {
                        this.GoTo("check_item_book_challenge_time","sub");
                    }
                    else if(this.scene == "グループアクティビティ")
                    {
                        this.GoTo("check_item_book_groupe_activity","sub");
                    }

                }
                else if (button.Name == "ToChallengeTimeButton")
                {
                    switch (this.scene)
                    {
                        case"アイテムの総復習①":
                            this.GoTo("item_question1","sub");
                            break;

                        case "アイテムの総復習②":
                            this.GoTo("item_question2","sub");
                            break;
                    }
                }
                else if(button.Name== "ToGroupeActivityButton")
                {
                    if(this.scene == "グループアクティビティ")
                    {
                        this.GoTo("groupe_activity","sub");
                    }
                }
                else if (button.Name == "ReturnButton")
                {
                    if (this.scene == "アイテムの総復習①" || this.scene == "アイテムの総復習②")
                    {
                        this.ToChallengeTimeButton.Visibility = Visibility.Visible;
                    }
                    else if (this.scene == "グループアクティビティ")
                    {
                        this.ToGroupeActivityButton.Visibility = Visibility.Visible;
                    }
                    this.ItemMainGrid.Visibility = Visibility.Visible;
                    this.ItemBookTitleTextBlock.Visibility = Visibility.Visible;
                    this.ItemReviewImage.Visibility = Visibility.Hidden;
                    this.ReturnButton.Visibility = Visibility.Hidden;

                    this.isClickable = true;
                }
                else if(button.Name == "NextItemMethodButton")
                {
                    string itemTitle = "";

                    var itemMethodDatas = this.InputItemMethodItemsControl.Items[0];

                    if (itemMethodDatas is SelectedItemMethodStrokeData)
                    {
                        itemTitle = ((SelectedItemMethodStrokeData)itemMethodDatas).SelectedItemTitle;
                    }
                    else if (itemMethodDatas is SelectedItemMethodTextData)
                    {
                        itemTitle = ((SelectedItemMethodTextData)itemMethodDatas).SelectedItemTitle;
                    }
                    switch (itemTitle)
                    {
                        case "発明品①":
                            this.InputDictionaryKey = "item_method2";
                            break;
                        case "発明品②":
                            this.InputDictionaryKey = "item_method3";
                            break;
                    }

                    if (this.dataOption.InputMethod == 0)
                    {
                        ObservableCollection<SelectedItemMethodStrokeData> datas = new ObservableCollection<SelectedItemMethodStrokeData>();
                        datas.Add(this.ITEM_METHOD_STROKE[this.InputDictionaryKey]);

                        this.InputItemMethodItemsControl.ItemsSource = datas;

                    }
                    else
                    {
                        ObservableCollection<SelectedItemMethodTextData> datas = new ObservableCollection<SelectedItemMethodTextData>();
                        datas.Add(this.ITEM_METHOD_TEXT[this.InputDictionaryKey]);

                        this.InputItemMethodItemsControl.ItemsSource = datas;
                    }

                    if (this.InputDictionaryKey == "item_method2")
                    {
                        this.BackItemMethodButton.Visibility = Visibility.Visible;
                    }
                    else if(this.InputDictionaryKey == "item_method3")
                    {
                        this.NextItemMethodButton.Visibility = Visibility.Hidden;
                    }
                    this.isClickable = true;
                }
                else if(button.Name== "BackItemMethodButton")
                {
                    string itemTitle = "";

                    var itemMethodDatas = this.InputItemMethodItemsControl.Items[0];

                    if (itemMethodDatas is SelectedItemMethodStrokeData)
                    {
                        itemTitle = ((SelectedItemMethodStrokeData)itemMethodDatas).SelectedItemTitle;
                    }
                    else if (itemMethodDatas is SelectedItemMethodTextData)
                    {
                        itemTitle = ((SelectedItemMethodTextData)itemMethodDatas).SelectedItemTitle;
                    }
                    switch (itemTitle)
                    {
                        case "発明品②":
                            this.InputDictionaryKey = "item_method1";
                            break;
                        case "発明品③":
                             this.InputDictionaryKey = "item_method2";
                            break;
                    }

                    if (this.dataOption.InputMethod == 0)
                    {
                        ObservableCollection<SelectedItemMethodStrokeData> datas = new ObservableCollection<SelectedItemMethodStrokeData>();
                        datas.Add(this.ITEM_METHOD_STROKE[this.InputDictionaryKey]);

                        this.InputItemMethodItemsControl.ItemsSource = datas;

                    }
                    else
                    {
                        ObservableCollection<SelectedItemMethodTextData> datas = new ObservableCollection<SelectedItemMethodTextData>();
                        datas.Add(this.ITEM_METHOD_TEXT[this.InputDictionaryKey]);

                        this.InputItemMethodItemsControl.ItemsSource = datas;
                    }

                    if (this.InputDictionaryKey == "item_method2")
                    {
                        this.NextItemMethodButton.Visibility = Visibility.Visible;
                    }
                    else if (this.InputDictionaryKey == "item_method1")
                    {
                        this.BackItemMethodButton.Visibility = Visibility.Hidden;
                    }
                    this.isClickable = true;
                }
                else if(button.Name == "InputButton")
                {
                    if(this.dataOption.InputMethod == 0)
                    {
                        this.ITEM_METHOD_STROKE[this.InputDictionaryKey].ItemMethodStroke = ((Grid)(this.InputItemMethodItemsControl.GetChildren<Button>().ToList()[0].Content)).Children.OfType<InkCanvas>().ToList()[0].Strokes;
                        this.InputCanvas.Strokes = this.ITEM_METHOD_STROKE[this.InputDictionaryKey].ItemMethodStroke;
                        this.GoTo("canvas_input_item_method","sub");
                    }
                    else
                    {
                        this.InputTextBox.Text = this.ITEM_METHOD_TEXT[this.InputDictionaryKey].ItemMethodText;
                        this.GoTo("keyboard_input_item_method","sub");
                        this.InputTextBox.Focus();
                    }
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

        // ハートゲージの角度をデータバインド
        private static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(Chapter12), new UIPropertyMetadata(0.0));

        private double Angle
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

            }
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

        private void GoTo(string tag, string tagType)
        {
            if (tagType == "sub")
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
            else if (tagType == "scene")
            {
                foreach (var (scenario, index) in this.scenarios.Indexed())
                {
                    if (scenario[0] == "scene" && scenario[1] == tag)
                    {
                        this.scenarioCount = index;
                        this.ScenarioPlay();

                        break;
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
        // TextBoxにフォーカスが当たったときに起動
        private void TriggerKeyboard(object sender, RoutedEventArgs e)
        {
            this.ReadyKeyboard();
        }

        private void TextBoxMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.ReadyKeyboard();
        }

        // OSKを完全に切ってしまう
        private void CloseOSK()
        {
            #region
            if (OnScreenKeyboard.IsOpened())
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
            #endregion
        }

        private void ReadyKeyboard()
        {
            #region
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
            #endregion
        }

        private void TextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox text = sender as TextBox;

            int caretPosition = text.SelectionStart;

            int MaxLine = 0;

            switch (text.Name)
            {

                case "InputTextBox":
                    MaxLine = 6;
                    break;
            }


            while (text.LineCount > MaxLine)
            {
                caretPosition -= 1;
                text.Text = text.Text.Remove(caretPosition, 1);
            }

            text.Select(caretPosition, 0);
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            TextBox text = sender as TextBox;

            int MaxLine = 0;

            switch (text.Name)
            {

                case "InputTextBox":
                    MaxLine = 6;
                    break;
            }

            if (e.Key == Key.OemComma)
            {
                e.Handled = true;
            }
            if (text.LineCount >= MaxLine)
            {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                }
            }
        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem listBoxItem = sender as ListBoxItem;

            switch (listBoxItem.Name)
            {
                case "PenButton":
                   this.InputCanvas.EditingMode = InkCanvasEditingMode.Ink;


                    break;

                case "EraserButton":
                   this.InputCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;

                    break;

                case "AllClearButton":
                   this.InputCanvas.Strokes.Clear();

                    break;
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image image = sender as Image;

            image.Visibility = Visibility.Hidden;

            string imagePath = Regex.Match(image.Source.ToString(),"Images/item.+_solo.png").ToString();

            if (this.dataOption.InputMethod == 0)
            {
                foreach (Image image1 in this.SelectItemImageGrid.Children.OfType<Image>())
                {
                    if (Regex.Match(image1.Source.ToString(), "Images/item.+_solo.png").ToString() == this.ITEM_METHOD_STROKE[this.InputDictionaryKey].SelectedItemImageSource)
                    {
                        image1.Visibility = Visibility.Visible;
                    }
                }

                this.ITEM_METHOD_STROKE[this.InputDictionaryKey].SelectedItemImageSource = imagePath;
                ObservableCollection<SelectedItemMethodStrokeData> datas = new ObservableCollection<SelectedItemMethodStrokeData>();
                datas.Add(this.ITEM_METHOD_STROKE[this.InputDictionaryKey]);

                this.InputItemMethodItemsControl.ItemsSource = datas;

                this.SelectFeelingCompleteButton.Visibility = Visibility.Hidden;
                foreach (SelectedItemMethodStrokeData data in this.ITEM_METHOD_STROKE.Values)
                {
                    if(data.ItemMethodStroke.Count > 0)
                    {
                        this.SelectFeelingNextButton.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                foreach (Image image1 in this.SelectItemImageGrid.Children.OfType<Image>())
                {
                    if (Regex.Match(image1.Source.ToString(), "Images/item.+_solo.png").ToString() == this.ITEM_METHOD_TEXT[this.InputDictionaryKey].SelectedItemImageSource)
                    {
                        image1.Visibility = Visibility.Visible;
                    }
                }

                this.ITEM_METHOD_TEXT[this.InputDictionaryKey].SelectedItemImageSource = imagePath;
                ObservableCollection<SelectedItemMethodTextData> datas = new ObservableCollection<SelectedItemMethodTextData>();
                datas.Add(this.ITEM_METHOD_TEXT[this.InputDictionaryKey]);

                this.InputItemMethodItemsControl.ItemsSource = datas;

                this.SelectFeelingCompleteButton.Visibility = Visibility.Hidden;
                foreach (SelectedItemMethodTextData data in this.ITEM_METHOD_TEXT.Values)
                {
                    if (data.ItemMethodText !="")
                    {
                        this.SelectFeelingNextButton.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private class ITEM_VALUESData
        {
            public ITEM_VALUESData(string itemName, string itemImageSource, string itemReviewText, string isCorrectItemName, string isCorrectItemReview)
            {
                ItemName = itemName;
                ItemImageSource = itemImageSource;
                ItemReviewText = itemReviewText;

                if (isCorrectItemName == "0")
                {
                    IsCorrectItemName = false;
                }
                else
                {
                    IsCorrectItemName = true;
                }
                if (isCorrectItemReview == "0")
                {
                    IsCorrectItemReview = false;
                }
                else
                {
                    IsCorrectItemReview = true;
                }

            }
            string ItemName { get; set; }
            string ItemImageSource { get; set; }
            string ItemReviewText { get; set; }
            bool IsCorrectItemName { get; set; }
            bool IsCorrectItemReview { get; set; }
        }

        private class SelectedItemMethodTextData
        {
            public SelectedItemMethodTextData(string selectedItemTitle, string selectedItemImageSource, string itemMethodText)
            {
                SelectedItemTitle = selectedItemTitle;
                SelectedItemImageSource = selectedItemImageSource;
                ItemMethodText = itemMethodText;
            }
            public string SelectedItemTitle { get; set; }
            public string SelectedItemImageSource { get; set; }
            public string ItemMethodText { get; set; }
        }


        private class SelectedItemMethodStrokeData
        {
            public SelectedItemMethodStrokeData(string selectedItemTitle, string selectedItemImageSource, StrokeCollection itemMethodStroke)
            {
                SelectedItemTitle = selectedItemTitle;
                SelectedItemImageSource = selectedItemImageSource;
                ItemMethodStroke = itemMethodStroke;
            }
            public string SelectedItemTitle { get; set; }
            public string SelectedItemImageSource {get;set;}
            public StrokeCollection ItemMethodStroke { get; set; }
        }

        // マウス押下中フラグ
        private bool _isMouseDown;

        // マウスの移動が開始されたときの座標
        private Point _startPoint;

        // ドラッグしたオブジェクトの初期位置
        private Point _initialPoint;

        // マウスの現在位置座標
        private Point _currentPoint;

        private FrameworkElement dragObject;

        // マウス移動イベントのイベントハンドラ
        private void OperationArea_MouseMove(object sender, MouseEventArgs e)
        {

            if (!this._isMouseDown || dragObject ==null)
            {
                return;
            }
            Point _dragPoint = this.dragObject.TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid);
            _currentPoint = e.GetPosition(this.MainGrid);

            double offsetX = (_currentPoint.X - _startPoint.X);
            double offsetY = (_currentPoint.Y - _startPoint.Y);

            Matrix matrix = (this.dragObject.RenderTransform as MatrixTransform).Matrix;
            matrix.Translate(offsetX, offsetY);

            this.dragObject.RenderTransform = new MatrixTransform(matrix);

            _startPoint = this._currentPoint;
        }

        private void OperationArea_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this._isMouseDown && this.dragObject!=null)
            {
                Point _leavePoint = this.dragObject.TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid);

                double offsetX = _initialPoint.X - _leavePoint.X;
                double offsetY = _initialPoint.Y - _leavePoint.Y;

                Matrix matrix = (this.dragObject.RenderTransform as MatrixTransform).Matrix;
                matrix.Translate(offsetX, offsetY);
                this.dragObject.RenderTransform = new MatrixTransform(matrix);

                this.dragObject = null;
            }

            this.isMouseDown = false;
            e.Handled = true;
        }

        private void OperationArea_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.isMouseDown)
            {
                Point _dropPoint = this.dragObject.TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid);
                Point _questionPanelPoint = this.ItemQuestionStackPanel.TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid);

                double offsetX = _initialPoint.X - _dropPoint.X;
                double offsetY = _initialPoint.Y - _dropPoint.Y;

                Matrix matrix = (this.dragObject.RenderTransform as MatrixTransform).Matrix;
                matrix.Translate(offsetX, offsetY);
                this.dragObject.RenderTransform = new MatrixTransform(matrix);
            }
            this.isMouseDown = false;
        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.dragObject = sender as FrameworkElement;

            Panel.SetZIndex(this.dragObject, 0);

            //// マウス押下中フラグを上げる
            _isMouseDown = true;

            this._startPoint = e.GetPosition(this.MainGrid);
            this._initialPoint = this.dragObject.TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid);

            // イベントを処理済みとする（当イベントがこの先伝搬されるのを止めるため）
            e.Handled = true;
        }

        private void Button_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // マウス押下中フラグを落とす
            if (this._isMouseDown && this.dragObject !=null)
            {
                Panel.SetZIndex(this.dragObject,Panel.GetZIndex(this.ItemNameButtonGrid)-1);
                //
                Point _dropPoint = this.dragObject.TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid);
                Point _questionPanelPoint = this.ItemQuestionStackPanel.TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid);

                if (((double)_questionPanelPoint.Y + this.ItemQuestionStackPanel.Height) > e.GetPosition(this.MainGrid).Y)//_dropPoint.Y + dragObject.Height / 2
                {
                    bool isLoopFinish = false;

                    if (Regex.IsMatch(dragObject.Name, "ItemNameButton0."))
                    {
                        foreach (var grid in this.ItemQuestionStackPanel.Children.OfType<Grid>())
                        {
                            if (isLoopFinish)
                                break;

                            Point _gridPoint = grid.TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid);
                            if (_gridPoint.X + grid.Width > e.GetPosition(this.MainGrid).X)//_dropPoint.X + this.dragObject.Width
                            {
                                //ドロップした枠内に既にItemNameButtonがあった場合入れ替える
                                foreach (Button button in ItemNameButtonGrid.Children)
                                {
                                    Point _buttonPoint = button.TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid);
                                    if (((double)_questionPanelPoint.Y + this.ItemQuestionStackPanel.Height) > _buttonPoint.Y + button.Height / 2)
                                    {
                                        if (_gridPoint.X + grid.Width > _buttonPoint.X + button.Width && _gridPoint.X < _buttonPoint.X)
                                        {

                                            if(this.dragObject.Name != button.Name)
                                            {
                                                double offsetX = _initialPoint.X - _buttonPoint.X;
                                                double offsetY = _initialPoint.Y - _buttonPoint.Y;

                                                Matrix matrix1 = (button.RenderTransform as MatrixTransform).Matrix;
                                                matrix1.Translate(offsetX, offsetY);
                                                button.RenderTransform = new MatrixTransform(matrix1);
                                               
                                            }
                                        }
                                    }
                                }
                                //特定の枠内でドロップすると特定の位置にItemNameButtonをはめる
                                foreach (var border in grid.Children)
                                {
                                    if (border is Border && Regex.IsMatch(((Border)border).Name, "ItemNameBorder.+"))
                                    {
                                        Point _borderPoint = ((Border)border).TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid);

                                        double offsetX = _borderPoint.X - _dropPoint.X;
                                        double offsetY = _borderPoint.Y - _dropPoint.Y;

                                        Matrix matrix1 = (this.dragObject.RenderTransform as MatrixTransform).Matrix;
                                        matrix1.Translate(offsetX, offsetY);
                                        this.dragObject.RenderTransform = new MatrixTransform(matrix1);

                                        isLoopFinish = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if(Regex.IsMatch(dragObject.Name, "ItemReviewButton0."))
                    {
                        foreach (var grid in this.ItemQuestionStackPanel.Children.OfType<Grid>())
                        {
                            if (isLoopFinish)
                                break;

                            Point _gridPoint = grid.TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid);
                            if (_gridPoint.X + grid.Width > e.GetPosition(this.MainGrid).X)//_dropPoint.X + this.dragObject.Width
                            {
                                //ドロップした枠内に既にItemNameButtonがあった場合入れ替える
                                foreach (Button button in ItemReviewButtonGrid.Children)
                                {
                                    Point _buttonPoint = button.TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid);
                                    if (((double)_questionPanelPoint.Y + this.ItemQuestionStackPanel.Height) > _buttonPoint.Y + button.Height / 2)
                                    {
                                        if (_gridPoint.X + grid.Width > _buttonPoint.X + button.Width && _gridPoint.X < _buttonPoint.X)
                                        {

                                            if (this.dragObject.Name != button.Name)
                                            {
                                                double offsetX = _initialPoint.X - _buttonPoint.X;
                                                double offsetY = _initialPoint.Y - _buttonPoint.Y;

                                                Matrix matrix1 = (button.RenderTransform as MatrixTransform).Matrix;
                                                matrix1.Translate(offsetX, offsetY);
                                                button.RenderTransform = new MatrixTransform(matrix1);

                                            }


                                        }
                                    }
                                }
                                //特定の枠内でドロップすると特定の位置にItemNameButtonをはめる
                                foreach (var border in grid.Children)
                                {
                                    if (border is Border && Regex.IsMatch(((Border)border).Name, "ItemReviewBorder.+"))
                                    {
                                        Point _borderPoint = ((Border)border).TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid);

                                        double offsetX = _borderPoint.X - _dropPoint.X;
                                        double offsetY = _borderPoint.Y - _dropPoint.Y;

                                        Matrix matrix1 = (this.dragObject.RenderTransform as MatrixTransform).Matrix;
                                        matrix1.Translate(offsetX, offsetY);
                                        this.dragObject.RenderTransform = new MatrixTransform(matrix1);

                                        isLoopFinish = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    double offsetX = _initialPoint.X - _dropPoint.X;
                    double offsetY = _initialPoint.Y - _dropPoint.Y;

                    Matrix matrix = (this.dragObject.RenderTransform as MatrixTransform).Matrix;
                    matrix.Translate(offsetX, offsetY);
                    this.dragObject.RenderTransform = new MatrixTransform(matrix);
                }


            }
            _isMouseDown = false;
            e.Handled = true;
        }

        private void PlaceInitialPoint()
        {
            foreach (Button button in this.ItemNameButtonGrid.Children)
            {
                if (button.Visibility == Visibility.Visible)
                {
                    Transform transform = button.RenderTransform;
                    Matrix matrix = (button.RenderTransform as MatrixTransform).Matrix;
                    matrix.Translate(-transform.Value.OffsetX, -transform.Value.OffsetY);
                    button.RenderTransform = new MatrixTransform(matrix);
                }
            }
            foreach (Button button in this.ItemReviewButtonGrid.Children)
            {
                if (button.Visibility == Visibility.Visible)
                {
                    Transform transform = button.RenderTransform;
                    Matrix matrix = (button.RenderTransform as MatrixTransform).Matrix;
                    matrix.Translate(-transform.Value.OffsetX, -transform.Value.OffsetY);
                    button.RenderTransform = new MatrixTransform(matrix);
                }
            }
        }

        private bool CheckAnswer(string scene)
        {
            bool iscorrect = true;

            foreach(Button button in this.ItemNameButtonGrid.Children)
            {
                Point point;
                if (this.ItemQuestionStackPanel.TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid).Y + this.ItemQuestionStackPanel.Height > button.TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid).Y + button.Height)
                {
                    if(button.Visibility == Visibility.Visible)
                    {
                        point = button.TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid);

                        int StorageNumber = (int)Math.Floor((point.X + button.Width) / this.ItemQuestionGrid01.Width);

                        if (scene == "second")
                            StorageNumber += 6;

                        if ((string)button.Content == this.ITEM_VALUES.Keys.ToList()[StorageNumber])
                        {
                            this.ITEM_VALUES[(string)button.Content][2] = "1";
                        }
                        else if ((string)button.Content != this.ITEM_VALUES.Keys.ToList()[StorageNumber])
                        {
                            this.ITEM_VALUES.Values.ToList()[StorageNumber][2] = "0";

                            iscorrect = false;
                        }
                    }
                }
                else
                {
                    if(scene == "second" && button.Name =="ItemNameButton06")
                    {

                    }
                    else
                    {
                        iscorrect = false;
                    }
                }
            }
            foreach (Button button in this.ItemReviewButtonGrid.Children)
            {
                Point point;
                if (this.ItemQuestionStackPanel.TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid).Y+this.ItemQuestionStackPanel.Height > button.TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid).Y + button.Height)
                {
                    if(button.Visibility == Visibility.Visible)
                    {
                        point = button.TranslatePoint(new Point(0.0d, 0.0d), this.MainGrid);

                        int StorageNumber = (int)Math.Floor((point.X + button.Width) / this.ItemQuestionGrid01.Width);

                        if (scene == "second" && StorageNumber < 5)
                            StorageNumber += 6;


                        if ((string)button.Content == this.ITEM_VALUES.Values.ToList()[StorageNumber][1])
                        {
                            this.ITEM_VALUES.Values.ToList()[StorageNumber][3] = "1";
                        }
                        else if ((string)button.Content != this.ITEM_VALUES.Values.ToList()[StorageNumber][1])
                        {
                            this.ITEM_VALUES.Values.ToList()[StorageNumber][3] = "0";
                            iscorrect = false;
                        }
                    }
                }
                else
                {
                    if (scene == "second" && button.Name == "ItemReviewButton06")
                    {

                    }
                    else
                    {
                        iscorrect = false;
                    }
                }
            }

            return iscorrect;
        }

        // マウスのドラッグ処理（マウスの左ボタンを押そうとしたとき）
        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string objName = "None";

            if (e.Source as FrameworkElement != null)
            {
                objName = (e.Source as FrameworkElement).Name;
            }

            logManager.SaveLog(objName, Mouse.GetPosition(this).X.ToString(), Mouse.GetPosition(this).Y.ToString(), this.isClickable.ToString());
        }


        private void SetData()
        {
            string dirPath = $"./Log/{this.initConfig.userName}/Chapter12";
            LoadManager loadManager = new LoadManager();
            loadManager.LoadDataChapterFromDB(this.dataChapter12, this.initConfig.dbPath);

            //ToDo 入力方法の記録
            foreach(var data in this.ITEM_METHOD_STROKE.Values.Select((Value,Index)=>new { Value,Index}))
            {
                PropertyInfo propertyInfo = typeof(DataChapter12).GetProperty($"SelectedItem{data.Index+1}");
                loadManager.ToStroke(data.Value.ItemMethodStroke, $"groupe_activity_item_method_stroke{data.Index+1}");
                data.Value.SelectedItemImageSource = (string)propertyInfo.GetValue(this.dataChapter12);
            }

            foreach (var data in this.ITEM_METHOD_TEXT.Values.Select((Value, Index) => new { Value, Index }))
            {
                PropertyInfo propertyInfo1 = typeof(Chapter12).GetProperty($"ItemMethodInputText{data.Index + 1}");
                PropertyInfo propertyInfo2 = typeof(DataChapter12).GetProperty($"SelectedItem{data.Index + 1}");
                data.Value.ItemMethodText = (string)propertyInfo2.GetValue(this.dataChapter12);
                data.Value.SelectedItemImageSource = (string)propertyInfo2.GetValue(this.dataChapter12);
            }
        }
    }
}