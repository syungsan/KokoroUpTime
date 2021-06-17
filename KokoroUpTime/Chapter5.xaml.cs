using CsvReadWrite;
using Expansion;
using FileIOUtils;
using Osklib;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
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

namespace KokoroUpTime
{
    /// <summary>
    /// GameWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class Chapter5 : Page
    {
        private string[] HOT_WORD_KEYS = { "ほめる言葉", "しんぱいする言葉", "はげます言葉", "おれいの言葉" };
        private string[] ROLE_PLAY_WORDS = { "ほめる言葉（場面①）のれんしゅう", "しんぱいする言葉（場面②）のれんしゅう", "はげます言葉（場面③）のれんしゅう", "おれいの言葉（場面④）のれんしゅう" };

        private Dictionary<string, string[]> HOT_WORD_VALUES = new Dictionary<string, string[]>()
        {
            {"ほめる言葉", new string[] { "すごい！上手だね。", "きれいな絵だね。", "いい色の使い方だね。", "とっても素敵だね。" } },
            {"しんぱいする言葉", new string[] { "だいじょうぶ？", "カゼは治った？", "しんどくない？", "心配してたんだよ。" } },
            {"はげます言葉", new string[] { "がんばってね！", "おうえんしてるよ。", "がんばってたの知ってるよ。", "大丈夫だよ！" } },
            {"おれいの言葉", new string[] { "ありがとう。", "ありがとう、助かったよ。", "ありがとう、やさしいね。", "ありがとう、うれしいよ。" } }
        };

        // 気持ちのリスト
        private string[] GOOD_FEELINGS = { "●　うれしい", "●　しあわせ", "●　たのしい", "●　ホッとした", "●　きもちいい", "●　まんぞく", "●　すき", "●　やる気マンマン", "●　かんしゃ", "●　わくわく", "●　うきうき", "●　ほこらしい" };
        private string[] BAD_FEELINGS = { "●　心配", "●　こまった", "●　不安", "●　こわい", "●　おちこみ", "●　がっかり", "●　いかり", "●　イライラ", "●　はずかしい", "●　ふまん", "●　かなしい", "●　おびえる" };

        private string[] RELAX_METHOD_COUNT = { "①", "②", "③", "④", "⑤", "⑥" };

        private float THREE_SECOND_RULE_TIME = 3.0f;

        private int RETURN_COUNT = 1;

        LogManager logManager;

        // ゲームを進行させるシナリオ
        private int scenarioCount = 0; //
        private List<List<string>> scenarios = null; //

        // 各種コントロールの名前を収める変数
        private string position = ""; //

        // マウスクリックを可能にするかどうかのフラグ
        private bool isClickable = false;

        // 気持ちの大きさ
        private int feelingSize = 0;

        private bool isAnimationSkip = false;

        // メッセージ表示関連
        private DispatcherTimer msgTimer; //
        private int word_num; //

        private int inlineCount;
        private int imageInlineCount;

        private Dictionary<string, List<Run>> runs = new Dictionary<string, List<Run>>();
        private Dictionary<string, List<InlineUIContainer>> imageInlines = new Dictionary<string, List<InlineUIContainer>>();

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

        // データベースに収めるデータモデルのインスタンス
        private DataChapter5 dataChapter5;

        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();
        public DataProgress dataProgress = new DataProgress();

        private int cHotWordButtonCount = 0;
        private int rolePlayButtonCount = 0;

        //データモデルのプロパティを呼び出すための辞書
        private Dictionary<string, PropertyInfo> KindOfFeelings = null;
        private Dictionary<string, PropertyInfo> SizeOfFeelings = null;
        private Dictionary<string, StrokeCollection> InputStroke = null;
        private Dictionary<string, PropertyInfo> InputText = null;

        private string FeelingDictionaryKey = "";
        private string InputDictionaryKey = "";

        private List<string> InputRelaxMethodText = null;

        private StrokeCollection InputFaceImageOfKimiStroke = null;
        private StrokeCollection InputHandImageOfKimiStroke = null;
        private StrokeCollection InputStomachImageOfKimiStroke = null;
        private StrokeCollection InputOthersImageOfKimiStroke = null;
        private StrokeCollection InputHeadImageOfKimiStroke = null;
        private StrokeCollection InputShoulderImageOfKimiStroke = null;
        private StrokeCollection InputLegImageOfKimiStroke = null;
        private StrokeCollection InputMyBodyImageStrokeNotUnderstandProblem = null;
        private StrokeCollection InputMyBodyImageStrokeRecorderProblem = null;
        private List<StrokeCollection> InputRelaxMethodStrokes = null;
        

        private bool isDeepBreath = false;

        public Chapter5()
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
            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(OnPreviewMouseLeftButtonDown);

            logManager = new LogManager();

            // データモデルインスタンス確保
            this.dataChapter5 = new DataChapter5();

            this.SelectGoodFeelingListBox.ItemsSource = GOOD_FEELINGS;
            this.SelectBadFeelingListBox.ItemsSource = BAD_FEELINGS;

            this.CHotWordKeyButtonItemControl.ItemsSource = HOT_WORD_KEYS;
            this.GHotWordKeyButtonItemControl.ItemsSource = HOT_WORD_KEYS;
            this.RolePlayButtonItemControl.ItemsSource = ROLE_PLAY_WORDS;

            this.SizeOfFeelingInputButton.IsEnabled = false;

            KindOfFeelings = new Dictionary<string, PropertyInfo>()
            {
               ["not_understand_problem"]=typeof(DataChapter5).GetProperty("KindOfFeelingNotUnderstandProblem"),
               ["recorder_problem"] =typeof(DataChapter5).GetProperty("KindOfFeelingRecorderProblem"),
            };

            SizeOfFeelings = new Dictionary<string, PropertyInfo>()
            {
                ["not_understand_problem"] = typeof(DataChapter5).GetProperty("SizeOfFeelingNotUnderstandProblem"),
                ["recorder_problem"] = typeof(DataChapter5).GetProperty("SizeOfFeelingRecorderProblem"),
            };                                                         

            this.DataContext = this.dataChapter5;
            


            this.InitControls();
        }

