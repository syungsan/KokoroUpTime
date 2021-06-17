using CsvReadWrite;
using Expansion;
using FileIOUtils;
using Osklib;
using OutlineTextMaker;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
    public partial class Chapter9 : Page
    {

        //タイトル表示用のテキスト
        private string[] GOOD_FEELINGS = { "●　うれしい", "●　しあわせ", "●　たのしい", "●　ホッとした", "●　きもちいい", "●　まんぞく", "●　すき", "●　やる気マンマン", "●　かんしゃ", "●　わくわく", "●　うきうき", "●　ほこらしい" };
        private string[] BAD_FEELINGS = { "●　心配", "●　こまった", "●　不安", "●　こわい", "●　おちこみ", "●　がっかり", "●　いかり", "●　イライラ", "●　はずかしい", "●　ふまん", "●　かなしい", "●　おびえる" };

        private Dictionary<string, List<string>> NOT_GOOD_EVENT;
        private float THREE_SECOND_RULE_TIME = 3.0f;

        LogManager logManager;

        // ゲームを進行させるシナリオ
        private int scenarioCount = 0;
        private List<List<string>> scenarios = null;

        // 各種コントロールの名前を収める変数
        private string position = "";

        // マウスクリックを可能にするかどうかのフラグ
        private bool isClickable = false;

        //アニメーションを表示させるか否か
        private bool isAnimationSkip = false;

        // 気持ちの大きさ
        private int feelingSize = 0;

        // マウス押下中フラグ
        private bool isMouseDown = false;

        // メッセージ表示関連
        private DispatcherTimer msgTimer;
        private int word_num;

        private int inlineCount;
        private int imageInlineCount;

        //きもちの種類と大きさのテキストを辞書から呼ぶためのKey
        private string FeelingDictionaryKey = "";

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
        private DataChapter9 dataChapter9;
        //データモデルのプロパティのリスト
        private Dictionary<string, PropertyInfo> KindOfFeelings = null;
        private Dictionary<string, PropertyInfo> SizeOfFeelings = null;
        
        // ゲームの切り替えシーン
        private string scene;

        // データベースに収めるデータモデルのインスタンス
        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();
        public DataProgress dataProgress = new DataProgress();

        //ItemsSource用のデータコレクション
        private ObservableCollection<AosukeSituationTextData> _aosukeSituationTextData = new ObservableCollection<AosukeSituationTextData>();
        private ObservableCollection<NotGoodEventData> _notGoodEventData = new ObservableCollection<NotGoodEventData>();

        //入力したStroke保存用
        private StrokeCollection InputChallengeCanvasStrokes = new StrokeCollection();

        public Chapter9()
        {
            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            // メディアプレーヤークラスのインスタンスを作成する
            this.mediaPlayer = new WindowsMediaPlayer();


            // データモデルインスタンス確保
            this.dataChapter9 = new DataChapter9();

            // マウスイベントの設定
            this.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
            this.MouseUp += new MouseButtonEventHandler(OnMouseUp);
            this.MouseMove += new MouseEventHandler(OnMouseMove);
            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(OnPreviewMouseLeftButtonDown);

            logManager = new LogManager();

            this.SelectGoodFeelingListBox.ItemsSource = GOOD_FEELINGS;
            this.SelectBadFeelingListBox.ItemsSource = BAD_FEELINGS;

            NOT_GOOD_EVENT = new Dictionary<string, List<string>>()
            {
                ["学校でのこと"] = new List<string>() { "学校に行く", "教室に入る", "移動教室に行く", "苦手な教科の授業", "班（グループ）活動", "テストを受ける", "その他の学校でのこと" },
                ["友だちとのこと"] =  new List<string>() { "友だちに話しかける", "知らない友だちと仲良くなる", "友だち同士の会話に入っていく", "自分からあいさつする", "友だちに意見を言う", "その他の友だちとの関係" },
                ["場所"] =  new List<string>() { "高いところ", "人がたくさんいるところ", "せまいところ", "暗いところ", "うるさいところ", "その他の場所" },
                ["動物、虫、自然"] =  new List<string>() { "犬", "ネコ", "ハチ", "ムカデ", "ヘビ", "その他の虫" },
                ["大きな音"] =  new List<string>() { "花火", "トイレのエアータオル", "雷", "その他の音" },
                ["家でのこと"] =  new List<string>() { "一人で留守番する", "一人で別の部屋にいる\n（二階など）", "お泊りにいく", "その他の家でのこと" },
                ["大人とのこと"] = new  List<string>() { "先生に質問する", "店員さんに注文する", "レジで買い物をする", "電話に出る", "親の友だちと話す", "その他の大人との関係" },
                ["発表"] =  new List<string>() { "手を挙げて発表する", "自分の意見を言う", "リコーダーや歌のテスト", "体育のテスト", "日直", "音読", "その他の発表" },
            };


            _aosukeSituationTextData.Add(new AosukeSituationTextData("", "学校の休み時間、ふだんしゃべらない友だちに\n自分から話しかけてみた"));
            _aosukeSituationTextData.Add(new AosukeSituationTextData("その１０分後、", "\nいっしょに話していると、同じテレビ番組が好きだとわかった。"));
            _aosukeSituationTextData.Add(new AosukeSituationTextData("次の日、", "\nまた、その友だちに青助くんから話しかけに行った。"));
            _aosukeSituationTextData.Add(new AosukeSituationTextData("次の次の日、", "\nその友だちとはちがうほかの友だちに自分から話しかけてみた。"));

           
            foreach(KeyValuePair<string, List<string>> keyValuePair in this.NOT_GOOD_EVENT)
            {
                _notGoodEventData.Add(new NotGoodEventData(keyValuePair.Key, keyValuePair.Value));
            }

            //_notGoodEventData.Add(new NotGoodEventData("学校でのこと", new List<string>() { "学校に行く", "教室に入る", "移動教室に行く", "苦手な教科の授業", "班（グループ）活動", "テストを受ける", "その他の学校でのこと" }));
            //_notGoodEventData.Add(new NotGoodEventData("友だちとのこと", new List<string>() { "友だちに話しかける", "知らない友だちと仲良くなる", "友だち同士の会話に入っていく", "自分からあいさつする", "友だちに意見を言う", "その他の友だちとの関係" }));
            //_notGoodEventData.Add(new NotGoodEventData("場所", new List<string>() { "高いところ", "人がたくさんいるところ", "せまいところ", "暗いところ", "うるさいところ", "その他の場所" }));
            //_notGoodEventData.Add(new NotGoodEventData("動物、虫、自然", new List<string>() { "犬", "ネコ", "ハチ", "ムカデ", "ヘビ", "その他の虫" }));
            //_notGoodEventData.Add(new NotGoodEventData("大きな音", new List<string>() { "花火", "トイレのエアータオル", "雷", "その他の音" }));
            //_notGoodEventData.Add(new NotGoodEventData("家でのこと", new List<string>() { "一人で留守番する", "一人で別の部屋にいる\n（二階など）", "お泊りにいく", "その他の家でのこと" }));
            //_notGoodEventData.Add(new NotGoodEventData("大人とのこと", new List<string>() { "先生に質問する", "店員さんに注文する", "レジで買い物をする", "電話に出る", "親の友だちと話す", "その他の大人との関係" }));
            //_notGoodEventData.Add(new NotGoodEventData("発表", new List<string>() { "手を挙げて発表する", "自分の意見を言う", "リコーダーや歌のテスト", "体育のテスト", "日直", "音読", "その他の発表" }));

            this.AosukeSituationItemControl.ItemsSource = _aosukeSituationTextData;
            this.NotGoodEventItemsControl.ItemsSource = _notGoodEventData;

            KindOfFeelings = new Dictionary<string, PropertyInfo>()
            {
                ["aosuke_feeling1"] = typeof(DataChapter9).GetProperty("AosukesKindOfFeelingTalkToFriends"),
                ["aosuke_feeling2"] = typeof(DataChapter9).GetProperty("AosukesKindOfFeelingAfter10minutes"),
                ["aosuke_feeling3"] = typeof(DataChapter9).GetProperty("AosukesKindOfFeelingNextDay"),
                ["aosuke_feeling4"] = typeof(DataChapter9).GetProperty("AosukesKindOfFeelingDayAfterTomorrow"),
            };

            SizeOfFeelings = new Dictionary<string, PropertyInfo>()
            {
                ["aosuke_feeling1"] = typeof(DataChapter9).GetProperty("AosukesSizeOfFeelingTalkToFriends"),
                ["aosuke_feeling2"] = typeof(DataChapter9).GetProperty("AosukesSizeOfFeelingAfter10minutes"),
                ["aosuke_feeling3"] = typeof(DataChapter9).GetProperty("AosukesSizeOfFeelingNextDay"),
                ["aosuke_feeling4"] = typeof(DataChapter9).GetProperty("AosukesSizeOfFeelingDayAfterTomorrow"),
            };
            this.dataChapter9.InputChallengeText = "";

            this.InitControls();
        }

        private void InitControls()
        {
            // 各種コントロールに任意の文字列をあてがう

            this.imageObjects = new Dictionary<string, Image>
            {
                ["bg_image"] = this.BackgroundImage,
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
                ["cover_layer_image"]=this.CoverLayerImage,

                ["main_msg_bubble_image"] = this.MainMessageBubbleImage,
                ["challenge_pre_use_item_image"] =this.ChallengePreUseItemImage,
                ["akamaru_pre_use_item_image_1"] =this.AkamaruPreUseItemImage1,
                ["akamaru_pre_use_item_image_2"] = this.AkamaruPreUseItemImage2,
                ["akamarus_feeling_pre_use_item_arrow_1"] = this.AkamarusFeelingPreUseItemArrow1,
                ["akamarus_feeling_pre_use_item_arrow_2"] = this.AkamarusFeelingPreUseItemArrow2,
                ["akamarus_feeling_pre_use_item_graph_1"] = this.AkamarusFeelingPreUseItemGraph1,
                ["akamarus_feeling_pre_use_item_graph_2"] = this.AkamarusFeelingPreUseItemGraph2,

                ["challenge_after_used_item_image_1"] = this.ChallengeAfterUsedItemImage1,
                ["challenge_after_used_item_image_2"] = this.ChallengeAfterUsedItemImage2,
                ["challenge_after_used_item_image_3"] = this.ChallengeAfterUsedItemImage3,
                ["challenge_after_used_item_image_4"] = this.ChallengeAfterUsedItemImage4,
                ["akamaru_after_used_item_image_1"] = this.AkamaruAfterUsedItemImage1,
                ["item_right_up_image"] =this.ItemRightUpImage,
                ["akamarus_feeling_after_used_item_arrow_1"] = this.AkamarusFeelingAfterUsedItemArrow1,
                ["akamarus_feeling_after_used_item_arrow_2"] = this.AkamarusFeelingAfterUsedItemArrow2,
                ["akamarus_feeling_after_used_item_arrow_3"] = this.AkamarusFeelingAfterUsedItemArrow3,
                ["akamarus_feeling_after_used_item_arrow_4"] = this.AkamarusFeelingAfterUsedItemArrow4,
                ["akamarus_feeling_after_used_item_graph_1"] = this.AkamarusFeelingAfterUsedItemGraph1,
                ["akamarus_feeling_after_used_item_graph_2"] = this.AkamarusFeelingAfterUsedItemGraph2,
                ["akamarus_feeling_after_used_item_graph_3"] = this.AkamarusFeelingAfterUsedItemGraph3,
                ["akamarus_feeling_after_used_item_graph_4"] = this.AkamarusFeelingAfterUsedItemGraph4,

                ["groupe_activity_aosuke_image"] =this.GroupeActivityAosukeImage,
                ["groupe_activity_item_image"] = this.GroupeActivityItemImage,

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
                ["session_frame_text"] = this.SessionFrameText,

                ["challenge_time_title_text"] =this.ChallengeTImeTitleText,
                ["input_challenge_title_text"] =this.InputChallengeTitleText,

                ["akamarus_feeling_pre_use_item_text_1"] =this.AkamarusFeelingPreUseItemText1,
                ["akamarus_feeling_pre_use_item_text_2"] = this.AkamarusFeelingPreUseItemText2,

                ["akamarus_feeling_after_used_item_text_1"] = this.AkamarusFeelingAfterUsedItemText1,
                ["akamarus_feeling_after_used_item_text_2"] = this.AkamarusFeelingAfterUsedItemText2,

                ["aosuke_kind_of_feeling_text_1"] = this.AosukeKindOfFeelingText1,
                ["aosuke_size_of_feeling_text_1"] = this.AosukeSizeOfFeelingText1,
                ["aosuke_kind_of_feeling_text_2"] = this.AosukeKindOfFeelingText2,
                ["aosuke_size_of_feeling_text_2"] = this.AosukeSizeOfFeelingText2,
                ["aosuke_kind_of_feeling_text_3"] = this.AosukeKindOfFeelingText3,
                ["aosuke_size_of_feeling_text_3"] = this.AosukeSizeOfFeelingText3,
                ["aosuke_kind_of_feeling_text_4"] = this.AosukeKindOfFeelingText4,
                ["aosuke_size_of_feeling_text_4"] = this.AosukeSizeOfFeelingText4,

                ["input_challenge_text"] =this.InputChallengeText,
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
                ["ok_button"] = this.OkButton,
                ["return_button"] = this.ReturnButton,
                ["complete_input_button"] = this.CompleteInputButton,

                ["akamarus_kind_of_feeling_button_next_day_border"]=this.AkamarusKindOfFeelingNextDayButton,
                ["akamarus_kind_of_feeling_button_after_three_days_border"]=this.AkamarusKindOfFeelingAfterThreeDaysButton,
                ["akamarus_kind_of_feeling_button_after_four_days_border"]=this.AkamarusKindOfFeelingAfterFourdaysButton,

                ["akamarus_size_of_feeling_button_next_day_border"] = this.AkamarusSizeOfFeelingNextDayButton,
                ["akamarus_size_of_feeling_button_after_three_days_border"] = this.AkamarusSizeOfFeelingAfterThreeDaysButton,
                ["akamarus_size_of_feeling_button_after_four_days_border"] = this.AkamarusSizeOfFeelingAfterFourdaysButton,
            };

            this.gridObjects = new Dictionary<string, Grid>
            {
                ["session_grid"] = this.SessionGrid,
                ["session_frame_grid"] = this.SessionFrameGrid,
                ["manga_grid"]=this.MangaGrid,
                ["title_grid"]=this.TitleGrid,

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

                ["select_feeling_grid"]=this.SelectFeelingGrid,
                ["select_heart_grid"]=this.SelectHeartGrid,
                ["view_size_of_feeling_grid"] =this.ViewSizeOfFeelingGrid,
                ["canvas_edit_grid"] = this.CanvasEditGrid,

                ["not_good_example_grid"] =this.NotGoodExampleGrid,
                ["not_good_example_title_grid"] = this.NotGoodExampleTitleGrid,
                ["not_good_example_children_image_grid"] = this.NotGoodExampleChildrenImageGrid,
                ["not_good_example_text_grid"] = this.NotGoodExampleTextGrid,

                ["input_grid"]=this.InputGrid,
                ["input_challenge_grid"] =this.InputChallengeGrid,

                ["akamaru_after_used_item_grid"] =this.AkamarusSizeOfFeelingAfterUsedItemGrid,
                ["change_akamarus_size_of_feeling_grid"] =this.ChangeAkamarusSizeOfFeelingGrid,
                ["change_akamarus_size_of_feeling_pre_use_item_grid"]=this.ChangeAkamarusSizeOfFeelingPreUseItemGrid,
                ["change_akamarus_size_of_feeling_after_used_item_grid"] = this.ChangeAkamarusSizeOfFeelingAfterUsedItemGrid,
                ["change_aosukes_feeling_grid"] =this.ChangeAosukesFeelingGrid,
                ["bad_feeling_event_grid"] =this.BadFeelingEventGrid,
                ["bad_feeling_grid"] =this.BadFeelingGrid,

                ["change_akamarus_size_of_feeling_grid"] =this.ChangeAkamarusSizeOfFeelingGrid,
                ["change_akamarus_size_of_feeling_pre_use_item_grid"] =this.ChangeAkamarusSizeOfFeelingPreUseItemGrid,
                ["change_akamarus_size_of_feeling_after_used_item_grid"] = this.ChangeAkamarusSizeOfFeelingAfterUsedItemGrid,

                ["state_of_akamaru_grid"] =this.StateOfAkamaruGird,
                ["not_good_event_grid"] =this.NotGoodEventGrid,
                ["input_feeling_grid"]=this.InputFeelingGrid,
            };

            this.borderObjects = new Dictionary<string, Border>
            {
                ["title_border"]=this.TitleBorder,
                ["light_green_border"] =this.LightGreenBorder,

                ["state_of_akamaru_next_day_border"]=this.StateOfAkamaruNextDayBorder,
                ["state_of_akamaru_after_three_days_border"]=this.StateOfAkamaruAfterThreeDaysBorder,
                ["state_of_akamaru_after_four_days_border"] =this.StateOfAkamaruAfterFourdaysBorder,

                ["input_canvas_border"] =this.InputCanvasBorder,
                ["input_text_border"] = this.InputTextBorder,
            };

            this.outlineTextObjects = new Dictionary<string, OutlineText>
            {
                ["why_is_akamaru_not_good_at_dogs_scene_title"] = this.WhyIsAkamaruNotGoodAtdogsSceneTitle,
                ["example_scene_title"] = this.ExampleSceneTitle,
                ["challenging_scene_title"] = this.ChallengingSceneTitle,
                ["akamaru_using_item_scene_title"] = this.AkamaruUsingItemSceneTitle,
                ["aosukes_feeling_scene_title"] = this.AosukesFeelingSceneTitle,
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
            this.SessionFrameGrid.Visibility = Visibility.Hidden;
            this.SessionFrameText.Visibility = Visibility.Hidden;


            this.TitleGrid.Visibility = Visibility.Hidden;
            this.TitleBorder.Visibility = Visibility.Hidden;
            this.ChallengeTImeTitleText.Visibility = Visibility.Hidden;

            this.SelectFeelingGrid.Visibility = Visibility.Hidden;
            this.ViewSizeOfFeelingGrid.Visibility = Visibility.Hidden;
            this.SelectHeartGrid.Visibility = Visibility.Hidden;
            this.WhyIsAkamaruNotGoodAtdogsSceneTitle.Visibility = Visibility.Hidden;
            this.ExampleSceneTitle.Visibility = Visibility.Hidden;
            this.ChallengingSceneTitle.Visibility = Visibility.Hidden;
            this.AkamaruUsingItemSceneTitle.Visibility = Visibility.Hidden;
            this.AosukesFeelingSceneTitle.Visibility = Visibility.Hidden;

            this.ShirojiSmallRightCenterImage.Visibility = Visibility.Hidden;

            this.LightGreenBorder.Visibility = Visibility.Hidden;
            this.NotGoodExampleGrid.Visibility = Visibility.Hidden;
            this.NotGoodExampleTitleGrid.Visibility= Visibility.Hidden;
            this.NotGoodExampleTextGrid.Visibility= Visibility.Hidden;
            this.NotGoodExampleChildrenImageGrid.Visibility = Visibility.Hidden;
            this.InputChallengeGrid.Visibility = Visibility.Hidden;
            this.InputGrid.Visibility = Visibility.Hidden;
            this.InputCanvasBorder.Visibility = Visibility.Hidden;
            this.InputTextBorder.Visibility = Visibility.Hidden;
            this.InputGrid.Visibility = Visibility.Hidden;
            this.AkamarusSizeOfFeelingAfterUsedItemGrid.Visibility = Visibility.Hidden;
            
            this.ChangeAkamarusSizeOfFeelingGrid.Visibility = Visibility.Hidden;
            this.ChangeAkamarusSizeOfFeelingPreUseItemGrid.Visibility= Visibility.Hidden;
            this.ChallengePreUseItemImage.Visibility = Visibility.Hidden;
            this.AkamaruPreUseItemImage1.Visibility = Visibility.Hidden;
            this.AkamaruPreUseItemImage2.Visibility = Visibility.Hidden;
            this.AkamarusFeelingPreUseItemArrow1.Visibility = Visibility.Hidden;
            this.AkamarusFeelingPreUseItemArrow2.Visibility = Visibility.Hidden;
            this.AkamarusFeelingPreUseItemGraph1.Visibility = Visibility.Hidden;
            this.AkamarusFeelingPreUseItemGraph2.Visibility = Visibility.Hidden;
            this.AkamarusFeelingPreUseItemText1.Visibility = Visibility.Hidden;
            this.AkamarusFeelingPreUseItemText2.Visibility = Visibility.Hidden;

            this.ChangeAkamarusSizeOfFeelingAfterUsedItemGrid.Visibility = Visibility.Hidden;
            this.ChallengeAfterUsedItemImage1.Visibility = Visibility.Hidden;
            this.ChallengeAfterUsedItemImage2.Visibility = Visibility.Hidden;
            this.ChallengeAfterUsedItemImage3.Visibility = Visibility.Hidden;
            this.ChallengeAfterUsedItemImage4.Visibility = Visibility.Hidden;
            this.AkamaruAfterUsedItemImage1.Visibility = Visibility.Hidden;
            this.ItemRightUpImage.Visibility = Visibility.Hidden;
            this.AkamarusFeelingAfterUsedItemArrow1.Visibility = Visibility.Hidden;
            this.AkamarusFeelingAfterUsedItemArrow2.Visibility = Visibility.Hidden;
            this.AkamarusFeelingAfterUsedItemArrow3.Visibility = Visibility.Hidden;
            this.AkamarusFeelingAfterUsedItemArrow4.Visibility = Visibility.Hidden;
            this.AkamarusFeelingAfterUsedItemGraph1.Visibility = Visibility.Hidden;
            this.AkamarusFeelingAfterUsedItemGraph2.Visibility = Visibility.Hidden;
            this.AkamarusFeelingAfterUsedItemGraph3.Visibility = Visibility.Hidden;
            this.AkamarusFeelingAfterUsedItemGraph4.Visibility = Visibility.Hidden;
            this.AkamarusFeelingAfterUsedItemText1.Visibility = Visibility.Hidden;
            this.AkamarusFeelingAfterUsedItemText2.Visibility = Visibility.Hidden;

            this.NotGoodEventGrid.Visibility = Visibility.Hidden;
            this.ChangeAosukesFeelingGrid.Visibility = Visibility.Hidden;
            this.InputFeelingGrid.Visibility = Visibility.Hidden;
            this.GroupeActivityAosukeImage.Visibility = Visibility.Hidden;
            this.GroupeActivityItemImage.Visibility = Visibility.Hidden;
            this.BadFeelingEventGrid.Visibility = Visibility.Hidden;
            this.BadFeelingGrid.Visibility = Visibility.Hidden;

            this.StateOfAkamaruGird.Visibility = Visibility.Hidden;
            this.StateOfAkamaruNextDayBorder.Visibility = Visibility.Hidden;
            this.StateOfAkamaruAfterFourdaysBorder.Visibility = Visibility.Hidden;
            this.StateOfAkamaruAfterThreeDaysBorder.Visibility = Visibility.Hidden;
            this.AkamarusKindOfFeelingNextDayButton.Visibility = Visibility.Hidden;
            this.AkamarusKindOfFeelingAfterThreeDaysButton.Visibility = Visibility.Hidden;
            this.AkamarusKindOfFeelingAfterFourdaysButton.Visibility = Visibility.Hidden;
            this.AkamarusSizeOfFeelingNextDayButton.Visibility = Visibility.Hidden;
            this.AkamarusSizeOfFeelingAfterThreeDaysButton.Visibility = Visibility.Hidden;
            this.AkamarusSizeOfFeelingAfterFourdaysButton.Visibility = Visibility.Hidden;

            



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
            this.MangaGrid.Visibility = Visibility.Hidden;
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
            this.CanvasEditGrid.Visibility = Visibility.Hidden;
            this.CompleteInputButton.Visibility = Visibility.Hidden;

            this.CoverLayerImage.Visibility = Visibility.Hidden;


            this.ReturnToTitleButton.Visibility = Visibility.Hidden;


            this.OkButton.Visibility = Visibility.Hidden;

            this.SelectFeelingBackButton.Visibility = Visibility.Hidden;

            this.SessionSubTitleTextBlock.Text = "";
            this.SessionSentenceTextBlock.Text = "";

            this.EndingMessageTextBlock.Text = "";

            this.MusicTitleTextBlock.Text = "";
            this.ComposerNameTextBlock.Text = "";

        }

        private void SetInputMethod()
        {
           
            if(this.dataOption.InputMethod == 0)
            {
                this.InputChallengeText.Visibility = Visibility.Hidden;
            }
            else
            {
                this.InputChallengeCanvas.Visibility = Visibility.Hidden;
            }

            this.PenButton.IsSelected = true;
        }

        public void SetNextPage(InitConfig _initConfig, DataOption _dataOption, DataItem _dataItem, DataProgress _dataProgress, bool isCreateNewTable)
        {
            this.initConfig = _initConfig;
            this.dataOption = _dataOption;
            this.dataItem = _dataItem;
            this.dataProgress = _dataProgress;

            // 現在時刻を取得
            this.dataChapter9.CreatedAt = DateTime.Now.ToString();
            if (isCreateNewTable)
            {
                // データベースのテーブル作成と現在時刻の書き込みを同時に行う
                using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                {
                    // 毎回のアクセス日付を記録
                    connection.Insert(this.dataChapter9);
                }
            }
            else
            {
                string lastCreatedAt = "";

                using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                {
                    var chapter9 = connection.Query<DataChapter9>($"SELECT * FROM DataChapter9 ORDER BY Id DESC LIMIT 1;");

                    foreach (var row in chapter9)
                    {
                        lastCreatedAt = row.CreatedAt;
                    }
                }

                using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                {
                    connection.Execute($@"UPDATE DataChapter9 SET CreatedAt = '{this.dataChapter9.CreatedAt}'WHERE CreatedAt = '{lastCreatedAt}';");
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var csv = new CsvReader("./Scenarios/chapter9.csv"))
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

                    this.dataProgress.CurrentChapter = 9;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET CurrentChapter = '{this.dataProgress.CurrentChapter}' WHERE Id = 1;");
                    }

                    this.SetInputMethod();

                    logManager.StartLog(this.initConfig, this.dataProgress);

                    //前回のつづきからスタート
                    if (this.dataProgress.CurrentScene != null)
                    {
                        this.GoTo(this.dataProgress.CurrentScene, "scene");
                    }
                    else
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    break;

                case "end":

                    // 画面のフェードアウト処理とか入れる（別関数を呼び出す）

                    this.StopBGM();

                    this.dataProgress.HasCompletedChapter9 = true;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET HasCompletedChapter9 = '{Convert.ToInt32(this.dataProgress.HasCompletedChapter9)}' WHERE Id = 1;");
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
                    this.dataProgress.LatestChapter9Scene = this.scene;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET CurrentScene = '{this.dataProgress.CurrentScene}', LatestChapter9Scene = '{this.dataProgress.LatestChapter9Scene}' WHERE Id = 1;");
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
                            case"red":
                                borderObject.BorderBrush= (Brush)converter.ConvertFromString("#FFFF0000");
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
                        var _objectsName = this.position;
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

                //ボーダーに対しての処理
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
                                        if (this.scene == "ちょうせんしたいこと")
                                        {
                                            if (this.dataOption.InputMethod == 0)
                                            {
                                                if(this.InputChallengeCanvasStrokes.Count > 1)
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Visible;
                                                }
                                            }
                                            else
                                            {
                                                if(this.InputChallengeText.Text != "")
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Visible;
                                                }
                                            }
                                            this.BackPageButton.Visibility = Visibility.Visible;
                                        }
                                        else if (this.scene == "グループアクティビティ")
                                        {
                                                bool CompleteInputFlag = true;
                                                if (CompleteInputFlag)
                                                {
                                                    foreach (KeyValuePair<string, PropertyInfo> kvp in this.KindOfFeelings)
                                                    {
                                                        if ((string)kvp.Value.GetValue(this.dataChapter9) == "")
                                                        {
                                                            CompleteInputFlag = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                                if (CompleteInputFlag)
                                                {
                                                    foreach (KeyValuePair<string, PropertyInfo> kvp in this.SizeOfFeelings)
                                                    {
                                                        if ((int)kvp.Value.GetValue(this.dataChapter9) == -1)
                                                        {
                                                            CompleteInputFlag = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                                if (CompleteInputFlag)
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Visible;
                                                }
                                            this.BackPageButton.Visibility = Visibility.Visible;
                                        }
                                        else
                                        {
                                            this.NextPageButton.Visibility = Visibility.Visible;
                                            this.BackPageButton.Visibility = Visibility.Visible;
                                        }
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
                                    if (this.scene == "ちょうせんしたいこと")
                                    {
                                        if (this.dataOption.InputMethod == 0)
                                        {
                                            if (this.InputChallengeCanvasStrokes.Count > 1)
                                            {
                                                this.NextPageButton.Visibility = Visibility.Visible;
                                            }
                                        }
                                        else
                                        {
                                            if (this.InputChallengeText.Text != "")
                                            {
                                                this.NextPageButton.Visibility = Visibility.Visible;
                                            }
                                        }
                                        this.BackPageButton.Visibility = Visibility.Visible;
                                    }
                                    else if (this.scene == "グループアクティビティ")
                                    {
                                        bool CompleteInputFlag = true;
                                        if (CompleteInputFlag)
                                        {
                                            foreach (KeyValuePair<string, PropertyInfo> kvp in this.KindOfFeelings)
                                            {
                                                if ((string)kvp.Value.GetValue(this.dataChapter9) == "")
                                                {
                                                    CompleteInputFlag = false;
                                                    break;
                                                }
                                            }
                                        }
                                        if (CompleteInputFlag)
                                        {
                                            foreach (KeyValuePair<string, PropertyInfo> kvp in this.SizeOfFeelings)
                                            {
                                                if ((int)kvp.Value.GetValue(this.dataChapter9) == -1)
                                                {
                                                    CompleteInputFlag = false;
                                                    break;
                                                }
                                            }
                                        }
                                        if (CompleteInputFlag)
                                        {
                                            this.NextPageButton.Visibility = Visibility.Visible;
                                        }
                                        this.BackPageButton.Visibility = Visibility.Visible;
                                    }
                                    else
                                    {
                                        this.NextPageButton.Visibility = Visibility.Visible;
                                        this.BackPageButton.Visibility = Visibility.Visible;
                                    }
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
                                case "グループアクティビティ":
                                    this.GoTo("input_aosuke_feeling","sub");
                                    break;

                                case "ちょうせんしたいこと":
                                    this.GoTo("input_challenge","sub");
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

                // ハートゲージに対する処理
                case "gauge":

                    if (this.ViewSizeOfFeelingTextBlock.Text == "")
                    {
                        this.ViewSizeOfFeelingTextBlock.Text = "50";
                        this.Angle = 0;
                    }

                    this.SelectHeartImage.Source = null;
                    this.SelectNeedleImage.Source = null;

                    if (Regex.IsMatch(this.scene , ".+のきもちときもちの大きさ.+")||this.scene=="グループアクティビティ")
                    {
                        if(((string)this.KindOfFeelings[this.FeelingDictionaryKey].GetValue(this.dataChapter9)).Split(",")[1] == "良い")
                        {
                            this.SelectHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_red.png", UriKind.Relative));
                            this.SelectNeedleImage.Source = new BitmapImage(new Uri(@"./Images/red_needle.png", UriKind.Relative));
                        }
                        else if(((string)this.KindOfFeelings[this.FeelingDictionaryKey].GetValue(this.dataChapter9)).Split(",")[1] == "悪い")
                        {
                            this.SelectHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_blue.png", UriKind.Relative));
                            this.SelectNeedleImage.Source = new BitmapImage(new Uri(@"./Images/blue_needle.png", UriKind.Relative));
                        }
                    }
                    
                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "get_item":

                    this.dataItem.HasGotItem09 = true;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataItem SET HasGotItem09 = 1 WHERE Id = 1;");
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();

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

                case "item_book":

                    Image[] itemMainImages = { this.Item01MainImage, this.Item02MainImage, this.Item03MainImage, this.Item04MainImage, this.Item05MainImage, this.Item06MainImage, this.Item07MainImage, this.Item08MainImage, this.Item10MainImage, this.Item11MainImage };

                    Image[] itemNoneImages = { this.Item01NoneImage, this.Item02NoneImage, this.Item03NoneImage, this.Item04NoneImage, this.Item05NoneImage, this.Item06NoneImage, this.Item07NoneImage, this.Item08NoneImage, this.Item10NoneImage, this.Item11NoneImage };

                    var hasGotItems = new bool[] { this.dataItem.HasGotItem01, this.dataItem.HasGotItem02, this.dataItem.HasGotItem03, this.dataItem.HasGotItem04, this.dataItem.HasGotItem05, this.dataItem.HasGotItem06, this.dataItem.HasGotItem07, this.dataItem.HasGotItem08, this.dataItem.HasGotItem10, this.dataItem.HasGotItem11 };

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

                case "is_animation_skip":

                 this.isAnimationSkip = Convert.ToBoolean(this.scenarios[this.scenarioCount][1]);

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
                    //this.KindOfFeelingInputButton.IsEnabled = true;

                    //if (text == "")
                    //    this.SizeOfFeelingInputButton.IsEnabled = false;
                    //else
                    //    this.SizeOfFeelingInputButton.IsEnabled = true;
                    case "$aosuke_kind_of_feeling_1$":
                        this.FeelingDictionaryKey = "aosuke_feeling1";
                        text = text.Replace("$aosuke_kind_of_feeling_1$", ((string)KindOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter9)).Split(",")[0]);
                        if(text!="")
                        {
                            this.AosukeSizeOfFeelingInputButton1.IsEnabled = true;
                        }
                        else if (text == "")
                        {
                            this.AosukeSizeOfFeelingInputButton1.IsEnabled = false;
                        }
                        break;
                    case "$aosuke_kind_of_feeling_2$":
                        this.FeelingDictionaryKey="aosuke_feeling2";
                        text = text.Replace("$aosuke_kind_of_feeling_2$", ((string)KindOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter9)).Split(",")[0]);
                        if(text!="")
                        {
                            this.AosukeSizeOfFeelingInputButton2.IsEnabled = true;
                        }
                        else if(text =="")
                        {
                            this.AosukeSizeOfFeelingInputButton2.IsEnabled = false;
                        }
                        break;
                    case "$aosuke_kind_of_feeling_3$":
                        this.FeelingDictionaryKey="aosuke_feeling3";
                        text = text.Replace("$aosuke_kind_of_feeling_3$", ((string)KindOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter9)).Split(",")[0]);
                        if(text!="")
                        {
                            this.AosukeSizeOfFeelingInputButton3.IsEnabled = true;
                        }
                        else if(text =="")
                        {
                            this.AosukeSizeOfFeelingInputButton3.IsEnabled = false;
                        }
                        break;
                    case "$aosuke_kind_of_feeling_4$":
                        this.FeelingDictionaryKey="aosuke_feeling4";
                        text = text.Replace("$aosuke_kind_of_feeling_4$", ((string)KindOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter9)).Split(",")[0]);
                        if(text!="")
                        {
                            this.AosukeSizeOfFeelingInputButton4.IsEnabled = true;
                        }
                        else if(text =="")
                        {
                            this.AosukeSizeOfFeelingInputButton4.IsEnabled = false;
                        }
                        break;

                    case "$aosuke_size_of_feeling_1$":
                        this.FeelingDictionaryKey = "aosuke_feeling1";
                        if ((int)this.SizeOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter9) != -1)
                        {
                            text = text.Replace("$aosuke_size_of_feeling_1$", this.SizeOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter9).ToString());
                        }
                        else
                        {
                            text = text.Replace("$aosuke_size_of_feeling_1$", "");
                        }
                        break;
                    case "$aosuke_size_of_feeling_2$":
                        this.FeelingDictionaryKey = "aosuke_feeling2";
                        if ((int)this.SizeOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter9) != -1)
                        {
                            text = text.Replace("$aosuke_size_of_feeling_2$", this.SizeOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter9).ToString());
                        }
                        else
                        {
                            text = text.Replace("$aosuke_size_of_feeling_2$", "");
                        }
                        break;
                    case "$aosuke_size_of_feeling_3$":
                         this.FeelingDictionaryKey = "aosuke_feeling3";
                        if ((int)this.SizeOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter9) != -1)
                        {
                            text = text.Replace("$aosuke_size_of_feeling_3$", this.SizeOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter9).ToString());
                        }
                        else
                        {
                            text = text.Replace("$aosuke_size_of_feeling_3$", "");
                        }
                        break;
                    case "$aosuke_size_of_feeling_4$":
                         this.FeelingDictionaryKey = "aosuke_feeling4";
                        if ((int)this.SizeOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter9) != -1)
                        {
                            text = text.Replace("$aosuke_size_of_feeling_4$", this.SizeOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter9).ToString());
                        }
                        else
                        {
                            text = text.Replace("$aosuke_size_of_feeling_4$", "");
                        }
                        break;

                    case "$input_challenge_text$":

                        text = text.Replace("$input_challenge_text$", this.dataChapter9.InputChallengeText);
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
#if DEBUG
            this.scenarioCount += 1;
            this.ScenarioPlay();
