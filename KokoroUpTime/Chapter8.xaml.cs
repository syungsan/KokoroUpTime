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

namespace KokoroUpTime


{
    /// <summary>
    /// GameWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class Chapter8 : Page
    {
        // すてきなせいかくのリスト
        private string[] NICE_PERSONALITY = { "●　親切にしてもらった", "●　アドバイスをくれた", "●　みんなをひっぱってくれた", "●　きちんと順番をまもってくれた", "●　みんなを笑顔にしてくれた", "●　（何か悪いことや失敗をゆるしてもらった）", "●　自分の気持ちを分かってもらった", "●　仲良くしてもらった", "●　ありがとうと言ってくれた" };

        //タイトル表示用のテキスト
        private string[] GOOD_FEELINGS = { "●　うれしい", "●　しあわせ", "●　たのしい", "●　ホッとした", "●　きもちいい", "●　まんぞく", "●　すき", "●　やる気マンマン", "●　かんしゃ", "●　わくわく", "●　うきうき", "●　ほこらしい" };
        private string[] BAD_FEELINGS = { "●　心配", "●　こまった", "●　不安", "●　こわい", "●　おちこみ", "●　がっかり", "●　いかり", "●　イライラ", "●　はずかしい", "●　ふまん", "●　かなしい", "●　おびえる" };


        private float THREE_SECOND_RULE_TIME = 3.0f;

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
        private DataChapter8 dataChapter8;
        //データモデルのプロパティを呼び出すための辞書
        private Dictionary<string, string> KindOfFeelings = null;
        private Dictionary<string, int?> SizeOfFeelings = null;
        private Dictionary<string, StrokeCollection> InputStroke = null;
        private Dictionary<string, string> InputText = null;

        //データモデルの辞書を呼び出すためのキー
        private string FeelingDictionaryKey="";
        private string InputDictionaryKey = "";

        //入力したストロークを保存するためのコレクション
        private StrokeCollection GroupeActivityInputStroke1 = new StrokeCollection();
        private StrokeCollection GroupeActivityInputStroke2 = new StrokeCollection();
        private StrokeCollection GroupeActivityInputStroke3 = new StrokeCollection();

        // ゲームの切り替えシーン
        private string scene;

        //選択した入力方法によってDataTemplateを切り替えるためのインスタンス
        private InputMethodStyleSelector styleSelector = new InputMethodStyleSelector();

        // データベースに収めるデータモデルのインスタンス
        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();
        public DataProgress dataProgress = new DataProgress();

        private ObservableCollection<GroupeActivityData> _groupeactivityData = new ObservableCollection<GroupeActivityData>();

    

        public Chapter8()
        {
            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            // メディアプレーヤークラスのインスタンスを作成する
            this.mediaPlayer = new WindowsMediaPlayer();


            // データモデルインスタンス確保
            this.dataChapter8 = new DataChapter8();

            // マウスイベントの設定
            this.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
            this.MouseUp += new MouseButtonEventHandler(OnMouseUp);
            this.MouseMove += new MouseEventHandler(OnMouseMove);

            
            _groupeactivityData.Add(new GroupeActivityData("た", "しかめてみよう！", "友だちはどのようにかんがえるか聞いてみよう！", "Images/grass.png"));
            _groupeactivityData.Add(new GroupeActivityData("い", "いきもちになる考えは？", "「お助け虫」に進化させてみよう！", "Images/butterfly.png"));
            _groupeactivityData.Add(new GroupeActivityData("じ", "ぶんいがいの人なら？", "相手をはげます言葉を自分にもかけてみよう！", "Images/cheering.png"));


            this.SelectGoodFeelingListBox.ItemsSource = GOOD_FEELINGS;
            this.SelectBadFeelingListBox.ItemsSource = BAD_FEELINGS;
            this.GroupeActivityItemsControl.ItemsSource = _groupeactivityData;

            KindOfFeelings = new Dictionary<string, string>()
            {
                ["aosuke_pre_use_item"] = this.dataChapter8.AosukesKindOfFeelingPreUseItem="",
                ["kimi_pre_use_item"] = this.dataChapter8.KimisKindOfFeelingPreUseItem = "",
                ["akamaru_pre_use_item"] = this.dataChapter8.AkamarusKindOfFeelingPreUseItem="",
                ["aosuke_after_used_item"] = this.dataChapter8.AosukesKindOfFeelingAfterUsedItem="",
                ["kimi_after_used_item"] = this.dataChapter8.KimisKindOfFeelingAfterUsedItem="",
                ["akamaru_after_used_item"] = this.dataChapter8.AkamarusKindOfFeelingAfterUsedItem="",
                ["let's_check"] = this.dataChapter8.Let_sCheckKindOfFeeling="",
                ["positive_thinking"] = this.dataChapter8.PositiveThinkingKindOfFeeling="",
                ["thoughts_of_others"] = this.dataChapter8.ThoughtsOfOthersKindOfFeeling=""
            };

            SizeOfFeelings = new Dictionary<string, int?>()
            {
                ["aosuke_pre_use_item"] = this.dataChapter8.AosukesSizeOfFeelingPreUseItem=-1,
                ["kimi_pre_use_item"] = this.dataChapter8.KimisSizeOfFeelingPreUseItem=-1,
                ["akamaru_pre_use_item"] = this.dataChapter8.AkamarusSizeOfFeelingPreUseItem=-1,
                ["aosuke_after_used_item"] = this.dataChapter8.AosukesSizeOfFeelingAfterUsedItem=-1,
                ["kimi_after_used_item"] = this.dataChapter8.KimisSizeOfFeelingAfterUsedItem=-1,
                ["akamaru_after_used_item"] = this.dataChapter8.AkamarusSizeOfFeelingAfterUsedItem=-1,
                ["let's_check"] = this.dataChapter8.Let_sCheckSizeOfFeeling=-1,
                ["positive_thinking"] = this.dataChapter8.PositiveThinkingSizeOfFeeling=-1,
                ["thoughts_of_others"] = this.dataChapter8.ThoughtsOfOthersSizeOfFeeling=-1
            };

            InputStroke = new Dictionary<string, StrokeCollection>()
            {
                ["groupe_activity_input1"] = this.GroupeActivityInputStroke1,
                ["groupe_activity_input2"] = this.GroupeActivityInputStroke2,
                ["groupe_activity_input3"] = this.GroupeActivityInputStroke3,
            };
            InputText = new Dictionary<string, string>()
            {
                ["groupe_activity_input1"] = this.dataChapter8.GroupeActivityInputText1,
                ["groupe_activity_input2"] = this.dataChapter8.GroupeActivityInputText2,
                ["groupe_activity_input3"] = this.dataChapter8.GroupeActivityInputText3,
            };

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
                ["cover_layer_image"] =this.CoverLayerImage,

                ["main_msg_bubble_image"] = this.MainMessageBubbleImage,
                ["chalenge_time_child_image"] = this.ChalengeTimeChildImage,
                ["ojamamushi_attached_speech_bubble_image"] =this.OjamamushiAttachedSpeechBubbleImage,
                ["challenge_item_image"] =this.ChallengeItemImage,
                ["any_character_image"] =this.AnyCharacterImage,
                ["how_to_use_item_1_ojamamushi_image"]=this.HowToUseItem_1OjamamushiImage,
                ["inspiration_light_image"] =this.InspirationLightImage,
                ["butterfly_image"] =this.ButterflyImage
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
                ["ojamamushi_discription_title_text"] = this.OjamamushiDiscriptionTitleText,
                ["childrens_thoughts_msg"] =this.ChildrensThoughtsText,
                ["challenge_msg_text"] = this.ChallengeMessageText,
                ["any_character_msg_text"] =this.AnyCharacterMessageText,
                ["childrens_thoughts_title_text"] =this.ChildrensThoughtsTitleText,
                ["ojamamushis_title_text"] =this.OjamamushisTitleText,
                ["kind_of_feeling_input_text"] =this.KindOfFeelingInputTextBlock,
                ["size_of_feeling_input_text"] =this.SizeOfFeelingInputTextBlock,
                ["let's_check_kind_of_feeling_input_text"] = this.Let_sCheckKindOfFeelingInputText,
                ["let's_check_size_of_feeling_input_text"] = this.Let_sCheckSizeOfFeelingInputText,
                ["positive_thinking_kind_of_feeling_input_text"] =this.PositiveThinkingKindOfFeelingInputText,
                ["positive_thinking_size_of_feeling_input_text"] =this.PositiveThinkingSizeOfFeelingInputText,
                ["thoughts_of_others_kind_of_feeling_input_text"] =this.ThoughtsOfOthersKindOfFeelingInputText,
                ["thoughts_of_others_size_of_feeling_input_text"] =this.ThoughtsOfOthersSizeOfFeelingInputText

               
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
                ["complete_input_button"] =this.CompleteInputButton,
            };

            this.gridObjects = new Dictionary<string, Grid>
            {
                ["session_grid"] = this.SessionGrid,
                ["session_frame_grid"] = this.SessionFrameGrid,
                ["manga_grid"]=this.MangaGrid,
                ["title_grid"]=this.TitleGrid,
                ["how_to_use_item_title1_grid"] =this.HowToUseItemTitle1Grid,
                ["how_to_use_item_title2_grid"] = this.HowToUseItemTitle2Grid,
                ["how_to_use_item_title3_grid"] = this.HowToUseItemTitle3Grid,

                ["summary_grid"] = this.SummaryGrid,
                ["ending_grid"] = this.EndingGrid,
                ["item_name_plate_left_grid"] = this.ItemNamePlateLeftGrid,
                ["item_name_bubble_grid"] = this.ItemNameBubbleGrid,
                ["item_name_plate_center_grid"] = this.ItemNamePlateCenterGrid,
                ["item_info_plate_grid"] = this.ItemInfoPlateGrid,
                ["item_info_sentence_grid"] = this.ItemInfoSentenceGrid,
                ["item_last_info_grid"] = this.ItemLastInfoGrid,
                ["item_review_grid"] = this.ItemReviewGrid,

                ["ojamamushi_discription_grid"] = this.OjamamushiDiscriptionGrid,
                ["ojamamushi_discription_plate_grid"] = this.OjamamsuhiDiscriptionPlateGrid,

                ["ojamamushi_grid"] = this.OjamamushiGrid,
                ["kimis_ojamamushi"]=this.KimisOjamamushi,
                ["aosukes_ojamamushi"]=this.AosukesOjamamushi,
                ["akamarus_ojamamushi"]=this.AkamarusOjamamushi,

                ["how_to_use_item_title1_grid"] =this.HowToUseItemTitle1Grid,
                ["how_to_use_item_title2_grid"] = this.HowToUseItemTitle2Grid,
                ["how_to_use_item_title3_grid"] = this.HowToUseItemTitle3Grid,

                ["children_thought_speech_bubble_grid"] =this.ChildrenThoughtSpeechBubbleGrid,
                ["any_character_speech_bubble_grid"] =this.AnyCharacterSpeecBubbleGrid,
                ["challenge_speech_bubble_grid"] =this.ChallengeSpeecBubbleGrid,

                ["how_to_use_item_inf_grid"] =this.HowToUseItemInformationGrid,
                ["how_to_use_item_1_grid"] =this.HowToUseItem_1Grid,
                ["how_to_use_item_2_grid"] = this.HowToUseItem_2Grid,

                ["aosukes_ojamamushi_situation_grid"] =this.AosukesOjamamushiSituationGrid,
                ["view_aosukes_feeling_grid"] = this.ViewAosukesFeelingGrid,

                ["ending_msg_grid"] = this.EndingMessageGrid,
                ["main_msg_grid"] = this.MainMessageGrid,
                ["music_info_grid"] = this.MusicInfoGrid,
                ["branch_select_grid"] = this.BranchSelectGrid,
                ["exit_back_grid"] = this.ExitBackGrid,

                ["challenge_time_grid"] = this.ChallengeTimeGrid,
                ["feeling_input_grid"] =this.FeelingInputGrid,
                ["select_feeling_grid"]=this.SelectFeelingGrid,
                ["select_heart_grid"]=this.SelectHeartGrid,
                ["view_size_of_feeling_grid"] =this.ViewSizeOfFeelingGrid,
                ["groupe_activity_grid"] = this.GroupeActivityGrid,
                ["hint_check_grid"] = this.HintCheckGrid,
                ["canvas_edit_grid"] = this.CanvasEditGrid,

            };

            this.borderObjects = new Dictionary<string, Border>
            {
                ["title_border"]=this.TitleBorder,
                ["challenge_time_border"] =this.ChallengeTimeBorder,
                ["children_speech_bubble_border"] =this.ChildrenSpeechBubbleBorder,
                ["any_character_speech_bubble_border"]=this.AnyCharacterSpeechBubbleBorder,
                ["challenge_speech_bubble_border"] =this.ChallengeSpeechBubbleBorder,
                ["any_character_speech_bubble_border"] =this.AnyCharacterSpeechBubbleBorder,
                ["challenge_speech_bubble_border"] =this.ChallengeSpeechBubbleBorder,
                ["input_canvas_border"]=this.InputCanvasBorder,
                ["input_text_border"] = this.InputTextBorder,
            };

            this.outlineTextObjects = new Dictionary<string, OutlineText>
            {
                ["let_sfind_ojamamushi_scene_title"] = this.Let_sFindOjamamushiSceneTitle,
                ["kimis_thoughts_and_feeling_scene_title"] = this.KimisThoughtsAndFeelingsSceneTitle,
                ["aosukes_thoughts_and_feeling_scene_title"] = this.AosukesThoughtsAndFeelingsSceneTitle,
                ["akamarus_thoughts_and_feeling_scene_title"] = this.AkamarusThoughtsAndFeelingsSceneTitle,
                ["aosukes_ojamamushi_scene_title"] = this.AosukesOjamamushiSceneTitle
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
            this.Let_sFindOjamamushiSceneTitle.Visibility = Visibility.Hidden;
            this.KimisThoughtsAndFeelingsSceneTitle.Visibility = Visibility.Hidden;
            this.AosukesThoughtsAndFeelingsSceneTitle.Visibility = Visibility.Hidden;
            this.AkamarusThoughtsAndFeelingsSceneTitle.Visibility = Visibility.Hidden;
            this.AosukesOjamamushiSceneTitle.Visibility = Visibility.Hidden;
            this.HowToUseItemTitle1Grid.Visibility = Visibility.Hidden;
            this.HowToUseItemTitle2Grid.Visibility = Visibility.Hidden;
            this.HowToUseItemTitle3Grid.Visibility = Visibility.Hidden;
            this.HowToUseItem_1Grid.Visibility = Visibility.Hidden;
            this.HowToUseItem_2Grid.Visibility = Visibility.Hidden;
            this.HowToUseItem_1OjamamushiImage.Visibility = Visibility.Hidden;
            this.ChildrensThoughtsTitleText.Visibility = Visibility.Hidden;

            this.SelectFeelingGrid.Visibility = Visibility.Hidden;
            this.ViewSizeOfFeelingGrid.Visibility = Visibility.Hidden;
            this.SelectHeartGrid.Visibility = Visibility.Hidden;


            this.ChallengeTimeGrid.Visibility = Visibility.Hidden;
            this.GroupeActivityGrid.Visibility = Visibility.Hidden;
            this.HintCheckGrid.Visibility = Visibility.Hidden;
            this.InspirationLightImage.Visibility = Visibility.Hidden;
            this.ButterflyImage.Visibility = Visibility.Hidden;

            this.ShirojiSmallRightCenterImage.Visibility = Visibility.Hidden;

            this.EndingMessageGrid.Visibility = Visibility.Hidden;
            this.MainMessageGrid.Visibility = Visibility.Hidden;
            this.MusicInfoGrid.Visibility = Visibility.Hidden;
            this.GetItemGrid.Visibility = Visibility.Hidden;
            this.ItemBookMainGrid.Visibility = Visibility.Hidden;
            this.ItemBookNoneGrid.Visibility = Visibility.Hidden;
            this.ItemBookTitleTextBlock.Visibility = Visibility.Hidden;

            this.OjamamushiDiscriptionGrid.Visibility = Visibility.Hidden;
            this.OjamamushiDiscriptionTitleText.Visibility = Visibility.Hidden;
            this.OjamamsuhiDiscriptionPlateGrid.Visibility = Visibility.Hidden;

            this.OjamamushiGrid.Visibility = Visibility.Hidden;
            this.KimisOjamamushi.Visibility = Visibility.Hidden;
            this.AosukesOjamamushi.Visibility = Visibility.Hidden;
            this.AkamarusOjamamushi.Visibility = Visibility.Hidden;

            this.ChallengeTimeGrid.Visibility = Visibility.Hidden;
            this.OjamamushiAttachedSpeechBubbleImage.Visibility = Visibility.Hidden;
            this.OjamamushisTitleText.Visibility = Visibility.Hidden;
            this.FeelingInputGrid.Visibility = Visibility.Hidden;
            this.AnyCharacterImage.Visibility = Visibility.Hidden;
            this.AnyCharacterSpeecBubbleGrid.Visibility = Visibility.Hidden;
            this.ChallengeItemImage.Visibility = Visibility.Hidden;
            this.ChallengeSpeecBubbleGrid.Visibility = Visibility.Hidden;
            this.ChildrensThoughtsText.Visibility = Visibility.Hidden;
            this.OjamamushisTitleText.Visibility = Visibility.Hidden;
            this.ChildrenThoughtSpeechBubbleGrid.Visibility = Visibility.Hidden;

            this.OjamamushiGrid.Visibility = Visibility.Hidden;
            this.HowToUseItemInformationGrid.Visibility = Visibility.Hidden;

            this.AosukesOjamamushiSituationGrid.Visibility = Visibility.Hidden;
            this.ViewAosukesFeelingGrid.Visibility = Visibility.Hidden;
            

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
            this.InputCanvasBorder.Visibility = Visibility.Hidden;
            this.InputTextBorder.Visibility = Visibility.Hidden;

            this.ReturnToTitleButton.Visibility = Visibility.Hidden;


            this.OkButton.Visibility = Visibility.Hidden;

            this.SelectFeelingBackButton.Visibility = Visibility.Hidden;

            this.SessionSubTitleTextBlock.Text = "";
            this.SessionSentenceTextBlock.Text = "";

            this.EndingMessageTextBlock.Text = "";

            this.MusicTitleTextBlock.Text = "";
            this.ComposerNameTextBlock.Text = "";

            this.AnyCharacterMessageText.Text = "";
            this.ChallengeMessageText.Text = "";
        }

        private void SetInputMethod()
        {
            if (this.dataOption.InputMethod == 1)
            {
                this.GroupeActivityInputCanvas1.Visibility = Visibility.Hidden;
                this.GroupeActivityInputCanvas2.Visibility = Visibility.Hidden;
                this.GroupeActivityInputCanvas3.Visibility = Visibility.Hidden;
            }
            else
            {
                this.GroupeActivityInputText1.Visibility = Visibility.Hidden;
                this.GroupeActivityInputText2.Visibility = Visibility.Hidden;
                this.GroupeActivityInputText3.Visibility = Visibility.Hidden;
            }
        }

        public void SetNextPage(InitConfig _initConfig, DataOption _dataOption, DataItem _dataItem, DataProgress _dataProgress)
        {
            this.initConfig = _initConfig;
            this.dataOption = _dataOption;
            this.dataItem = _dataItem;
            this.dataProgress = _dataProgress;

            // 現在時刻を取得
            this.dataChapter8.CreatedAt = DateTime.Now.ToString();

            // データベースのテーブル作成と現在時刻の書き込みを同時に行う
            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
            {
                // 毎回のアクセス日付を記録
                connection.Insert(this.dataChapter8);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var csv = new CsvReader("./Scenarios/chapter8.csv"))
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

                    this.dataProgress.CurrentChapter = 8;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET CurrentChapter = '{this.dataProgress.CurrentChapter}' WHERE Id = 1;");
                    }

                    this.SetInputMethod();

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

                    this.dataProgress.HasCompletedChapter8 = true;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET HasCompletedChapter8 = '{Convert.ToInt32(this.dataProgress.HasCompletedChapter8)}' WHERE Id = 1;");
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
                    this.dataProgress.LatestChapter8Scene = this.scene;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET CurrentScene = '{this.dataProgress.CurrentScene}', LatestChapter8Scene = '{this.dataProgress.LatestChapter8Scene}' WHERE Id = 1;");
                    }

