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

namespace KokoroUpTime
{
    /// <summary>
    /// GameWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class Chapter1 : Page
    {
        private float MESSAGE_SPEED = 30.0f;

        private int scenarioCount = 0;
        private List<List<string>> scenarios = null;

        private string position = "";

        private bool isClickable = false;
        private int tapCount = 0;

        private int feelingSize = 0;

        // メッセージ表示関連
        private DispatcherTimer msgTimer;
        private int word_num;

        private Dictionary<string, Image> imageObjects = null;
        private Dictionary<string, TextBlock> textBlockObjects = null;
        private Dictionary<string, Button> buttonObjects = null;
        private Dictionary<string, Grid> gridObjects = null;

        private CheckBox[] checkBoxs;

        private WindowsMediaPlayer mediaPlayer;
        private SoundPlayer sePlayer = null;

        public Chapter1()
        {
            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            // メディアプレーヤークラスのインスタンスを作成する
            this.mediaPlayer = new WindowsMediaPlayer();

            this.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
            this.MouseUp += new MouseButtonEventHandler(OnMouseUp);
            this.MouseMove += new MouseEventHandler(OnMouseMove);

            this.InitControls();
        }

        private void InitControls()
        {
            this.imageObjects = new Dictionary<string, Image>
            {
                ["bg_image"] = this.BackgroundImage, //
                ["manga_title_image"] = this.MangaTitleImage, //
                ["manga_image"] = this.MangaImage, //
                ["item_center_image"] = this.ItemCenterImage, //
                ["item_left_image"] = this.ItemLeftImage, //
                ["item_left_last_image"] = this.ItemLeftLastImage,
                ["item_last_info_image"] = this.ItemLastInfoImage,
                // ["item_name_plate_center_image"] = this.ItemNamePlateCenterImage,
                // ["item_name_bubble_image"] = this.ItemNameBubbleImage,
                // ["item_name_plate_left_image"] = this.ItemNamePlateLeftImage,
                // ["item_info_plate_image"] = this.ItemInfoPlateImage,
                ["session_frame_image"] = this.SessionFrameImage,
                ["session_title_image"] = this.SessionTitleImage,
                ["challenge_good_feeling_image"] = this.ChallengeGoodFeelingImage,
                ["challenge_bad_feeling_image"] = this.ChallengeBadFeelingImage,
                ["challenge_title_image"] = this.ChallengeTitleImage,
                ["challenge_small_bubble_image"] = this.ChallengeSmallBubbleImage,
                ["challenge_item_image"] = this.ChallengeItemImage,
                ["challenge_message_box_image"] = this.ChallengeMessageBoxImage,
                ["challenge_complete_image"] = this.ChallengeCompleteImage,
                ["challenge_next_image"] = this.ChallengeNextImage,
                ["select_good_feeling_image"] = this.SelectGoodFeelingImage,
                ["select_bad_feeling_image"] = this.SelectBadFeelingImage,
                ["select_item_feeling_image"] = this.SelectItemFeelingImage,

                ["kimi_plate_outer_image"] = this.KimiPlateOuterImage,
                ["kimi_in_plate_image"] = this.KimiInPlateImage,
                ["kimi_plate_inner_up_image"] = this.KimiPlateInnerUpImage,
                ["kimi_plate_inner_down_image"] = this.KimiPlateInnerDownImage,

                ["view_kind_of_feeling_image"] = this.ViewKindOfFeelingImage,
                ["view_feeling_value_image"] = this.ViewFeelingValueImage,
                ["heart_image"] = this.HeartImage,
                ["needle_image"] = this.NeedleImage,

                ["summary_board_image"] = this.SummaryBoardImage,
                ["summary_title_image"] = this.SummaryTitleImage,
                ["children_info_image"] = this.ChildrenInfoImage, //
                
                ["ending_title_back_image"] = this.EndingTitleBackImage,
                ["ending_item_image"] = this.EndingItemImage,
                ["shiroji_ending_image"] = this.ShirojiEndingImage,
                ["kimi_ending_image"] = this.KimiEndingImage,
                ["aosuke_ending_image"] = this.AosukeEndingImage,
                ["akamaru_ending_image"] = this.AkamaruEndingImage,
                ["ending_short_bubble_image"] = this.EndingShortBubbleImage,

                ["shiroji_right_image"] = this.ShirojiRightImage, //
                ["shiroji_right_up_image"] = this.ShirojiRightUpImage,
                ["shiroji_small_right_up_image"] = this.ShirojiSmallRightUpImage, //
                ["shiroji_small_right_down_image"] = this.ShirojiSmallRightDownImage, //
                ["shiroji_right_center_image"] = this.ShirojiRightCenterImage,
                ["shiroji_small_right_center_image"] = this.ShirojiSmallRightCenterImage,
                ["shiroji_very_small_right_image"] = this.ShirojiVerySmallRightImage,
                ["children_stand_left_image"] = this.ChildrenStandLeftImage,
                ["children_stand_right_image"] = this.ChildrenStandRightImage,
                ["kimi_stand__small_left_image"] = this.KimiStandSmallLeftImage,
                ["kimi_stand_left_symbol_image"] = this.KimiStandLeftSymbolImage,
                ["intro_akamaru_face_image"] = this.IntroAkamaruFaceImage, //
                ["intro_aosuke_face_image"] = this.IntroAosukeFaceImage, //
                ["intro_kimi_face_image"] = this.IntroKimiFaceImage, //
                ["children_face_small_left_image"] = this.ChildrenFaceSmallLeftImage,
                ["children_face_small_left_comment_image"] = this.ChildrenFaceSmallLeftCommentImage,
                ["teacher_image"] = this.TeacherImage,
                ["main_msg_bubble_image"] = this.MainMessageBubbleImage, //
                // ["next_message_arrow_down_image"] = this.NextMessageArrowDownImage,
                // ["back_message_arrow_up_image"] = this.BackMessageAllowUpImage,
                // ["thin_message_bubble_image"] = this.ThinMessageBubbleImage,
                ["next_page_after_image"] = this.NextPageAfterImage,
                ["back_page_before_image"] = this.BackPageBeforeImage,
                // ["manga_flip_arrow_image"] = this.MangaFlipArrowImage,
                // ["music_info_back_image"] = this.MusicInfoBackImage,
                ["cover_layer_image"] = this.CoverLayerImage,
                ["exit_back_image"] = this.ExitBackImage,
                ["exit_yes_button_image"] = this.ExitYesButtonImage,
                ["exit_no_button_image"] = this.ExitNoButtonImage,
            };

            this.textBlockObjects = new Dictionary<string, TextBlock>
            {
                ["rule_board_title_msg"] = this.RuleBoardTitleTextBlock, //
                ["rule_board_check1_msg"] = this.RuleBoardCheck1TextBlock, //
                ["rule_board_check2_msg"] = this.RuleBoardCheck2TextBlock, //
                ["rule_board_check3_msg"] = this.RuleBoardCheck3TextBlock, //
                ["item_name_left_text"] = this.ItemNameLeftTextBlock, //
                ["item_name_center_text"] = this.ItemNameCenterTextBlock, //
                ["item_number_text"] = this.ItemNumberTextBlock, //
                ["item_info_title_text"] = this.ItemInfoTitleTextBlock, //
                ["item_info_sentence_text1"] = this.ItemInfoSentenceTextBlock1,
                ["item_info_sentence_text2"] = this.ItemInfoSentenceTextBlock2,
                ["item_last_info_text"] = this.ItemLastInfoTextBlock,
                ["session_sub_title_text"] = this.SessionSubTitleTextBlock,
                ["session_sentence_text"] = this.SessionSentenceTextBlock,
                ["challenge_msg_text"] = this.ChallengeMessageTextBlock,
                ["challenge_msg_box_text"] = this.ChallengeMessageBoxTextBlock,
                ["view_kind_of_feeling_person_text"] = this.ViewKindOfFeelingPersonTextBlock,
                ["view_kind_of_feeling_text"] = this.ViewKindOfFeelingTextBlock,
                ["view_feeling_value_text"] = this.ViewFeelingValueTextBlock,

                ["case_of_kimi_text"] = this.CaseOfKimiTextBlock,
                ["scene1_text"] = this.Scene1TextBlock,
                ["kind_of_mind_up_text"] = this.KindOfMindUpTextBlock,
                ["scale_of_mind_up_text"] = this.ScaleOfMindUpTextBlock,
                ["scene2_text"] = this.Scene2TextBlock,
                ["kind_of_mind_down_text"] = this.KindOfMindDownTextBlock,
                ["scale_of_mind_down_text"] = this.ScaleOfMindDownTextBlock,


                ["summary_sub_title_text"] = this.SummarySubTitleTextBlock,
                ["summary_sentence_text"] = this.SummarySentenceTextBlock,

                ["ending_chapter_no_text"] = this.EndingChapterNoTextBlock,
                ["ending_msg_text"] = this.EndingMessageTextBlock,

                ["kimi_stand_left_comment_text"] = this.KimiStandLeftCommentTextBlock,
                ["children_face_small_left_comment_text"] = this.ChildrenFaceSmallLeftCommentTextBlock,
                ["main_msg"] = this.MainMessageTextBlock, //
                ["thin_msg"] = this.ThinMessageTextBlock,
                ["music_title_text"] = this.MusicTitleTextBlock, //
                ["composer_name_text"] = this.ComposerNameTextBlock, //
            };

            this.buttonObjects = new Dictionary<string, Button>
            {
                ["rule_board_button"] = this.RuleBoardButton, //
                ["next_msg_button"] = this.NextMessageButton, //
                ["back_msg_button"] = this.BackMessageButton, //
                ["thin_msg_button"] = this.ThinMessageButton, //
                ["next_page_button"] = this.NextPageButton, //
                ["back_page_button"] = this.BackPageButton, //
                ["manga_flip_button"] = this.MangaFlipButton, //
                ["exit_button"] = this.ExitButton,
                ["exit_yes_button"] = this.ExitYesButton,
                ["exit_no_button"] = this.ExitNoButton,
                ["challenge_complete_button"] = this.ChallengeCompleteButton,
                ["challenge_next_button"] = this.ChallengeNextButton,
            };

            this.gridObjects = new Dictionary<string, Grid>
            {
                // ["base_grid"] = this.BaseGrid,
                // ["manga_grid"] = this.MangaGrid,
                // ["item_grid"] = this.ItemGrid,
                ["session_grid"] = this.SessionGrid,
                ["challenge_grid"] = this.ChallengeGrid,
                ["select_feeling_grid"] = this.SelectFeelingGrid,
                ["view_feeling_grid"] = this.ViewFeelingGrid,
                ["gauge_grid"] = this.GaugeGrid,
                ["summary_grid"] = this.SummaryGrid,

                ["ending_grid"] = this.EndingGrid,

                // ["any_frame_grid"] = this.AnyFrameGrid,
                // ["shiroji_grid"] = this.ShirojiGrid,
                ["children_stand_grid"] = this.ChildrenStandGrid,
                // ["children_face_grid"] = this.ChildrenFaceGrid,
                ["any_chara_grid"] = this.AnyCharaGrid,
                // ["system_grid"] = this.SystemGrid,
                // ["rule_button_grid"] = this.RuleButtonGrid,
                ["item_name_plate_left_grid"] = this.ItemNamePlateLeftGrid, // 
                ["item_name_bubble_grid"] = this.ItemNameBubbleGrid, //
                ["item_name_plate_center_grid"] = this.ItemNamePlateCenterGrid, //
                ["item_info_plate_grid"] = this.ItemInfoPlateGrid,
                ["item_info_sentence_grid"] = this.ItemInfoSentenceGrid,
                ["item_last_info_grid"] = this.ItemLastInfoGrid,
                ["session_frame_grid"] = this.SessionFrameGrid,
                ["challenge_msg_grid"] = this.ChallengeMessageGrid,
                ["challenge_msg_box_grid"] = this.ChallengeMessageBoxGrid,
                ["challenge_good_feeling_grid"] = this.ChallengeGoodFeelingGrid,
                ["challenge_bad_feeling_grid"] = this.ChallengeBadFeelingGrid,
                ["select_good_feeling_grid"] = this.SelectGoodFeelingGrid,
                ["select_bad_feeling_grid"] = this.SelectBadFeelingGrid,

                ["kimi_plate_grid"] = this.KimiPlateGrid,
                ["kimi_plate_inner_up_grid"] = this.KimiPlateInnerUpGrid,
                ["kimi_plate_inner_down_grid"] = this.KimiPlateInnerDownGrid,


                ["view_kind_of_feeling_grid"] = this.ViewKindOfFeelingGrid,
                ["view_feeling_value_grid"] = this.ViewFeelingValueGrid,
                ["children_face_small_left_comment_grid"] = this.ChildrenFaceSmallLeftCommentGrid,
                ["heart_grid"] = this.HeartGrid,
                ["summary_board_grid"] = this.SummaryBoardGrid,

                ["ending_title_grid"] = this.EndingTitleGrid,
                ["ending_msg_grid"] = this.EndingMessageGrid,

                ["main_msg_grid"] = this.MainMessageGrid, //
                // ["thin_message_grid"] = this.ThinMessageGrid,
                ["music_info_grid"] = this.MusicInfoGrid, //
                ["exit_grid"] = this.ExitGrid,
            };
        }

        private void ResetControls()
        {
            // this.BaseGrid.Visibility = Visibility.Hidden;
            // this.MangaGrid.Visibility = Visibility.Hidden;
            // this.ItemGrid.Visibility = Visibility.Hidden;
            this.SessionGrid.Visibility = Visibility.Hidden;
            this.ChallengeGrid.Visibility = Visibility.Hidden;
            this.SelectFeelingGrid.Visibility = Visibility.Hidden;
            this.ViewFeelingGrid.Visibility = Visibility.Hidden;
            this.GaugeGrid.Visibility = Visibility.Hidden;
            this.SummaryGrid.Visibility = Visibility.Hidden;

            this.EndingGrid.Visibility = Visibility.Hidden;

            // this.AnyFrameGrid.Visibility = Visibility.Hidden;
            // this.ShirojiGrid.Visibility = Visibility.Hidden;
            this.ChildrenStandGrid.Visibility = Visibility.Hidden;
            // this.ChildrenFaceGrid.Visibility = Visibility.Hidden;
            this.AnyCharaGrid.Visibility = Visibility.Hidden;
            // this.SystemGrid.Visibility = Visibility.Hidden;

            // this.RuleButtonGrid.Visibility = Visibility.Hidden;
            this.ItemNamePlateLeftGrid.Visibility = Visibility.Hidden; //
            this.ItemNameBubbleGrid.Visibility = Visibility.Hidden; //
            this.ItemNamePlateCenterGrid.Visibility = Visibility.Hidden; //
            this.ItemInfoPlateGrid.Visibility = Visibility.Hidden;
            this.ItemInfoSentenceGrid.Visibility = Visibility.Hidden;
            this.ItemLastInfoGrid.Visibility = Visibility.Hidden;
            this.SessionFrameGrid.Visibility = Visibility.Hidden;
            this.ViewKindOfFeelingGrid.Visibility = Visibility.Hidden;
            this.ViewFeelingValueGrid.Visibility = Visibility.Hidden;
            this.ChildrenFaceSmallLeftCommentGrid.Visibility = Visibility.Hidden;
            this.ChallengeMessageGrid.Visibility = Visibility.Hidden;
            this.ChallengeMessageBoxGrid.Visibility = Visibility.Hidden;
            this.ChallengeGoodFeelingGrid.Visibility = Visibility.Hidden;
            this.ChallengeBadFeelingGrid.Visibility = Visibility.Hidden;
            this.SelectGoodFeelingGrid.Visibility = Visibility.Hidden;
            this.SelectBadFeelingGrid.Visibility = Visibility.Hidden;

            this.KimiPlateGrid.Visibility = Visibility.Hidden;
            this.KimiPlateInnerUpGrid.Visibility = Visibility.Hidden;
            this.KimiPlateInnerDownGrid.Visibility = Visibility.Hidden;

            this.HeartGrid.Visibility = Visibility.Hidden;
            this.SummaryBoardGrid.Visibility = Visibility.Hidden;

            this.EndingTitleGrid.Visibility = Visibility.Hidden;
            this.EndingMessageGrid.Visibility = Visibility.Hidden;

            this.MainMessageGrid.Visibility = Visibility.Hidden; //
            // this.ThinMessageGrid.Visibility = Visibility.Hidden;
            this.MusicInfoGrid.Visibility = Visibility.Hidden; //
            this.ExitGrid.Visibility = Visibility.Hidden;

            this.BackgroundImage.Visibility = Visibility.Hidden; //
            this.RuleBoardButton.Visibility = Visibility.Hidden; //
            this.RuleBoardTitleTextBlock.Visibility = Visibility.Hidden; //
            this.RuleBoardCheck1TextBlock.Visibility = Visibility.Hidden; //
            this.RuleBoardCheck2TextBlock.Visibility = Visibility.Hidden; //
            this.RuleBoardCheck3TextBlock.Visibility = Visibility.Hidden; //
            this.RuleBoardCheck1Box.Visibility = Visibility.Hidden; //
            this.RuleBoardCheck2Box.Visibility = Visibility.Hidden; //
            this.RuleBoardCheck3Box.Visibility = Visibility.Hidden; //
            this.MangaTitleImage.Visibility = Visibility.Hidden; //
            this.MangaImage.Visibility = Visibility.Hidden; //
            this.ItemCenterImage.Visibility = Visibility.Hidden; //
            this.ItemLeftImage.Visibility = Visibility.Hidden; //
            this.ItemLeftLastImage.Visibility = Visibility.Hidden;
            this.ItemLastInfoImage.Visibility = Visibility.Hidden;
            // this.ItemNamePlateCenterImage.Visibility = Visibility.Hidden;
            // this.ItemNameBubbleImage.Visibility = Visibility.Hidden;
            // this.ItemNamePlateLeftImage.Visibility = Visibility.Hidden;
            // this.ItemNameLeftTextBlock.Visibility = Visibility.Hidden;
            // this.ItemNameCenterTextBlock.Visibility = Visibility.Hidden;
            // this.ItemNumberTextBlock.Visibility = Visibility.Hidden;
            this.ItemInfoTitleTextBlock.Visibility = Visibility.Hidden;
            this.ItemInfoSentenceTextBlock1.Visibility = Visibility.Hidden;
            this.ItemInfoSentenceTextBlock2.Visibility = Visibility.Hidden;
            this.ItemLastInfoTextBlock.Visibility = Visibility.Hidden;
            this.SessionTitleImage.Visibility = Visibility.Hidden;
            this.SessionFrameImage.Visibility = Visibility.Hidden;
            this.SessionSubTitleTextBlock.Visibility = Visibility.Hidden;
            this.SessionSentenceTextBlock.Visibility = Visibility.Hidden;
            this.ChallengeGoodFeelingImage.Visibility = Visibility.Hidden;
            this.ChallengeBadFeelingImage.Visibility = Visibility.Hidden;
            this.ChallengeTitleImage.Visibility = Visibility.Hidden;
            this.ChallengeSmallBubbleImage.Visibility = Visibility.Hidden;
            this.ChallengeItemImage.Visibility = Visibility.Hidden;
            this.ChallengeMessageBoxImage.Visibility = Visibility.Hidden;
            this.ChallengeCompleteImage.Visibility = Visibility.Hidden;
            this.ChallengeNextImage.Visibility = Visibility.Hidden;
            this.ChallengeMessageTextBlock.Visibility = Visibility.Hidden;
            this.ChallengeMessageBoxTextBlock.Visibility = Visibility.Hidden;
            this.ChallengeCompleteButton.Visibility = Visibility.Hidden;
            this.ChallengeNextButton.Visibility = Visibility.Hidden;
            this.ChallengeGoodFeelingStackPanel.Visibility = Visibility.Hidden;
            this.ChallengeBadFeelingStackPanel.Visibility = Visibility.Hidden;
            this.SelectGoodFeelingImage.Visibility = Visibility.Hidden;
            this.SelectBadFeelingImage.Visibility = Visibility.Hidden;
            this.SelectItemFeelingImage.Visibility = Visibility.Hidden;

            this.Scene1Label.Visibility = Visibility.Hidden;
            this.KindOfMindUpLabel.Visibility = Visibility.Hidden;
            this.ScaleOfMindUpLabel.Visibility = Visibility.Hidden;
            this.Scene2Label.Visibility = Visibility.Hidden;
            this.KindOfMindDownLabel.Visibility = Visibility.Hidden;
            this.ScaleOfMindDownLabel.Visibility = Visibility.Hidden;
            this.KimiPlateOuterImage.Visibility = Visibility.Hidden;
            this.KimiInPlateImage.Visibility = Visibility.Hidden;
            this.KimiPlateInnerUpImage.Visibility = Visibility.Hidden;
            this.KimiPlateInnerDownImage.Visibility = Visibility.Hidden;
            this.CaseOfKimiTextBlock.Visibility = Visibility.Hidden;
            this.Scene1TextBlock.Visibility = Visibility.Hidden;
            this.KindOfMindUpTextBlock.Visibility = Visibility.Hidden;
            this.ScaleOfMindUpTextBlock.Visibility = Visibility.Hidden;
            this.Scene2TextBlock.Visibility = Visibility.Hidden;
            this.KindOfMindDownTextBlock.Visibility = Visibility.Hidden;
            this.ScaleOfMindDownTextBlock.Visibility = Visibility.Hidden;


            this.ViewKindOfFeelingPersonTextBlock.Visibility = Visibility.Hidden;
            this.ViewKindOfFeelingImage.Visibility = Visibility.Hidden;
            this.ViewFeelingValueLabel.Visibility = Visibility.Hidden;
            this.ViewFeelingValueImage.Visibility = Visibility.Hidden;
            this.ChildrenFaceSmallLeftCommentImage.Visibility = Visibility.Hidden;
            this.ViewFeelingValueTextBlock.Visibility = Visibility.Hidden;
            this.ChildrenFaceSmallLeftCommentTextBlock.Visibility = Visibility.Hidden;
            this.HeartImage.Visibility = Visibility.Hidden;
            this.NeedleImage.Visibility = Visibility.Hidden;
            this.SummaryBoardImage.Visibility = Visibility.Hidden;
            this.SummaryTitleImage.Visibility = Visibility.Hidden;
            this.SummarySubTitleTextBlock.Visibility = Visibility.Hidden;
            this.SummarySentenceTextBlock.Visibility = Visibility.Hidden;
            this.ChildrenInfoImage.Visibility = Visibility.Hidden; //

            this.EndingTitleBackImage.Visibility = Visibility.Hidden;
            this.EndingItemImage.Visibility = Visibility.Hidden;
            this.ShirojiEndingImage.Visibility = Visibility.Hidden;
            this.KimiEndingImage.Visibility = Visibility.Hidden;
            this.AosukeEndingImage.Visibility = Visibility.Hidden;
            this.AkamaruEndingImage.Visibility = Visibility.Hidden;
            this.EndingShortBubbleImage.Visibility = Visibility.Hidden;
            this.EndingChapterNoTextBlock.Visibility = Visibility.Hidden;
            this.EndingMessageTextBlock.Visibility = Visibility.Hidden;


            this.ShirojiRightImage.Visibility = Visibility.Hidden; //
            this.ShirojiRightUpImage.Visibility = Visibility.Hidden;
            this.ShirojiSmallRightUpImage.Visibility = Visibility.Hidden; //
            this.ShirojiSmallRightDownImage.Visibility = Visibility.Hidden; //
            this.ShirojiRightCenterImage.Visibility = Visibility.Hidden;
            this.ShirojiSmallRightCenterImage.Visibility = Visibility.Hidden;
            this.ShirojiVerySmallRightImage.Visibility = Visibility.Hidden;
            this.ChildrenStandLeftImage.Visibility = Visibility.Hidden;
            this.ChildrenStandRightImage.Visibility = Visibility.Hidden;
            this.KimiStandSmallLeftImage.Visibility = Visibility.Hidden;
            this.KimiStandLeftSymbolImage.Visibility = Visibility.Hidden;
            this.KimiStandLeftCommentTextBlock.Visibility = Visibility.Hidden;
            this.IntroAkamaruFaceImage.Visibility = Visibility.Hidden; //
            this.IntroAosukeFaceImage.Visibility = Visibility.Hidden; //
            this.IntroKimiFaceImage.Visibility = Visibility.Hidden; //
            this.ChildrenFaceSmallLeftImage.Visibility = Visibility.Hidden;
            this.TeacherImage.Visibility = Visibility.Hidden;
            // this.MainMessageBubbleImage.Visibility = Visibility.Hidden;
            // this.NextMessageArrowDownImage.Visibility = Visibility.Hidden;
            // this.BackMessageAllowUpImage.Visibility = Visibility.Hidden;
            // this.ThinMessageBubbleImage.Visibility = Visibility.Hidden;
            this.MainMessageTextBlock.Visibility = Visibility.Hidden; //
            this.ThinMessageTextBlock.Visibility = Visibility.Hidden; //
            this.NextMessageButton.Visibility = Visibility.Hidden; //
            this.BackMessageButton.Visibility = Visibility.Hidden; //
            this.ThinMessageButton.Visibility = Visibility.Hidden; //
            // this.NextPageAfterImage.Visibility = Visibility.Hidden;
            // this.BackPageBeforeImage.Visibility = Visibility.Hidden;
            // this.MangaFlipArrowImage.Visibility = Visibility.Hidden;
            // this.MusicInfoBackImage.Visibility = Visibility.Hidden;
            // this.MusicTitleTextBlock.Visibility = Visibility.Hidden;
            // this.ComposerTextBlock.Visibility = Visibility.Hidden;
            this.NextPageButton.Visibility = Visibility.Hidden; //
            this.BackPageButton.Visibility = Visibility.Hidden; //
            this.MangaFlipButton.Visibility = Visibility.Hidden; //
            // this.BGMLabel.Visibility = Visibility.Hidden;
            // this.WriterArrangerLabel.Visibility = Visibility.Hidden;
            this.CoverLayerImage.Visibility = Visibility.Hidden;
            this.ExitBackImage.Visibility = Visibility.Hidden;
            this.ExitYesButtonImage.Visibility = Visibility.Hidden;
            this.ExitNoButtonImage.Visibility = Visibility.Hidden;
            this.ExitButton.Visibility = Visibility.Hidden;
            this.ExitYesButton.Visibility = Visibility.Hidden;
            this.ExitNoButton.Visibility = Visibility.Hidden;
            this.ExitTitleLabel.Visibility = Visibility.Hidden;

            this.RuleBoardTitleTextBlock.Text = ""; //
            this.RuleBoardCheck1TextBlock.Text = ""; //
            this.RuleBoardCheck2TextBlock.Text = ""; //
            this.RuleBoardCheck3TextBlock.Text = ""; //
            this.ItemNameLeftTextBlock.Text = ""; //
            this.ItemNameCenterTextBlock.Text = "";
            this.ItemNumberTextBlock.Text = "";
            this.ItemInfoTitleTextBlock.Text = "";
            this.ItemInfoSentenceTextBlock1.Text = "";
            this.ItemInfoSentenceTextBlock2.Text = "";
            this.ItemLastInfoTextBlock.Text = "";
            this.SessionSubTitleTextBlock.Text = "";
            this.SessionSentenceTextBlock.Text = "";
            this.ChallengeMessageTextBlock.Text = "";
            this.ChallengeMessageBoxTextBlock.Text = "";
            this.ViewKindOfFeelingPersonTextBlock.Text = "";
            this.ViewKindOfFeelingTextBlock.Text = "";
            this.ViewFeelingValueTextBlock.Text = "";

            this.CaseOfKimiTextBlock.Text = "";
            this.Scene1TextBlock.Text = "";
            this.KindOfMindUpTextBlock.Text = "";
            this.ScaleOfMindUpTextBlock.Text = "";
            this.Scene2TextBlock.Text = "";
            this.KindOfMindDownTextBlock.Text = "";
            this.ScaleOfMindDownTextBlock.Text = "";

            this.ChildrenFaceSmallLeftCommentTextBlock.Text = "";
            this.SummarySubTitleTextBlock.Text = "";
            this.SummarySentenceTextBlock.Text = "";
            this.EndingChapterNoTextBlock.Text = "";
            this.EndingMessageTextBlock.Text = "";
            this.KimiStandLeftCommentTextBlock.Text = "";
            this.MainMessageTextBlock.Text = ""; //
            this.ThinMessageTextBlock.Text = ""; //
            this.MusicTitleTextBlock.Text = ""; //
            this.ComposerNameTextBlock.Text = ""; //

            this.RuleBoardCheck1Box.IsEnabled = false; //
            this.RuleBoardCheck2Box.IsEnabled = false; //
            this.RuleBoardCheck3Box.IsEnabled = false; //
        }

        // TitlePageからscenarioプロパティの書き換えができないのでメソッドでセットする
        public void SetScenario(string scenario)
        {
            this.scenarios = this.LoadScenario(scenario);
            this.ScenarioPlay();
        }

        // CSVから2次元配列へシナリオデータの収納（CsvReaderクラスを使用）
        private List<List<string>> LoadScenario(string filePath)
        {
            using (var csv = new CsvReader(filePath))
            {
                this.scenarios = csv.ReadToEnd();
            }
            return scenarios;
        }

        // ゲーム進行の中核
        private void ScenarioPlay()
        {
            Debug.Print((this.scenarioCount + 1).ToString());

            var tag = this.scenarios[this.scenarioCount][0];

            // メッセージ表示関連
            this.word_num = 0;

            switch (tag)
            {
                case "reset":

                    this.ResetControls();

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "grid":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var gridObject = this.gridObjects[this.position];

                    gridObject.Visibility = Visibility.Visible;

                    string gridAnimeIsSync = "sync";

                    if (this.scenarios[this.scenarioCount].Count > 3 && this.scenarios[this.scenarioCount][3] != "")
                    {
                        gridAnimeIsSync = this.scenarios[this.scenarioCount][3];
                    }

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var gridStoryBoard = this.scenarios[this.scenarioCount][2];

                        gridStoryBoard += $"_{this.position}";

                        this.ShowAnime(storyBoard: gridStoryBoard, isSync: gridAnimeIsSync);
                    }
                    else
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    break;

                case "image":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var imageObject = this.imageObjects[this.position];

                    string imageFile;

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        imageFile = this.scenarios[this.scenarioCount][2];

                        imageObject.Source = new BitmapImage(new Uri($"Images/{imageFile}", UriKind.Relative));
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

                case "msg":

                    this.NextMessageButton.Visibility = Visibility.Hidden;
                    this.BackMessageButton.Visibility = Visibility.Hidden;

                    this.position = this.scenarios[this.scenarioCount][1];

                    var _textObject = this.textBlockObjects[this.position];

                    var _message = this.scenarios[this.scenarioCount][2];

                    _textObject.Visibility = Visibility.Visible;

                    this.ShowMessage(textObject: _textObject, message: _message);

                    break;

                case "text":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var textObject = this.textBlockObjects[this.position];

                    var _text = this.scenarios[this.scenarioCount][2];

                    var text = _text.Replace("鬱", "\u2028");

                    textObject.Text = text;

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

                    string textIsSync = "sync";

                    if (this.scenarios[this.scenarioCount].Count > 5 && this.scenarios[this.scenarioCount][5] != "")
                    {
                        textIsSync = this.scenarios[this.scenarioCount][5];
                    }

                    if (this.scenarios[this.scenarioCount].Count > 4 && this.scenarios[this.scenarioCount][4] != "")
                    {
                        var textStoryBoard = this.scenarios[this.scenarioCount][4];

                        textStoryBoard += $"_{this.position}";

                        this.ShowAnime(storyBoard: textStoryBoard, isSync: textIsSync);
                    }
                    else
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    break;

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

                case "next":

                    this.NextPageButton.Visibility = Visibility.Visible;
                    this.BackPageButton.Visibility = Visibility.Visible;

                    this.isClickable = true;

                    break;

                case "flip":

                    this.MangaFlipButton.Visibility = Visibility.Visible;
                    this.isClickable = true;

                    break;

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

                            this.position = this.scenarios[this.scenarioCount][2];
                            this.gridObjects[this.position].Visibility = Visibility.Hidden;

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
                    }
                    break;

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

                case "wait_tap":

                    this.isClickable = false;
                    break;

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
                    this.Angle = 0.0f;
                    this.ViewFeelingValueTextBlock.Text = "50";

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;
            }
        }

        void ShowMessage(TextBlock textObject, string message, object obj=null)
        {
            // 苦悶の改行処理（文章中の「鬱」を疑似改行コードとする）
            var _message = message.Replace("鬱", "\u2028");

            // メッセージ表示処理
            this.msgTimer = new DispatcherTimer();
            this.msgTimer.Tick += ViewMsg;
            this.msgTimer.Interval = TimeSpan.FromSeconds(1.0f / MESSAGE_SPEED);
            this.msgTimer.Start();

            // 一文字ずつメッセージ表示（Inner Func）
            void ViewMsg(object sender, EventArgs e)
            {
                textObject.Text = _message.Substring(0, word_num);

                if (word_num < _message.Length)
                {
                    word_num++;
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
            Button button = sender as Button;

            if (button.Name.Contains("Back"))
            {
                this.scenarioCount -= 1;
                this.ScenarioPlay();
                // 連続Backの実現にはもっと複雑な処理がいる
            }

            if (this.isClickable && (button.Name == "NextMessageButton" || button.Name == "NextPageButton" || button.Name == "RuleBoardButton" || button.Name == "ThinMessageButton" || button.Name == "MangaFlipButton"))
            {
                this.isClickable = false;

                this.scenarioCount += 1;
                this.ScenarioPlay();
            }

            if (button.Name == "ExitButton")
            {
                this.CoverLayerImage.Visibility = Visibility.Visible;
                this.ExitGrid.Visibility = Visibility.Visible;
            }
            
            if (button.Name == "ExitYesButton")
            {
                Application.Current.Shutdown();
            }
            
            if (button.Name == "ExitNoButton")
            {
                this.ExitGrid.Visibility = Visibility.Hidden;
                this.CoverLayerImage.Visibility = Visibility.Hidden;
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
                    this.isClickable = true;
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            if (this.checkBoxs.Contains(checkBox))
            {
                this.tapCount -= 1;
            }
        }

        private void SetBGM(string soundFile, bool isLoop, int volume)
        {
            string exePath = Environment.GetCommandLineArgs()[0];
            string exeFullPath = System.IO.Path.GetFullPath(exePath);
            string startupPath = System.IO.Path.GetDirectoryName(exeFullPath);

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

        void GestureCanvas_Gesture(object sender, InkCanvasGestureEventArgs e)
        {
            // 信頼性 (RecognitionConfidence) を無視したほうが、Circle と Triangle の認識率は上がるようです。
            var gestureResult = e.GetGestureRecognitionResults()
                .FirstOrDefault(r => r.ApplicationGesture != ApplicationGesture.NoGesture);

            string exePath = Environment.GetCommandLineArgs()[0];
            string exeFullPath = System.IO.Path.GetFullPath(exePath);
            string startupPath = System.IO.Path.GetDirectoryName(exeFullPath);

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

            gestureCanvas.Strokes.Clear();
            gestureCanvas.Strokes.Add(e.Strokes);
        }

        private enum AnswerResult
        {
            None,
            Incorrect,
            Intermediate,
            Correct,
        }

        private static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(Chapter1), new UIPropertyMetadata(0.0));

        private double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this);

            this.CalcAngle();

            this.ViewFeelingValueTextBlock.Text = this.feelingSize.ToString();
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.Captured == this)
            {
                this.CalcAngle();

                this.ViewFeelingValueTextBlock.Text = this.feelingSize.ToString();
            }
        }

        private void CalcAngle()
        {
            Point currentLocation = Mouse.GetPosition(this);

            Point knobCenter = new Point(this.ActualWidth * 0.5, this.ActualHeight * 0.8);

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
