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
using System.Security.Cryptography.X509Certificates;

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
        private string scene = "";

        private bool isClickable = false;
        private int tapCount = 0;

        public GamePage()
        {
            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            this.scenarios = this.LoadScenario("Scenarios/chapter1.csv");
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
            var tag = this.scenarios[this.scenarioCount][0];

            // メッセージ表示関連
            DispatcherTimer msgTimer;
            string message = "";
            int word_num = 0;

            // 各シーンの初期化はここで
            if (tag == "scene")
            {
                this.HiddeAllScene();

                var _scene = this.scenarios[this.scenarioCount][1];
                this.scene = _scene;

                switch (this.scene)
                {
                    case "main":

                        this.MainGrid.Visibility = Visibility.Visible;
                        this.BigSpeech.Text = "";

                        this.MainCharaRight.Visibility = Visibility.Hidden;
                        this.MainCharaLeftA.Visibility = Visibility.Hidden;
                        this.MainCharaLeftB.Visibility = Visibility.Hidden;
                        this.MainCharaLeftC.Visibility = Visibility.Hidden;

                        break;

                    case "board":

                        this.BoardGrid.Visibility = Visibility.Visible;

                        this.BoardTitle.Text = "";

                        this.BoardChecK1Msg.Text = "";
                        this.BoardChecK2Msg.Text = "";
                        this.BoardChecK3Msg.Text = "";

                        this.BoardCheck1Box.Visibility = Visibility.Hidden;
                        this.BoardCheck2Box.Visibility = Visibility.Hidden;
                        this.BoardCheck3Box.Visibility = Visibility.Hidden;

                        this.BoardCheck1Box.IsEnabled = false;
                        this.BoardCheck2Box.IsEnabled = false;
                        this.BoardCheck3Box.IsEnabled = false;

                        this.BoardCharacter.Visibility = Visibility.Hidden;
                        this.LongBubble.Visibility = Visibility.Hidden;
                        this.LongSpeech.Text = "";

                        this.AfterButton.Visibility = Visibility.Hidden;

                        break;
                }
                this.scenarioCount += 1;
                this.ScenarioPlay();
            }

            // Commonメインシーン
            if (this.scene == "main")
            {
                switch (tag)
                {
                    case "bg":

                        // 後々背景もクロスフェードなどの処理を入れる
                        var fileName = this.scenarios[this.scenarioCount][1];
                        this.BG.Source = new BitmapImage(new Uri($"Images/{fileName}", UriKind.Relative));

                        this.scenarioCount += 1;
                        this.ScenarioPlay();

                        break;

                    case "chara":

                        var _fileName = this.scenarios[this.scenarioCount][1];
                        this.position = this.scenarios[this.scenarioCount][2];

                        string _storyBoardName = "";

                        var animeName = this.scenarios[this.scenarioCount][3];
                        if (animeName != "")
                        {
                            _storyBoardName = animeName + "_main_" + this.position;
                        }
                        SetCharactor(fileName: _fileName, image: this.MainCharaRight, storyBoardName: _storyBoardName);

                        break;

                    case "msg":

                        this.BigBubble.Visibility = Visibility.Visible;
                        this.BigSpeech.Visibility = Visibility.Visible;

                        this.BigSpeech.Text = "";

                        this.NextMessageButton.Visibility = Visibility.Hidden;
                        this.BackMessageButton.Visibility = Visibility.Hidden;

                        this.position = this.scenarios[this.scenarioCount][2];

                        SetMessage(textBlock: this.BigSpeech);

                        break;

                    case "wait":

                        this.NextMessageButton.Visibility = Visibility.Visible;

                        if (this.scenarios[this.scenarioCount - 1] != null)
                        {
                            this.BackMessageButton.Visibility = Visibility.Visible;
                        }
                        this.isClickable = true;

                        break;

                    case "hide":

                        // オブジェクトを消すときは後々ほとんどアニメで処理するようにする
                        var hideObj = this.scenarios[this.scenarioCount][1];

                        switch (hideObj)
                        {
                            case "msg":

                                this.HideMessage();

                                this.scenarioCount += 1;
                                this.ScenarioPlay();

                                break;
                        }
                        break;

                    case "visible":

                        // オブジェクトを現すときは後々ほとんどアニメで処理するようにする
                        var visbleObj = this.scenarios[this.scenarioCount][1];
                        
                        switch (visbleObj)
                        {
                            case "msg":

                                this.VisibleMessage();

                                this.scenarioCount += 1;
                                this.ScenarioPlay();

                                break;
                        }
                        break;
                }
            }

            // 黒板チェックシーン
            if (this.scene == "board")
            {
                switch (tag)
                {
                    case "title":

                        SetMessage(textBlock: this.BoardTitle);
                        break;

                    case "check1msg":

                        SetMessage(textBlock: this.BoardChecK1Msg, checkBox: this.BoardCheck1Box);
                        break;

                    case "check2msg":

                        SetMessage(textBlock: this.BoardChecK2Msg, checkBox: this.BoardCheck2Box);
                        break;

                    case "check3msg":

                        SetMessage(textBlock: this.BoardChecK3Msg, checkBox: this.BoardCheck3Box);
                        break;

                    case "wait":

                        this.isClickable = true;
                        break;

                    case "chara":;

                        this.BoardCharacter.Visibility = Visibility.Visible;
                        this.LongBubble.Visibility = Visibility.Visible;

                        var _fileName = this.scenarios[this.scenarioCount][1];

                        string _storyBoardName = "";

                        var animeName = this.scenarios[this.scenarioCount][2];
                        if (animeName != "")
                        {
                            _storyBoardName = animeName + "_board";
                        }
                        SetCharactor(fileName: _fileName, image: this.MainCharaRight, storyBoardName: _storyBoardName);

                        break;

                    case "suggest":

                        CheckBox[] _checkBoxs = new CheckBox[] { this.BoardCheck1Box, this.BoardCheck2Box, this.BoardCheck3Box };
                        SetMessage(textBlock: this.LongSpeech, checkBoxes: _checkBoxs);
                        break;

                    case "tap":

                        this.isClickable = false;
                        break;
                }
            }
            void SetMessage(TextBlock textBlock, CheckBox checkBox = null, CheckBox[] checkBoxes = null)
            {
                var _message = this.scenarios[this.scenarioCount][1];

                // 苦悶の改行処理（文章中の「鬱」を疑似改行コードとする）
                message = _message.Replace("鬱", "\u2028");

                // メッセージ表示処理
                msgTimer = new DispatcherTimer();
                msgTimer.Tick += ViewMsg;
                msgTimer.Interval = TimeSpan.FromSeconds(1.0f / MESSAGE_SPEED);
                msgTimer.Start();

                // 一文字ずつメッセージ表示（Inner Func）
                void ViewMsg(object sender, EventArgs e)
                {
                    textBlock.Text = message.Substring(0, word_num);

                    if (word_num < message.Length)
                    {
                        word_num++;
                    }
                    else
                    {
                        msgTimer.Stop();
                        msgTimer = null;

                        if (checkBox != null)
                        {
                            checkBox.Visibility = Visibility.Visible;
                        }
                        if (checkBoxes != null)
                        {
                            foreach (CheckBox checkBox in checkBoxes)
                            {
                                checkBox.IsEnabled = true;
                            }
                        }
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
                }
            }

            void SetCharactor(string fileName, Image image, string storyBoardName="")
            {
                image.Source = new BitmapImage(new Uri($"Images/{fileName}", UriKind.Relative));

                if (storyBoardName != "")
                {
                    Storyboard sb = this.FindResource(storyBoardName) as Storyboard;
                    if (sb != null)
                    {
                        sb.Completed += (s, e) =>
                        {

                            this.scenarioCount += 1;
                            this.ScenarioPlay();
                        };
                        sb.Begin(this);
                    }
                }
                else
                {
                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }
            }

        }

        private void HideMessage()
        {
            this.BigBubble.Visibility = Visibility.Hidden;
            this.BigSpeech.Visibility = Visibility.Hidden;
            this.NextMessageButton.Visibility = Visibility.Hidden;
            this.BackMessageButton.Visibility = Visibility.Hidden;
        }

        private void VisibleMessage()
        {
            this.BigBubble.Visibility = Visibility.Visible;
            this.BigSpeech.Visibility = Visibility.Visible;
            this.NextMessageButton.Visibility = Visibility.Visible;
            this.BackMessageButton.Visibility = Visibility.Visible;
        }

        // 後々以下のコールバック関数は一つにまとめる
        private void BoardButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.isClickable)
            {
                this.isClickable = false;

                this.scenarioCount += 1;
                this.ScenarioPlay();
            }
        }

        private void NextMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.isClickable)
            {
                this.isClickable = false;

                this.scenarioCount += 1;
                this.ScenarioPlay();
            }
        }

        private void BackMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.isClickable)
            {
                this.isClickable = false;

                this.scenarioCount -= 1;
                this.ScenarioPlay();
            }
            // 連続Backの実現にはもっと複雑な処理がいる
        }

        private void BoardCheck1Box_Checked(object sender, RoutedEventArgs e)
        {
            if (this.BoardCheck1Box.IsChecked == true)
            {
                this.tapCount += 1;
                this.checkAllBox();
            }
            else
            {
                this.tapCount -= 1;
            }
        }

        private void BoardCheck2Box_Checked(object sender, RoutedEventArgs e)
        {
            if (this.BoardCheck2Box.IsChecked == true)
            {
                this.tapCount += 1;
                this.checkAllBox();
            }
            else
            {
                this.tapCount -= 1;
            }
        }

        private void BoardCheck3Box_Checked(object sender, RoutedEventArgs e)
        {
            if (this.BoardCheck3Box.IsChecked == true)
            {
                this.tapCount += 1;
                this.checkAllBox();
            }
            else
            {
                this.tapCount -= 1;
            }
        }

        private void checkAllBox()
        {
            if (this.tapCount >= 3)
            {
                this.BoardCheck1Box.IsEnabled = false;
                this.BoardCheck2Box.IsEnabled = false;
                this.BoardCheck3Box.IsEnabled = false;

                this.AfterButton.Visibility = Visibility.Visible;
                this.isClickable = true;
            }
        }

        private void AfterButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.isClickable)
            {
                this.isClickable = false;

                this.scenarioCount += 1;
                this.ScenarioPlay();
            }
        }

        private void LongBubble_Click(object sender, RoutedEventArgs e)
        {
            if (this.isClickable)
            {
                this.isClickable = false;

                this.scenarioCount += 1;
                this.ScenarioPlay();
            }
        }

        private void HiddeAllScene()
        {
            this.MainGrid.Visibility = Visibility.Hidden;
            this.BoardGrid.Visibility = Visibility.Hidden;
        }

        // UTF-8からShift-JISへの変換にそなえて取り置き
        public static string ConvertEncoding(string src, System.Text.Encoding destEnc)
        {
            byte[] src_temp = System.Text.Encoding.ASCII.GetBytes(src);
            byte[] dest_temp = System.Text.Encoding.Convert(System.Text.Encoding.ASCII, destEnc, src_temp);
            string ret = destEnc.GetString(dest_temp);
            return ret;
        }
    }
}
