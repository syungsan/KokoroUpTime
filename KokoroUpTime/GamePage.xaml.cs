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

        // メッセージ表示関連
        private DispatcherTimer msgTimer;
        private int word_num;

        private Dictionary<string, Image> imageObjects = null;

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
            this.word_num = 0;

            // 各シーンの初期化はここで
            if (tag == "scene")
            {
                this.HiddeAllScene();

                var _scene = this.scenarios[this.scenarioCount][1];
                this.scene = _scene;

                switch (this.scene)
                {
                    case "main":

                        this.BigSpeech.Text = "";

                        this.MainCharaDownRight.Visibility = Visibility.Hidden;
                        this.MainCharaSmallLeftA.Visibility = Visibility.Hidden;
                        this.MainCharaSmallLeftB.Visibility = Visibility.Hidden;
                        this.MainCharaSmallLeftC.Visibility = Visibility.Hidden;
                        this.MainInfoCenter.Visibility = Visibility.Hidden;
                        this.MainCharaUpRight.Visibility = Visibility.Hidden;
                        this.MainNextPageButton.Visibility = Visibility.Hidden;
                        this.MainBackPageButton.Visibility = Visibility.Hidden;

                        imageObjects = new Dictionary<string, Image>
                        {
                            ["down_right"] = this.MainCharaDownRight,
                            ["small_left_a"] = this.MainCharaSmallLeftA,
                            ["small_left_b"] = this.MainCharaSmallLeftB,
                            ["small_left_c"] = this.MainCharaSmallLeftC,
                            ["info_center"] = this.MainInfoCenter,
                            ["up_right"] = this.MainCharaUpRight,
                        };

                        this.MainGrid.Visibility = Visibility.Visible;

                        break;

                    case "board":

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

                        this.BoardNextPageButton.Visibility = Visibility.Hidden;

                        this.BoardGrid.Visibility = Visibility.Visible;

                        break;

                    case "manga":

                        this.MangaTitle.Visibility = Visibility.Hidden;
                        this.MangaImage.Visibility = Visibility.Hidden;
                        this.MangaNextButton.Visibility = Visibility.Hidden;

                        this.MangaGrid.Visibility = Visibility.Visible;

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
                        var bgImage = this.scenarios[this.scenarioCount][1];
                        this.BG.Source = new BitmapImage(new Uri($"Images/{bgImage}", UriKind.Relative));

                        this.scenarioCount += 1;
                        this.ScenarioPlay();

                        break;

                    case "image":

                        this.BigSpeech.Text = "";

                        var _imageFile = this.scenarios[this.scenarioCount][1];

                        this.position = this.scenarios[this.scenarioCount][2];

                        var _imageObject = this.imageObjects[this.position];

                        _imageObject.Visibility = Visibility.Visible;

                        var _storyBoardName = this.scenarios[this.scenarioCount][3];
                        if (_storyBoardName != "")
                        {
                            _storyBoardName+= "_main_" + this.position;
                        }
                        this.ShowImage(imageFile: _imageFile, imageObject: _imageObject, storyBoardName: _storyBoardName);

                        break;

                    case "msg":

                        this.BigSpeech.Text = "";

                        this.NextMessageButton.Visibility = Visibility.Hidden;
                        this.BackMessageButton.Visibility = Visibility.Hidden;

                        var _message = this.scenarios[this.scenarioCount][1];

                        this.ShowMessage(textBlock: this.BigSpeech, message: _message);

                        break;

                    case "wait":

                        this.NextMessageButton.Visibility = Visibility.Visible;

                        if (this.scenarios[this.scenarioCount - 1] != null)
                        {
                            this.BackMessageButton.Visibility = Visibility.Visible;
                        }
                        this.isClickable = true;

                        break;

                    case "msg_win":

                        var showBigBuubleImage = this.scenarios[this.scenarioCount][1];
                        this.BigBubble.Source = new BitmapImage(new Uri($"Images/{showBigBuubleImage}", UriKind.Relative));

                        this.BigBubble.Visibility = Visibility.Visible;
                        this.BigSpeech.Visibility = Visibility.Visible;

                        this.scenarioCount += 1;
                        this.ScenarioPlay();

                        break;

                    case "next":

                        this.MainNextPageButton.Visibility = Visibility.Visible;
                        this.isClickable = true;

                        break;

                    case "hide":

                        // オブジェクトを消すときは後々ほとんどアニメで処理するようにする
                        var hideTarget = this.scenarios[this.scenarioCount][1];

                        switch (hideTarget)
                        {
                            case "msg_win":

                                this.BigBubble.Visibility = Visibility.Hidden;
                                this.BigSpeech.Visibility = Visibility.Hidden;
                                this.NextMessageButton.Visibility = Visibility.Hidden;
                                this.BackMessageButton.Visibility = Visibility.Hidden;

                                this.scenarioCount += 1;
                                this.ScenarioPlay();

                                break;

                            case "image":

                                this.position = this.scenarios[this.scenarioCount][2];
                                this.imageObjects[this.position].Visibility = Visibility.Hidden;

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

                        this.ShowMessage(textBlock: this.BoardTitle, message: this.scenarios[this.scenarioCount][1]);
                        break;

                    case "check1msg":

                        this.ShowMessage(textBlock: this.BoardChecK1Msg, message: this.scenarios[this.scenarioCount][1], checkBox: this.BoardCheck1Box);
                        break;

                    case "check2msg":

                        this.ShowMessage(textBlock: this.BoardChecK2Msg, message: this.scenarios[this.scenarioCount][1], checkBox: this.BoardCheck2Box);
                        break;

                    case "check3msg":

                        this.ShowMessage(textBlock: this.BoardChecK3Msg, message: this.scenarios[this.scenarioCount][1], checkBox: this.BoardCheck3Box);
                        break;

                    case "wait":

                        this.isClickable = true;
                        break;

                    case "image":

                        this.BoardCharacter.Visibility = Visibility.Visible;
                        this.LongBubble.Visibility = Visibility.Visible;

                        var _imageFile = this.scenarios[this.scenarioCount][1];

                        var _storyBoardName = this.scenarios[this.scenarioCount][2];
                        if (_storyBoardName != "")
                        {
                            _storyBoardName += "_board";
                        }
                        this.ShowImage(imageFile: _imageFile, imageObject: this.MainCharaDownRight, storyBoardName: _storyBoardName);

                        break;

                    case "suggest":

                        CheckBox[] _checkBoxs = new CheckBox[] { this.BoardCheck1Box, this.BoardCheck2Box, this.BoardCheck3Box };
                        this.ShowMessage(textBlock: this.LongSpeech, message: this.scenarios[this.scenarioCount][1], checkBoxes: _checkBoxs);
                        break;

                    case "tap":

                        this.isClickable = false;
                        break;
                }
            }

            if (this.scene == "manga")
            {
                switch (tag)
                {
                    case "title":

                        this.MangaTitle.Visibility = Visibility.Visible;

                        var titleImage = this.scenarios[this.scenarioCount][1];
                        this.ShowImage(imageFile: titleImage, imageObject: this.MangaTitle, "");

                        break;

                    case "flip":

                        this.MangaImage.Visibility = Visibility.Visible;

                        var mangaImage = this.scenarios[this.scenarioCount][1];

                        var _storyBoardName = this.scenarios[this.scenarioCount][2];

                        if (_storyBoardName != "")
                        {
                            _storyBoardName += "_manga";
                        }
                        this.ShowImage(imageFile: mangaImage, imageObject: this.MangaImage, storyBoardName: _storyBoardName);

                        break;

                    case "next":

                        this.MangaNextButton.Visibility = Visibility.Visible;
                        this.isClickable = true;

                        break;
                }
            }
        }

        void ShowMessage(TextBlock textBlock, string message, CheckBox checkBox = null, CheckBox[] checkBoxes = null)
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
                textBlock.Text = _message.Substring(0, word_num);

                if (word_num < _message.Length)
                {
                    word_num++;
                }
                else
                {
                    this.msgTimer.Stop();
                    this.msgTimer = null;

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

        private void ShowImage(string imageFile, Image imageObject, string storyBoardName = "")
        {
            imageObject.Source = new BitmapImage(new Uri($"Images/{imageFile}", UriKind.Relative));

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

                this.BoardNextPageButton.Visibility = Visibility.Visible;
                this.isClickable = true;
            }
        }

        private void BoardNextPageButton_Click(object sender, RoutedEventArgs e)
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
            this.MangaGrid.Visibility = Visibility.Hidden;
        }

        // UTF-8からShift-JISへの変換にそなえて取り置き
        public static string ConvertEncoding(string src, System.Text.Encoding destEnc)
        {
            byte[] src_temp = System.Text.Encoding.ASCII.GetBytes(src);
            byte[] dest_temp = System.Text.Encoding.Convert(System.Text.Encoding.ASCII, destEnc, src_temp);
            string ret = destEnc.GetString(dest_temp);
            return ret;
        }

        private void MainNextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.isClickable)
            {
                this.isClickable = false;

                this.scenarioCount += 1;
                this.ScenarioPlay();
            }
        }

        private void MangaNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.isClickable)
            {
                this.isClickable = false;

                this.scenarioCount += 1;
                this.ScenarioPlay();
            }
        }
    }
}
