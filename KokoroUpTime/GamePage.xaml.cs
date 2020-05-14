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
    public partial class GamePage : Page
    {
        private float MESSAGE_SPEED = 30.0f;

        private int scenarioCount = 0;
        private List<List<string>> scenarios = null;

        private string position = "";

        private bool isClickable = false;
        private int tapCount = 0;

        // メッセージ表示関連
        private DispatcherTimer msgTimer;
        private int word_num;

        private Dictionary<string, Image> imageObjects = null;
        private Dictionary<string, TextBlock> textObjects = null;
        private Dictionary<string, Button> buttonObjects = null;
        private Dictionary<string, Grid> gridObjects = null;

        private CheckBox[] checkBoxs;

        private WindowsMediaPlayer mediaPlayer;

        private SoundPlayer sePlayer = null;

        private int feelingSize = 0;

        public GamePage()
        {
            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            this.InitControls();

            this.CoverLayer.Visibility = Visibility.Hidden;
            this.ExitGrid.Visibility = Visibility.Hidden;

            // メディアプレーヤークラスのインスタンスを作成する
            this.mediaPlayer = new WindowsMediaPlayer();

            this.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
            this.MouseUp += new MouseButtonEventHandler(OnMouseUp);
            this.MouseMove += new MouseEventHandler(OnMouseMove);
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

        private void ResetControls()
        {
            this.BG.Visibility = Visibility.Hidden;
            this.MainMsg.Visibility = Visibility.Hidden;
            this.CharaStandRight.Visibility = Visibility.Hidden;
            this.CharaStandLeft.Visibility = Visibility.Hidden;
            this.CharaFaceLeftA.Visibility = Visibility.Hidden;
            this.CharaFaceLeftB.Visibility = Visibility.Hidden;
            this.CharaFaceLeftC.Visibility = Visibility.Hidden;
            this.BigInfo.Visibility = Visibility.Hidden;        
            this.MainMsgBubble.Visibility = Visibility.Hidden;
            this.MainMsg.Visibility = Visibility.Hidden;
            this.SessionTitle.Visibility = Visibility.Hidden;
            this.SessionFrame.Visibility = Visibility.Hidden;
            this.SessionSubTitle.Visibility = Visibility.Hidden;
            this.SessionSentence.Visibility = Visibility.Hidden;
            this.CharaFaceLeft.Visibility = Visibility.Hidden;

            this.NextMsgButton.Visibility = Visibility.Hidden;
            this.BackMsgButton.Visibility = Visibility.Hidden;
 
            this.BoardButton.Visibility = Visibility.Hidden;
            this.RuleCheck1Box.Visibility = Visibility.Hidden;
            this.RuleCheck2Box.Visibility = Visibility.Hidden;
            this.RuleCheck3Box.Visibility = Visibility.Hidden;
            this.CharaStandSmallDownRight.Visibility = Visibility.Hidden;
            this.LongMsgBubble.Visibility = Visibility.Hidden;
            // this.LongMsgImage.Visibility = Visibility.Hidden;
            // this.LongMsg.Visibility = Visibility.Hidden;
            this.RuleTitle.Visibility = Visibility.Hidden;
            this.RuleChecK1Msg.Visibility = Visibility.Hidden;
            this.RuleChecK2Msg.Visibility = Visibility.Hidden;
            this.RuleChecK3Msg.Visibility = Visibility.Hidden;

            this.MangaTitle.Visibility = Visibility.Hidden;
            this.MangaImage.Visibility = Visibility.Hidden;
            this.MangaNextButton.Visibility = Visibility.Hidden;

            this.ItemImageCenter.Visibility = Visibility.Hidden;
            this.ItemNamePlateCenter.Visibility = Visibility.Hidden;
            this.ItemNameTextCenter.Visibility = Visibility.Hidden;
            this.ItemNameBubble.Visibility = Visibility.Hidden;
            this.ItemNumber.Visibility = Visibility.Hidden;
            this.ItemImageLeft.Visibility = Visibility.Hidden;
            this.ItemNamePlateLeft.Visibility = Visibility.Hidden;
            this.ItemNameTextLeft.Visibility = Visibility.Hidden;
            this.ItemInfoPlate.Visibility = Visibility.Hidden;
            this.CharaStandSmallUpRight.Visibility = Visibility.Hidden;
            this.ItemInfoTitle.Visibility = Visibility.Hidden;
            this.ItemInfoSentence.Visibility = Visibility.Hidden;

            this.MainMsgGrid.Visibility = Visibility.Hidden;

            this.GoodWordsGrid.Visibility = Visibility.Hidden;
            this.BadWordsGrid.Visibility = Visibility.Hidden;

            // this.FeelingGrid.Visibility = Visibility.Hidden;
            this.GaugeGrid.Visibility = Visibility.Hidden;

            this.CharaStandLeftComment.Visibility = Visibility.Hidden;
            this.CharaStandLeftSymbol.Visibility = Visibility.Hidden;

            this.CharaStandLeftSmall.Visibility = Visibility.Hidden;
            this.CharaStandRightSmall.Visibility = Visibility.Hidden;

            this.FeelingValueGrid.Visibility = Visibility.Hidden;

            this.MusicInfoGrid.Visibility = Visibility.Hidden;

            this.NextPageButton.Visibility = Visibility.Hidden;
            this.BackPageButton.Visibility = Visibility.Hidden;

            this.MainMsg.Text = "";

            this.RuleTitle.Text = "";
            this.RuleChecK1Msg.Text = "";
            this.RuleChecK2Msg.Text = "";
            this.RuleChecK3Msg.Text = "";
            this.RuleCheck1Box.IsEnabled = false;
            this.RuleCheck2Box.IsEnabled = false;
            this.RuleCheck3Box.IsEnabled = false;
            this.LongMsg.Text = "";

            this.ItemNameTextLeft.Text = "";
            this.ItemNameTextCenter.Text = "";
            this.ItemNumber.Text = "";
            this.ItemInfoTitle.Text = "";
            this.ItemInfoSentence.Text = "";
        }

        private void InitControls()
        {
            this.imageObjects = new Dictionary<string, Image>
            {
                ["bg"] = this.BG,
                ["chara_stand_right"] = this.CharaStandRight,
                ["chara_stand_left"] = this.CharaStandLeft,
                ["chara_face_left_a"] = this.CharaFaceLeftA,
                ["chara_face_left_b"] = this.CharaFaceLeftB,
                ["chara_face_left_c"] = this.CharaFaceLeftC,
                ["chara_face_left"] = this.CharaFaceLeft,
                ["main_msg_bubble"] = this.MainMsgBubble,
                ["big_info"] = this.BigInfo,
                ["chara_stand_small_right"] = this.CharaStandSmallDownRight,
                ["manga_title"] = this.MangaTitle,
                ["manga_image"] = this.MangaImage,
                ["session_frame"] = this.SessionFrame,
                ["session_title"] = this.SessionTitle,
                ["item_image_center"] = this.ItemImageCenter,
                ["item_image_left"] = this.ItemImageLeft,
                ["item_name_plate_center"] = this.ItemNamePlateCenter,
                ["item_name_bubble"] = this.ItemNameBubble,
                ["item_name_plate_left"] = this.ItemNamePlateLeft,
                ["item_info_plate"] = this.ItemInfoPlate,
                ["chara_stand_small_up_right"] = this.CharaStandSmallUpRight,
                ["long_msg_image"] = this.LongMsgImage,
                ["heart_image"] = this.HeartImage,
                ["needle_image"] = this.NeedleImage,
                ["chara_stand_left_symbol"] = this.CharaStandLeftSymbol,
                ["chara_stand_left_small"] = this.CharaStandLeftSmall,
                ["chara_stand_right_small"] = this.CharaStandRightSmall,
            };

            this.textObjects = new Dictionary<string, TextBlock>
            {
                ["main_msg"] = this.MainMsg,
                ["session_sub_title"] = this.SessionSubTitle,
                ["session_sentence"] = this.SessionSentence,
                ["rule_title"] = this.RuleTitle,
                ["rule_check1_msg"] = this.RuleChecK1Msg,
                ["rule_check2_msg"] = this.RuleChecK2Msg,
                ["rule_check3_msg"] = this.RuleChecK3Msg,
                ["long_msg"] = this.LongMsg,
                ["item_name_text_left"] = this.ItemNameTextLeft,
                ["item_name_text_center"] = this.ItemNameTextCenter,
                ["item_number"] = this.ItemNumber,
                ["item_info_title"] = this.ItemInfoTitle,
                ["item_info_sentence"] = this.ItemInfoSentence,
                ["music_title"] = this.MusicTitle,
                ["composer"] = this.Composer,
                ["chara_stand_left_comment"] = this.CharaStandLeftComment,
                ["feeling_person_text"] = this.FeelingPersonText,
                ["feeling_value_text"] = this.FeelingValueText,
            };

            this.buttonObjects = new Dictionary<string, Button>
            {
                ["next_msg_button"] = this.NextMsgButton,
                ["back_msg_button"] = this.BackMsgButton,
                ["next_page_button"] = this.NextPageButton,
                ["back_page_button"] = this.BackPageButton,
                ["manga_next_button"] = this.MangaNextButton,
                ["board_button"] = this.BoardButton,
                ["long_msg_bubble"] = this.LongMsgBubble,
            };

            this.gridObjects = new Dictionary<string, Grid>
            {
                ["main_msg_grid"] = this.MainMsgGrid,
                ["good_words_grid"] = this.GoodWordsGrid,
                ["bad_words_grid"] = this.BadWordsGrid,
                // ["feeling_grid"] = this.FeelingGrid,
                ["gauge_grid"] = this.GaugeGrid,
                ["music_info_grid"] = this.MusicInfoGrid,
                ["feeling_value_grid"] = this.FeelingValueGrid,
            };
        }

        // ゲーム進行の中核
        private void ScenarioPlay()
        {
            var tag = this.scenarios[this.scenarioCount][0];

            Debug.Print((this.scenarioCount + 1).ToString());

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

                    string gridIsSync = "sync";

                    if (this.scenarios[this.scenarioCount].Count > 3 && this.scenarios[this.scenarioCount][3] != "")
                    {
                        gridIsSync = this.scenarios[this.scenarioCount][3];
                    }

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var gridStoryBoard = this.scenarios[this.scenarioCount][2];

                        gridStoryBoard += $"_{this.position}";

                        this.ShowAnime(storyBoard: gridStoryBoard, isSync: gridIsSync);
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

                    string imageIsSync = "sync";

                    if (this.scenarios[this.scenarioCount].Count > 4 && this.scenarios[this.scenarioCount][4] != "")
                    {
                       imageIsSync = this.scenarios[this.scenarioCount][4];
                    }

                    if (this.scenarios[this.scenarioCount].Count > 3 && this.scenarios[this.scenarioCount][3] != "")
                    {
                        var imageStoryBoard = this.scenarios[this.scenarioCount][3];

                        imageStoryBoard += $"_{this.position}";

                        this.ShowAnime(storyBoard: imageStoryBoard, isSync: imageIsSync);
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

                    Image buttonImageObject = null;

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        this.position = this.scenarios[this.scenarioCount][2];

                        buttonImageObject = this.imageObjects[this.position];
                    }
                    // 後々これいらんImageタグでセットできる
                    if (this.scenarios[this.scenarioCount].Count > 3 && this.scenarios[this.scenarioCount][3] != "")
                    {
                        var buttonImageFile = this.scenarios[this.scenarioCount][3];

                        buttonImageObject.Source = new BitmapImage(new Uri($"Images/{buttonImageFile}", UriKind.Relative));
                    }
                    buttonObject.Visibility = Visibility.Visible;

                    string buttonIsSync = "sync";

                    if (this.scenarios[this.scenarioCount].Count > 5 && this.scenarios[this.scenarioCount][5] != "")
                    {
                        buttonIsSync = this.scenarios[this.scenarioCount][5];
                    }

                    if (this.scenarios[this.scenarioCount].Count > 4 && this.scenarios[this.scenarioCount][4] != "")
                    {
                        var buttonStoryBoard = this.scenarios[this.scenarioCount][4];

                        buttonStoryBoard += $"_{this.position}";

                        this.ShowAnime(storyBoard: buttonStoryBoard, isSync: buttonIsSync);
                    }
                    else
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                    break;

                case "msg":

                    this.NextMsgButton.Visibility = Visibility.Hidden;
                    this.BackMsgButton.Visibility = Visibility.Hidden;

                    this.position = this.scenarios[this.scenarioCount][1];

                    var _textObject = this.textObjects[this.position];

                    var _message = this.scenarios[this.scenarioCount][2];

                    _textObject.Visibility = Visibility.Visible;

                    this.ShowMessage(textObject: _textObject, message: _message);

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
                        this.NextMsgButton.Visibility = Visibility.Visible;
                        this.BackMsgButton.Visibility = Visibility.Visible;
                    }
                    this.isClickable = true;

                    break;

                case "next":

                    this.NextPageButton.Visibility = Visibility.Visible;
                    this.BackPageButton.Visibility = Visibility.Visible;

                    this.isClickable = true;

                    break;

                case "flip":

                    this.MangaNextButton.Visibility = Visibility.Visible;
                    this.isClickable = true;

                    break;

                case "text":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var textObject = this.textObjects[this.position];

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
                            this.textObjects[this.position].Visibility = Visibility.Hidden;
                            // this.textObjects[this.position].Text = "";

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
                            this.textObjects[this.position].Text = "";

                            this.scenarioCount += 1;
                            this.ScenarioPlay();

                            break;
                    }
                    break;

                case "rule":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var ruleObject = this.textObjects[this.position];

                    var rule = this.scenarios[this.scenarioCount][2];

                    this.checkBoxs = new CheckBox[] { this.RuleCheck1Box, this.RuleCheck2Box, this.RuleCheck3Box };

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
                    this.FeelingScaleText.Text = "50";

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

            if (this.isClickable && (button.Name == "NextMsgButton" || button.Name == "NextPageButton" || button.Name == "BoardButton" || button.Name == "LongMsgBubble" || button.Name == "MangaNextButton"))
            {
                this.isClickable = false;

                this.scenarioCount += 1;
                this.ScenarioPlay();
            }

            if (button.Name == "ExitButton")
            {
                this.CoverLayer.Visibility = Visibility.Visible;
                this.ExitGrid.Visibility = Visibility.Visible;
            }
            
            if (button.Name == "ExitYesButton")
            {
                Application.Current.Shutdown();
            }
            
            if (button.Name == "ExitNoButton")
            {
                this.ExitGrid.Visibility = Visibility.Hidden;
                this.CoverLayer.Visibility = Visibility.Hidden;
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

        private static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(GamePage), new UIPropertyMetadata(0.0));

        private double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this);

            this.CalcAngle();

            this.FeelingScaleText.Text = this.feelingSize.ToString();
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

                this.FeelingScaleText.Text = this.feelingSize.ToString();
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