#else
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
#endif

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

                if ((button.Name == "NextMessageButton" || button.Name == "NextPageButton" || button.Name == "MangaFlipButton" || button.Name == "SelectFeelingCompleteButton" || button.Name == "BranchButton2" || button.Name == "MangaPrevBackButton" || button.Name == "GroupeActivityNextMessageButton" || button.Name == "ReturnButton"))
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

                        if (this.SelectFeelingGrid.IsVisible)
                        {
                            if (this.SelectBadFeelingListBox.SelectedItem != null || this.SelectGoodFeelingListBox.SelectedItem != null)
                            {

                                if (this.SelectGoodFeelingListBox.SelectedItem != null)
                                {
                                    this.KindOfFeelings[FeelingDictionaryKey].SetValue(this.dataChapter9, $"{this.SelectGoodFeelingListBox.SelectedItem.ToString().Replace("●　", "")},良い");
                                }
                                else if (this.SelectBadFeelingListBox.SelectedItems != null)
                                {
                                    this.KindOfFeelings[FeelingDictionaryKey].SetValue(this.dataChapter9, $"{this.SelectBadFeelingListBox.SelectedItem.ToString().Replace("●　", "")},悪い");
                                }
                            }
                        }
                        else if (this.SelectHeartGrid.IsVisible)
                        {
                            this.SizeOfFeelings[FeelingDictionaryKey].SetValue(this.dataChapter9, int.Parse(this.ViewSizeOfFeelingTextBlock.Text));
                        }
                        else if(this.NotGoodEventGrid.Visibility == Visibility.Visible)
                        {
                            foreach (var listbox in this.NotGoodEventItemsControl.GetChildren<ListBox>())
                            {
                                foreach (var text in listbox.SelectedItems)
                                {
                                    this.dataChapter9.CheckedNotGoodEvent += $"{text},";
                                }
                                foreach (var border in ((StackPanel)listbox.Parent).Children)
                                {
                                    if(border is Border)
                                    {
                                        var text = ((Border)border).Child;
                                        if (text is TextBlock)
                                        {
                                            this.dataChapter9.CheckedNotGoodEvent += $"{((TextBlock)text).Text};"; 
                                        }
                                    }
                                }
                               
                            }

                            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                            {
                                connection.Execute($"Update DataChapter9 SET CheckedNotGoodEvent ='{this.dataChapter9.CheckedNotGoodEvent}' WHERE CreatedAt = '{this.dataChapter9.CreatedAt}' ;");
                            }
                        }
                        else if(this.InputChallengeGrid.Visibility == Visibility.Visible)
                        {
                            if(this.dataOption.InputMethod == 0)
                            {
                                StrokeConverter strokeConverter = new StrokeConverter();
                                strokeConverter.ConvertToBmpImage(this.InputCanvas,this.InputChallengeCanvasStrokes,"challenge_time_input_challege_event",this.initConfig.userName, this.dataProgress.CurrentChapter);
                            }
                            else
                            {
                                this.dataChapter9.InputChallengeText = this.InputTextBox.Text;
                                using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                                {
                                    connection.Execute($"Update DataChapter9 SET InputChallengeText ='{this.dataChapter9.InputChallengeText}' WHERE CreatedAt = '{this.dataChapter9.CreatedAt}' ;");
                                }
                            }
                        }
                        else if(this.ChangeAosukesFeelingGrid.Visibility == Visibility.Visible)
                        {
                            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                            {
                                connection.Execute($"Update DataChapter9 SET AosukesKindOfFeelingTalkToFriends ='{this.dataChapter9.AosukesKindOfFeelingTalkToFriends}' WHERE CreatedAt = '{this.dataChapter9.CreatedAt}' ;");
                                connection.Execute($"Update DataChapter9 SET AosukesKindOfFeelingAfter10minutes ='{this.dataChapter9.AosukesKindOfFeelingAfter10minutes}' WHERE CreatedAt = '{this.dataChapter9.CreatedAt}' ;");
                                connection.Execute($"Update DataChapter9 SET AosukesKindOfFeelingNextDay ='{this.dataChapter9.AosukesKindOfFeelingNextDay}' WHERE CreatedAt = '{this.dataChapter9.CreatedAt}' ;");
                                connection.Execute($"Update DataChapter9 SET AosukesKindOfFeelingDayAfterTomorrow ='{this.dataChapter9.AosukesKindOfFeelingDayAfterTomorrow}' WHERE CreatedAt = '{this.dataChapter9.CreatedAt}' ;");

                                connection.Execute($"Update DataChapter9 SET AosukesSizeOfFeelingTalkToFriends ='{this.dataChapter9.AosukesSizeOfFeelingTalkToFriends}' WHERE CreatedAt = '{this.dataChapter9.CreatedAt}' ;");
                                connection.Execute($"Update DataChapter9 SET AosukesSizeOfFeelingAfter10minutes ='{this.dataChapter9.AosukesSizeOfFeelingAfter10minutes}' WHERE CreatedAt = '{this.dataChapter9.CreatedAt}' ;");
                                connection.Execute($"Update DataChapter9 SET AosukesSizeOfFeelingNextDay ='{this.dataChapter9.AosukesSizeOfFeelingNextDay}' WHERE CreatedAt = '{this.dataChapter9.CreatedAt}' ;");
                                connection.Execute($"Update DataChapter9 SET AosukesSizeOfFeelingDayAfterTomorrow ='{this.dataChapter9.AosukesSizeOfFeelingDayAfterTomorrow}' WHERE CreatedAt = '{this.dataChapter9.CreatedAt}' ;");
                            }
                        }
                    }
                    else if (button.Name == "MangaFlipButton")
                    {
                        this.MangaFlipButton.Visibility = Visibility.Hidden;
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }
                else if (Regex.IsMatch(button.Name, ".*KindOfFeelingInputButton.*"))
                {
                    this.SelectBadFeelingListBox.SelectedIndex = -1;
                    this.SelectGoodFeelingListBox.SelectedIndex = -1;

                    if (button.Name == "AosukeKindOfFeelingInputButton1")
                    {
                        this.FeelingDictionaryKey = "aosuke_feeling1";
                    }
                    else if (button.Name == "AosukeKindOfFeelingInputButton2")
                    {
                        this.FeelingDictionaryKey = "aosuke_feeling2";
                    }
                    else if (button.Name == "AosukeKindOfFeelingInputButton3")
                    {
                        this.FeelingDictionaryKey = "aosuke_feeling3";
                    }
                    else if (button.Name == "AosukeKindOfFeelingInputButton4")
                    {
                        this.FeelingDictionaryKey = "aosuke_feeling4";
                    }

                    this.GoTo("kind_of_feeling","sub");
                }
                else if (Regex.IsMatch(button.Name, ".*SizeOfFeelingInputButton.*"))
                {
                    if (button.Name == "AosukeSizeOfFeelingInputButton1")
                    {
                        this.FeelingDictionaryKey = "aosuke_feeling1";
                    }
                    else if (button.Name == "AosukeSizeOfFeelingInputButton2")
                    {
                        this.FeelingDictionaryKey = "aosuke_feeling2";
                    }
                    else if (button.Name == "AosukeSizeOfFeelingInputButton3")
                    {
                        this.FeelingDictionaryKey = "aosuke_feeling3";
                    }
                    else if (button.Name == "AosukeSizeOfFeelingInputButton4")
                    {
                        this.FeelingDictionaryKey = "aosuke_feeling4";
                    }

                    this.GoTo("size_of_feeling","sub");
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
                else if (button.Name == "SelectFeelingNextButton")
                {
                    this.SelectFeelingNextButton.Visibility = Visibility.Hidden;

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }
                else if (button.Name == "BranchButton1")
                {
                    this.GoTo("manga","sub");
                }
                else if(button.Name == "InputChallengeButton")
                {
                    if(this.dataOption.InputMethod == 0)
                    {
                        this.InputChallengeCanvasStrokes = this.InputChallengeCanvas.Strokes;
                        this.ClipStrokes(this.InputCanvas, this.InputChallengeCanvasStrokes);
                        this.InputCanvas.Strokes = this.InputChallengeCanvasStrokes;
                        this.GoTo("canvas_input","sub");
                    }
                    else
                    {
                        this.InputTextBox.Text = this.dataChapter9.InputChallengeText;
                        this.GoTo("keyboard_input","sub");
                        this.InputTextBox.Focus();
                    }
                }
                else if(button.Name == "CompleteInputButton")
                {
                    if(this.dataOption.InputMethod == 0)
                    {
                       if(this.InputChallengeCanvasStrokes != null)
                       {
                            this.ClipStrokes(this.InputCanvas, this.InputChallengeCanvasStrokes);
                            this.InputChallengeCanvas.Strokes = this.InputChallengeCanvasStrokes;
                       }
                    }
                    else
                    {
                        this.dataChapter9.InputChallengeText = this.InputTextBox.Text;
                        this.InputChallengeText.Text = this.dataChapter9.InputChallengeText;
                        this.CloseOSK();
                    }

                    this.scenarioCount += 1;
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

        // ハートゲージの角度をデータバインド
        private static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(Chapter9), new UIPropertyMetadata(0.0));

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

            if (this.isMouseDown && this.SelectHeartGrid.IsVisible)
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
                if (this.SelectHeartGrid.IsVisible)
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

            Point knobCenter = this.SelectHeartImage.PointToScreen(new Point(this.SelectHeartImage.ActualWidth * 0.5, this.SelectHeartImage.ActualHeight * 0.7));

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

        private void FeelingListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if ((this.SelectGoodFeelingListBox.IsVisible && this.SelectBadFeelingListBox.IsVisible))
            {
                if (this.SelectGoodFeelingListBox.SelectedItem != null && this.SelectBadFeelingListBox.SelectedItem != null)
                {
                    if (listBox.Name == "SelectBadFeelingListBox")
                    {
                        if (this.SelectGoodFeelingListBox.SelectedItem != null)
                            this.SelectGoodFeelingListBox.SelectedIndex = -1;
                    }
                    else
                    {
                        if (this.SelectBadFeelingListBox.SelectedItem != null)
                            this.SelectBadFeelingListBox.SelectedIndex = -1;
                    }
                    this.NextPageButton.Visibility = Visibility.Visible;
                }
                else if (this.SelectGoodFeelingListBox.SelectedItem != null || this.SelectBadFeelingListBox.SelectedItem != null)
                {
                    this.NextPageButton.Visibility = Visibility.Visible;
                }

                var startupPath = FileUtils.GetStartupPath();

                PlaySE($@"{startupPath}/Sounds/Decision.wav");
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

            while (text.LineCount > 7)
            {
                caretPosition -= 1;
                text.Text = text.Text.Remove(caretPosition, 1);
            }

            text.Select(caretPosition, 0);
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            TextBox text = sender as TextBox;

            if (text.LineCount > 6)
            {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                }
            }
        }

        private class AosukeSituationTextData
        {
            public AosukeSituationTextData(string underLineText, string situationText )
            {
                UnderLineText = underLineText;
                SituationText = situationText;
            }

            public string UnderLineText { get; set; }
            public string SituationText { get; set; }
        }

        private class NotGoodEventData
        {
            public NotGoodEventData(string headerText, List<string> notGoodEvent)
            {
                HeaderText = headerText;
                NotGoodEvent = notGoodEvent;
            }

            public string HeaderText { get; set; }
            public List<string> NotGoodEvent { get; set; }
        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem listBoxItem = sender as ListBoxItem;

            switch (listBoxItem.Name)
            {
                case "PenButton":
                    this.InputCanvas.EditingMode = InkCanvasEditingMode.Ink;
                    this.ClipStrokes(this.InputCanvas, this.InputChallengeCanvasStrokes);
                    break;

                case "EraserButton":
                    this.InputCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    break;

                case "AllClearButton":
                    this.InputCanvas.Strokes.Clear();

                    this.ClipStrokes(this.InputCanvas, this.InputChallengeCanvasStrokes);
                    break;
            }
        }
        private void ClipStrokes(InkCanvas inkCanvas, StrokeCollection strokes)
        {
            StylusPoint point1 = new StylusPoint() { X = 0, Y = 0 };
            StylusPoint point2 = new StylusPoint() { X = inkCanvas.ActualWidth, Y = 0 };
            StylusPoint point3 = new StylusPoint() { X = inkCanvas.ActualWidth, Y = inkCanvas.ActualHeight };
            StylusPoint point4 = new StylusPoint() { X = 0, Y = inkCanvas.ActualHeight };

            StylusPointCollection points1 = new StylusPointCollection();
            StylusPointCollection points2 = new StylusPointCollection();
            points1.Add(point1);
            points1.Add(point2);
            points1.Add(point3);
            points1.Add(point4);

            Point[] strokePoints = (Point[])points1;
            strokes.Clip(strokePoints);

            points2.Add(point3);
            DrawingAttributes attributes = new DrawingAttributes() { Height = 1, Width = 1, Color = Colors.Transparent };
            Stroke stroke = new Stroke(points2) { DrawingAttributes = attributes };
            strokes.Add(stroke);
        }

        // マウスのドラッグ処理（マウスの左ボタンを押そうとしたとき）
        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string objName = "None";

            if (e.Source as FrameworkElement != null)
            {
                objName = (e.Source as FrameworkElement).Name;
            }

            logManager.SaveLog(this.initConfig, this.dataProgress, objName, Mouse.GetPosition(this).X.ToString(), Mouse.GetPosition(this).Y.ToString(), this.isClickable.ToString());
        }

    }
}