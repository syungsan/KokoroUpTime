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

        private CheckBox[] checkBoxs;

        // メディアプレーヤークラスのインスタンスを作成する
        WindowsMediaPlayer mediaPlayer = new WindowsMediaPlayer();

        string startupPath;

        public GamePage()
        {
            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            this.InitControls();

            this.CoverLayer.Visibility = Visibility.Hidden;
            this.ExitGrid.Visibility = Visibility.Hidden;

            string exePath = Environment.GetCommandLineArgs()[0];
            string exeFullPath = System.IO.Path.GetFullPath(exePath);
            this.startupPath = System.IO.Path.GetDirectoryName(exeFullPath);
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
            this.CharaDownRight.Visibility = Visibility.Hidden;
            this.CharaSmallLeftA.Visibility = Visibility.Hidden;
            this.CharaSmallLeftB.Visibility = Visibility.Hidden;
            this.CharaSmallLeftC.Visibility = Visibility.Hidden;
            this.BigInfo.Visibility = Visibility.Hidden;
            this.CharaUpRight.Visibility = Visibility.Hidden;            
            this.MainMsgBubble.Visibility = Visibility.Hidden;
            this.MainMsg.Visibility = Visibility.Hidden;
            this.SessionTitle.Visibility = Visibility.Hidden;
            this.SessionFrame.Visibility = Visibility.Hidden;
            this.SessionSubTitle.Visibility = Visibility.Hidden;
            this.SessionSentence.Visibility = Visibility.Hidden;
            this.CharaSmallLeft.Visibility = Visibility.Hidden;

            this.NextMsgButton.Visibility = Visibility.Hidden;
            this.BackMsgButton.Visibility = Visibility.Hidden;
 
            this.BoardButton.Visibility = Visibility.Hidden;
            this.RuleCheck1Box.Visibility = Visibility.Hidden;
            this.RuleCheck2Box.Visibility = Visibility.Hidden;
            this.RuleCheck3Box.Visibility = Visibility.Hidden;
            this.CharaBottomRight.Visibility = Visibility.Hidden;
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
            this.CharaSmallUpRight.Visibility = Visibility.Hidden;
            this.ItemInfoTitle.Visibility = Visibility.Hidden;
            this.ItemInfoSentence.Visibility = Visibility.Hidden;

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
                ["chara_down_right"] = this.CharaDownRight,
                ["chara_small_left_a"] = this.CharaSmallLeftA,
                ["chara_small_left_b"] = this.CharaSmallLeftB,
                ["chara_small_left_c"] = this.CharaSmallLeftC,
                ["chara_small_left"] = this.CharaSmallLeft,
                ["main_msg_bubble"] = this.MainMsgBubble,
                ["big_info"] = this.BigInfo,
                ["chara_up_right"] = this.CharaUpRight,
                ["chara_bottom_right"] = this.CharaBottomRight,
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
                ["chara_small_up_right"] = this.CharaSmallUpRight,
                ["long_msg_image"] = this.LongMsgImage,
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

                    if (this.scenarios[this.scenarioCount].Count > 3 && this.scenarios[this.scenarioCount][3] != "")
                    {
                        var imageStoryBoard = this.scenarios[this.scenarioCount][3];

                        imageStoryBoard += $"_{this.position}";

                        this.ShowAnime(storyBoard: imageStoryBoard);
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
                    if (this.scenarios[this.scenarioCount].Count > 3 && this.scenarios[this.scenarioCount][3] != "")
                    {
                        var buttonImageFile = this.scenarios[this.scenarioCount][3];

                        buttonImageObject.Source = new BitmapImage(new Uri($"Images/{buttonImageFile}", UriKind.Relative));
                    }
                    buttonObject.Visibility = Visibility.Visible;

                    if (this.scenarios[this.scenarioCount].Count > 4 && this.scenarios[this.scenarioCount][4] != "")
                    {
                        var buttonStoryBoard = this.scenarios[this.scenarioCount][4];

                        buttonStoryBoard += $"_{this.position}";

                        this.ShowAnime(storyBoard: buttonStoryBoard);
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

                    textObject.Visibility = Visibility.Visible;

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

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
                            this.textObjects[this.position].Text = "";

                            this.scenarioCount += 1;
                            this.ScenarioPlay();

                            break;

                        case "button":

                            this.position = this.scenarios[this.scenarioCount][2];
                            this.buttonObjects[this.position].Visibility = Visibility.Hidden;

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

                case "sound":

                    var _soundFile = this.scenarios[this.scenarioCount][1];

                    bool _isLoop = false;

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var loopStr = this.scenarios[this.scenarioCount][2];

                        if (loopStr == "loop")
                        {
                            _isLoop = true;
                        }
                    }
                    this.SoundPlay(soundFile: _soundFile, isLoop: _isLoop);

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

        private void ShowAnime(string storyBoard)
        {
            Storyboard sb = this.FindResource(storyBoard) as Storyboard;

            if (sb != null)
            {
                // 二重終了防止策
                bool isDuplicate = false;

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

            if (this.isClickable)
            {
                this.isClickable = false;

                if (button.Name.Contains("Back"))
                {
                    this.scenarioCount -= 1;
                    this.ScenarioPlay();
                    // 連続Backの実現にはもっと複雑な処理がいる
                }
                else if (button.Name == "NextMsgButton" || button.Name == "NextPageButton" || button.Name == "BoardButton" || button.Name == "LongMsgBubble" || button.Name == "MangaNextButton")
                {
                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }
            }

            if (button.Name == "ExitButton")
            {
                this.CoverLayer.Visibility = Visibility.Visible;
                this.ExitGrid.Visibility = Visibility.Visible;
            }
            else if (button.Name == "ExitYesButton")
            {
                Application.Current.Shutdown();
            }
            else if (button.Name == "ExitNoButton")
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

        private void SoundPlay(string soundFile, bool isLoop=false)
        {
            // ループ再生を指定
            // mediaPlayer.settings.setMode("loop", isLoop);

            Debug.Print(soundFile);

            // 通常は自動再生にファイルを指定すればループ再生がはじまる
            mediaPlayer.URL = $@"{this.startupPath}/Sounds/{soundFile}";

            mediaPlayer.controls.play(); // 再生
            
            // mediaPlayer.controls.stop(); // 停止(再生中停止すればplay()で頭から再生)
            // mediaPlayer.controls.pause();// ポーズ(play()で再開)
            // mediaPlayer.settings.volume = 10; // 0から100
        }
    }
}