        private void InitControls()
        {
            // 各種コントロールに任意の文字列をあてがう

            this.imageObjects = new Dictionary<string, Image>
            {
                ["bg_image"] = this.BackgroundImage,
                // ["manga_title_image"] = this.MangaTitleImage,
                ["cover_layer_image"] =this.CoverLayerImage,
                ["manga_image"] = this.MangaImage,
                ["item_center_up_image"] = this.ItemCenterUpImage,
                ["item_center_image"] = this.ItemCenterImage,
                ["item_left_image"] = this.ItemLeftImage,
                ["item_detail_info_image"] = this.ItemDetailInfoImage,
                ["children_face_left_center_image"] = this.ChildrenFaceLeftCenterImage,
                ["children_feeling_comment_image"] = this.ChildrenFeelingCommentImage,
                ["children_feeling_comment_big_image"] = this.ChildrenFeelingCommentBigImage,
                ["shiroji_very_small_right_up_image"] = this.ShirojiVerySmallRightUpImage,
                ["item_center_right_image"] = this.ItemCenterRightImage,
                ["item_left_last_image"] = this.ItemLeftLastImage,
                ["session_title_image"] = this.SessionTitleImage,
                ["shiroji_ending_image"] = this.ShirojiEndingImage,
                ["shiroji_right_image"] = this.ShirojiRightImage,
                ["shiroji_right_up_image"] = this.ShirojiRightUpImage,
                ["shiroji_left_image"] = this.ShirojiLeftImage,
                ["shiroji_small_right_up_image"] = this.ShirojiSmallRightUpImage,
                ["shiroji_small_right_down_image"] = this.ShirojiSmallRightDownImage,
                ["shiroji_right_center_image"] = this.ShirojiRightCenterImage,
                ["shiroji_small_right_center_image"] = this.ShirojiSmallRightCenterImage,
                ["shiroji_right_down_small_image"] = this.ShirojiRightDownSmallImage,
                ["children_face_left_image"] = this.ChildrenFaceLeftImage,
                ["children_face_right_image"] = this.ChildrenFaceRightImage,
                ["main_msg_bubble_image"] = this.MainMessageBubbleImage,
                ["shiroji_small_left_down_image"] = this.ShirojiSmallLeftDownImage,
                ["hot_word_title_image"] = this.HotWordTitleImage,
                ["glad_comment_up_image"] = this.GladCommentUpImage,
                ["glad_comment_down_image"] = this.GladCommentDownImage,
                ["item_left_center_small_image"] = this.ItemLeftCenterSmallImage,
                ["let's_use_hot_word_title_image"] = this.Let_sUseHotWordTitleImage,
                ["hot_word_arrow_image"] = this.HotWordArrowImage,
                ["item_left_down_image"] = this.ItemLeftDownImage,

                ["body_image_of_akamaru_angry_image"] = this.BodyImageOfAkamaruAngryImage,
                ["body_image_of_kimi_fear_image"] = this.BodyImageOfKimiFearImage,

                ["relax_children_image"] = this.RelaxChildrenImage, //
                ["relax_children_bubble_image"] = this.RelaxChildrenBubbleImage, //

                ["deep_breath_shiroji_image"] = this.DeepBreathShirojiImage, //

                ["relax_shade_image"] = this.RelaxShadeImage, //
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
                ["challenge_time_title_text"] = this.ChallengeTimeTitleTextBlock,
                ["let_s_try_title_text"] = this.Let_sTryTitleTextBlock,
                ["hot_word_text"] = this.HotWordTextBlock,
                ["children_body_image_title_text"] = ChildrenBodyImageTitleTextBlock, //
                ["children_feeling_comment_text"] = this.ChildrenFeelingCommentTextBlock,
                ["children_feeling_comment_big_text"] = this.ChildrenFeelingCommentBigTextBlock,
                ["let's_use_hot_word_msg"] = this.Let_sUseHotWordMessageTextBlock,
                ["selected_hot_word_title_text"] = this.SelectHotWordTitleTextBlock,
                ["selected_hot_word_value_title_text"] = this.SelectHotWordValueTitleTextBlock,
                ["selected_hot_word_value_text"] = this.SelectHotWordValueTextBlock,
                ["complimentary_situation_title_text"] = this.ComplimentarySituationTitleTextBlock,
                ["complimentary_situation_value_text"] = this.ComplimentarySituationValueTextBlock,
                ["kind_of_feeling_input_text"] = this.KindOfFeelingInputTextBlock,
                ["size_of_feeling_input_text"] = this.SizeOfFeelingInputTextBlock,

                ["problem_text"] = this.ProblemTextBlock, //


                ["relax_info_text"] = this.RelaxInfoTextBlock, //
                ["relax_children_bubble_text"] = this.RelaxChildrenBubbleTextBlock, //

                ["relax_title_text"] = this.RelaxTitleTextBlock, //

                ["deep_breath_msg"] = this.DeepBreathMessageTextBlock, //

                ["relax_method_text"] = this.RelaxMethodTextBlock, //
                ["relax_method_msg_text"] = this.RelaxMethodMessageTextBlock, //
                ["input_canvas_relax_method_msg_text"] = this.InputCanvasRelaxMethodMessageTextBlock,
                ["input_keyboard_relax_method_msg_text"] = this.InputKeyboardRelaxMethodMessageTextBlock,

            };

            this.buttonObjects = new Dictionary<string, Button>
            {
                ["next_msg_button"] = this.NextMessageButton,
                ["back_msg_button"] = this.BackMessageButton,
                ["next_page_button"] = this.NextPageButton,
                ["back_page_button"] = this.BackPageButton,
                ["manga_flip_button"] = this.MangaFlipButton,
                ["check_manga_button"] = this.CheckMangaButton,
                ["feeling_next_go_button"] = this.FeelingNextGoButton,
                ["feeling_prev_back_button"] = this.FeelingPrevBackButton,
                // ["manga_prev_back_button"] = this.MangaPrevBackButton,
                ["complete_next_button"] = this.CompleteNextButton,
                ["complete_input_button"] = this.CompleteInputButton,

                ["next_go_button"] = this.NextGoButton, //
                ["back_go_button"] = this.BackGoButton, //
            };

            this.gridObjects = new Dictionary<string, Grid>
            {
                ["base_grid"] = this.BaseGrid,

                ["memory_check_grid"] = this.MemoryCheckGrid,
                ["session_grid"] = this.SessionGrid,
                ["select_feeling_grid"] = this.SelectFeelingGrid,
                ["summary_grid"] = this.SummaryGrid,
                ["ending_grid"] = this.EndingGrid,
                ["item_name_plate_left_grid"] = this.ItemNamePlateLeftGrid,
                ["item_name_bubble_grid"] = this.ItemNameBubbleGrid,
                ["item_name_plate_center_grid"] = this.ItemNamePlateCenterGrid,
                ["item_info_plate_grid"] = this.ItemInfoPlateGrid,
                ["item_info_sentence_grid"] = this.ItemInfoSentenceGrid,
                ["item_last_info_grid"] = this.ItemLastInfoGrid,
                ["item_detail_info_grid"] = this.ItemDetailInfoGrid, //
                ["children_feeling_comment_grid"] = this.ChildrenFeelingCommentGrid,
                ["children_feeling_comment_big_grid"] = this.ChildrenFeelingCommentBigGrid,
                ["select_heart_grid"] = this.SelectHeartGrid,
                ["ending_msg_grid"] = this.EndingMessageGrid,
                ["main_msg_grid"] = this.MainMessageGrid,
                ["music_info_grid"] = this.MusicInfoGrid,
                ["c_hot_word_key_button_grid"] = this.CHotWordKeyButtonGrid,
                ["g_hot_word_key_button_grid"] = this.GHotWordKeyButtonGrid,
                ["hot_word_comment_grid"] = this.HotWordCommentGrid,
                ["situations_grid"] = this.SituationsGrid,
                ["let's_use_hot_word_msg_grid"] = this.Let_sUseHotWordMessageGrid,
                ["hot_word_value_button_grid"] = this.HotWordValueButtonGrid,
                ["role_play_info_grid"] = this.RolePlayInfoGrid,
                ["role_play_button_grid"] = this.RolePlayButtonGrid,
                // ["feeling_input_grid"] = this.FeelingInputGrid,

                ["body_image_of_akamaru_bubbles_grid"] = this.BodyImageOfAkamaruBubblesGrid, //
                ["body_image_of_kimi_bubbles_grid"] = this.BodyImageOfKimiBubblesGrid, //
                ["my_status_input_grid"] = this.MyStatusInputGrid, //

                ["relax_grid"] = this.RelaxGrid, //
                ["relax_human_grid"] = this.RelaxHumanGrid, //
                ["relax_children_grid"] = this.RelaxChildrenGrid, //

                ["deep_breath_grid"] = this.DeepBreathGrid, //
                ["deep_breath_bubble_grid"] = this.DeepBreathBubbleGrid, //

                ["relax_method_grid"] = this.RelaxMethodGrid, //

                ["challenge_time_grid"] = this.ChallengeTimeGrid,
                ["feeling_input_grid"] = this.FeelingInputGrid,
                ["select_feeling_grid"] = this.SelectFeelingGrid,
                ["select_heart_grid"] = this.SelectHeartGrid,
                ["view_size_of_feeling_grid"] = this.ViewSizeOfFeelingGrid,
                ["canvas_edit_grid"] = this.CanvasEditGrid,
                ["input_canvas_grid"] =this.InputCanvasGrid,
                ["input_text_grid"] = this.InputTextGrid,

                ["input_relax_method_text_grid"] =this.InputRelaxMethodTextGrid,
                ["input_relax_method_canvas_grid"] =this.InputRelaxMethodCanvasGrid,

                ["right_body_image_of_kimi_bubble_canvas_grid"] =this.RightBodyImageOfKimiBubbleCanvasGrid,
                ["right_body_image_of_kimi_bubble_text_box_grid"] =this.RightBodyImageOfKimiBubbleTextBoxGrid,
                ["left_body_image_of_kimi_bubble_canvas_grid"] = this.LeftBodyImageOfKimiBubbleCanvasGrid,
                ["left_body_image_of_kimi_bubble_text_box_grid"] = this.LeftBodyImageOfKimiBubbleTextBoxGrid,
            };

            this.borderObjects = new Dictionary<string, Border>
            {
                ["challenge_time_title_border"] = this.ChallengeTimeTitleBorder,
                ["children_body_image_title_border"] = this.ChildrenBodyImageTitleBorder,
                ["let_s_try_title_border"] = this.Let_sTryTitleBorder,
                ["hot_word_border"] = this.HotWordBorder,
                ["selected_hot_word_border"] = this.SelectHotWordBorder,
                ["complimentary_situation_border"] = this.ComplimentarySituationBorder,
                ["role_play_msg_border"] = this.RolePlayMessageBorder,

                ["item_check_border"] = this.ItemCheckBorder, //
                ["manga_title_border"] = this.MangaTitleBorder, //
                ["body_image_of_akamaru_angry_border"] = this.BodyImageOfAkamaruAngryBorder, //
                ["body_image_of_kimi_fear_border"] = this.BodyImageOfKimiFearBorder, //

                ["relax_title1_border"] = this.RelaxTitle1Border, //
                ["relax_title2_border"] = this.RelaxTitle2Border, //
                ["relax_title3_border"] = this.RelaxTitle3Border, //

                ["relax_title_border"] = this.RelaxTitleBorder, //

                ["input_my_body_image_canvas_border"] =this.InputMyBodyImageCanvasBorder,
                ["input_my_body_image_text_border"] = this.InputMyBodyImageTextBorder,
            };
        }