                    if (this.scene == "キミちゃんのきもちときもちの大きさ")
                    {
                        this.FeelingDictionaryKey = "kimi_pre_use_item";
                    }
                    else if (this.scene == "青助くんのきもちときもちの大きさ")
                    {
                        this.FeelingDictionaryKey = "aosuke_pre_use_item";
                    }
                    else if (this.scene == "赤丸くんの考えときもちは…")
                    {
                        this.FeelingDictionaryKey = "akamaru_pre_use_item";
                    }
                    else if (this.scene == "青助くんのきもちときもちの大きさの変化")
                    {
                        this.FeelingDictionaryKey = "aosuke_after_used_item";
                    }
                    else if (this.scene == "赤丸くんのきもちときもちの大きさの変化")
                    {
                        this.FeelingDictionaryKey = "akamaru_after_used_item";
                    }
                    else if (this.scene == "キミちゃんのきもちときもちの大きさの変化")
                    {
                        this.FeelingDictionaryKey = "kimi_after_used_item";
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
                        borderAnimeIsSync = this.scenarios[this.scenarioCount][4];
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
                                        if (this.scene == "キミちゃんのきもちときもちの大きさ" ||this.scene== "青助くんのきもちときもちの大きさ"|| this.scene == "赤丸くんのきもちときもちの大きさ"||this.scene == "キミちゃんのきもちときもちの大きさの変化" || this.scene == "青助くんのきもちときもちの大きさの変化" || this.scene == "赤丸くんのきもちときもちの大きさの変化")
                                        {
                                            if(this.SizeOfFeelingInputTextBlock.Text !="" && this.KindOfFeelingInputTextBlock.Text != "")
                                            {
                                                this.NextPageButton.Visibility = Visibility.Visible;
                                            }
                                            this.BackPageButton.Visibility = Visibility.Visible;
                                        }
                                        else if(this.scene == "グループアクティビティ")
                                        {
                                            if (this.dataOption.InputMethod == 0)
                                            {
                                                bool CompleteInputFlag = true;
                                                foreach (KeyValuePair<string, StrokeCollection> kvp in this.InputStroke)
                                                {
                                                    if (Regex.IsMatch(kvp.Key, "groupe_activity_input."))
                                                    {
                                                        if (kvp.Value.Count < 2)
                                                        {
                                                            CompleteInputFlag = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                                if (CompleteInputFlag)
                                                {
                                                    foreach (KeyValuePair<string, string> kvp in this.KindOfFeelings)
                                                    {
                                                        if (kvp.Key == "let's_check" || kvp.Key == "positive_thinking" || kvp.Key == "thoughts_of_others")
                                                        {
                                                            if (kvp.Value == "")
                                                            {
                                                                CompleteInputFlag = false;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (CompleteInputFlag)
                                                {
                                                    foreach (KeyValuePair<string, int?> kvp in this.SizeOfFeelings)
                                                    {
                                                        if (kvp.Key == "let's_check" || kvp.Key == "positive_thinking" || kvp.Key == "thoughts_of_others")
                                                        {
                                                            if (kvp.Value == -1)
                                                            {
                                                                CompleteInputFlag = false;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (CompleteInputFlag)
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Visible;
                                                }
                                            }
                                            else
                                            {
                                                bool CompleteInputFlag = true;

                                                foreach (KeyValuePair<string, string> kvp in this.InputText)
                                                {
                                                    if (Regex.IsMatch(kvp.Key, "groupe_activity_input."))
                                                    {
                                                        if (kvp.Value == "")
                                                        {
                                                            CompleteInputFlag = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                                if (CompleteInputFlag)
                                                {
                                                    
                                                    foreach (KeyValuePair<string, string> kvp in this.KindOfFeelings)
                                                    {
                                                        if (kvp.Key == "let's_check" || kvp.Key == "positive_thinking" || kvp.Key == "thoughts_of_others")
                                                        {
                                                            if (kvp.Value == "")
                                                            {
                                                                CompleteInputFlag = false;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                                if (CompleteInputFlag)
                                                {
                                                    foreach (KeyValuePair<string, int?> kvp in this.SizeOfFeelings)
                                                    {
                                                        if (kvp.Key == "let's_check" || kvp.Key == "positive_thinking" || kvp.Key == "thoughts_of_others")
                                                        {
                                                            if (kvp.Value == -1)
                                                            {
                                                                CompleteInputFlag = false;
                                                                break;
                                                            }
                                                        }

                                                            
                                                    }
                                                }
                                                if (CompleteInputFlag)
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Visible;
                                                }
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
                                    if (this.scene == "キミちゃんのきもちときもちの大きさ" || this.scene == "青助くんのきもちときもちの大きさ" || this.scene == "赤丸くんのきもちときもちの大きさ" || this.scene == "キミちゃんのきもちときもちの大きさの変化" || this.scene == "青助くんのきもちときもちの大きさの変化" || this.scene == "赤丸くんのきもちときもちの大きさの変化")
                                    {
                                        if (this.SizeOfFeelingInputTextBlock.Text != "" && this.KindOfFeelingInputTextBlock.Text != "")
                                        {
                                            this.NextPageButton.Visibility = Visibility.Visible;
                                        }
                                        this.BackPageButton.Visibility = Visibility.Visible;
                                    }
                                    else if (this.scene == "グループアクティビティ")
                                    {
                                        if (this.dataOption.InputMethod == 0)
                                        {
                                            bool CompleteInputFlag = true;
                                            foreach (KeyValuePair<string, StrokeCollection> kvp in this.InputStroke)
                                            {
                                                if (Regex.IsMatch(kvp.Key, "groupe_activity_input."))
                                                {
                                                    if (kvp.Value.Count < 2)
                                                    {
                                                        CompleteInputFlag = false;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (CompleteInputFlag)
                                            {
                                                foreach (KeyValuePair<string, string> kvp in this.KindOfFeelings)
                                                {
                                                    if (kvp.Key == "let's_check" || kvp.Key == "positive_thinking" || kvp.Key == "thoughts_of_others")
                                                    {
                                                        if (kvp.Value == "")
                                                        {
                                                            CompleteInputFlag = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            if (CompleteInputFlag)
                                            {
                                                foreach (KeyValuePair<string, int?> kvp in this.SizeOfFeelings)
                                                {
                                                    if (kvp.Key == "let's_check" || kvp.Key == "positive_thinking" || kvp.Key == "thoughts_of_others")
                                                    {
                                                        if (kvp.Value == -1)
                                                        {
                                                            CompleteInputFlag = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            if (CompleteInputFlag)
                                            {
                                                this.NextPageButton.Visibility = Visibility.Visible;
                                            }
                                        }
                                        else
                                        {
                                            bool CompleteInputFlag = true;

                                            foreach (KeyValuePair<string, string> kvp in this.InputText)
                                            {
                                                if (Regex.IsMatch(kvp.Key, "groupe_activity_input."))
                                                {
                                                    if (kvp.Value == "")
                                                    {
                                                        CompleteInputFlag = false;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (CompleteInputFlag)
                                            {

                                                foreach (KeyValuePair<string, string> kvp in this.KindOfFeelings)
                                                {
                                                    if (kvp.Key == "let's_check" || kvp.Key == "positive_thinking" || kvp.Key == "thoughts_of_others")
                                                    {
                                                        if (kvp.Value == "")
                                                        {
                                                            CompleteInputFlag = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            if (CompleteInputFlag)
                                            {
                                                foreach (KeyValuePair<string, int?> kvp in this.SizeOfFeelings)
                                                {
                                                    if (kvp.Key == "let's_check" || kvp.Key == "positive_thinking" || kvp.Key == "thoughts_of_others")
                                                    {
                                                        if (kvp.Value == -1)
                                                        {
                                                            CompleteInputFlag = false;
                                                            break;
                                                        }
                                                    }


                                                }
                                            }
                                            if (CompleteInputFlag)
                                            {
                                                this.NextPageButton.Visibility = Visibility.Visible;
                                            }
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
                                case "キミちゃんのきもちときもちの大きさ":
                                    this.GoTo("think_kimi's_feeling","sub");
                                    break;
                                case "青助くんのきもちときもちの大きさ":
                                    this.GoTo("think_aosuke's_feeling","sub");
                                    break;
                                case "赤丸くんのきもちときもちの大きさ":
                                    this.GoTo("think_akamaru's_feeling","sub");
                                    break;

                                case "青助くんのきもちときもちの大きさの変化":
                                    this.GoTo("think_aosuke's_feeling_after_using_item","sub");
                                    break;
                                case "赤丸くんのきもちときもちの大きさの変化":
                                    this.GoTo("think_akamaru's_feeling_after_using_item","sub");
                                    break;
                                case "キミちゃんのきもちときもちの大きさの変化":
                                    this.GoTo("think_kimi's_feeling_after_using_item","sub");
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

                // ハートゲージに対する処理
                case "gauge":

                    if (this.ViewSizeOfFeelingTextBlock.Text == "")
                    {
                        this.ViewSizeOfFeelingTextBlock.Text = "50";
                        this.Angle = 0;
                    }

                    this.SelectHeartImage.Source = null;
                    this.SelectNeedleImage.Source = null;

                    if (this.KindOfFeelings[FeelingDictionaryKey].Split(",")[1] == "良い")
                    {
                        this.SelectHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_red.png", UriKind.Relative));
                        this.SelectNeedleImage.Source = new BitmapImage(new Uri(@"./Images/red_needle.png", UriKind.Relative));
                    }
                    else if (this.KindOfFeelings[FeelingDictionaryKey].Split(",")[1] == "悪い")
                    {
                        this.SelectHeartImage.Source = new BitmapImage(new Uri(@"./Images/heart_blue.png", UriKind.Relative));
                        this.SelectNeedleImage.Source = new BitmapImage(new Uri(@"./Images/blue_needle.png", UriKind.Relative));
                    }

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "get_item":

                    this.dataItem.HasGotItem08 = true;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataItem SET HasGotItem08 = 1 WHERE Id = 1;");
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

                    Image[] itemMainImages = { this.Item01MainImage, this.Item02MainImage, this.Item03MainImage, this.Item04MainImage, this.Item05MainImage, this.Item06MainImage, this.Item07MainImage, this.Item09MainImage, this.Item10MainImage, this.Item11MainImage };

                    Image[] itemNoneImages = { this.Item01NoneImage, this.Item02NoneImage, this.Item03NoneImage, this.Item04NoneImage, this.Item05NoneImage, this.Item06NoneImage, this.Item07NoneImage, this.Item09NoneImage, this.Item10NoneImage, this.Item11NoneImage };

                    var hasGotItems = new bool[] { this.dataItem.HasGotItem01, this.dataItem.HasGotItem02, this.dataItem.HasGotItem03, this.dataItem.HasGotItem04, this.dataItem.HasGotItem05, this.dataItem.HasGotItem06, this.dataItem.HasGotItem07, this.dataItem.HasGotItem09, this.dataItem.HasGotItem10, this.dataItem.HasGotItem11 };

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

                    case "$kind_of_feeling$":

                        text = text.Replace("$kind_of_feeling$", KindOfFeelings[FeelingDictionaryKey].Split(",")[0]);

                        this.KindOfFeelingInputButton.IsEnabled = true;

                        if (text == "")
                            this.SizeOfFeelingInputButton.IsEnabled = false;
                        else
                            this.SizeOfFeelingInputButton.IsEnabled = true;
                        break;
                        

                    case "$kind_of_feeling_before_change$":

                        if(this.scene == "青助くんのきもちときもちの大きさの変化")
                        {
                            this.FeelingDictionaryKey = "aosuke_pre_use_item";
                        }
                        else if (this.scene == "赤丸くんのきもちときもちの大きさの変化")
                        {
                            this.FeelingDictionaryKey="akamaru_pre_use_item";
                        }
                        else if (this.scene == "キミちゃんのきもちときもちの大きさの変化")
                        {
                            this.FeelingDictionaryKey= "kimi_pre_use_item";
                        }


                        text = text.Replace("$kind_of_feeling_before_change$", KindOfFeelings[FeelingDictionaryKey].Split(",")[0]);

                        this.KindOfFeelingInputButton.IsEnabled = false;

                        break;

                    case "$let's_check_kind_of_feeling$":

                        this.FeelingDictionaryKey = "let's_check";

                        text = text.Replace("$let's_check_kind_of_feeling$", KindOfFeelings[FeelingDictionaryKey].Split(",")[0]);

                        if(text != "")
                        {
                            this.Let_sCheckSizeOfFeelingInputButton.IsEnabled = true;
                        }
                        else
                        {
                            this.Let_sCheckSizeOfFeelingInputButton.IsEnabled = false;
                        }
                        break;

                    case "$positive_thinking_kind_of_feeling$":

                        this.FeelingDictionaryKey = "positive_thinking";

                        text = text.Replace("$positive_thinking_kind_of_feeling$", KindOfFeelings[FeelingDictionaryKey].Split(",")[0]);

                        if (text != "")
                        {
                            this.PositiveThinkingSizeOfFeelingInputButton.IsEnabled = true;
                        }
                        else
                        {
                            this.PositiveThinkingSizeOfFeelingInputButton.IsEnabled = false;
                        }
                       
                        break;

                    case "$thoughts_of_others_kind_of_feeling$":

                        this.FeelingDictionaryKey = "thoughts_of_others";

                        text = text.Replace("$thoughts_of_others_kind_of_feeling$", KindOfFeelings[FeelingDictionaryKey].Split(",")[0]);

                        if (text != "")
                        {
                            this.ThoughtsOfOthersSizeOfFeelingInputButton.IsEnabled = true;
                        }
                        else
                        {
                            this.ThoughtsOfOthersSizeOfFeelingInputButton.IsEnabled = false;
                        }
                        break;

                    case "$size_of_feeling$":

                        if (this.SizeOfFeelings[FeelingDictionaryKey] != -1)
                        {
                            text = text.Replace("$size_of_feeling$", this.SizeOfFeelings[FeelingDictionaryKey].ToString());
                        }
                        else 
                        {
                            text = text.Replace("$size_of_feeling$", "");
                        }

                        break;

                    case "$size_of_feeling_before_change$":

                        if (this.scene == "青助くんのきもちときもちの大きさの変化")
                        {
                            this.FeelingDictionaryKey = "aosuke_pre_use_item";
                        }
                        else if (this.scene == "赤丸くんのきもちときもちの大きさの変化")
                        {
                            this.FeelingDictionaryKey = "akamaru_pre_use_item";
                        }
                        else if (this.scene == "キミちゃんのきもちときもちの大きさの変化")
                        {
                            this.FeelingDictionaryKey = "kimi_pre_use_item";
                        }

                        if (this.SizeOfFeelings[FeelingDictionaryKey] != -1)
                        {
                            text = text.Replace("$size_of_feeling_before_change$", this.SizeOfFeelings[FeelingDictionaryKey].ToString());
                        }
                        else
                        {
                            text = text.Replace("$size_of_feeling_before_change$", "");
                        }

                        this.SizeOfFeelingInputButton.IsEnabled = false;

                        break;

                    case "$let's_check_size_of_feeling$":

                        this.FeelingDictionaryKey = "let's_check";

                        if (this.SizeOfFeelings[FeelingDictionaryKey] != -1)
                        {
                            text = text.Replace("$let's_check_size_of_feeling$", this.SizeOfFeelings[FeelingDictionaryKey].ToString());
                        }
                        else
                        {
                            text = text.Replace("$let's_check_size_of_feeling$", "");
                        }

                        break;

                    case "$positive_thinking_size_of_feeling$":

                        this.FeelingDictionaryKey = "positive_thinking";

                        if (this.SizeOfFeelings[FeelingDictionaryKey] != -1)
                        {
                            text = text.Replace("$positive_thinking_size_of_feeling$", this.SizeOfFeelings[FeelingDictionaryKey].ToString());
                        }
                        else
                        {
                            text = text.Replace("$positive_thinking_size_of_feeling$", "");
                        }

                        break;
                    case "$thoughts_of_others_size_of_feeling$":

                        this.FeelingDictionaryKey = "thoughts_of_others";

                        if (this.SizeOfFeelings[FeelingDictionaryKey] != -1)
                        {
                            text = text.Replace("$thoughts_of_others_size_of_feeling$", this.SizeOfFeelings[FeelingDictionaryKey].ToString());
                        }
                        else
                        {
                            text = text.Replace("$thoughts_of_others_size_of_feeling$", "");
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
            if(!this.isAnimationSkip)
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
                                    this.KindOfFeelings[FeelingDictionaryKey] = $"{this.SelectGoodFeelingListBox.SelectedItem.ToString().Replace("●　", "")},良い";
                                }
                                else if (this.SelectBadFeelingListBox.SelectedItems != null)
                                {
                                    this.KindOfFeelings[FeelingDictionaryKey] = $"{this.SelectBadFeelingListBox.SelectedItem.ToString().Replace("●　", "")},悪い";
                                }

                                this.SizeOfFeelingInputButton.IsEnabled = true;
                            }
                        }
                        else if (this.SelectHeartGrid.IsVisible)
                        {
                            this.SizeOfFeelings[FeelingDictionaryKey] = int.Parse(this.ViewSizeOfFeelingTextBlock.Text);
                        }
                    }
                    else if (button.Name == "MangaFlipButton")
                    {
                        this.MangaFlipButton.Visibility = Visibility.Hidden;
                    }




                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }
                else if (Regex.IsMatch(button.Name, ".+indOfFeelingInputButton"))
                {
                    this.SelectBadFeelingListBox.SelectedIndex = -1;
                    this.SelectGoodFeelingListBox.SelectedIndex = -1;

                    if (button.Name == "Let_sCheckKindOfFeelingInputButton")
                    {
                        this.FeelingDictionaryKey = "let's_check";
                    }
                    else if (button.Name== "PositiveThinkingKindOfFeelingInputButton")
                    {
                        this.FeelingDictionaryKey= "positive_thinking";
                    }
                    else if (button.Name == "ThoughtsOfOthersKindOfFeelingInputButton")
                    {
                        this.FeelingDictionaryKey = "thoughts_of_others";
                    }

                    this.GoTo("kind_of_feeling","sub");
                }
                else if (Regex.IsMatch(button.Name, ".+izeOfFeelingInputButton"))
                {
                    if (button.Name == "Let_sCheckSizeOfFeelingInputButton")
                    {
                        this.FeelingDictionaryKey = "let's_check";
                    }
                    else if (button.Name == "PositiveThinkingSizeOfFeelingInputButton")
                    {
                        this.FeelingDictionaryKey = "positive_thinking";
                    }
                    else if (button.Name == "ThoughtsOfOthersSizeOfFeelingInputButton")
                    {
                        this.FeelingDictionaryKey = "thoughts_of_others";
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

                else if (button.Name == "HintCheckButton")
                {
                    if (this.scene == "青助くんのきもちときもちの大きさの変化")
                    {
                        this.GoTo("aosuke's_feeling_before_change","sub");
                    }
                    else if (this.scene == "赤丸くんのきもちときもちの大きさの変化")
                    {
                        this.GoTo("akamaru's_feeling_before_change","sub");
                    }
                    else if (this.scene == "キミちゃんのきもちときもちの大きさの変化")
                    {
                        this.GoTo("kimi's_feeling_before_change","sub");
                    }
                    if(this.scene == "グループアクティビティ")
                    {
                        this.GoTo("check_aosuke's_ojamamushi","sub");
                    }

                    this.KindOfFeelingInputButton.IsEnabled = false;
                    this.SizeOfFeelingInputButton.IsEnabled = false;
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
                else if(Regex.IsMatch(button.Name , "GroupeActivityInputButton."))
                {
                    if (this.dataOption.InputMethod == 0)
                    {
                        switch (button.Name)
                        {
                            case "GroupeActivityInputButton1":
                                this.InputDictionaryKey = "groupe_activity_input1";
                                this.InputStroke[this.InputDictionaryKey] = this.GroupeActivityInputCanvas1.Strokes;
                                break;

                            case "GroupeActivityInputButton2":
                                this.InputDictionaryKey = "groupe_activity_input2";
                                this.InputStroke[this.InputDictionaryKey] = this.GroupeActivityInputCanvas2.Strokes;
                                break;

                            case "GroupeActivityInputButton3":
                                this.InputDictionaryKey = "groupe_activity_input3";
                                this.InputStroke[this.InputDictionaryKey] = this.GroupeActivityInputCanvas3.Strokes;
                                break;
                        };

                        this.InputCanvas.Strokes = this.InputStroke[this.InputDictionaryKey];
                        this.ClipStrokes(this.InputCanvas, this.InputStroke[this.InputDictionaryKey]);
                        this.GoTo("canvas_input","sub");
                    }
                    else
                    {
                        switch (button.Name)
                        {
                            case "GroupeActivityInputButton1":
                                this.InputDictionaryKey = "groupe_activity_input1";
                                this.InputText[this.InputDictionaryKey] = this.GroupeActivityInputText1.Text;
                                break;

                            case "GroupeActivityInputButton2":
                                this.InputDictionaryKey = "groupe_activity_input2";
                                this.InputText[this.InputDictionaryKey] = this.GroupeActivityInputText2.Text;
                                break;

                            case "GroupeActivityInputButton3":
                                this.InputDictionaryKey = "groupe_activity_input3";
                                this.InputText[this.InputDictionaryKey] = this.GroupeActivityInputText3.Text;
                                break;
                        };
                        
                        this.InputTextBox.Text = this.InputText[this.InputDictionaryKey];
                        this.GoTo("keyboard_input","sub");
                        this.InputTextBox.Focus();
                    }
                }
                else if (button.Name == "CompleteInputButton")
                {
                    if (this.dataOption.InputMethod == 0)
                    {
                        switch (this.InputDictionaryKey)
                        {
                            case "groupe_activity_input1":
                                if (this.InputStroke[this.InputDictionaryKey] != null)
                                {
                                    this.ClipStrokes(this.InputCanvas, this.InputStroke[this.InputDictionaryKey]);
                                    this.GroupeActivityInputCanvas1.Strokes = this.InputStroke[this.InputDictionaryKey];
                                }
                                break;

                            case "groupe_activity_input2":
                                if (this.InputStroke[this.InputDictionaryKey] != null)
                                {
                                    this.ClipStrokes(this.InputCanvas, this.InputStroke[this.InputDictionaryKey]);
                                    this.GroupeActivityInputCanvas2.Strokes = this.InputStroke[this.InputDictionaryKey];
                                }
                                break;

                            case "groupe_activity_input3":
                                if (this.InputStroke[this.InputDictionaryKey] != null)
                                {
                                    this.ClipStrokes(this.InputCanvas, this.InputStroke[this.InputDictionaryKey]);
                                    this.GroupeActivityInputCanvas3.Strokes = this.InputStroke[this.InputDictionaryKey];
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (this.InputDictionaryKey)
                        {
                            case "groupe_activity_input1":
                                this.InputText[this.InputDictionaryKey] = this.InputTextBox.Text;
                                this.GroupeActivityInputText1.Text = this.InputText[this.InputDictionaryKey];


                                break;

                            case "groupe_activity_input2":
                                this.InputText[this.InputDictionaryKey] = this.InputTextBox.Text;
                                this.GroupeActivityInputText2.Text = this.InputText[this.InputDictionaryKey];

                                break;

                            case "groupe_activity_input3":
                                this.InputText[this.InputDictionaryKey] = this.InputTextBox.Text;
                                this.GroupeActivityInputText3.Text = this.InputText[this.InputDictionaryKey];
                                break;
                        }
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
        private static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(Chapter8), new UIPropertyMetadata(0.0));

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
                        this.scenarioCount = index + 1;
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
                    Process.Start("./tabtip.vbs");
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

            int MaxLine = 2;

            while (text.LineCount > MaxLine + 1)
            {
                caretPosition -= 1;
                text.Text = text.Text.Remove(caretPosition, 1);
            }

            text.Select(caretPosition, 0);
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            TextBox text = sender as TextBox;

            int MaxLine = 2;

            if (text.LineCount > MaxLine)
            {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                }
            }
        }

        private class GroupeActivityData
        {
            public GroupeActivityData(string firstRedText, string subsequentBlueText, string methodBlackText, string fileName )
            {
                FirstRedText = firstRedText;
                SubsequentBlueText = subsequentBlueText;
                MethodBlackText = methodBlackText;
                FileName = fileName;
            }

            public string FirstRedText { get; set; }
            public string SubsequentBlueText { get; set; }
            public string MethodBlackText { get; set; }
            public string FileName { get; set; }
        }


        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem listBoxItem = sender as ListBoxItem;

            switch (listBoxItem.Name)
            {
                case "PenButton":
                    this.InputCanvas.EditingMode = InkCanvasEditingMode.Ink;

                    this.ClipStrokes(this.InputCanvas, this.InputStroke[this.InputDictionaryKey]);

                    break;

                case "EraserButton":
                    this.InputCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;

                    break;

                case "AllClearButton":
                    this.InputCanvas.Strokes.Clear();

                    this.ClipStrokes(this.InputCanvas, this.InputStroke[this.InputDictionaryKey]);
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
    }
}