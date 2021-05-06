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

namespace KokoroUpTime


{
    /// <summary>
    /// GameWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class Chapter11 : Page
    {
        //待ち時間

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

        // マウス押下中フラグ
        private bool isMouseDown = false;

        ////Gesture
        //string RecognizingCanvasName = "";

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
        private DataChapter11 dataChapter11;

        //入力したストロークを保存するためのコレクション
        private StrokeCollection AkamaruOtherSolutionUseItemInputStroke = new StrokeCollection();
        private StrokeCollection[] Step2AkamaruMethodInputStrokes = new StrokeCollection[] { new StrokeCollection(), new StrokeCollection(), new StrokeCollection(), };
        private StrokeCollection[] Step2AosukeMethodInputStrokes = new StrokeCollection[] { new StrokeCollection(), new StrokeCollection(), new StrokeCollection(), new StrokeCollection(), new StrokeCollection(), new StrokeCollection()};

        private string[] Step2AkamaruMethodInputText = new string[] { "","","" };
        private string[] Step2AosukeMethodInputText = new string[] { "","","","","","" };

        private Dictionary<string, string[]> AKAMARU_METHOD_TEXT_VALUES;

        private Dictionary<string, string[]> AOSUKE_METHOD_TEXT_VALUES;
        private Dictionary<StrokeCollection, string[]> AOSUKE_METHOD_STROKE_VALUES;

        private ObservableCollection<MethodStrokeValueData> _methodstrokedata;
        private ObservableCollection<MethodTextValueData> _methodtextdata;
        private ObservableCollection<EvaluatedMethodStrokeData> _evaluatedmethodstrokedata;
        private ObservableCollection<EvaluatedMethodTextData> _evaluatedmethodtextdata;


        // ゲームの切り替えシーン
        private string scene;

        //選択した入力方法によってDataTemplateを切り替えるためのインスタンス
        private InputMethodStyleSelector styleSelector = new InputMethodStyleSelector();

        // データベースに収めるデータモデルのインスタンス
        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();
        public DataProgress dataProgress = new DataProgress();

        public Chapter11()
        {
            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            // メディアプレーヤークラスのインスタンスを作成する
            this.mediaPlayer = new WindowsMediaPlayer();


            // データモデルインスタンス確保
            this.dataChapter11 = new DataChapter11();

            // マウスイベントの設定
            this.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
            this.MouseUp += new MouseButtonEventHandler(OnMouseUp);
            this.MouseMove += new MouseEventHandler(OnMouseMove);

            if (this.dataChapter11.SelectedAkamaruGoalText == null)
            {
                this.dataChapter11.SelectedAkamaruGoalText = "";
            }

            this.AKAMARU_METHOD_TEXT_VALUES = new Dictionary<string, string[]>()
            {
                [(string)this.EvaluateAkamaruMethodListView.Items[0]] = new string[] { "", "", "", "" },
                [(string)this.EvaluateAkamaruMethodListView.Items[1]] = new string[] { "", "", "", "" },
                [(string)this.EvaluateAkamaruMethodListView.Items[2]] = new string[] { "", "", "", "" },

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
                ["session_title_right_image"] = this.SessionTitleRightImage,
                ["cover_layer_image"] = this.CoverLayerImage,

                ["main_msg_bubble_image"] = this.MainMessageBubbleImage,
               
                ["item_left_large_image"]=this.ItemLeftLargeImage,
                ["item_center_large_image"]=this.ItemCenterLargeImage,
                ["dog_image"]=this.DogImage,
                ["anger_image"] = this.AngerImage,
                ["akamaru_ojamamushi_image"] =this.AkamaruOjamamushiImage,
                ["akamaru_ojamamushi_image_1"] = this.AkamaruOjamamushiImage1,
                ["item_11_image"]=this.Item11Image,
                ["akamaru_problem_right_image"]=this.AkamaruProblemRightImage,
                ["akamaru_problem_left_image"]=this.AkamaruProblemLeftImage,
                ["angel_image"]=this.AngelImage,
                ["step1_aosuke_image"]=this.Step1AosukeImage,



        };

            this.textBlockObjects = new Dictionary<string, TextBlock>
            {
                ["session_sub_title_text"] = this.SessionSubTitleTextBlock,
                ["session_sentence_text"] = this.SessionSentenceTextBlock,
                ["session_sub_right_title_text"] = this.SessionSubTitleRightTextBlock,
                ["session_sentence_right_text"] = this.SessionSentenceRightTextBlock,

                ["ending_msg_text"] = this.EndingMessageTextBlock,
                ["main_msg"] = this.MainMessageTextBlock,
                ["music_title_text"] = this.MusicTitleTextBlock,
                ["composer_name_text"] = this.ComposerNameTextBlock,
                ["item_book_title_text"] = this.ItemBookTitleTextBlock,
                ["hint_check_text"] = this.HintCheckText,

                ["challenge_time_title_text"] = this.ChallengeTImeTitleText,
                ["how_to_use_item_text"] =this.HowToUseItemText,
                ["step_to_solve_text"] =this.StepToSolveText,

                ["solution_title_text"] = this.SolutionTitleText,
                ["solution_text"] = this.SolutionText,

                ["s2_input_aka_many_solutions_title_text"]=this.Step2InputAkamaruManySolutionTitleText,
                ["s1_aka_goal_text"]=this.Step1AkamaruGoalText,
                ["s2_exa_aka_method_text"]=this.Step2ExampleAkamaruMethodText,
                ["evaluate_akamaru_method_text"]=this.EvaluateAkamaruMethodText,
                ["step1_bad_feeling_scene_text"] = this.Step1BadFeelingSceneText,
                ["evaluate_aosuke_method_title_text"] = this.EvaluateAosukeMethodTitleText,

                ["how_to_use_item_text"] =this.HowToUseItemText,
                ["grow_up_children_text"] = this.GrowUpChildrenText,
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

                ["item_03_button"] = this.Item03Button,
                ["item_04_button"] = this.Item04Button,
                ["item_05_button"] = this.Item05Button,
               
                ["select_akamaru_goal_button_1"] =this.SelectAkamaruGoalButton1,
                ["select_akamaru_goal_button_2"] = this.SelectAkamaruGoalButton2,
                ["next_method_button"] = this.NextMethodButton,
                ["back_method_button"] = this.BackMethodButton,

                ["input_button2"] = this.InputButton2,

            };

            this.gridObjects = new Dictionary<string, Grid>
            {
                ["session_grid"] = this.SessionGrid,
                ["session_frame_grid"] = this.SessionFrameGrid,
                ["session_frame_right_grid"] = this.SessionFrameRightGrid,
                ["manga_grid"] = this.MangaGrid,
                ["title_grid"] = this.TitleGrid,

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
                ["hint_check_grid"] =this.HintCheckGrid,

                ["akamaru_current_status_grid"]=this.AkamaruCurrentStatusGrid,
                ["akamaru_frustrated_situation_grid"] =this.AkamaruFrustratedSituationGrid,
                ["multiple_items_grid"]=this.MultipleItemsGrid,
                ["step_to_solve_grid"] =this.StepToSolveGrid,
                ["akamaru_problem_grid"] =this.AkamaruProblemGrid,
                ["select_akamaru_goal_grid"]=this.SelectAkamaruGoalGrid,
                ["multiple_items_grid"] =this.MultipleItemsGrid,
                ["solution_grid"]=this.SolutionGrid,
                ["input_akamaru_other_solution_grid"] =this.InputAkamaruOtherSolutionGrid,
                ["step2_input_aosuke_many_solutions_grid"] =this.Step2InputAosukeManySolutionsGrid,
                ["step2_aosuke_solution_grid"] = this.Step2AosukeSolutionGrid,
                ["step2_input_akamaru_many_solution_grid"] =this.Step2InputAkamaruManySolutionGrid,
                ["example_step3_evaluate_akamaru_methods_grid"] =this.ExampleEvaluateAkamaruMethodsGrid,
                ["example_evaluate_grid"] =this.ExampleEvaluateGrid,
                ["step3_evaluate_akamaru_methods_grid"] =this.Step3EvaluateAkamaruMethodGrid,
                ["evaluate_akamaru_method_grid"]=this.EvaluateAkamaruMethodGrid,
                ["step3_evaluate_aosuke_methods_grid"] = this.Step3EvaluateAosukeMethodGrid,
                ["evaluate_aosuke_method_grid"] = this.EvaluateAosukeMethodGrid,
                ["step1_aosukes_bad_feeling_scene_grid"]=this.Step1AosukeBadFeelingSceneGrid,
                ["confirm_button_grid"]=this.ConfirmButtonGrid,
                ["grow_up_children_grid"] = this.GrowUpChildrenGrid,

                ["canvas_edit_grid"] = this.CanvasEditGrid,

                ["input_canvas_grid_1"] =this.InputCanvasGrid1,
                ["input_text_grid_1"] = this.InputTextBoxGrid1,
                ["input_canvas_grid_2"] = this.InputCanvasGrid2,
                ["input_text_grid_2"] = this.InputTextBoxGrid2,
                ["input_canvas_grid_3"] = this.InputCanvasGrid3,
                ["input_text_grid_3"] = this.InputTextBoxGrid3,
            };

            this.borderObjects = new Dictionary<string, Border>
            {
                ["title_border"] = this.TitleBorder,
            
                ["input_canvas_border"] =this.InputCanvasBorder,
                ["input_text_box_border"] = this.InputTextBoxBorder,
                ["light_green_border"]=this.LightGreenBorder,
                ["light_green_c_border"]=this.LightGreenCenterBorder,
                ["light_green_l_border"]=this.LightGreenLeftBorder,
                ["light_green_r_border"]=this.LightGreenRightBorder,

                ["input_canvas_border"]=this.InputCanvasBorder,
                ["input_text_border"] = this.InputTextBoxBorder,
                ["what_akamaru_solved_border"]=this.WhatAkamaruSolvedBorder,
                ["what_akamaru_will_solve_border"]=this.WhatAkamaruWillSolveBorder,
                ["select_akamaru_goal_border"] =this.SelectAkamaruGoalBorder,
                ["example_akamaru_method_border"]=this.ExampleAkamaruMethodBorder,
                ["step1_aosuke_goal_border"] = this.Step1AosukeGoalBorder,
                ["view_input_aosuke_method_border"] = this.ViewInputAosukeMethodBorder,

                ["input_button_border"] = this.InputButtonBorder,
            };

            this.outlineTextObjects = new Dictionary<string, OutlineText>
            {
                ["lets_solve_problems_scene_title"] =this.LetsSolveProblemsSceneTitle,
                ["lets_use_item_scene_title"] =this.LetsUseItemSceneTitle,
                ["step_to_solve_problem_scene_title"] =this.StepToSolveProblemSceneTitle,
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
            this.SessionFrameRightGrid.Visibility = Visibility.Hidden;
            this.SessionTitleImage.Visibility = Visibility.Hidden;
            this.SessionTitleRightImage.Visibility = Visibility.Hidden;


            this.TitleGrid.Visibility = Visibility.Hidden;
            this.TitleBorder.Visibility = Visibility.Hidden;
            this.LetsSolveProblemsSceneTitle.Visibility = Visibility.Hidden;
            this.StepToSolveProblemSceneTitle.Visibility = Visibility.Hidden;
            this.LetsUseItemSceneTitle.Visibility = Visibility.Hidden;
            this.ChallengeTImeTitleText.Visibility = Visibility.Hidden;

            this.LightGreenBorder.Visibility = Visibility.Hidden;
            this.Step1AosukeBadFeelingSceneGrid.Visibility = Visibility.Hidden;
            this.Step1AosukeImage.Visibility = Visibility.Hidden;
            this.Step1BadFeelingSceneText.Visibility = Visibility.Hidden;
            this.Step1AosukeGoalBorder.Visibility = Visibility.Hidden;
            this.ExampleEvaluateAkamaruMethodsGrid.Visibility =Visibility.Hidden;
            this.ExampleEvaluateGrid.Visibility = Visibility.Hidden;
            this.Step3EvaluateAkamaruMethodGrid.Visibility = Visibility.Hidden;
            this.EvaluateAkamaruMethodText.Visibility = Visibility.Hidden;
            this.EvaluateAkamaruMethodGrid.Visibility = Visibility.Hidden;
            this.Step3EvaluateAosukeMethodGrid.Visibility = Visibility.Hidden;
            this.EvaluateAosukeMethodTitleText.Visibility = Visibility.Hidden; 
            this.EvaluateAosukeMethodGrid.Visibility = Visibility.Hidden; 
            this.NextMethodButton.Visibility = Visibility.Hidden;
            this.BackMethodButton.Visibility = Visibility.Hidden;

            this.LightGreenCenterBorder.Visibility = Visibility.Hidden;
            this.AkamaruProblemGrid.Visibility = Visibility.Hidden;
            this.AkamaruOjamamushiImage1.Visibility = Visibility.Hidden;
            this.AkamaruProblemRightImage.Visibility = Visibility.Hidden;
            this.AkamaruProblemLeftImage.Visibility = Visibility.Hidden;
            this.SelectAkamaruGoalGrid.Visibility = Visibility.Hidden;
            this.SelectAkamaruGoalButton1.Visibility = Visibility.Hidden;
            this.SelectAkamaruGoalButton2.Visibility = Visibility.Hidden;
            this.SelectAkamaruGoalBorder.Visibility = Visibility.Hidden;
            this.AngelImage.Visibility = Visibility.Hidden;
            this.GrowUpChildrenGrid.Visibility = Visibility.Hidden;
            this.GrowUpChildrenText.Visibility = Visibility.Hidden;



            this.LightGreenLeftBorder.Visibility = Visibility.Hidden;
            this.MultipleItemsGrid.Visibility = Visibility.Hidden;
            this.Item03Button.Visibility = Visibility.Hidden;
            this.Item04Button.Visibility = Visibility.Hidden;
            this.Item05Button.Visibility = Visibility.Hidden;
            this.StepToSolveGrid.Visibility = Visibility.Hidden;
            this.SolutionGrid.Visibility = Visibility.Hidden;
            this.GrowUpChildrenGrid.Visibility = Visibility.Hidden;

            this.LightGreenRightBorder.Visibility = Visibility.Hidden;
            this.AkamaruFrustratedSituationGrid.Visibility = Visibility.Hidden;
            this.WhatAkamaruSolvedBorder.Visibility = Visibility.Hidden;
            this.WhatAkamaruWillSolveBorder.Visibility = Visibility.Hidden;
            this.DogImage.Visibility = Visibility.Hidden;
            this.AngerImage.Visibility = Visibility.Hidden;
            this.AkamaruCurrentStatusGrid.Visibility = Visibility.Hidden;
            this.AkamaruFrustratedSituationGrid.Visibility = Visibility.Hidden;
            this.AkamaruOjamamushiImage.Visibility = Visibility.Hidden;
            this.StepToSolveGrid.Visibility = Visibility.Hidden;
            this.MultipleItemsGrid.Visibility = Visibility.Hidden;
            this.HowToUseItemText.Visibility = Visibility.Hidden;

            this.ViewInputAosukeMethodBorder.Visibility = Visibility.Hidden;
            this.InputButtonBorder.Visibility = Visibility.Hidden;
            this.InputAkamaruOtherSolutionGrid.Visibility = Visibility.Hidden;
            this.Step2InputAosukeManySolutionsGrid.Visibility = Visibility.Hidden;
            this.Step2AosukeSolutionGrid.Visibility = Visibility.Hidden;
            this.EvaluateAosukeMethodTitleText.Visibility = Visibility.Hidden;
            this.NextMethodButton.Visibility = Visibility.Hidden;
            this.Step2InputAkamaruManySolutionGrid.Visibility = Visibility.Hidden;
            this.Step2InputAkamaruManySolutionTitleText.Visibility = Visibility.Hidden;
            this.Step1AkamaruGoalText.Visibility=Visibility.Hidden;
            this.Step2ExampleAkamaruMethodText.Visibility=Visibility.Hidden;
            this.InputButton2.Visibility = Visibility.Hidden;

            this.ConfirmButtonGrid.Visibility = Visibility.Hidden;
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
            this.HintCheckGrid.Visibility = Visibility.Hidden;


            this.CoverLayerImage.Visibility = Visibility.Hidden;
            this.ReturnToTitleButton.Visibility = Visibility.Hidden;
      
            this.ItemLeftLargeImage.Visibility = Visibility.Hidden;
            this.ItemCenterLargeImage.Visibility = Visibility.Hidden;
            this.ItemNamePlateLeftGrid.Visibility = Visibility.Hidden;

            this.InputTextBoxGrid1.Visibility = Visibility.Hidden;
            this.InputTextBoxGrid2.Visibility = Visibility.Hidden;
            this.InputTextBoxGrid3.Visibility = Visibility.Hidden;
            this.InputTextBoxBorder.Visibility = Visibility.Hidden;
            this.InputCanvasGrid1.Visibility = Visibility.Hidden;
            this.InputCanvasGrid2.Visibility = Visibility.Hidden;
            this.InputCanvasGrid3.Visibility = Visibility.Hidden;
            this.InputCanvasBorder.Visibility = Visibility.Hidden;


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
            if (this.dataOption.InputMethod == 0)
            {
                this.OutputText1.Visibility = Visibility.Hidden;
                this.OutputText2_1.Visibility = Visibility.Hidden;
                this.OutputText2_2.Visibility = Visibility.Hidden;
                this.OutputText2_3.Visibility = Visibility.Hidden;
                this.OutputText3_1.Visibility = Visibility.Hidden;
                this.OutputText3_2.Visibility = Visibility.Hidden;
                this.OutputText3_3.Visibility = Visibility.Hidden;
                this.OutputText3_4.Visibility = Visibility.Hidden;
                this.OutputText3_5.Visibility = Visibility.Hidden;
                this.OutputText3_6.Visibility = Visibility.Hidden;

                this.ViewInputAosukeMethodItemsControl.ItemTemplate = this.FindResource("ViewEvaluatedAosukeMethodCanvasTemplate") as DataTemplate;
                this.AOSUKE_METHOD_STROKE_VALUES = new Dictionary<StrokeCollection, string[]>();
                this._methodstrokedata = new ObservableCollection<MethodStrokeValueData>();
                this._evaluatedmethodstrokedata = new ObservableCollection<EvaluatedMethodStrokeData>();

            }
            else
            {
                this.OutputCanvas1.Visibility = Visibility.Hidden;
                this.OutputCanvas2_1.Visibility = Visibility.Hidden;
                this.OutputCanvas2_2.Visibility = Visibility.Hidden;
                this.OutputCanvas2_3.Visibility = Visibility.Hidden;
                this.OutputCanvas3_1.Visibility = Visibility.Hidden;
                this.OutputCanvas3_2.Visibility = Visibility.Hidden;
                this.OutputCanvas3_3.Visibility = Visibility.Hidden;
                this.OutputCanvas3_4.Visibility = Visibility.Hidden;
                this.OutputCanvas3_5.Visibility = Visibility.Hidden;
                this.OutputCanvas3_6.Visibility = Visibility.Hidden;

                this.dataChapter11.AkamaruOtherSolutionUseItemInputText = "";

                this.ViewInputAosukeMethodItemsControl.ItemTemplate = this.FindResource("ViewEvaluatedAosukeMethodTextTemplate") as DataTemplate;
                this.AOSUKE_METHOD_TEXT_VALUES = new Dictionary<string, string[]>();
                this._methodtextdata = new ObservableCollection<MethodTextValueData>();
                this._evaluatedmethodtextdata = new ObservableCollection<EvaluatedMethodTextData>();

            }
        }

        public void SetNextPage(InitConfig _initConfig, DataOption _dataOption, DataItem _dataItem, DataProgress _dataProgress)
        {
            this.initConfig = _initConfig;
            this.dataOption = _dataOption;
            this.dataItem = _dataItem;
            this.dataProgress = _dataProgress;

            // 現在時刻を取得
            this.dataChapter11.CreatedAt = DateTime.Now.ToString();

            // データベースのテーブル作成と現在時刻の書き込みを同時に行う
            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
            {
                // 毎回のアクセス日付を記録
                connection.Insert(this.dataChapter11);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var csv = new CsvReader("./Scenarios/chapter11.csv"))
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

                    this.dataProgress.CurrentChapter = 11;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET CurrentChapter = '{this.dataProgress.CurrentChapter}' WHERE Id = 1;");
                    }

                    this.SetInputMethod();

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "end":

                    // 画面のフェードアウト処理とか入れる（別関数を呼び出す）

                    this.StopBGM();

                    this.dataProgress.HasCompletedChapter11 = true;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET HasCompletedChapter11 = '{Convert.ToInt32(this.dataProgress.HasCompletedChapter11)}' WHERE Id = 1;");
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
                    this.dataProgress.LatestChapter11Scene = this.scene;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataProgress SET CurrentScene = '{this.dataProgress.CurrentScene}', LatestChapter11Scene = '{this.dataProgress.LatestChapter11Scene}' WHERE Id = 1;");
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

                    if(Regex.IsMatch(this.position, ".*_method_button"))
                    {
                        this.IsVisibileMethodButton();
                    }
                    else
                    {
                        buttonObject.Visibility = Visibility.Visible;
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
                                        if(this.scene == "チャレンジタイム！　パート①"　&& this.InputAkamaruOtherSolutionGrid.IsVisible)
                                        {
                                            if (this.dataOption.InputMethod == 0)
                                            {
                                                if(this.AkamaruOtherSolutionUseItemInputStroke.Count > 0)
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Visible;
                                                    this.BackPageButton.Visibility = Visibility.Visible;
                                                }
                                                else
                                                {
                                                    this.BackPageButton.Visibility = Visibility.Visible;
                                                }
                                            }
                                            else
                                            {
                                                if (this.dataChapter11.AkamaruOtherSolutionUseItemInputText != "")
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Visible;
                                                    this.BackPageButton.Visibility = Visibility.Visible;
                                                }
                                                else
                                                {
                                                    this.BackPageButton.Visibility = Visibility.Visible;
                                                }
                                            }
                                        }
                                        else if(this.scene == "チャレンジタイム！　パート②")
                                        {
                                            bool checkFlag = false;
                                            
                                            if (this.Step2InputAkamaruManySolutionGrid.IsVisible)
                                            {
                                                if (this.dataOption.InputMethod == 0)
                                                {
                                                    foreach (StrokeCollection strokes in this.Step2AkamaruMethodInputStrokes)
                                                    {
                                                        if(strokes.Count > 0)
                                                        {
                                                            checkFlag = true;
                                                            break;
                                                        }
                                                    }
                                                    if (checkFlag)
                                                    {
                                                        this.NextPageButton.Visibility = Visibility.Visible;
                                                        this.BackPageButton.Visibility = Visibility.Visible;
                                                    }
                                                    else
                                                    {
                                                        this.BackPageButton.Visibility = Visibility.Visible;
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (string text in this.Step2AkamaruMethodInputText)
                                                    {
                                                        if (text != "")
                                                        {
                                                            checkFlag = true;
                                                            break;
                                                        }
                                                    }
                                                    if (checkFlag)
                                                    {
                                                        this.NextPageButton.Visibility = Visibility.Visible;
                                                        this.BackPageButton.Visibility = Visibility.Visible;
                                                    }
                                                    else
                                                    {
                                                        this.BackPageButton.Visibility = Visibility.Visible;
                                                    }
                                                }
                                            }
                                            else if (this.EvaluateAkamaruMethodListView.IsVisible)
                                            {
                                                this.CheckFillInTheBlanks("赤丸");
                                            }
                                            else
                                            {
                                                this.NextPageButton.Visibility = Visibility.Visible;
                                                this.BackPageButton.Visibility = Visibility.Visible;
                                            }
                                        }
                                        else if(this.scene == "グループアクティビティ")
                                        {
                                            if (this.Step2InputAosukeManySolutionsGrid.IsVisible)
                                            {
                                                bool checkFlag = false;
                                                if(this.dataOption.InputMethod == 0)
                                                {
                                                    foreach(StrokeCollection strokes in this.Step2AosukeMethodInputStrokes)
                                                    {
                                                        if(strokes.Count > 0)
                                                        {
                                                            checkFlag = true;
                                                        }
                                                    }
                                                    if (checkFlag)
                                                    {
                                                        this.NextPageButton.Visibility = Visibility.Visible;
                                                        this.BackPageButton.Visibility = Visibility.Visible;
                                                    }
                                                    else
                                                    {
                                                        this.BackPageButton.Visibility = Visibility.Visible;
                                                    }
                                                    
                                                }
                                                
                                                else
                                                {
                                                    foreach (string text in this.Step2AosukeMethodInputText)
                                                    {
                                                        if (text !="")
                                                        {
                                                            checkFlag = true;
                                                        }
                                                    }
                                                    if (checkFlag)
                                                    {
                                                        this.NextPageButton.Visibility = Visibility.Visible;
                                                        this.BackPageButton.Visibility = Visibility.Visible;
                                                    }
                                                    else
                                                    {
                                                        this.BackPageButton.Visibility = Visibility.Visible;
                                                    }
                                                }
                                            }
                                            else if (this.EvaluateAosukeMethodListView.IsVisible)
                                            {
                                                this.CheckFillInTheBlanks("青助");
                                            }
                                            else
                                            {
                                                this.NextPageButton.Visibility = Visibility.Visible;
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
                                    if (this.scene == "チャレンジタイム！　パート①" && this.InputAkamaruOtherSolutionGrid.IsVisible)
                                    {
                                        if (this.dataOption.InputMethod == 0)
                                        {
                                            if (this.AkamaruOtherSolutionUseItemInputStroke.Count > 0)
                                            {
                                                this.NextPageButton.Visibility = Visibility.Visible;
                                                this.BackPageButton.Visibility = Visibility.Visible;
                                            }
                                            else
                                            {
                                                this.BackPageButton.Visibility = Visibility.Visible;
                                            }
                                        }
                                        else
                                        {
                                            if (this.dataChapter11.AkamaruOtherSolutionUseItemInputText != "")
                                            {
                                                this.NextPageButton.Visibility = Visibility.Visible;
                                                this.BackPageButton.Visibility = Visibility.Visible;
                                            }
                                            else
                                            {
                                                this.BackPageButton.Visibility = Visibility.Visible;
                                            }
                                        }
                                    }
                                    else if (this.scene == "チャレンジタイム！　パート②")
                                    {
                                        bool checkFlag = false;

                                        if (this.Step2InputAkamaruManySolutionGrid.IsVisible)
                                        {
                                            if (this.dataOption.InputMethod == 0)
                                            {
                                                foreach (StrokeCollection strokes in this.Step2AkamaruMethodInputStrokes)
                                                {
                                                    if (strokes.Count > 0)
                                                    {
                                                        checkFlag = true;
                                                        break;
                                                    }
                                                }
                                                if (checkFlag)
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Visible;
                                                    this.BackPageButton.Visibility = Visibility.Visible;
                                                }
                                                else
                                                {
                                                    this.BackPageButton.Visibility = Visibility.Visible;
                                                }
                                            }
                                            else
                                            {
                                                foreach (string text in this.Step2AkamaruMethodInputText)
                                                {
                                                    if (text != "")
                                                    {
                                                        checkFlag = true;
                                                        break;
                                                    }
                                                }
                                                if (checkFlag)
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Visible;
                                                    this.BackPageButton.Visibility = Visibility.Visible;
                                                }
                                                else
                                                {
                                                    this.BackPageButton.Visibility = Visibility.Visible;
                                                }
                                            }
                                        }
                                        else if (this.EvaluateAkamaruMethodListView.IsVisible)
                                        {
                                            this.CheckFillInTheBlanks("赤丸");
                                        }
                                        else
                                        {
                                            this.NextPageButton.Visibility = Visibility.Visible;
                                            this.BackPageButton.Visibility = Visibility.Visible;
                                        }
                                    }
                                    else if (this.scene == "グループアクティビティ")
                                    {
                                        if (this.Step2InputAosukeManySolutionsGrid.IsVisible)
                                        {
                                            bool checkFlag = false;
                                            if (this.dataOption.InputMethod == 0)
                                            {
                                                foreach (StrokeCollection strokes in this.Step2AosukeMethodInputStrokes)
                                                {
                                                    if (strokes.Count > 0)
                                                    {
                                                        checkFlag = true;
                                                    }
                                                }
                                                if (checkFlag)
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Visible;
                                                    this.BackPageButton.Visibility = Visibility.Visible;
                                                }
                                                else
                                                {
                                                    this.BackPageButton.Visibility = Visibility.Visible;
                                                }

                                            }

                                            else
                                            {
                                                foreach (string text in this.Step2AosukeMethodInputText)
                                                {
                                                    if (text != "")
                                                    {
                                                        checkFlag = true;
                                                    }
                                                }
                                                if (checkFlag)
                                                {
                                                    this.NextPageButton.Visibility = Visibility.Visible;
                                                    this.BackPageButton.Visibility = Visibility.Visible;
                                                }
                                                else
                                                {
                                                    this.BackPageButton.Visibility = Visibility.Visible;
                                                }
                                            }
                                        }
                                        else if (this.EvaluateAosukeMethodListView.IsVisible)
                                        {
                                            this.CheckFillInTheBlanks("青助");
                                        }
                                        else
                                        {
                                            this.NextPageButton.Visibility = Visibility.Visible;
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
                                    this.GoTo("challenge_time_part1");
                                    break;

                                case "チャレンジタイム！　パート②":
                                    this.GoTo("think_akamaru_method");
                                    break;

                                case "グループアクティビティ":
                                    this.GoTo("think_aosuke_method");
                                    break;
                            }
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

                    this.dataItem.HasGotItem11 = true;

                    using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                    {
                        connection.Execute($@"UPDATE DataItem SET HasGotItem11 = 1 WHERE Id = 1;");
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

                    Image[] itemMainImages = { this.Item01MainImage, this.Item02MainImage, this.Item03MainImage, this.Item04MainImage, this.Item05MainImage, this.Item06MainImage, this.Item07MainImage, this.Item08MainImage, this.Item09MainImage, this.Item10MainImage };
                                                                                                                                                                                                                                                               
                    Image[] itemNoneImages = { this.Item01NoneImage, this.Item02NoneImage, this.Item03NoneImage, this.Item04NoneImage, this.Item05NoneImage, this.Item06NoneImage, this.Item07NoneImage, this.Item08NoneImage, this.Item09NoneImage, this.Item10NoneImage };

                    var hasGotItems = new bool[] { this.dataItem.HasGotItem01, this.dataItem.HasGotItem02, this.dataItem.HasGotItem03, this.dataItem.HasGotItem04, this.dataItem.HasGotItem05, this.dataItem.HasGotItem06, this.dataItem.HasGotItem07, this.dataItem.HasGotItem08, this.dataItem.HasGotItem09, this.dataItem.HasGotItem10 };

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

                    case "$selected_akamaru_goal$":
                        if(this.dataChapter11.SelectedAkamaruGoalText != "")
                        {
                            text = "目標："+this.dataChapter11.SelectedAkamaruGoalText;

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

                if ((button.Name == "NextMessageButton" || button.Name == "NextPageButton" || button.Name == "MangaFlipButton" || button.Name == "SelectFeelingCompleteButton" || button.Name == "BranchButton2" || button.Name == "MangaPrevBackButton" || button.Name == "GroupeActivityNextMessageButton" || button.Name == "ReturnButton"||button.Name== "DecideAosukeMethodButton"))
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

                        if(this.scene == "チャレンジタイム！　パート①")
                        {

                        }
                        else if(this.scene== "チャレンジタイム！　パート②")
                        {
                            
                            
                        }
                        else if (this.scene == "グループアクティビティ")
                        {
                            if (this.Step2InputAosukeManySolutionsGrid.IsVisible)
                            {
                                this.InitializedAosukeMethodListView();
                            }
                            else if (this.EvaluateAosukeMethodListView.IsVisible)
                            {
                                if(this.dataOption.InputMethod == 0)
                                {
                                    this._evaluatedmethodstrokedata.Clear();
                                    foreach (StrokeCollection strokes in this.AOSUKE_METHOD_STROKE_VALUES.Keys)
                                    {
                                        if(this.AOSUKE_METHOD_STROKE_VALUES[strokes][5] == "BestMethod")
                                        {
                                            foreach(System.Windows.Ink.Stroke stroke in strokes)
                                            {
                                                stroke.DrawingAttributes.Color = Colors.Red;
                                            }
                                            this._evaluatedmethodstrokedata.Add(new EvaluatedMethodStrokeData(strokes));
                                        }
                                        else
                                        {
                                            foreach (System.Windows.Ink.Stroke stroke in strokes)
                                            {
                                                stroke.DrawingAttributes.Color = Colors.Gray;
                                            }
                                            this._evaluatedmethodstrokedata.Add(new EvaluatedMethodStrokeData(strokes));
                                        }
                                    }
                                    this.ViewInputAosukeMethodItemsControl.ItemsSource = this._evaluatedmethodstrokedata;
                                }
                                else
                                {
                                    this._evaluatedmethodtextdata.Clear();
                                    foreach (string text in this.AOSUKE_METHOD_TEXT_VALUES.Keys)
                                    {
                                        if (this.AOSUKE_METHOD_TEXT_VALUES[text][5] == "BestMethod")
                                        {
                                            this._evaluatedmethodtextdata.Add(new EvaluatedMethodTextData(text,Brushes.Red));
                                        }
                                        else
                                        {
                                            this._evaluatedmethodtextdata.Add(new EvaluatedMethodTextData(text, Brushes.Gray));
                                        }
                                    }
                                    this.ViewInputAosukeMethodItemsControl.ItemsSource = this._evaluatedmethodtextdata;
                                }
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

                else if (button.Name == "KindOfFeelingInputButton")
                {
                    this.GoTo("select_kind_of_feeling");
                }
                else if (button.Name == "SizeOfFeelingInputButton")
                {
                    this.GoTo("select_size_of_feeling");
                }
                else if (button.Name == "BranchButton1")
                {
                    this.GoTo("manga");
                }
                else if (Regex.IsMatch(button.Name, "InputButton."))
                {
                    if (this.dataOption.InputMethod == 0)
                    {
                        switch (this.scene)
                        {
                            case "チャレンジタイム！　パート①":
                                this.GoTo("canvas_input_akamaru_other_solution");
                                break;

                            case "チャレンジタイム！　パート②":
                                this.GoTo("canvas_input_akamaru_many_solution");
                                break;

                            case "グループアクティビティ":
                                this.GoTo("canvas_input_aosuke_many_solution");
                                break;
                        }
                    }
                    else
                    {
                        switch (this.scene)
                        {
                            case "チャレンジタイム！　パート①":
                                this.GoTo("keyboard_input_akamaru_other_solution");
                                this.InputTextBox1.Focus();
                                break;

                            case "チャレンジタイム！　パート②":
                                this.GoTo("keyboard_input_akamaru_many_solution");
                                this.InputTextBox2_1.Focus();
                                break;

                            case "グループアクティビティ":
                                this.GoTo("keyboard_input_aosuke_many_solution");
                                 this.InputTextBox3_1.Focus();
                                break;
                        }
                    }
                }
                else if (button.Name == "CompleteInputButton")
                {
                    if (this.dataOption.InputMethod == 0)
                    {
                        switch (this.scene)
                        {
                            case "チャレンジタイム！　パート①":
                                this.AkamaruOtherSolutionUseItemInputStroke = this.InputCanvas1.Strokes;
                                this.OutputCanvas1.Strokes = this.AkamaruOtherSolutionUseItemInputStroke;
                                break;

                            case "チャレンジタイム！　パート②":
                                for (int i = 0; i < this.Step2AkamaruMethodInputStrokes.Length; i++)
                                {
                                    this.Step2AkamaruMethodInputStrokes[i] = ((InkCanvas)this.FindName($"InputCanvas2_{ i + 1}")).Strokes;
                                    ((InkCanvas)this.FindName($"OutputCanvas2_{ i + 1}")).Strokes = this.Step2AkamaruMethodInputStrokes[i];
                                }
                                break;

                            case "グループアクティビティ":
                                for (int i = 0; i < this.Step2AosukeMethodInputStrokes.Length; i++)
                                {
                                    this.Step2AosukeMethodInputStrokes[i] = ((InkCanvas)this.FindName($"InputCanvas3_{ i + 1}")).Strokes;
                                    ((InkCanvas)this.FindName($"OutputCanvas3_{ i + 1}")).Strokes = this.Step2AosukeMethodInputStrokes[i];
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (this.scene)
                        {
                            case "チャレンジタイム！　パート①":
                                this.dataChapter11.AkamaruOtherSolutionUseItemInputText = this.InputTextBox1.Text;
                                this.OutputText1.Text = this.dataChapter11.AkamaruOtherSolutionUseItemInputText;
                                break;

                            case "チャレンジタイム！　パート②":
                                for (int i = 0; i < this.Step2AkamaruMethodInputText.Length; i++)
                                {
                                    this.Step2AkamaruMethodInputText[i] = ((TextBox)this.FindName($"InputTextBox2_{ i + 1}")).Text;
                                    ((TextBlock)this.FindName($"OutputText2_{ i + 1}")).Text = this.Step2AkamaruMethodInputText[i];
                                }
                                break;

                            case "グループアクティビティ":
                                for(int i=0;i < this.Step2AosukeMethodInputText.Length ; i++)
                                {
                                    this.Step2AosukeMethodInputText[i] = ((TextBox)this.FindName($"InputTextBox3_{ i+1}")).Text;
                                    ((TextBlock)this.FindName($"OutputText3_{ i + 1}")).Text = this.Step2AosukeMethodInputText[i];
                                }
                                break;
                        }
                        this.CloseOSK();
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                }
                else if (Regex.IsMatch(button.Name, "Item0.Button"))
                {
                    switch (button.Name)
                    {
                        case "Item03Button":
                            this.InputItemImage.Source = new BitmapImage(new Uri($"Images/item03_solo.png", UriKind.Relative));
                            break;

                        case "Item04Button":
                            this.InputItemImage.Source = new BitmapImage(new Uri($"Images/item04_solo.png", UriKind.Relative));
                            break;

                        case "Item05Button":
                            this.InputItemImage.Source = new BitmapImage(new Uri($"Images/item05_solo.png", UriKind.Relative));

                            break;

                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }
                else if (button.Name == "HintCheckButton")
                {
                    switch (this.scene)
                    {
                        case "チャレンジタイム！　パート①":
                            this.GoTo("check_akamaru_situation");
                            break;

                        case "チャレンジタイム！　パート②":
                        case "グループアクティビティ":
                            this.GoTo("check_tips_for_thinking");
                            break;
                    }
                }
                else if (Regex.IsMatch(button.Name, "SelectAkamaruGoalButton."))
                {
                    if (button.Name == "SelectAkamaruGoalButton1")
                    {
                        this.dataChapter11.SelectedAkamaruGoalText = "ケンカした友だちと仲直りする";
                    }
                    else if (button.Name == "SelectAkamaruGoalButton2")
                    {
                        this.dataChapter11.SelectedAkamaruGoalText = "友だちが思っていたことを教えてもらう";
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }
                else if (Regex.IsMatch(button.Name, "Evaluate.+Button[1-4]"))
                {
                    TextBlock textBlock = button.Content as TextBlock;
                        switch (textBlock.Text)
                        {
                            case "":
                            case "✕":
                                textBlock.Text = "〇";
                                break;

                            case "〇":
                                textBlock.Text = "△";
                                break;
                            case "△":
                                textBlock.Text = "✕";
                                break;
                        }
                           
                        if (this.scene == "グループアクティビティ")
                        {
                            this.WriteMethodValueData((TextBlock)textBlock, this.EvaluateAosukeMethodListView);
                        }
                        else if (this.scene == "チャレンジタイム！　パート②")
                        {
                            this.WriteMethodValueData((TextBlock)textBlock, this.EvaluateAkamaruMethodListView);
                        }

                        this.isClickable = true;
                }
                else if (button.Name == "NextMethodButton")
                {
                    this.MovetoNextOrPreviousMethod("next");
                    this.IsVisibileMethodButton();
                    this.isClickable = true;
                }
                else if (button.Name == "BackMethodButton")
                {
                    this.MovetoNextOrPreviousMethod("prev");
                    this.IsVisibileMethodButton();
                    this.isClickable = true;
                }
                else if(button.Name == "DecideAosukeMethodButton")
                {
                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }
                else if(button.Name == "RetryButton")
                {
                    this.InitializedAosukeMethodListView();
                    this.GoTo("evaluate_aosuke_method");
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
        private static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(Chapter11), new UIPropertyMetadata(0.0));

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

                case "InputTextBox1":
                    MaxLine = 6;
                    break;

                case "InputTextBox2_1":
                case "InputTextBox2_2":
                case "InputTextBox2_3":
                    MaxLine = 2;
                    break;

                case "InputTextBox3_1":
                case "InputTextBox3_2":
                case "InputTextBox3_3":
                case "InputTextBox3_4":
                case "InputTextBox3_5":
                case "InputTextBox3_6":
                    MaxLine = 2;
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

                case "InputTextBox1":
                    MaxLine = 6;
                    break;

                case "InputTextBox2_1":
                case "InputTextBox2_2":
                case "InputTextBox2_3":
                    MaxLine = 2;
                    break;


                case "InputTextBox3_1":
                case "InputTextBox3_2":
                case "InputTextBox3_3":
                case "InputTextBox3_4":
                case "InputTextBox3_5":
                case "InputTextBox3_6":
                    MaxLine = 2;
                    break;
            }

            if (text.LineCount >= MaxLine)
            {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                }
            }
        }

        private class MethodTextValueData
        {
            public MethodTextValueData(string methodText, string[] methodValueText)
            {
                MethodText = methodText;
                MethodValueText1 = methodValueText[0];
                MethodValueText2 = methodValueText[1];
                MethodValueText3 = methodValueText[2];
                MethodValueText4 = methodValueText[3];
            }

            public string MethodText { get; set; }
            public string MethodValueText1 { get; set; }
            public string MethodValueText2 { get; set; }
            public string MethodValueText3 { get; set; }
            public string MethodValueText4 { get; set; }
        }

        private class MethodStrokeValueData
        {
            public MethodStrokeValueData(StrokeCollection methodStroke, string[] methodValueText)
            {
                MethodStroke = methodStroke;
                MethodValueText1 = methodValueText[0];
                MethodValueText2 = methodValueText[1];
                MethodValueText3 = methodValueText[2];
                MethodValueText4 = methodValueText[3];
            }

            public StrokeCollection MethodStroke { get; set; }
            public string MethodValueText1 { get; set; }
            public string MethodValueText2 { get; set; }
            public string MethodValueText3 { get; set; }
            public string MethodValueText4 { get; set; }
        }

        private class EvaluatedMethodTextData
        {
            private string text;
            private SolidColorBrush red;

            public EvaluatedMethodTextData(string evaluatedMethodText, SolidColorBrush textColor)
            {
                EvaluatedMethodText = evaluatedMethodText;
                TextColor = textColor;
            }
            public string EvaluatedMethodText { get; set; }
            public SolidColorBrush TextColor { get; set; }

        }

        private class EvaluatedMethodStrokeData
        {
            public EvaluatedMethodStrokeData(StrokeCollection evaluatedMethodStroke)
            {
                EvaluatedMethodStroke = evaluatedMethodStroke;
            }

            public StrokeCollection EvaluatedMethodStroke { get; set; }
        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem listBoxItem = sender as ListBoxItem;

            switch (listBoxItem.Name)
            {
                case "PenButton":
                    if (this.InputCanvasGrid1.IsVisible)
                    {
                        this.InputCanvas1.EditingMode = InkCanvasEditingMode.Ink;
                    }
                    if (this.InputCanvasGrid2.IsVisible)
                    {
                        this.InputCanvas2_1.EditingMode = InkCanvasEditingMode.Ink;
                        this.InputCanvas2_2.EditingMode = InkCanvasEditingMode.Ink;
                        this.InputCanvas2_3.EditingMode = InkCanvasEditingMode.Ink;
                    }
                    if (this.InputCanvasGrid3.IsVisible)
                    {
                        this.InputCanvas3_1.EditingMode = InkCanvasEditingMode.Ink;
                        this.InputCanvas3_2.EditingMode = InkCanvasEditingMode.Ink;
                        this.InputCanvas3_3.EditingMode = InkCanvasEditingMode.Ink;
                        this.InputCanvas3_4.EditingMode = InkCanvasEditingMode.Ink;
                        this.InputCanvas3_5.EditingMode = InkCanvasEditingMode.Ink;
                        this.InputCanvas3_6.EditingMode = InkCanvasEditingMode.Ink;
                    }

                    break;

                case "EraserButton":
                    if (this.InputCanvasGrid1.IsVisible)
                    {
                        this.InputCanvas1.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    }
                    if (this.InputCanvasGrid2.IsVisible)
                    {
                        this.InputCanvas2_1.EditingMode = InkCanvasEditingMode.EraseByPoint;
                        this.InputCanvas2_2.EditingMode = InkCanvasEditingMode.EraseByPoint;
                        this.InputCanvas2_3.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    }
                    if (this.InputCanvasGrid3.IsVisible)
                    {
                        this.InputCanvas3_1.EditingMode = InkCanvasEditingMode.EraseByPoint;
                        this.InputCanvas3_2.EditingMode = InkCanvasEditingMode.EraseByPoint;
                        this.InputCanvas3_3.EditingMode = InkCanvasEditingMode.EraseByPoint;
                        this.InputCanvas3_4.EditingMode = InkCanvasEditingMode.EraseByPoint;
                        this.InputCanvas3_5.EditingMode = InkCanvasEditingMode.EraseByPoint;
                        this.InputCanvas3_6.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    }


                    break;

                case "AllClearButton":
                    if (this.InputCanvasGrid1.IsVisible)
                    {
                        this.InputCanvas1.Strokes.Clear();
                    }
                    if (this.InputCanvasGrid2.IsVisible)
                    {
                        this.InputCanvas2_1.Strokes.Clear();
                        this.InputCanvas2_2.Strokes.Clear();
                        this.InputCanvas2_3.Strokes.Clear();
                    }
                    if (this.InputCanvasGrid3.IsVisible)
                    {
                        this.InputCanvas3_1.Strokes.Clear();
                        this.InputCanvas3_2.Strokes.Clear();
                        this.InputCanvas3_3.Strokes.Clear();
                        this.InputCanvas3_4.Strokes.Clear();
                        this.InputCanvas3_5.Strokes.Clear();
                        this.InputCanvas3_6.Strokes.Clear();
                    }

                    break;
            }
        }

        //private void InkCanvas_Loaded(object sender, RoutedEventArgs e)
        //{
        //    var gestureCanvas = (InkCanvas)sender;

        //    gestureCanvas.SetEnabledGestures(new[]
        //    {
        //        System.Windows.Ink.ApplicationGesture.Circle,
        //        System.Windows.Ink.ApplicationGesture.DoubleCircle,
        //        System.Windows.Ink.ApplicationGesture.Triangle,

        //    });
        //}

        //private void InkCanvas_Gesture(object sender, InkCanvasGestureEventArgs e)
        //{
        //    try
        //    {

        //        var gestureCanvas = (InkCanvas)sender;
        //        if (RecognizingCanvasName != gestureCanvas.Name)
        //        {
        //            RecognizingCanvasName = gestureCanvas.Name;

        //            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        //            timer.Start();
        //            timer.Tick += (s, args) =>
        //            {
        //                // タイマーの停止
        //                timer.Stop();

        //                // 信頼性 (RecognitionConfidence) を無視したほうが、Circle と Triangle の認識率は上がるようです。
        //                var gestureResult = e.GetGestureRecognitionResults().FirstOrDefault(r => r.ApplicationGesture != System.Windows.Ink.ApplicationGesture.NoGesture);

        //                if (gestureResult != null)
        //                {
        //                    switch (gestureResult.ApplicationGesture)
        //                    {
        //                        case System.Windows.Ink.ApplicationGesture.Circle:
        //                        case System.Windows.Ink.ApplicationGesture.DoubleCircle:
        //                            break;
        //                        case System.Windows.Ink.ApplicationGesture.Triangle:
        //                            break;
        //                        default:
        //                            throw new InvalidOperationException();
        //                    }

        //                    if (gestureResult.ApplicationGesture == System.Windows.Ink.ApplicationGesture.Circle || gestureResult.ApplicationGesture == System.Windows.Ink.ApplicationGesture.DoubleCircle)
        //                    {
        //                        foreach (var textBlock in ((Grid)gestureCanvas.Parent).Children)
        //                        {
        //                            if (textBlock is TextBlock)
        //                            {
        //                                ((TextBlock)textBlock).Text = "〇";
        //                                gestureCanvas.Strokes.Clear();
        //                                if (this.scene == "グループアクティビティ")
        //                                {
        //                                    this.WriteMethodValueData((TextBlock)textBlock, this.EvaluateAosukeMethodListView);
        //                                }
        //                                else if (this.scene == "チャレンジタイム！　パート②")
        //                                {
        //                                    this.WriteMethodValueData((TextBlock)textBlock, this.EvaluateAkamaruMethodListView);
        //                                }
        //                            }
        //                        }
        //                    }
        //                    else if (gestureResult.ApplicationGesture == System.Windows.Ink.ApplicationGesture.Triangle)
        //                    {
        //                        foreach (var textBlock in ((Grid)gestureCanvas.Parent).Children)
        //                        {
        //                            if (textBlock is TextBlock)
        //                            {
        //                                ((TextBlock)textBlock).Text = "△";
        //                                gestureCanvas.Strokes.Clear();

        //                                if (this.EvaluateAosukeMethodListView.IsVisible)
        //                                {
        //                                    this.WriteMethodValueData((TextBlock)textBlock, this.EvaluateAosukeMethodListView);
        //                                }
        //                                else if (this.EvaluateAkamaruMethodListView.IsVisible)
        //                                {
        //                                    this.WriteMethodValueData((TextBlock)textBlock, this.EvaluateAkamaruMethodListView);
        //                                }
        //                            }
        //                        }
        //                    }

        //                    RecognizingCanvasName = "";
        //                }
        //                else
        //                {
        //                    this.CrossMarkRecognize(gestureCanvas);
        //                    gestureCanvas.Strokes.Clear();


        //                }
        //            };
        //        }
        //    }
        //    catch (Exception e1)
        //    {
        //        MessageBox.Show(e1.ToString());
        //    }
        //}

        //private void CrossMarkRecognize(InkCanvas inkCanvas)
        //{
        //    try
        //    {
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            inkCanvas.Strokes.Save(ms);
        //            var myInkCollector = new InkCollector();
        //            var ink = new Ink();
        //            ink.Load(ms.ToArray());

        //            using (RecognizerContext context = new RecognizerContext())
        //            {
        //                if (ink.Strokes.Count > 0)
        //                {
        //                    context.Strokes = ink.Strokes;
        //                    RecognitionStatus status;

        //                    var result = context.Recognize(out status);

        //                    var aaa = result.GetAlternatesFromSelection();

        //                    foreach (var tet in aaa)
        //                    {
        //                        if (tet.ToString() == "×" || tet.ToString() == "x" || tet.ToString() == "×")
        //                        {
        //                            if (status == RecognitionStatus.NoError)
        //                            {
        //                                foreach (var textBlock in ((Grid)inkCanvas.Parent).Children)
        //                                {
        //                                    if (textBlock is TextBlock)
        //                                    {
        //                                        ((TextBlock)textBlock).Text = "✕";

        //                                        if (this.scene == "グループアクティビティ")
        //                                        {
        //                                            this.WriteMethodValueData((TextBlock)textBlock, this.EvaluateAosukeMethodListView);
        //                                        }
        //                                        else if (this.scene == "チャレンジタイム！　パート②")
        //                                        {
        //                                            this.WriteMethodValueData((TextBlock)textBlock, this.EvaluateAkamaruMethodListView);
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e2)
        //    {
        //        MessageBox.Show(e2.ToString());
        //    }
            

        //    RecognizingCanvasName = "";
        //}

        private void WriteMethodValueData(TextBlock textBlock, ListView listView)
        {
            int number = int.Parse(textBlock.Name.Substring(textBlock.Name.Length - 1));
            if (listView.Name == "EvaluateAosukeMethodListView")
            {
                if (this.dataOption.InputMethod == 0)
                {
                    this.AOSUKE_METHOD_STROKE_VALUES[(StrokeCollection)this._methodstrokedata.First().MethodStroke][number - 1] = textBlock.Text;
                }
                else
                {
                    this.AOSUKE_METHOD_TEXT_VALUES[(string)this._methodtextdata.First().MethodText][number-1] = textBlock.Text;
                }
                this.CheckFillInTheBlanks("青助");
            }
            else if(listView.Name == "EvaluateAkamaruMethodListView")
            {
                this.AKAMARU_METHOD_TEXT_VALUES[(string)textBlock.DataContext.ToString()][number - 1] = textBlock.Text;
                this.CheckFillInTheBlanks("赤丸");
            }

        }

        private  void CountNumberOfCorrect(string methodText,StrokeCollection methodStrokes)
        {
            int countnumber1 = 0;
            bool isMaximize = true;
            if (this.dataOption.InputMethod == 0)
            {
                foreach(string text1 in this.AOSUKE_METHOD_STROKE_VALUES[methodStrokes])
                {
                    if(text1 == "〇")
                    {
                        countnumber1++;
                    }
                }
                this.AOSUKE_METHOD_STROKE_VALUES[methodStrokes][4] = countnumber1.ToString();

                foreach (string[] text2 in this.AOSUKE_METHOD_STROKE_VALUES.Values)
                {
                    if (text2[4] != "")
                    {
                        int countnumber2 = int.Parse(text2[4]);
                        if (countnumber2 > countnumber1)
                        {
                            isMaximize = false;
                            break;
                        }
                    }
                }
                if (isMaximize)
                {
                    this.AOSUKE_METHOD_STROKE_VALUES[methodStrokes][5] = "BestMethod";
                }
                else
                {
                    this.AOSUKE_METHOD_STROKE_VALUES[methodStrokes][5] = "NotBestMethod";
                }

            }
            else
            {
                foreach (string text1 in this.AOSUKE_METHOD_TEXT_VALUES[methodText])
                {
                    if (text1 == "〇")
                    {
                        countnumber1++;
                    }
                }
                this.AOSUKE_METHOD_TEXT_VALUES[methodText][4] = countnumber1.ToString();

                foreach (string[] text2 in this.AOSUKE_METHOD_TEXT_VALUES.Values)
                {
                    if (text2[4] != "")
                    {
                        int countnumber2 = int.Parse(text2[4]);
                        if (countnumber2 > countnumber1)
                        {
                            isMaximize = false;
                            break;
                        }
                    }
                }
                if (isMaximize)
                {
                    this.AOSUKE_METHOD_TEXT_VALUES[methodText][5] = "BestMethod";
                }
                else
                {
                    this.AOSUKE_METHOD_TEXT_VALUES[methodText][5] = "NotBestMethod";
                }
            }
        }

        private void IsVisibileMethodButton()
        {
            if (this.dataOption.InputMethod == 0)
            {
                

                if ((StrokeCollection)this._methodstrokedata.First().MethodStroke == this.AOSUKE_METHOD_STROKE_VALUES.Reverse().FirstOrDefault().Key && this.AOSUKE_METHOD_STROKE_VALUES.Count > 1)
                {
                    this.NextMethodButton.Visibility = Visibility.Hidden;
                    this.BackMethodButton.Visibility = Visibility.Visible;
                }
                else if((StrokeCollection) this._methodstrokedata.First().MethodStroke == this.AOSUKE_METHOD_STROKE_VALUES.FirstOrDefault().Key &&this.AOSUKE_METHOD_STROKE_VALUES.Count > 1)
                {
                    this.NextMethodButton.Visibility = Visibility.Visible;
                    this.BackMethodButton.Visibility = Visibility.Hidden;
                }
                else if (this.AOSUKE_METHOD_STROKE_VALUES.Count <= 1)
                {
                    this.NextMethodButton.Visibility = Visibility.Hidden;
                    this.BackMethodButton.Visibility = Visibility.Hidden;
                }
                else
                {
                    this.NextMethodButton.Visibility = Visibility.Visible;
                    this.BackMethodButton.Visibility = Visibility.Visible;
                }
            }
            else
            {
                
                if ((string)this._methodtextdata.First().MethodText == this.AOSUKE_METHOD_TEXT_VALUES.Reverse().FirstOrDefault().Key && this.AOSUKE_METHOD_TEXT_VALUES.Count > 1)
                {
                    this.NextMethodButton.Visibility = Visibility.Hidden;
                    this.BackMethodButton.Visibility = Visibility.Visible;
                }
                else if ((string)this._methodtextdata.First().MethodText == this.AOSUKE_METHOD_TEXT_VALUES.FirstOrDefault().Key && this.AOSUKE_METHOD_TEXT_VALUES.Count > 1)
                {
                    this.NextMethodButton.Visibility = Visibility.Visible;
                    this.BackMethodButton.Visibility = Visibility.Hidden;
                }
                else if (this.AOSUKE_METHOD_TEXT_VALUES.Count <= 1)
                {
                    this.NextMethodButton.Visibility = Visibility.Hidden;
                    this.BackMethodButton.Visibility = Visibility.Hidden;
                }
                else
                {
                    this.NextMethodButton.Visibility = Visibility.Visible;
                    this.BackMethodButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void MovetoNextOrPreviousMethod(string Direction)
        {
            bool checkflag = false;
            if(this.dataOption.InputMethod == 0)
            {
                this.CountNumberOfCorrect(null, (StrokeCollection)this._methodstrokedata.First().MethodStroke);
                
                if(Direction == "next")
                {
                    foreach (StrokeCollection stroke in this.AOSUKE_METHOD_STROKE_VALUES.Keys)
                    {
                        if (checkflag)
                        {
                            this._methodstrokedata = new ObservableCollection<MethodStrokeValueData>()
                            {
                                new MethodStrokeValueData(stroke,this.AOSUKE_METHOD_STROKE_VALUES[stroke])
                            };
                            this.EvaluateAosukeMethodListView.ItemsSource = _methodstrokedata;
                            break;
                        }
                        if ((StrokeCollection)this._methodstrokedata.First().MethodStroke == stroke)
                        {
                            checkflag = true;
                        }
                    }
                }
                else if (Direction == "prev")
                {
                    foreach (StrokeCollection stroke in this.AOSUKE_METHOD_STROKE_VALUES.Keys.Reverse())
                    {
                        if (checkflag)
                        {
                            this._methodstrokedata = new ObservableCollection<MethodStrokeValueData>()
                            {
                                new MethodStrokeValueData(stroke,this.AOSUKE_METHOD_STROKE_VALUES[stroke])
                            };
                            this.EvaluateAosukeMethodListView.ItemsSource = _methodstrokedata;
                            break;
                        }
                        if ((StrokeCollection)this._methodstrokedata.First().MethodStroke == stroke)
                        {
                            checkflag = true;
                        }
                    }
                }
                
            }
            else
            {
                this.CountNumberOfCorrect((string)this._methodtextdata.First().MethodText, null);

                if (Direction =="next")
                {
                    foreach (string text in this.AOSUKE_METHOD_TEXT_VALUES.Keys)
                    {
                        if (checkflag)
                        {
                            this._methodtextdata = new ObservableCollection<MethodTextValueData>()
                        {
                            new MethodTextValueData(text,this.AOSUKE_METHOD_TEXT_VALUES[text])
                        };
                            this.EvaluateAosukeMethodListView.ItemsSource = _methodtextdata;
                            break;
                        }
                        if ((string)this._methodtextdata.First().MethodText == text)
                        {
                            checkflag = true;
                        }
                    }
                }
                else if(Direction == "prev")
                {
                    foreach (string text in this.AOSUKE_METHOD_TEXT_VALUES.Keys.Reverse())
                    {
                        if (checkflag)
                        {
                            this._methodtextdata = new ObservableCollection<MethodTextValueData>()
                        {
                            new MethodTextValueData(text,this.AOSUKE_METHOD_TEXT_VALUES[text])
                        };
                            this.EvaluateAosukeMethodListView.ItemsSource = _methodtextdata;
                            break;
                        }
                        if ((string)this._methodtextdata.First().MethodText == text)
                        {
                            checkflag = true;
                        }
                    }
                }
            }
        }

        private void CheckFillInTheBlanks(string characterName)
        {
            bool checkFlag = true;
            if (characterName == "赤丸")
            {
                foreach (string[] texts in this.AKAMARU_METHOD_TEXT_VALUES.Values)
                {
                    foreach (string text in texts)
                    {
                        if (text == "")
                        {
                            checkFlag = false;
                            break;
                        }
                    }
                }
                if (checkFlag)
                {
                    this.NextPageButton.Visibility = Visibility.Visible;
                    this.BackPageButton.Visibility = Visibility.Visible;
                }
                else
                {
                    this.NextPageButton.Visibility = Visibility.Hidden;
                    this.BackPageButton.Visibility = Visibility.Visible;
                }
            }
            else if(characterName =="青助")
            {
                if (this.dataOption.InputMethod == 0)
                {
                    foreach (string[] texts in this.AOSUKE_METHOD_STROKE_VALUES.Values)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (texts[i] == "")
                            {
                                checkFlag = false;
                                break;
                            }
                        }
                    }
                    if (checkFlag)
                    {
                        this.NextPageButton.Visibility = Visibility.Visible;
                        this.BackPageButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this.NextPageButton.Visibility = Visibility.Hidden;
                        this.BackPageButton.Visibility = Visibility.Visible;
                    }

                }
                else
                {
                    foreach (string[] texts in this.AOSUKE_METHOD_TEXT_VALUES.Values)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (texts[i] == "")
                            {
                                checkFlag = false;
                                break;
                            }
                        }
                    }
                    if (checkFlag)
                    {
                        this.NextPageButton.Visibility = Visibility.Visible;
                        this.BackPageButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        this.NextPageButton.Visibility = Visibility.Hidden;
                        this.BackPageButton.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void InitializedAosukeMethodListView()
        {
            if (this.dataOption.InputMethod == 0)
            {
                this.AOSUKE_METHOD_STROKE_VALUES.Clear();
                foreach (StrokeCollection stroke in this.Step2AosukeMethodInputStrokes)
                {
                    if (stroke.Count > 0 && !this.AOSUKE_METHOD_STROKE_VALUES.ContainsKey(stroke))
                    {
                        this.AOSUKE_METHOD_STROKE_VALUES.Add(stroke, new string[] { "", "", "", "", "", "NotBestMethod" });
                    }
                }

                this._methodstrokedata = new ObservableCollection<MethodStrokeValueData>()
                {
                    new MethodStrokeValueData(this.AOSUKE_METHOD_STROKE_VALUES.FirstOrDefault().Key,this.AOSUKE_METHOD_STROKE_VALUES.FirstOrDefault().Value)
                };
                this.EvaluateAosukeMethodListView.ItemsSource = _methodstrokedata;

            }
            else
            {
                this.AOSUKE_METHOD_TEXT_VALUES.Clear();
                foreach (string text in this.Step2AosukeMethodInputText)
                {
                    if (text != "" && !this.AOSUKE_METHOD_TEXT_VALUES.ContainsKey(text))
                    {
                        this.AOSUKE_METHOD_TEXT_VALUES.Add(text, new string[] { "", "", "", "", "", "NotBestMethod" });
                    }
                }

                this._methodtextdata = new ObservableCollection<MethodTextValueData>()
                {
                    new MethodTextValueData(this.AOSUKE_METHOD_TEXT_VALUES.FirstOrDefault().Key,this.AOSUKE_METHOD_TEXT_VALUES.FirstOrDefault().Value)
                };
                this.EvaluateAosukeMethodListView.ItemsSource = _methodtextdata;
            }
        }
    }
}