        private void ResetControls()
        {
            // 各種コントロールを隠すことでフルリセット

            this.MemoryCheckGrid.Visibility = Visibility.Hidden;
            this.SessionGrid.Visibility = Visibility.Hidden;
            this.SelectFeelingGrid.Visibility = Visibility.Hidden;
            this.SummaryGrid.Visibility = Visibility.Hidden;
            this.EndingGrid.Visibility = Visibility.Hidden;
            this.ItemNamePlateLeftGrid.Visibility = Visibility.Hidden;
            this.ItemNameBubbleGrid.Visibility = Visibility.Hidden;
            this.ItemNamePlateCenterGrid.Visibility = Visibility.Hidden;
            this.ItemInfoPlateGrid.Visibility = Visibility.Hidden;
            this.ItemLastInfoGrid.Visibility = Visibility.Hidden;
            this.Let_sUseHotWordMessageGrid.Visibility = Visibility.Hidden;
            this.SelectHeartGrid.Visibility = Visibility.Hidden;
            this.EndingMessageGrid.Visibility = Visibility.Hidden;
            this.MainMessageGrid.Visibility = Visibility.Hidden;
            this.MusicInfoGrid.Visibility = Visibility.Hidden;
            this.ExitBackGrid.Visibility = Visibility.Hidden;
            this.BackgroundImage.Visibility = Visibility.Hidden;
            // this.MangaTitleImage.Visibility = Visibility.Hidden;
            this.MangaImage.Visibility = Visibility.Hidden;
            this.ItemCenterUpImage.Visibility = Visibility.Hidden;
            this.ItemCenterImage.Visibility = Visibility.Hidden;
            this.ItemLeftImage.Visibility = Visibility.Hidden;
            // this.ItemDetailInfoImage.Visibility = Visibility.Hidden;
            this.ItemDetailInfoGrid.Visibility = Visibility.Hidden; //
            this.ItemLeftLastImage.Visibility = Visibility.Hidden;
            this.SessionTitleImage.Visibility = Visibility.Hidden;
            this.SessionSubTitleTextBlock.Visibility = Visibility.Hidden;
            this.SessionSentenceTextBlock.Visibility = Visibility.Hidden;
            this.ChildrenFaceLeftImage.Visibility = Visibility.Hidden;
            this.ChildrenFaceRightImage.Visibility = Visibility.Hidden;
            this.EndingMessageTextBlock.Visibility = Visibility.Hidden;
            this.ShirojiRightImage.Visibility = Visibility.Hidden;
            this.ShirojiRightUpImage.Visibility = Visibility.Hidden;
            this.ShirojiLeftImage.Visibility = Visibility.Hidden;
            this.ShirojiSmallRightUpImage.Visibility = Visibility.Hidden;
            this.ShirojiSmallRightDownImage.Visibility = Visibility.Hidden;
            this.ShirojiRightCenterImage.Visibility = Visibility.Hidden;
            this.ShirojiSmallRightCenterImage.Visibility = Visibility.Hidden;
            this.ShirojiRightDownSmallImage.Visibility = Visibility.Hidden;
            this.ShirojiEndingImage.Visibility = Visibility.Hidden;
            this.MainMessageTextBlock.Visibility = Visibility.Hidden;
            this.NextMessageButton.Visibility = Visibility.Hidden;
            this.BackMessageButton.Visibility = Visibility.Hidden;
            this.NextPageButton.Visibility = Visibility.Hidden;
            this.BackPageButton.Visibility = Visibility.Hidden;
            this.MangaFlipButton.Visibility = Visibility.Hidden;
            this.CoverLayerImage.Visibility = Visibility.Hidden;
            this.ChallengeTimeTitleBorder.Visibility = Visibility.Hidden;
            this.ChallengeTimeTitleTextBlock.Visibility = Visibility.Hidden;
            this.CHotWordKeyButtonGrid.Visibility = Visibility.Hidden;
            this.GHotWordKeyButtonGrid.Visibility = Visibility.Hidden;
            this.HotWordCommentGrid.Visibility = Visibility.Hidden;
            this.ShirojiSmallLeftDownImage.Visibility = Visibility.Hidden;
            this.HotWordValueButtonGrid.Visibility = Visibility.Hidden;
            this.HotWordTitleImage.Visibility = Visibility.Hidden;
            this.Let_sTryTitleBorder.Visibility = Visibility.Hidden;
            this.ItemCenterRightImage.Visibility = Visibility.Hidden;
            this.HotWordBorder.Visibility = Visibility.Hidden;
            this.GladCommentUpImage.Visibility = Visibility.Hidden;
            this.GladCommentDownImage.Visibility = Visibility.Hidden;
            this.ChildrenBodyImageTitleBorder.Visibility = Visibility.Hidden; //
            this.ChildrenFaceLeftCenterImage.Visibility = Visibility.Hidden;
            this.ChildrenFeelingCommentGrid.Visibility = Visibility.Hidden;
            this.ChildrenFeelingCommentBigGrid.Visibility = Visibility.Hidden;
            this.ShirojiVerySmallRightUpImage.Visibility = Visibility.Hidden;
            this.CheckMangaButton.Visibility = Visibility.Hidden;
            this.FeelingNextGoButton.Visibility = Visibility.Hidden;
            this.FeelingPrevBackButton.Visibility = Visibility.Hidden;
            this.ChildrenBodyImageTitleTextBlock.Visibility = Visibility.Hidden; //
            // this.MangaPrevBackButton.Visibility = Visibility.Hidden;
            this.CompleteNextButton.Visibility = Visibility.Hidden;
            this.ItemLeftCenterSmallImage.Visibility = Visibility.Hidden;
            this.Let_sUseHotWordMessageGrid.Visibility = Visibility.Hidden;
            this.SituationsGrid.Visibility = Visibility.Hidden;
            this.Let_sUseHotWordTitleImage.Visibility = Visibility.Hidden;
            this.SelectHotWordBorder.Visibility = Visibility.Hidden;
            this.SelectHotWordTitleTextBlock.Visibility = Visibility.Hidden;
            this.SelectHotWordValueTextBlock.Visibility = Visibility.Hidden;
            this.ComplimentarySituationBorder.Visibility = Visibility.Hidden;
            this.HotWordArrowImage.Visibility = Visibility.Hidden;
            this.ItemLeftDownImage.Visibility = Visibility.Hidden;
            this.RolePlayInfoGrid.Visibility = Visibility.Hidden;
            this.RolePlayButtonGrid.Visibility = Visibility.Hidden;
            this.SelectHotWordValueTitleTextBlock.Visibility = Visibility.Hidden;
            this.RolePlayMessageBorder.Visibility = Visibility.Hidden;
            this.ItemBookTitleTextBlock.Visibility = Visibility.Hidden;
            this.ItemBookMainGrid.Visibility = Visibility.Hidden;
            this.ItemBookNoneGrid.Visibility = Visibility.Hidden;
            this.ReturnToTitleButton.Visibility = Visibility.Hidden;
            // this.FeelingInputGrid.Visibility = Visibility.Hidden;

            this.MangaTitleBorder.Visibility = Visibility.Hidden;

            this.ItemCheckBorder.Visibility = Visibility.Hidden; //

            this.BodyImageOfAkamaruAngryBorder.Visibility = Visibility.Hidden; //
            this.BodyImageOfAkamaruAngryImage.Visibility = Visibility.Hidden; //
            this.BodyImageOfAkamaruBubblesGrid.Visibility = Visibility.Hidden; //

            this.BodyImageOfKimiFearBorder.Visibility = Visibility.Hidden; //
            this.BodyImageOfKimiFearImage.Visibility = Visibility.Hidden; //
            this.BodyImageOfKimiBubblesGrid.Visibility = Visibility.Hidden; //

            this.RightBodyImageOfKimiBubbleCanvasGrid.Visibility = Visibility.Hidden;
            this.RightBodyImageOfKimiBubbleTextBoxGrid.Visibility = Visibility.Hidden;
            this.LeftBodyImageOfKimiBubbleCanvasGrid.Visibility = Visibility.Hidden;
            this.LeftBodyImageOfKimiBubbleTextBoxGrid.Visibility = Visibility.Hidden;

            this.MyStatusInputGrid.Visibility = Visibility.Hidden; //
            this.ProblemTextBlock.Visibility = Visibility.Hidden; //

            this.RelaxGrid.Visibility = Visibility.Hidden; //
            // this.RelaxTitle1Border.Visibility = Visibility.Hidden; //
            this.RelaxTitle2Border.Visibility = Visibility.Hidden; //
            this.RelaxTitle3Border.Visibility = Visibility.Hidden; //
            // this.RelaxInfoTextBlock.Visibility = Visibility.Hidden; //
            // this.RelaxHumanGrid.Visibility = Visibility.Hidden; //
            this.RelaxChildrenGrid.Visibility = Visibility.Hidden; //

            this.InputCanvasGrid.Visibility = Visibility.Hidden;
            this.InputTextGrid.Visibility = Visibility.Hidden;
            this.InputRelaxMethodCanvasGrid.Visibility = Visibility.Hidden;
            this.InputRelaxMethodTextGrid.Visibility = Visibility.Hidden;
            this.InputMyBodyImageCanvasBorder.Visibility = Visibility.Hidden;
            this.InputMyBodyImageTextBorder.Visibility = Visibility.Hidden;
            this.CanvasEditGrid.Visibility = Visibility.Hidden;
            this.CompleteInputButton.Visibility = Visibility.Hidden;
            this.ViewSizeOfFeelingGrid.Visibility = Visibility.Hidden;

            this.RelaxTitleBorder.Visibility = Visibility.Hidden; //
            this.RelaxTitleTextBlock.Visibility = Visibility.Hidden; //

            this.DeepBreathGrid.Visibility = Visibility.Hidden; //
            this.DeepBreathShirojiImage.Visibility = Visibility.Hidden; //
            this.DeepBreathBubbleGrid.Visibility = Visibility.Hidden; //
            this.DeepBreathMessageTextBlock.Visibility = Visibility.Hidden; //

            this.RelaxShadeImage.Visibility = Visibility.Hidden; //

            this.RelaxMethodGrid.Visibility = Visibility.Hidden; //
            this.RelaxMethodTextBlock.Visibility = Visibility.Hidden; //
            this.RelaxMethodMessageTextBlock.Visibility = Visibility.Hidden; //
            this.NextGoButton.Visibility = Visibility.Hidden; //
            this.BackGoButton.Visibility = Visibility.Hidden; //

            this.SelectHotWordValueTitleTextBlock.Text = "";
            this.SessionSubTitleTextBlock.Text = "";
            this.SessionSentenceTextBlock.Text = "";
            this.ViewSizeOfFeelingTextBlock.Text = "";
            this.EndingMessageTextBlock.Text = "";
            this.MainMessageTextBlock.Text = "";
            this.MusicTitleTextBlock.Text = "";
            this.ComposerNameTextBlock.Text = "";
            this.ChildrenFeelingCommentTextBlock.Text = "";
            this.ChildrenFeelingCommentBigTextBlock.Text = "";
            this.ChildrenBodyImageTitleTextBlock.Text = ""; //
            this.SelectHotWordTitleTextBlock.Text = "";
            this.SelectHotWordValueTextBlock.Text = "";
            this.KindOfFeelingInputTextBlock.Text = "";
            this.SizeOfFeelingInputTextBlock.Text = "";

            this.ProblemTextBlock.Text = ""; //

            this.RelaxInfoTextBlock.Text = ""; //

            this.RelaxTitleTextBlock.Text = ""; //

            this.DeepBreathMessageTextBlock.Text = ""; //
            
            this.RelaxMethodTextBlock.Text = ""; //
            this.RelaxMethodMessageTextBlock.Text = ""; //
        }

        private void SetInputMethod()
        {
            if (this.dataOption.InputMethod == 0)
            {
                this.RelaxMethodInputListView.ItemContainerStyle = FindResource("CanvasInput") as Style;
                this.MyBodyImageInputTextBlock.Visibility = Visibility.Hidden;

                for(int i = 1; i < 8; i++)
                {
                    TextBlock textBlock = FindName($"BodyImageOfKimiTextBlock{i.ToString()}") as TextBlock;
                    textBlock.Visibility = Visibility.Hidden;
                }

                InputStroke = new Dictionary<string, StrokeCollection>()
                {
                    ["face_image_of_kimi"] = this.InputFaceImageOfKimiStroke =new StrokeCollection(),
                    ["hand_image_of_kimi"] = this.InputHandImageOfKimiStroke=new StrokeCollection(),
                    ["stomach_image_of_kimi"] = this.InputStomachImageOfKimiStroke=new StrokeCollection(),
                    ["others_image_of_kimi"] = this.InputOthersImageOfKimiStroke = new StrokeCollection(),
                    ["head_image_of_kimi"] = this.InputHeadImageOfKimiStroke=new StrokeCollection(),
                    ["shoulder_image_of_kimi"] = this.InputShoulderImageOfKimiStroke=new StrokeCollection(),
                    ["leg_image_of_kimi"] = this.InputLegImageOfKimiStroke=new StrokeCollection(),
                    ["not_understand_problem"] = this.InputMyBodyImageStrokeNotUnderstandProblem=new StrokeCollection(),
                    ["recorder_problem"] = this.InputMyBodyImageStrokeRecorderProblem=new StrokeCollection(),
                };

                this.InputRelaxMethodStrokes = new List<StrokeCollection>();


            }
            else
            {
                this.RelaxMethodInputListView.ItemContainerStyle = FindResource("TextInput") as Style;
                this.MyBodyImageInputInkCanvas.Visibility = Visibility.Hidden;

                for (int i = 1; i < 8; i++)
                {
                    InkCanvas canvas = FindName($"BodyImageOfKimiCanvas{i.ToString()}") as InkCanvas;
                    canvas.Visibility = Visibility.Hidden;
                }
                InputText = new Dictionary<string, PropertyInfo>()
                {
                    ["face_image_of_kimi"] = typeof(DataChapter5).GetProperty("InputFaceImageOfKimiText"),
                    ["hand_image_of_kimi"] = typeof(DataChapter5).GetProperty("InputHandImageOfKimiText"),
                    ["stomach_image_of_kimi"] = typeof(DataChapter5).GetProperty("InputStomachImageOfKimiText"),
                    ["others_image_of_kimi"] = typeof(DataChapter5).GetProperty("InputOthersImageOfKimiText"),
                    ["head_image_of_kimi"] = typeof(DataChapter5).GetProperty("InputHeadImageOfKimiText"),
                    ["shoulder_image_of_kimi"] = typeof(DataChapter5).GetProperty("InputShoulderImageOfKimiText"),
                    ["leg_image_of_kimi"] = typeof(DataChapter5).GetProperty("InputLegImageOfKimiText"),
                    ["not_understand_problem"] = typeof(DataChapter5).GetProperty("InputMyBodyImageTextNotUnderstandProblem"),
                    ["recorder_problem"] = typeof(DataChapter5).GetProperty("InputMyBodyImageTextRecorderProblem"),
                };

                InputRelaxMethodText = new List<string>();
            }
        }

        public void SetNextPage(InitConfig _initConfig, DataOption _dataOption, DataItem _dataItem, DataProgress _dataProgress, bool isCreateNewTable)
        {
            this.initConfig = _initConfig;
            this.dataOption = _dataOption;
            this.dataItem = _dataItem;
            this.dataProgress = _dataProgress;

            // 現在時刻を取得
            this.dataChapter5.CreatedAt = DateTime.Now.ToString();
            if (isCreateNewTable)
            {
                // データベースのテーブル作成と現在時刻の書き込みを同時に行う
                using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                {
                    // 毎回のアクセス日付を記録
                    connection.Insert(this.dataChapter5);
                }
            }
            else
            {
                string lastCreatedAt = "";

                using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                {
                    var chapter5 = connection.Query<DataChapter5>($"SELECT * FROM DataChapter5 ORDER BY Id DESC LIMIT 1;");

                    foreach (var row in chapter5)
                    {
                        lastCreatedAt = row.CreatedAt;
                    }
                }

                using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                {
                    connection.Execute($@"UPDATE DataChapter5 SET CreatedAt = '{this.dataChapter5.CreatedAt}'WHERE CreatedAt = '{lastCreatedAt}';");
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var csv = new CsvReader("./Scenarios/chapter5.csv"))
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

                    this.dataProgress.CurrentChapter = 5;

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

                    this.dataProgress.HasCompletedChapter5 = true;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET HasCompletedChapter5 = '{Convert.ToInt32(this.dataProgress.HasCompletedChapter5)}' WHERE Id = 1;");
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
                    this.dataProgress.LatestChapter5Scene = this.scene;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET CurrentScene = '{this.dataProgress.CurrentScene}', LatestChapter5Scene = '{this.dataProgress.LatestChapter5Scene}' WHERE Id = 1;");
                    }

                    if(this.scene == "キミちゃんのからだの変化")
                    {
                        if(this.dataOption.InputMethod == 0)
                        {
                            var list = this.InputStroke.Values.ToList();
                            for(int i =1;i<8; i++)
                            {
                                if(list[i-1] != null)
                                {
                                    InkCanvas canvas = FindName($"BodyImageOfKimiCanvas{i.ToString()}") as InkCanvas;
                                    canvas.Strokes = (StrokeCollection)list[i-1];
                                }
                            }
                        }
                        else
                        {
                            var list = this.InputText.Values.ToList();
                            for (int i = 1; i < 8; i++)
                            {
                                if (list[i-1] != null)
                                {
                                    TextBlock text = FindName($"BodyImageOfKimiTextBlock{i.ToString()}") as TextBlock;
                                    text.Text = (string)list[i-1].GetValue(this.dataChapter5);
                                }
                            }
                        }
                    }
                    else if (this.scene == "わからない問題")
                    {
                        this.FeelingDictionaryKey = "not_understand_problem";
                        this.InputDictionaryKey = "not_understand_problem";

                        
                        if (this.dataOption.InputMethod == 0)
                        {
                            this.MyBodyImageInputInkCanvas.Strokes = this.InputStroke[this.InputDictionaryKey];
                        }
                        else
                        {
                            this.MyBodyImageInputTextBlock.Text = (string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5);
                        }
                    }
                    else if (this.scene == "リコーダー問題")
                    {
                        this.FeelingDictionaryKey = "recorder_problem";
                        this.InputDictionaryKey = "recorder_problem";
                        if (this.dataOption.InputMethod == 0)
                        {
                            this.MyBodyImageInputInkCanvas.Strokes = this.InputStroke[this.InputDictionaryKey];
                        }
                        else
                        {
                            this.MyBodyImageInputTextBlock.Text =(string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5);
                        }
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

                    if (this.position == "next_go_button")
                    {
                        if(this.scene == "グループアクティビティ")
                        {
                            buttonObject.Visibility = Visibility.Hidden;
                            if (this.dataOption.InputMethod == 0)
                            {
                                foreach(InkCanvas canvas in this.RelaxMethodInputCanvasListView.GetChildren<InkCanvas>().ToList())
                                {
                                    if(canvas.Strokes.Count > 1)
                                    {
                                        buttonObject.Visibility = Visibility.Visible;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                foreach (TextBox text in this.RelaxMethodInputTextListView.GetChildren<TextBox>().ToList())
                                {
                                    if (text.Text!="")
                                    {
                                        buttonObject.Visibility = Visibility.Visible;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                   
                    
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

                    this.position = this.scenarios[this.scenarioCount][1];

                    var _textObject = this.textBlockObjects[this.position];

                    // _textObject.Visibility = Visibility.Hidden;

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var _message = this.scenarios[this.scenarioCount][2];

                        var _messages = this.SequenceCheck(_message);

                        this.ShowSentence(textObject: _textObject, sentences: _messages, mode: "msg");
                    }
                    else
                    {
                        if (_textObject.Text != "")
                        {
                            var _messages = this.SequenceCheck(_textObject.Text);

                            // xamlに直接書いた単一のStaticな文章を表示する場合
                            this.ShowSentence(textObject: _textObject, sentences: _messages, mode: "msg");
                        }
                        else
                        {
                            // TextBlockがInlineContentを含む場合は対処のしようがない
                        }
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

                            // xamlに直接書いた単一のStaticな文章を表示する場合
                            this.ShowSentence(textObject: __textObject, sentences: _texts, mode: "text");
                        }
                        else
                        {
                            // TextBlockがInlineContentを含む場合は何もせずそのまま表示
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
                                        if (this.SelectFeelingGrid.IsVisible)
                                        {
                                            //なにもしない
                                        }
                                        else if (this.SelectHeartGrid.IsVisible)
                                        {
                                            //なにもしない
                                        }
                                        else if (this.scene == "キミちゃんのからだの変化" && this.BodyImageOfKimiFearGrid.IsVisible)
                                        {
                                            if (this.dataOption.InputMethod == 0)
                                            {
                                                var list =InputStroke.Values.ToList();
                                                bool isVisible = true;
                                                for (int i =0; i < 7; i++)
                                                {
                                                    if (list[i] == null || ((StrokeCollection)list[i]).Count < 2)
                                                    {
                                                        isVisible = false;
                                                    }
                                                }
                                                if (isVisible)
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Visible;
                                                }
                                                else
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Hidden;
                                                }
                                                this.BackPageButton.Visibility = Visibility.Visible;
                                            }
                                            else
                                            {
                                                var list = InputText.Values.ToList();
                                                bool isVisible = true;
                                                for (int i = 0; i < 7; i++)
                                                {
                                                    if (list[i] == null || (string)list[i].GetValue(this.dataChapter5) == "")
                                                    {
                                                        isVisible = false;
                                                    }
                                                }
                                                if (isVisible)
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Visible;
                                                }
                                                else
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Hidden;
                                                }
                                                this.BackPageButton.Visibility = Visibility.Visible;

                                                if (this.InputStroke[this.InputDictionaryKey].Count > 1 && ((string)this.KindOfFeelings[this.InputDictionaryKey].GetValue(this.dataChapter5) != "" && (int)this.SizeOfFeelings[this.InputDictionaryKey].GetValue(this.dataChapter5) != -1))
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Visible;
                                                }
                                                this.BackPageButton.Visibility = Visibility.Visible;
                                            }
                                        }
                                        else if(this.scene =="わからない問題"|| this.scene == "リコーダー問題")
                                        {
                                            if(this.dataOption.InputMethod == 0)
                                            {
                                                if (this.InputStroke[this.InputDictionaryKey]!=null)
                                                {
                                                    if (this.InputStroke[this.InputDictionaryKey].Count > 1 && ((string)this.KindOfFeelings[this.InputDictionaryKey].GetValue(this.dataChapter5) != "" && (int)this.SizeOfFeelings[this.InputDictionaryKey].GetValue(this.dataChapter5) != -1))
                                                    {
                                                        this.NextPageButton.Visibility = Visibility.Visible;
                                                    }
                                                }
                                                this.BackPageButton.Visibility = Visibility.Visible;
                                            }
                                            else
                                            {
                                                if (this.InputText[this.InputDictionaryKey] !=null)
                                                {
                                                    if ((string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5) != "" && ((string)this.KindOfFeelings[this.InputDictionaryKey].GetValue(this.dataChapter5) != "" && (int)this.SizeOfFeelings[this.InputDictionaryKey].GetValue(this.dataChapter5) != -1))
                                                    {
                                                        this.NextPageButton.Visibility = Visibility.Visible;
                                                    }
                                                }
                                                this.BackPageButton.Visibility = Visibility.Visible;
                                            }
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
                                    if (this.scene == "キミちゃんのからだの変化" && this.BodyImageOfKimiFearGrid.IsVisible)
                                    {
                                        if (this.dataOption.InputMethod == 0)
                                        {
                                            var list = InputStroke.Values.ToList();
                                            bool isVisible = true;
                                            for (int i = 0; i < 7; i++)
                                            {
                                                if (list[i] == null || ((StrokeCollection)list[i]).Count < 2)
                                                {
                                                    isVisible = false;
                                                }
                                            }
                                            if (isVisible)
                                            {
                                                this.NextPageButton.Visibility = Visibility.Visible;
                                            }
                                            else
                                            {
                                                this.NextPageButton.Visibility = Visibility.Hidden;
                                            }
                                            this.BackPageButton.Visibility = Visibility.Visible;
                                        }
                                        else
                                        {
                                            var list = InputText.Values.ToList();
                                            bool isVisible = true;
                                            for (int i = 0; i < 7; i++)
                                            {
                                                if (list[i] == null || (string)list[i].GetValue(this.dataChapter5) == "")
                                                {
                                                    isVisible = false;
                                                }
                                            }
                                            if (isVisible)
                                            {
                                                this.NextPageButton.Visibility = Visibility.Visible;
                                            }
                                            else
                                            {
                                                this.NextPageButton.Visibility = Visibility.Hidden;
                                            }
                                            this.BackPageButton.Visibility = Visibility.Visible;
                                        }
                                    }
                                    else if (this.scene == "わからない問題" || this.scene == "リコーダー問題")
                                    {
                                        if (this.dataOption.InputMethod == 0)
                                        {
                                            if (this.InputStroke[this.InputDictionaryKey] != null)
                                            {
                                                if (this.InputStroke[this.InputDictionaryKey].Count > 1 && ((string)this.KindOfFeelings[this.InputDictionaryKey].GetValue(this.dataChapter5) != "" && (int)this.SizeOfFeelings[this.InputDictionaryKey].GetValue(this.dataChapter5) != -1))
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Visible;
                                                }
                                            }
                                            this.BackPageButton.Visibility = Visibility.Visible;
                                        }
                                        else
                                        {
                                            if (this.InputText[this.InputDictionaryKey] != null)
                                            {
                                                if ((string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5) != "" && ((string)this.KindOfFeelings[this.InputDictionaryKey].GetValue(this.dataChapter5) != "" && (int)this.SizeOfFeelings[this.InputDictionaryKey].GetValue(this.dataChapter5) != -1))
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Visible;
                                                }
                                            }
                                            this.BackPageButton.Visibility = Visibility.Visible;
                                        }
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

                        case "select_feeling":



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

                        if(GoToLabel == "current_scene")
                        {
                            switch (this.scene)
                            {
                                case "キミちゃんのからだの変化":
                                    this.GoTo("kimi's_body_image","sub");
                                    break;

                                case "わからない問題":
                                    this.GoTo("not_understand_problem","sub");
                                    break;

                                case "リコーダー問題":
                                    this.GoTo("recorder_problem","sub");
                                    break;

                                case "グループアクティビティ":
                                    this.GoTo("groupe_activity","sub");
                                    break;
                            }
                        }

                        this.GoTo(GoToLabel,"sub");
                    }
            
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

                    if (((string)this.KindOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter5)).Split(",")[1] == "良い")
                    {
                        this.SelectHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_red.png", UriKind.Relative));
                        this.SelectNeedleImage.Source = new BitmapImage(new Uri(@"./Images/red_needle.png", UriKind.Relative));
                    }
                    else if (((string)this.KindOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter5)).Split(",")[1] == "悪い")
                    {
                        this.SelectHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_blue.png", UriKind.Relative));
                        this.SelectNeedleImage.Source = new BitmapImage(new Uri(@"./Images/blue_needle.png", UriKind.Relative));
                    }

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "get_item":

                    this.dataItem.HasGotItem05 = true;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataItem SET HasGotItem05 = 1 WHERE Id = 1;");
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

                    Image[] itemMainImages = { this.Item01MainImage, this.Item02MainImage, this.Item03MainImage, this.Item04MainImage, this.Item06MainImage, this.Item07MainImage, this.Item08MainImage, this.Item09MainImage, this.Item10MainImage, this.Item11MainImage };
                    Image[] itemNoneImages = { this.Item01NoneImage, this.Item02NoneImage, this.Item03NoneImage, this.Item04NoneImage, this.Item06NoneImage, this.Item07NoneImage, this.Item08NoneImage, this.Item09NoneImage, this.Item10NoneImage, this.Item11NoneImage };

                    var hasGotItems = new bool[] { this.dataItem.HasGotItem01, this.dataItem.HasGotItem02, this.dataItem.HasGotItem03, this.dataItem.HasGotItem04, this.dataItem.HasGotItem06, this.dataItem.HasGotItem07, this.dataItem.HasGotItem08, this.dataItem.HasGotItem09, this.dataItem.HasGotItem10, this.dataItem.HasGotItem11 };

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

                case "deep_breath":

                    this.isDeepBreath = true;

                    break;

                case "fade_out":

                    Storyboard mainGridFadeOutSB = this.FindResource("fade_out_main_grid") as Storyboard;

                    if (mainGridFadeOutSB != null)
                    {
                        // 二重終了防止策
                        bool isDuplicate = false;

                        mainGridFadeOutSB.Completed += (s, e) =>
                        {
                            if (!isDuplicate)
                            {
                                this.scenarioCount += 1;
                                this.ScenarioPlay();

                                isDuplicate = true;
                            }
                        };
                        mainGridFadeOutSB.Begin(this);
                    }

                    break;

                case "fade_in":

                    Storyboard mainGridFadeInSB = this.FindResource("fade_in_main_grid") as Storyboard;

                    if (mainGridFadeInSB != null)
                    {
                        // 二重終了防止策
                        bool isDuplicate = false;

                        mainGridFadeInSB.Completed += (s, e) =>
                        {
                            if (!isDuplicate)
                            {
                                this.scenarioCount += 1;
                                this.ScenarioPlay();

                                isDuplicate = true;
                            }
                        };
                        mainGridFadeInSB.Begin(this);
                    }

                    break;

                case "base_color":


                    var baseGridBackgroundColor = this.scenarios[this.scenarioCount][1];

                    switch (baseGridBackgroundColor)
                    {
                        case "black":

                            this.BaseGrid.Background = Brushes.Black;

                            break;

                        case "white":

                            this.BaseGrid.Background = Brushes.White;

                            break;
                    }
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

        private void SetGaugeColor(string color)
        {
            if (color == "red")
            {
                this.SelectHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_red.png", UriKind.Relative));
                this.SelectNeedleImage.Source = new BitmapImage(new Uri(@"./Images/red_needle.png", UriKind.Relative));

                this.ViewSizeOfFeelingTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            else if (color == "blue")
            {
                this.SelectHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_blue.png", UriKind.Relative));
                this.SelectNeedleImage.Source = new BitmapImage(new Uri(@"./Images/blue_needle.png", UriKind.Relative));

                this.ViewSizeOfFeelingTextBlock.Foreground = new SolidColorBrush(Colors.Blue);
            }
        }

        private List<List<string>> SequenceCheck(string text)
        {
            var Matches = new Regex(@"\$(.+?)\$").Matches(text);

            for (int i = 0; i < Matches.Count; i++)
            {
                var sequence = Matches[i].Value;

                switch (sequence)
                {
                    case "$kind_of_feeling$":
                        text = text.Replace("$kind_of_feeling$", ((string)KindOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter5)).Split(",")[0]);

                        this.KindOfFeelingInputButton.IsEnabled = true;

                        if (text == "")
                            this.SizeOfFeelingInputButton.IsEnabled = false;
                        else
                            this.SizeOfFeelingInputButton.IsEnabled = true;
                        break;

                    case "$size_of_feeling$":
                        try
                        {
                            if ((int)this.SizeOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter5) != -1)
                            {
                                text = text.Replace("$size_of_feeling$", this.SizeOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter5).ToString());
                            }
                            else
                            {
                                text = text.Replace("$size_of_feeling$", "");
                            }
                        }
                        catch
                        {

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

        private void ShowSentence(TextBlock textObject, List<List<string>> sentences, string mode)
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
                            case "yellow": { textDecoration.Pen = new Pen(Brushes.Yellow, 1); break; };
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
                    this.runs[textObject.Name][inlineCount].Text = stns[0];
                    this.inlineCount++;
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
            // Debug.Print(this.isClickable.ToString());

            // 各種ボタンが押されたときの処理

            Button button = sender as Button;

            if (this.isClickable)
            {
                this.isClickable = false;

                if (button.Name == "BackMessageButton")
                {
                    this.BackMessageButton.Visibility = Visibility.Hidden;
                    this.NextMessageButton.Visibility = Visibility.Hidden;
                    this.BackPageButton.Visibility = Visibility.Hidden;
                    this.NextPageButton.Visibility = Visibility.Hidden;

                    BackScenario();
                }

                if (button.Name == "BackPageButton")
                {
                    this.BackMessageButton.Visibility = Visibility.Hidden;
                    this.NextMessageButton.Visibility = Visibility.Hidden;
                    this.BackPageButton.Visibility = Visibility.Hidden;
                    this.NextPageButton.Visibility = Visibility.Hidden;

                    BackScenario();
                }

                if (button.Name == "BackGoButton")
                {
                    this.BackGoButton.Visibility = Visibility.Hidden;
                    this.NextGoButton.Visibility = Visibility.Hidden;

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

                if (button.Name == "WishCheckButton")
                {
                    this.GoTo("remember_item","sub");
                }

                if (button.Name == "ImRememberButton")
                {
                    this.GoTo("manga","sub");
                }

                if (button.Name == "CheckMangaButton")
                {
                    switch (this.scene)
                    {
                        case "キミちゃんのきもちを考える1":

                            this.GoTo("manga_kimi_part","sub");
                            break;

                        case "青助くんのきもちを考える1":

                            this.GoTo("manga_aosuke_part","sub");
                            break;

                        default: { break; }
                    }
                }

                if (button.Name == "MangaPrevBackButton")
                {
                    switch (this.scene)
                    {
                        case "キミちゃんのきもちを考える1":

                            this.GoTo("think_kimi's_feeling_1","sub");
                            break;

                        case "青助くんのきもちを考える1":

                            this.GoTo("think_aosuke's_feeling_1","sub");
                            break;

                        default: { break; }
                    }
                }

                if (button.Name == "KindOfFeelingInputButton")
                {
                    this.SelectBadFeelingListBox.SelectedIndex = -1;
                    this.SelectGoodFeelingListBox.SelectedIndex = -1;

                    this.GoTo("kind_of_feeling","sub");
                }

                if (button.Name == "SizeOfFeelingInputButton")
                {
                    this.GoTo("size_of_feeling","sub");
                }

                if (button.Name == "FeelingNextGoButton")
                {
                    switch (this.scene)
                    {
                       

                        default: { break; }
                    }

                    this.SizeOfFeelingInputButton.IsEnabled = false;
                }

                if (this.scene == "さがしてみようホットワードボタン" && this.CHotWordKeyButtonGrid.IsVisible)
                {
                    var buttonText = button.GetChildren<TextBlock>().ToList()[0].Text;

                    var buttonIndex = Array.IndexOf(HOT_WORD_KEYS, buttonText);

                    var buttonImage = button.GetChildren<Image>().ToList()[0];

                    buttonImage.Source = this.Image2Gray(buttonImage.Source);

                    button.IsEnabled = false;

                    this.cHotWordButtonCount += 1;

                    this.GoTo($"search_hot_word_{buttonIndex + 1}","sub");
                }

                if (this.GHotWordKeyButtonGrid.IsVisible)
                {
                    var buttonText = button.GetChildren<TextBlock>().ToList()[0].Text;

                    Debug.Print(buttonText);

                    if (buttonText == this.scene.Replace("を選んでみよう", ""))
                    {
                        this.HotWordValueButtonItemControl.ItemsSource = HOT_WORD_VALUES[buttonText];

                        this.GoTo("select_hot_word_value","sub");
                    }
                    else
                    {
                        this.GoTo($"select_other_word_{Array.IndexOf(HOT_WORD_KEYS, buttonText) + 1}","sub");
                    }
                }

                if (this.HotWordValueButtonGrid.IsVisible)
                {
                    var targetHotWord = this.scene.Replace("を選んでみよう", "");

                    var hotWordIndex = Array.IndexOf(HOT_WORD_KEYS, targetHotWord);

                    if (HOT_WORD_VALUES[targetHotWord].Contains(button.Content.ToString()))
                    {
                        switch (hotWordIndex)
                        {
                            

                            default: { break; }
                        }
                        this.GoTo($"let's_use_selected_hot_word_{hotWordIndex + 1}","sub");
                    }
                }

                if (this.RolePlayButtonGrid.IsVisible)
                {
                    var rolePlayHotWord = button.Content.ToString().Substring(0, button.Content.ToString().Length - 11);

                    if (HOT_WORD_KEYS.Contains(rolePlayHotWord))
                    {
                        button.IsEnabled = false;

                        this.rolePlayButtonCount += 1;

                        this.GoTo($"role_play_hot_word_{Array.IndexOf(HOT_WORD_KEYS, rolePlayHotWord) + 1}","sub");
                    }
                }

                if (button.Name == "NextPageButton")
                {
                    this.BackMessageButton.Visibility = Visibility.Hidden;
                    this.NextMessageButton.Visibility = Visibility.Hidden;
                    this.BackPageButton.Visibility = Visibility.Hidden;
                    this.NextPageButton.Visibility = Visibility.Hidden;

                    if (this.cHotWordButtonCount >= HOT_WORD_KEYS.Length)
                    {
                        this.cHotWordButtonCount = 0;

                        this.GoTo("search_hot_word_complete","sub");
                    }
                    else if (this.rolePlayButtonCount >= HOT_WORD_KEYS.Length)
                    {
                        this.rolePlayButtonCount = 0;

                        this.GoTo("role_play_hot_word_complete","sub");
                    }
                    if (this.SelectFeelingGrid.IsVisible)
                    {
                        if (this.SelectBadFeelingListBox.SelectedItem != null || this.SelectGoodFeelingListBox.SelectedItem != null)
                        {
                            if (this.SelectGoodFeelingListBox.SelectedItem != null)
                            {
                                this.KindOfFeelings[FeelingDictionaryKey].SetValue(this.dataChapter5, $"{this.SelectGoodFeelingListBox.SelectedItem.ToString().Replace("●　", "")},良い");
                            }
                            else if (this.SelectBadFeelingListBox.SelectedItems != null)
                            {
                                this.KindOfFeelings[FeelingDictionaryKey].SetValue(this.dataChapter5, $"{this.SelectBadFeelingListBox.SelectedItem.ToString().Replace("●　", "")},悪い");
                            }

                            if(this.scene == "リコーダー問題")
                            {
                                this.dataChapter5.KindOfFeelingRecorderProblem = (string)this.KindOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter5);
                            }
                            else if (this.scene == "わからない問題")
                            {
                                this.dataChapter5.KindOfFeelingNotUnderstandProblem = (string)this.KindOfFeelings[FeelingDictionaryKey].GetValue(this.dataChapter5);
                            }

                            this.SizeOfFeelingInputButton.IsEnabled = true;
                        }

                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    else if (this.SelectHeartGrid.IsVisible)
                    {
                        this.SizeOfFeelings[FeelingDictionaryKey].SetValue(this.dataChapter5, int.Parse(this.ViewSizeOfFeelingTextBlock.Text));
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    else if (this.BodyImageOfKimiFearBorder.Visibility == Visibility.Visible)
                    {
                        if (this.dataOption.InputMethod == 0)
                        {
                            StrokeConverter strokeConverter = new StrokeConverter();

                            strokeConverter.ConvertToBmpImage(LeftBodyImageOfKimiCanvas,this.InputFaceImageOfKimiStroke,"challenge_time_face_image_of_kimi",this.initConfig.userName,this.dataProgress.CurrentChapter);
                            strokeConverter.ConvertToBmpImage(LeftBodyImageOfKimiCanvas, this.InputHandImageOfKimiStroke, "challengetime_hand_image_of_kimi", this.initConfig.userName,this.dataProgress.CurrentChapter);
                            strokeConverter.ConvertToBmpImage(LeftBodyImageOfKimiCanvas, this.InputStomachImageOfKimiStroke, "challengetime_stomach_image_of_kimi", this.initConfig.userName,this.dataProgress.CurrentChapter);
                            strokeConverter.ConvertToBmpImage(LeftBodyImageOfKimiCanvas, this.InputOthersImageOfKimiStroke, "challengetime_others_image_of_kimi", this.initConfig.userName,this.dataProgress.CurrentChapter);

                            strokeConverter.ConvertToBmpImage(RightBodyImageOfKimiCanvas, this.InputHeadImageOfKimiStroke, "challenge_time_head_image_of_kimi", this.initConfig.userName,this.dataProgress.CurrentChapter);
                            strokeConverter.ConvertToBmpImage(RightBodyImageOfKimiCanvas, this.InputShoulderImageOfKimiStroke, "challenge_time_shoulder_image_of_kimi", this.initConfig.userName,this.dataProgress.CurrentChapter);
                            strokeConverter.ConvertToBmpImage(RightBodyImageOfKimiCanvas, this.InputLegImageOfKimiStroke, "challenge_time_leg_image_of_kimi", this.initConfig.userName,this.dataProgress.CurrentChapter);
                        }
                        else
                        {
                            this.dataChapter5.InputFaceImageOfKimiText = (string)this.InputText["face_image_of_kimi"].GetValue(this.dataChapter5);
                            this.dataChapter5.InputHandImageOfKimiText = (string)this.InputText["hand_image_of_kimi"].GetValue(this.dataChapter5);
                            this.dataChapter5.InputStomachImageOfKimiText = (string)this.InputText["stomach_image_of_kimi"].GetValue(this.dataChapter5);
                            this.dataChapter5.InputOthersImageOfKimiText = (string)this.InputText["others_image_of_kimi"].GetValue(this.dataChapter5);
                            this.dataChapter5.InputHeadImageOfKimiText = (string)this.InputText["head_image_of_kimi"].GetValue(this.dataChapter5);
                            this.dataChapter5.InputShoulderImageOfKimiText = (string)this.InputText["shoulder_image_of_kimi"].GetValue(this.dataChapter5);
                            this.dataChapter5.InputLegImageOfKimiText = (string)this.InputText["leg_image_of_kimi"].GetValue(this.dataChapter5);

                            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                            {
                                connection.Execute($@"Update DataChapter5 SET InputFaceImageOfKimiText ='{this.dataChapter5.InputFaceImageOfKimiText}' WHERE CreatedAt ='{this.dataChapter5.CreatedAt}';");
                            }
                            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                            {
                                connection.Execute($@"Update DataChapter5 SET InputHandImageOfKimiText ='{this.dataChapter5.InputHandImageOfKimiText}' WHERE CreatedAt ='{this.dataChapter5.CreatedAt}';");
                            }
                            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                            {
                                connection.Execute($@"Update DataChapter5 SET InputStomachImageOfKimiText ='{this.dataChapter5.InputStomachImageOfKimiText}' WHERE CreatedAt ='{this.dataChapter5.CreatedAt}';");
                            }
                            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                            {
                                connection.Execute($@"Update DataChapter5 SET InputOthersImageOfKimiText ='{this.dataChapter5.InputOthersImageOfKimiText}' WHERE CreatedAt ='{this.dataChapter5.CreatedAt}';");
                            }
                            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                            {
                                connection.Execute($@"Update DataChapter5 SET InputHeadImageOfKimiText ='{this.dataChapter5.InputHeadImageOfKimiText}' WHERE CreatedAt ='{this.dataChapter5.CreatedAt}';");
                            }
                            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                            {
                                connection.Execute($@"Update DataChapter5 SET InputShoulderImageOfKimiText ='{this.dataChapter5.InputShoulderImageOfKimiText}' WHERE CreatedAt ='{this.dataChapter5.CreatedAt}';");
                            }
                            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                            {
                                connection.Execute($@"Update DataChapter5 SET InputLegImageOfKimiText ='{this.dataChapter5.InputLegImageOfKimiText}' WHERE CreatedAt ='{this.dataChapter5.CreatedAt}';");
                            }
                        }

                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    else if (this.MyStatusInputGrid.Visibility == Visibility.Visible)
                    {
                        if (this.scene =="リコーダー問題")
                        {
                            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                            {
                                connection.Execute($@"Update DataChapter5 SET KindOfFeelingRecorderProblem ='{this.dataChapter5.KindOfFeelingRecorderProblem}' WHERE CreatedAt ='{this.dataChapter5.CreatedAt}';");
                            }
                            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                            {
                                connection.Execute($@"Update DataChapter5 SET SizeOfFeelingRecorderProblem ='{this.dataChapter5.SizeOfFeelingRecorderProblem}' WHERE CreatedAt ='{this.dataChapter5.CreatedAt}';");
                            }

                            if(this.dataOption.InputMethod == 0)
                            {
                                StrokeConverter strokeConverter = new StrokeConverter(); 
                                strokeConverter.ConvertToBmpImage(InputMyBodyImageCanvas, this.InputStroke[this.InputDictionaryKey], "challenge_time_recorder_problem_input_stroke", this.initConfig.userName,this.dataProgress.CurrentChapter);
                            }
                            else
                            {
                                this.dataChapter5.InputMyBodyImageTextRecorderProblem = (string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5);
                                using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                                {
                                    connection.Execute($@"Update DataChapter5 SET InputMyBodyImageTextRecorderProblem ='{this.dataChapter5.InputMyBodyImageTextRecorderProblem}' WHERE CreatedAt='{this.dataChapter5.CreatedAt}';");
                                }
                            }
                        }
                        else if (this.scene == "わからない問題")
                        {
                            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                            {
                                connection.Execute($@"Update DataChapter5 SET KindOfFeelingNotUnderstandProblem ='{this.dataChapter5.KindOfFeelingNotUnderstandProblem}' WHERE CreatedAt ='{this.dataChapter5.CreatedAt}';");
                            }
                            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                            {
                                connection.Execute($@"Update DataChapter5 SET SizeOfFeelingNotUnderstandProblem ='{this.dataChapter5.SizeOfFeelingNotUnderstandProblem}' WHERE CreatedAt ='{this.dataChapter5.CreatedAt}';");
                            }

                            if (this.dataOption.InputMethod == 0)
                            {
                                StrokeConverter strokeConverter = new StrokeConverter();
                                strokeConverter.ConvertToBmpImage(InputMyBodyImageCanvas, this.InputStroke[this.InputDictionaryKey], "challenge_time_Not_Understand_problem_input_stroke", this.initConfig.userName,this.dataProgress.CurrentChapter);
                            }
                            else
                            {
                                this.dataChapter5.InputMyBodyImageTextNotUnderstandProblem = (string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5);
                                using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                                {
                                    connection.Execute($@"Update DataChapter5 SET InputMyBodyImageTextNotUnderstandProblem ='{this.dataChapter5.InputMyBodyImageTextNotUnderstandProblem}' WHERE CreatedAt='{this.dataChapter5.CreatedAt}';");
                                }
                            }
                        }

                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    else
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                }

                
   

                if (Regex.IsMatch(button.Name, "BodyImageOfKimiBubble.Button"))
                {
                    string number = button.Name.Substring(21, 1);

                    switch (number)
                    {
                        case "1":
                            this.InputDictionaryKey = "face_image_of_kimi";
                            break;

                        case "2":
                            this.InputDictionaryKey = "hand_image_of_kimi";
                            break;

                        case "3":
                            this.InputDictionaryKey = "stomach_image_of_kimi";
                            break;

                        case "4":
                            this.InputDictionaryKey = "others_image_of_kimi";
                            break;

                        case "5":
                            this.InputDictionaryKey = "head_image_of_kimi";
                            break;

                        case "6":
                            this.InputDictionaryKey = "shoulder_image_of_kimi";
                            break;

                        case "7":
                            this.InputDictionaryKey = "leg_image_of_kimi";
                            break;
                    }

                    if (this.dataOption.InputMethod == 0)
                    {
                        
                        if(number =="1"|| number == "2" || number == "3" || number == "4")
                        {
                            this.LeftBodyImageOfKimiBubbleCanvasText.Text = ((TextBlock)FindName($"BodyImageOfKimiBubble{number}Text")).Text;
                            this.LeftCanvasSpeechBubbleTail.Data = ((System.Windows.Shapes.Path)FindName($"SpeechBubbleTail{number}")).Data;
                            this.LeftCanvasSpeechBubbleTailMask.Data = ((System.Windows.Shapes.Path)FindName($"SpeechBubbleTailMask{number}")).Data;

                            var canvas = FindName($"BodyImageOfKimiCanvas{number}") as InkCanvas;
                            this.InputStroke[this.InputDictionaryKey] = canvas.Strokes;
                            this.LeftBodyImageOfKimiCanvas.Strokes = this.InputStroke[this.InputDictionaryKey];
                            this.GoTo("canvas_input_kimi's_body_image_left","sub");
                        }
                        else
                        {
                            this.RightBodyImageOfKimiBubbleCanvasText.Text = ((TextBlock)FindName($"BodyImageOfKimiBubble{number}Text")).Text;
                            this.RightCanvasSpeechBubbleTail.Data = ((System.Windows.Shapes.Path)FindName($"SpeechBubbleTail{number}")).Data;
                            this.RightCanvasSpeechBubbleTailMask.Data = ((System.Windows.Shapes.Path)FindName($"SpeechBubbleTailMask{number}")).Data;

                            var canvas = FindName($"BodyImageOfKimiCanvas{number}") as InkCanvas;
                            this.InputStroke[this.InputDictionaryKey] = canvas.Strokes;
                            this.RightBodyImageOfKimiCanvas.Strokes = this.InputStroke[this.InputDictionaryKey]; 
                            this.GoTo("canvas_input_kimi's_body_image_right","sub");
                        }
                    }
                    else
                    {
                        if (number == "1" || number == "2" || number == "3" || number == "4")
                        {
                            this.LeftBodyImageOfKimiBubbleTextBoxText.Text = ((TextBlock)FindName($"BodyImageOfKimiBubble{number}Text")).Text;
                            this.LeftTextBoxSpeechBubbleTail.Data = ((System.Windows.Shapes.Path)FindName($"SpeechBubbleTail{number}")).Data;
                            this.LeftTextBoxSpeechBubbleTailMask.Data = ((System.Windows.Shapes.Path)FindName($"SpeechBubbleTailMask{number}")).Data;

                            this.LeftBodyImageOfKimiTextBox.Text= (string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5);
                            this.GoTo("keyboard_input_kimi's_body_image_left","sub");
                            this.LeftBodyImageOfKimiTextBox.Focus();

                        }
                        else
                        {
                            this.RightBodyImageOfKimiBubbleTextBoxText.Text = ((TextBlock)FindName($"BodyImageOfKimiBubble{number}Text")).Text;
                            this.RightTextBoxSpeechBubbleTail.Data = ((System.Windows.Shapes.Path)FindName($"SpeechBubbleTail{number}")).Data;
                            this.RightTextBoxSpeechBubbleTailMask.Data = ((System.Windows.Shapes.Path)FindName($"SpeechBubbleTailMask{number}")).Data;

                            this.RightBodyImageOfKimiTextBox.Text = (string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5);
                            this.GoTo("keyboard_input_kimi's_body_image_right","sub");
                            this.RightBodyImageOfKimiTextBox.Focus();
                        }
                    }
                }
                if (button.Name == "MyBodyImageInputButton")
                {
                    
                    if (this.dataOption.InputMethod == 0)
                    {
                        this.InputStroke[this.InputDictionaryKey] = this.MyBodyImageInputInkCanvas.Strokes;
                        this.InputMyBodyImageCanvas.Strokes = this.InputStroke[this.InputDictionaryKey];
                        this.GoTo("canvas_input_my_body_image","sub");
                    }
                    else
                    {
                        this.InputText[this.InputDictionaryKey].SetValue(this.dataChapter5, this.MyBodyImageInputTextBlock.Text);
                        this.InputMyBodyImageTextBox.Text = (string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5);
                        this.GoTo("keyboard_input_my_body_image","sub");
                        this.InputMyBodyImageTextBox.Focus();
                    }

                }
               
                if (button.Name == "RelaxMethodInputButton")
                {
                    this.RelaxMethodInputButton_Click();
                }
                if (Regex.IsMatch(button.Name ,".*PlusButton"))
                {
                    button.IsEnabled = false;

                    if (this.dataOption.InputMethod == 0)
                    {
                        if (this.RelaxMethodInputCanvasListView.Items.Count < this.RELAX_METHOD_COUNT.Length)
                        {
                            this.RelaxMethodInputCanvasListView.Items.Add(this.RELAX_METHOD_COUNT[this.RelaxMethodInputCanvasListView.Items.Count]);
                        }
                    }
                    else
                    {
                        if (this.RelaxMethodInputTextListView.Items.Count < this.RELAX_METHOD_COUNT.Length)
                        {
                            this.RelaxMethodInputTextListView.Items.Add(this.RELAX_METHOD_COUNT[this.RelaxMethodInputTextListView.Items.Count]);
                        }

                            
                    }
                    this.RelaxMethodInputListView.Items.Add(this.RELAX_METHOD_COUNT[this.RelaxMethodInputListView.Items.Count]);
                    this.CheckInputMinusButton();
                    this.isClickable = true;
                }

                if (Regex.IsMatch(button.Name, ".*MinusButton"))
                {
                    if (this.dataOption.InputMethod == 0)
                    {
                        this.RelaxMethodInputCanvasListView.Items.Remove(this.RelaxMethodInputCanvasListView.Items[this.RelaxMethodInputCanvasListView.Items.Count - 1]);
                    }
                    else
                    {
                        this.RelaxMethodInputTextListView.Items.Remove(this.RelaxMethodInputTextListView.Items[this.RelaxMethodInputTextListView.Items.Count - 1]);
                    }
                    this.RelaxMethodInputListView.Items.Remove(this.RelaxMethodInputListView.Items[this.RelaxMethodInputListView.Items.Count - 1]);
                    this.CheckInputPlusButton();
                    this.CheckInputMinusButton();
                    this.isClickable = true;
                }

                if (button.Name == "CompleteInputButton")
                {
                    if (this.scene == "キミちゃんのからだの変化")
                    {
                        if (this.dataOption.InputMethod == 0)
                        {
                            if (this.InputStroke[this.InputDictionaryKey] != null)
                            {
                                if (this.LeftBodyImageOfKimiBubbleCanvasGrid.IsVisible)
                                {
                                    this.ClipStrokes(this.LeftBodyImageOfKimiCanvas, this.LeftBodyImageOfKimiCanvas.Strokes);
                                    this.InputStroke[this.InputDictionaryKey] = this.LeftBodyImageOfKimiCanvas.Strokes;

                                    switch (this.InputDictionaryKey)
                                    {
                                        case "face_image_of_kimi":
                                            this.BodyImageOfKimiCanvas1.Strokes = this.InputStroke[this.InputDictionaryKey];
                                            break;

                                        case "hand_image_of_kimi":
                                            this.BodyImageOfKimiCanvas2.Strokes = this.InputStroke[this.InputDictionaryKey];
                                            break;

                                        case "stomach_image_of_kimi":
                                            this.BodyImageOfKimiCanvas3.Strokes = this.InputStroke[this.InputDictionaryKey];
                                            break;

                                        case "others_image_of_kimi":
                                            this.BodyImageOfKimiCanvas4.Strokes = this.InputStroke[this.InputDictionaryKey];
                                            break;
                                    }
                                }
                                else if (this.RightBodyImageOfKimiBubbleCanvasGrid.IsVisible)
                                {
                                    this.ClipStrokes(this.RightBodyImageOfKimiCanvas, this.RightBodyImageOfKimiCanvas.Strokes);
                                    this.InputStroke[this.InputDictionaryKey] = this.RightBodyImageOfKimiCanvas.Strokes;

                                    switch (this.InputDictionaryKey)
                                    {
                                        case "head_image_of_kimi":
                                            this.BodyImageOfKimiCanvas5.Strokes = this.InputStroke[this.InputDictionaryKey];
                                            break;

                                        case "shoulder_image_of_kimi":
                                            this.BodyImageOfKimiCanvas6.Strokes = this.InputStroke[this.InputDictionaryKey];
                                            break;

                                        case "leg_image_of_kimi":
                                            this.BodyImageOfKimiCanvas7.Strokes = this.InputStroke[this.InputDictionaryKey];
                                            break;

                                    }
                                }
                                
                            }
                        }
                        else
                        {
                            if (this.LeftBodyImageOfKimiBubbleTextBoxGrid.IsVisible)
                            {
                                this.InputText[this.InputDictionaryKey].SetValue(this.dataChapter5,this.LeftBodyImageOfKimiTextBox.Text);
                                switch (this.InputDictionaryKey)
                                {
                                    case "face_image_of_kimi":
                                        this.BodyImageOfKimiTextBlock1.Text = (string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5);
                                        break;

                                    case "hand_image_of_kimi":
                                        this.BodyImageOfKimiTextBlock2.Text = (string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5);
                                        break;

                                    case "stomach_image_of_kimi":
                                        this.BodyImageOfKimiTextBlock3.Text = (string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5);
                                        break;

                                    case "others_image_of_kimi":
                                        this.BodyImageOfKimiTextBlock4.Text = (string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5);
                                        break;
                                }

                                this.CloseOSK();

                            }
                            else if (this.RightBodyImageOfKimiBubbleTextBoxGrid.IsVisible)
                            {
                                this.InputText[this.InputDictionaryKey].SetValue(this.dataChapter5,this.RightBodyImageOfKimiTextBox.Text);
                                this.MyBodyImageInputTextBlock.Text = (string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5);

                                switch (this.InputDictionaryKey)
                                {
                                    case "head_image_of_kimi":
                                        this.BodyImageOfKimiTextBlock5.Text = (string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5);
                                        break;

                                    case "shoulder_image_of_kimi":
                                        this.BodyImageOfKimiTextBlock6.Text = (string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5);
                                        break;

                                    case "leg_image_of_kimi":
                                        this.BodyImageOfKimiTextBlock7.Text = (string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5);
                                        break;

                                }

                                this.CloseOSK();
                            }
                        }
                    }
                    else if (this.scene == "わからない問題" || this.scene == "リコーダー問題")
                    {
                        if (this.dataOption.InputMethod == 0)
                        {
                            if (this.InputStroke[this.InputDictionaryKey] != null)
                            {
                                this.ClipStrokes(this.InputMyBodyImageCanvas, this.InputMyBodyImageCanvas.Strokes);
                                this.InputStroke[this.InputDictionaryKey] = this.MyBodyImageInputInkCanvas.Strokes;
                                this.MyBodyImageInputInkCanvas.Strokes = this.InputStroke[this.InputDictionaryKey];
                            }
                        }
                        else
                        {
                            this.InputText[this.InputDictionaryKey].SetValue(this.dataChapter5,this.InputMyBodyImageTextBox.Text);
                            this.MyBodyImageInputTextBlock.Text = (string)this.InputText[this.InputDictionaryKey].GetValue(this.dataChapter5);

                            this.CloseOSK();
                        }
                    }

                    else if(this.scene == "グループアクティビティ")
                    {
                        this.SetRelaxMethodInputListView();
                        if (this.dataOption.InputMethod == 1)
                            this.CloseOSK();
                    }

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }

                if (button.Name == "NextMessageButton")
                {
                    this.BackMessageButton.Visibility = Visibility.Hidden;
                    this.NextMessageButton.Visibility = Visibility.Hidden;
                    this.BackPageButton.Visibility = Visibility.Hidden;
                    this.NextPageButton.Visibility = Visibility.Hidden;

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }

                if (button.Name == "NextGoButton")
                {
                    this.NextGoButton.Visibility = Visibility.Hidden;
                    this.BackGoButton.Visibility = Visibility.Hidden;

                    if (this.RelaxMethodGrid.Visibility == Visibility.Visible)
                    {
                        int number = 1;
                        if (this.dataOption.InputMethod == 0)
                        {
                            foreach (var canvas in this.RelaxMethodInputCanvasListView.GetChildren<InkCanvas>())
                            {
                                StrokeConverter strokeConverter = new StrokeConverter();
                                strokeConverter.ConvertToBmpImage(canvas, canvas.Strokes, $"groupe_avtivity_relax_method_{number}", this.initConfig.userName, this.dataProgress.CurrentChapter);
                                number ++;
                            }
                        }
                        else
                        {
                            this.dataChapter5.InputRelaxMethodText = "";

                            foreach (var text in this.RelaxMethodInputTextListView.GetChildren<TextBlock>())
                            {
                                this.dataChapter5.InputRelaxMethodText += $"{text.Text},";
                            }

                            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                            {
                                connection.Execute($"Update DataChapter5 SET InputRelaxMethodText ='{this.dataChapter5.InputRelaxMethodText}' WHERE CreatedAt ='{this.dataChapter5.CreatedAt}';");
                            }
                        }
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    else
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                }

                if (button.Name == "MangaFlipButton")
                {
                    this.MangaFlipButton.Visibility = Visibility.Hidden;
                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }

                if (button.Name == "CompleteNextButton")
                {
                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }

                if (button.Name == "SelectHotWordButton")
                {
                    this.GoTo("select_hot_word_key","sub");
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

                titlePage.SetReloadPageFlag(false);

                titlePage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                this.NavigationService.Navigate(titlePage);
            }

            if (button.Name == "ExitBackNoButton")
            {
                this.ExitBackGrid.Visibility = Visibility.Hidden;
                this.CoverLayerImage.Visibility = Visibility.Hidden;

                this.isClickable = true;
            }

            if (button.Name == "ReturnToTitleButton")
            {
                TitlePage titlePage = new TitlePage();

                titlePage.SetReloadPageFlag(false);

                titlePage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                this.NavigationService.Navigate(titlePage);

                this.StopBGM();
            }
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
        private static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(Chapter5), new UIPropertyMetadata(0.0));

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


            if (this.isDeepBreath && this.DeepBreathGrid.IsVisible)
            {
                this.isDeepBreath = false;

                int deepBreathCount = 0;

                string[] deepBreathWipeAnimes = { "wipe_deep_breath_count5_image", "wipe_deep_breath_count1_image", "wipe_deep_breath_count10_image" };

                var maskOfCount5Image = this.DeepBreathCount5Image.OpacityMask.CloneCurrentValue();
                var maskOfCount1Image = this.DeepBreathCount1Image.OpacityMask.CloneCurrentValue();
                var maskOfCount10Image = this.DeepBreathCount10Image.OpacityMask.CloneCurrentValue();

                DeepBreathWipeAnime();

                void DeepBreathWipeAnime()
                {
                    Storyboard sb = this.FindResource(deepBreathWipeAnimes[deepBreathCount]) as Storyboard;

                    if (sb != null)
                    {
                        // 二重終了防止策
                        bool isDuplicate = false;

                        sb.Completed += (s, e) =>
                        {
                            if (!isDuplicate)
                            {
                                deepBreathCount += 1;

                                if (deepBreathCount < deepBreathWipeAnimes.Length)
                                {
                                    DeepBreathWipeAnime();
                                }
                                else
                                {
                                    this.DeepBreathCount5Image.OpacityMask = maskOfCount5Image;
                                    this.DeepBreathCount1Image.OpacityMask = maskOfCount1Image;
                                    this.DeepBreathCount10Image.OpacityMask = maskOfCount10Image;

                                    this.scenarioCount += 1;
                                    this.ScenarioPlay();
                                }
                                isDuplicate = true;
                            }
                        };
                        sb.Begin(this);
                    }
                }
            }
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

            int MaxLineCount = 0;

            switch (text.Name)
            {
                case "RelaxMethodInputTextBox":
                    MaxLineCount = 2;
                    break;

                case "InputMyBodyImageTextBox":
                    MaxLineCount = 5;
                    break;

                case "LeftBodyImageOfKimiTextBox":
                    MaxLineCount = 3;
                    break;

                case "RightBodyImageOfKimiTextBox":
                    MaxLineCount = 3;
                    break;
            }

            while (text.LineCount > MaxLineCount)
            {
                caretPosition -= 1;
                text.Text = text.Text.Remove(caretPosition, 1);
            }

            text.Select(caretPosition, 0);
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            TextBox text = sender as TextBox;
            int MaxLineCount = 0;

            switch (text.Name)
            {
                case "RelaxMethodInputTextBox":
                    MaxLineCount = 2;
                    break;

                case "InputMyBodyImageTextBox":
                    MaxLineCount = 5;
                    break;

                case "LeftBodyImageOfKimiTextBox":
                    MaxLineCount = 3;
                    break;

                case "RightBodyImageOfKimiTextBox":
                    MaxLineCount = 3;
                    break;
            }
            if (text.LineCount > MaxLineCount-1)
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

                    if (this.scene == "キミちゃんのからだの変化")
                    {
                        if (this.RightBodyImageOfKimiBubbleCanvasGrid.IsVisible)
                        {
                            this.RightBodyImageOfKimiCanvas.EditingMode = InkCanvasEditingMode.Ink;
                        }
                        else
                        {
                            this.LeftBodyImageOfKimiCanvas.EditingMode = InkCanvasEditingMode.Ink;
                        }
                    }
                    else if (this.scene == "わからない問題" || this.scene == "リコーダー問題")
                    {
                        this.InputMyBodyImageCanvas.EditingMode = InkCanvasEditingMode.Ink;
                    }
                    else if (this.scene == "グループアクティビティ")
                    {
                        foreach (InkCanvas canvas in this.RelaxMethodInputCanvasListView.GetChildren<InkCanvas>().ToList())
                        {
                            canvas.EditingMode = InkCanvasEditingMode.Ink;
                            this.ClipStrokes(canvas, canvas.Strokes);
                        }
                    }
                    break;

                case "EraserButton":


                    if (this.scene == "キミちゃんのからだの変化")
                    {
                        if (this.RightBodyImageOfKimiBubbleCanvasGrid.IsVisible)
                        {
                            this.RightBodyImageOfKimiCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                        }
                        else
                        {
                            this.LeftBodyImageOfKimiCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                        }

                    }
                    else if (this.scene == "わからない問題" || this.scene == "リコーダー問題")
                    {
                        this.InputMyBodyImageCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    }
                    else if (this.scene =="グループアクティビティ")
                    {
                        foreach (InkCanvas canvas in this.RelaxMethodInputCanvasListView.GetChildren<InkCanvas>().ToList())
                        {
                            canvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                        }
                    }
                    
                    break;

                case "AllClearButton":


                    if (this.scene == "キミちゃんのからだの変化")
                    {
                        if (this.RightBodyImageOfKimiBubbleCanvasGrid.IsVisible)
                        {
                            this.RightBodyImageOfKimiCanvas.Strokes.Clear();
                        }
                        else
                        {
                            this.LeftBodyImageOfKimiCanvas.Strokes.Clear();
                        }
                    }
                    else if (this.scene == "わからない問題" || this.scene == "リコーダー問題")
                    {
                        this.InputMyBodyImageCanvas.Strokes.Clear();
                    }
                    else if (this.scene =="グループアクティビティ")
                    {
                        this.RelaxMethodInputCanvasListView.Items.Clear();
                        this.RelaxMethodInputCanvasListView.Items.Add("①");
                        this.RelaxMethodInputListView.Items.Clear();
                        this.RelaxMethodInputListView.Items.Add("①");

                        this.InputCanvasPlusButton.IsEnabled = false;
                        this.InputCanvasMinusButton.IsEnabled = false;
                    }
                    
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

        private void InkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            this.CheckInputPlusButton();
        }

        private void InkCanvas_StrokeErased(object sender, RoutedEventArgs e)
        {
            this.CheckInputPlusButton();
        }

        private void SetRelaxMethodInputListView()
        {
            int index = 0;
            if (this.dataOption.InputMethod == 0)
            {
                this.InputRelaxMethodStrokes.Clear();
                foreach (InkCanvas canvas1 in this.RelaxMethodInputCanvasListView.GetChildren<InkCanvas>().ToList())
                {
                    this.ClipStrokes(canvas1, canvas1.Strokes);
                    this.InputRelaxMethodStrokes.Add(canvas1.Strokes);
                }
                foreach (Viewbox viewbox in this.RelaxMethodInputListView.GetChildren<Viewbox>().ToList())
                {
                    InkCanvas canvas2 = viewbox.Child as InkCanvas;
                    if (this.InputRelaxMethodStrokes[index] != canvas2.Strokes)
                        canvas2.Strokes = this.InputRelaxMethodStrokes[index];
                    index++;
                }
            }
            else
            {
                this.InputRelaxMethodText.Clear();
                foreach (TextBox text1 in this.RelaxMethodInputTextListView.GetChildren<TextBox>().ToList())
                {
                    this.InputRelaxMethodText.Add(text1.Text);
                }
                //foreach (var panel in this.RelaxMethodInputListView.GetChildren<ListViewItem>().ToList())
                //{

                //    foreach (var aaa in this.RelaxMethodInputTextListView.Items)
                //    {
                //        try
                //        {
                //            Debug.Print(aaa.ToString());
                //        }
                //        catch
                //        {

                //        }
                //    }
                //    Debug.Print(index.ToString());
                //    index++;
                //}
                foreach (TextBlock text2 in this.RelaxMethodInputListView.GetChildren<TextBlock>().ToList())
                {
                    if (this.RELAX_METHOD_COUNT.Contains(text2.Text) == false)
                    {
                        if (this.InputRelaxMethodText[index] != text2.Text)
                        {
                            text2.Text = this.InputRelaxMethodText[index];

                        }
                        index++;
                    }

                }
            }
        }

        private void CheckInputPlusButton()
        {
            bool isEnaledFlag = true;
            //空白の欄を埋めないとListViewのItemを追加できないようにする。
            if (this.dataOption.InputMethod == 0)
            {
                foreach (var canvas in this.RelaxMethodInputCanvasListView.GetChildren<InkCanvas>().ToList())
                {
                    if (((InkCanvas)canvas).Strokes.Count < 1)
                    {
                        isEnaledFlag = false;
                    }
                }
                if (this.RelaxMethodInputListView.Items.Count < this.RELAX_METHOD_COUNT.Length &&isEnaledFlag)
                {
                    this.InputCanvasPlusButton.IsEnabled = true;
                }
                else
                {
                    this.InputCanvasPlusButton.IsEnabled = false;
                }

            }
            else
            {
                foreach (var text in this.RelaxMethodInputTextListView.GetChildren<TextBox>().ToList())
                {
                    if (((TextBox)text).Text == "")
                    {
                        isEnaledFlag = false;
                    }
                }
                if (this.RelaxMethodInputListView.Items.Count < this.RELAX_METHOD_COUNT.Length && isEnaledFlag)
                {
                    this.InputKeyboardPlusButton.IsEnabled = true;
                }
                else
                {
                    this.InputKeyboardPlusButton.IsEnabled = false;
                }
            }
            
        }

        private void CheckInputMinusButton()
        {
            //空白の欄を埋めないとListViewのItemを追加できないようにする。
            if (this.dataOption.InputMethod == 0)
            {
                if (this.RelaxMethodInputCanvasListView.Items.Count > 1)
                {
                    this.InputCanvasMinusButton.IsEnabled = true;
                }
                else
                {
                    this.InputCanvasMinusButton.IsEnabled = false;
                }
            }
            else
            {
                if (this.RelaxMethodInputTextListView.Items.Count > 1)
                {
                    this.InputKeyboardMinusButton.IsEnabled = true;
                }
                else
                {
                    this.InputKeyboardMinusButton.IsEnabled = false;
                }
            }

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.CheckInputPlusButton();
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            var converter = new BrushConverter();
            if (Regex.IsMatch(button.Name, "BodyImageOfKimiBubble.Button"))
            {
                string number = button.Name.Substring(21, 1);
                Border border = FindName($"BodyImageOfKimiBubble{number}Border") as Border;
                border.Background = (Brush)converter.ConvertFromString("#FFBFE6FD");
            }
            else if (button.Name == "RelaxMethodInputButton")
            {
                RelaxMethodInputRectangle.Fill = (Brush)converter.ConvertFromString("#FFBFE6FD");
                RelaxMethodInputListView.Background = (Brush)converter.ConvertFromString("#FFBFE6FD");
            }
          
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            var converter = new BrushConverter();

            if (Regex.IsMatch(button.Name, "BodyImageOfKimiBubble.Button"))
            {
                string number = button.Name.Substring(21, 1);
                Border border = FindName($"BodyImageOfKimiBubble{number}Border") as Border;
                border.Background = (Brush)converter.ConvertFromString("#FFFFFFFF");
            }
            else if (button.Name == "RelaxMethodInputButton")
            {
                RelaxMethodInputRectangle.Fill = (Brush)converter.ConvertFromString("#FFFFFFFF");
                RelaxMethodInputListView.Background = (Brush)converter.ConvertFromString("#FFFFFFFF");
            }
        }

        private void RelaxMethodInputButton_Click()
        {
            if (this.dataOption.InputMethod == 0)
            {
                this.GoTo("canvas_input_relax_method","sub");
            }
            else
            {
                this.GoTo("keyboard_input_relax_method","sub");
                foreach (TextBox text1 in this.RelaxMethodInputTextListView.GetChildren<TextBox>().ToList())
                {
                    text1.Focus();
                    break;
                }
            }
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.RelaxMethodInputButton_Click();
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
