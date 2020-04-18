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

        private Dictionary<string, Image> mainImages = null;
        private Dictionary<string, Image> itemImages = null;

        private Dictionary<string, TextBlock> mainTextBlocks = null;
        private Dictionary<string, TextBlock> itemTextBlocks = null;

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

            Debug.Print((this.scenarioCount + 1).ToString());

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

                        this.MainBigSpeech.Text = "";

                        this.MainCharaDownRight.Visibility = Visibility.Hidden;
                        this.MainCharaSmallLeftA.Visibility = Visibility.Hidden;
                        this.MainCharaSmallLeftB.Visibility = Visibility.Hidden;
                        this.MainCharaSmallLeftC.Visibility = Visibility.Hidden;
                        this.MainInfoCenter.Visibility = Visibility.Hidden;
                        this.MainCharaUpRight.Visibility = Visibility.Hidden;
                        this.MainNextPageButton.Visibility = Visibility.Hidden;
                        this.MainBackPageButton.Visibility = Visibility.Hidden;

                        this.MainItemImageCenter.Visibility = Visibility.Hidden;
                        this.MainItemNamePlate.Visibility = Visibility.Hidden;
                        this.MainItemNameText.Visibility = Visibility.Hidden;

                        this.MainBigBubble.Visibility = Visibility.Hidden;
                        this.MainBigSpeech.Visibility = Visibility.Hidden;
                        this.MainNextMsgButton.Visibility = Visibility.Hidden;
                        this.MainBackMsgButton.Visibility = Visibility.Hidden;

                        this.MainSessionTitle.Visibility = Visibility.Hidden;
                        this.MainSessionFrame.Visibility = Visibility.Hidden;
                        this.MainSessionSubTitle.Visibility = Visibility.Hidden;
                        this.MainSessionInfo.Visibility = Visibility.Hidden;

                        this.mainImages = new Dictionary<string, Image>
                        {
                            ["chara_down_right"] = this.MainCharaDownRight,
                            ["chara_small_left_a"] = this.MainCharaSmallLeftA,
                            ["chara_small_left_b"] = this.MainCharaSmallLeftB,
                            ["chara_small_left_c"] = this.MainCharaSmallLeftC,
                            ["info_center"] = this.MainInfoCenter,
                            ["chara_up_right"] = this.MainCharaUpRight,
                            ["item_image_center"] = this.MainItemImageCenter,
                            ["item_name_plate"] = this.MainItemNamePlate,
                            ["session_title"] = this.MainSessionTitle,
                            ["session_frame"] = this.MainSessionFrame,
                        };

                        this.mainTextBlocks = new Dictionary<string, TextBlock>
                        {
                            ["item_name_text"] = this.MainItemNameText,
                            ["session_sub_title"] = this.MainSessionSubTitle,
                            ["session_info"] = this.MainSessionInfo,
                        };

                        this.MainGrid.Visibility = Visibility.Visible;

                        this.scenarioCount += 1;
                        this.ScenarioPlay();

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
                        this.BoardLongBubble.Visibility = Visibility.Hidden;
                        this.BoardLongSpeech.Text = "";

                        this.BoardNextPageButton.Visibility = Visibility.Hidden;

                        this.BoardGrid.Visibility = Visibility.Visible;

                        this.scenarioCount += 1;
                        this.ScenarioPlay();

                        break;

                    case "manga":

                        this.MangaTitle.Visibility = Visibility.Hidden;
                        this.MangaImage.Visibility = Visibility.Hidden;
                        this.MangaNextButton.Visibility = Visibility.Hidden;

                        this.MangaGrid.Visibility = Visibility.Visible;

                        this.scenarioCount += 1;
                        this.ScenarioPlay();

                        break;

                    case "item":

                        this.ItemNameText.Text = "";
                        this.ItemInfoTitle.Text = "";
                        this.ItemInfoSentence.Text = "";

                        this.ItemNextPageButton.Visibility = Visibility.Hidden;
                        this.ItemBackPageButton.Visibility = Visibility.Hidden;

                        this.ItemBubble.Visibility = Visibility.Hidden;
                        this.ItemNumber.Visibility = Visibility.Hidden;
                        this.ItemObject.Visibility = Visibility.Hidden;
                        this.ItemNamePlate.Visibility = Visibility.Hidden;
                        this.ItemNameText.Visibility = Visibility.Hidden;
                        this.ItemInfoPlate.Visibility = Visibility.Hidden;
                        this.ItemChara.Visibility = Visibility.Hidden;
                        this.ItemInfoTitle.Visibility = Visibility.Hidden;
                        this.ItemInfoSentence.Visibility = Visibility.Hidden;

                        this.itemImages = new Dictionary<string, Image>
                        {
                            ["chara"] = this.ItemChara,
                            ["name_plate"] = this.ItemNamePlate,
                            ["object"] = this.ItemObject,
                            ["bubble"] = this.ItemBubble,
                            ["info_plate"] = this.ItemInfoPlate,
                        };

                        this.itemTextBlocks = new Dictionary<string, TextBlock>
                        {
                            ["name_text"] = this.ItemNameText,
                            ["number"] = this.ItemNumber,
                            ["info_title"] = this.ItemInfoTitle,
                            ["info_sentence"] = this.ItemInfoSentence,
                        };

                        this.ItemGrid.Visibility = Visibility.Visible;

                        this.scenarioCount += 1;
                        this.ScenarioPlay();

                        break;
                }
            }

            // Commonメインシーン
            if (this.scene == "main")
            {
                switch (tag)
                {
                    case "bg":

                        // 後々背景もクロスフェードなどの処理を入れる
                        var bgImage = this.scenarios[this.scenarioCount][1];
                        this.MainBG.Source = new BitmapImage(new Uri($"Images/{bgImage}", UriKind.Relative));

                        this.scenarioCount += 1;
                        this.ScenarioPlay();

                        break;

                    case "image":

                        this.MainBigSpeech.Text = "";

                        var _imageFile = this.scenarios[this.scenarioCount][1];

                        this.position = this.scenarios[this.scenarioCount][2];

                        var _imageObject = this.mainImages[this.position];

                        _imageObject.Visibility = Visibility.Visible;

                        var _storyBoardName = this.scenarios[this.scenarioCount][3];
                        if (_storyBoardName != "")
                        {
                            _storyBoardName+= "_main_" + this.position;
                        }
                        this.ShowImage(imageFile: _imageFile, imageObject: _imageObject, storyBoardName: _storyBoardName);

                        break;

                    case "msg":

                        this.MainBigSpeech.Text = "";

                        this.MainNextMsgButton.Visibility = Visibility.Hidden;
                        this.MainBackMsgButton.Visibility = Visibility.Hidden;

                        var _message = this.scenarios[this.scenarioCount][1];

                        this.ShowMessage(textBlock: this.MainBigSpeech, message: _message);

                        break;

                    case "wait":

                        this.MainNextMsgButton.Visibility = Visibility.Visible;

                        if (this.scenarios[this.scenarioCount - 1] != null)
                        {
                            this.MainBackMsgButton.Visibility = Visibility.Visible;
                        }
                        this.isClickable = true;

                        break;

                    case "msg_win":

                        var showBigBubbleImage = this.scenarios[this.scenarioCount][1];
                        this.MainBigBubble.Source = new BitmapImage(new Uri($"Images/{showBigBubbleImage}", UriKind.Relative));

                        this.MainBigBubble.Visibility = Visibility.Visible;
                        this.MainBigSpeech.Visibility = Visibility.Visible;

                        this.scenarioCount += 1;
                        this.ScenarioPlay();

                        break;

                    case "next":

                        this.MainNextPageButton.Visibility = Visibility.Visible;
                        this.MainBackPageButton.Visibility = Visibility.Visible;
                        this.isClickable = true;

                        break;

                    case "text":

                        this.position = this.scenarios[this.scenarioCount][2];

                        var textObject = this.mainTextBlocks[this.position];

                        var _text = this.scenarios[this.scenarioCount][1];

                        textObject.Text = _text;

                        textObject.Visibility = Visibility.Visible;

                        this.scenarioCount += 1;
                        this.ScenarioPlay();

                        break;

                    case "hide":

                        // オブジェクトを消すときは後々ほとんどアニメで処理するようにする
                        var hideTarget = this.scenarios[this.scenarioCount][1];

                        switch (hideTarget)
                        {
                            case "msg_win":

                                this.MainBigBubble.Visibility = Visibility.Hidden;
                                this.MainBigSpeech.Visibility = Visibility.Hidden;
                                this.MainNextMsgButton.Visibility = Visibility.Hidden;
                                this.MainBackMsgButton.Visibility = Visibility.Hidden;

                                this.scenarioCount += 1;
                                this.ScenarioPlay();

                                break;

                            case "image":

                                this.position = this.scenarios[this.scenarioCount][2];
                                this.mainImages[this.position].Visibility = Visibility.Hidden;

                                this.scenarioCount += 1;
                                this.ScenarioPlay();

                                break;

                            case "text":

                                this.position = this.scenarios[this.scenarioCount][2];
                                this.mainTextBlocks[this.position].Visibility = Visibility.Hidden;
                                this.mainTextBlocks[this.position].Text = "";

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
                        this.BoardLongBubble.Visibility = Visibility.Visible;

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
                        this.ShowMessage(textBlock: this.BoardLongSpeech, message: this.scenarios[this.scenarioCount][1], checkBoxes: _checkBoxs);
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

            if (this.scene == "item")
            {
                
                switch (tag)
                {
                    case "bg":

                        var bgImage = this.scenarios[this.scenarioCount][1];
                        this.ItemBG.Source = new BitmapImage(new Uri($"Images/{bgImage}", UriKind.Relative));

                        this.scenarioCount += 1;
                        this.ScenarioPlay();

                        break;

                    case "image":

                        var _imageFile = this.scenarios[this.scenarioCount][1];

                        this.position = this.scenarios[this.scenarioCount][2];

                        var _imageObject = this.itemImages[this.position];

                        _imageObject.Visibility = Visibility.Visible;

                        var _storyBoardName = this.scenarios[this.scenarioCount][3];
                        if (_storyBoardName != "")
                        {
                            _storyBoardName += "_item_" + this.position;
                        }
                        this.ShowImage(imageFile: _imageFile, imageObject: _imageObject, storyBoardName: _storyBoardName);

                        break;

                    case "text":

                        this.position = this.scenarios[this.scenarioCount][2];

                        var textObject = this.itemTextBlocks[this.position];

                        var _text = this.scenarios[this.scenarioCount][1];

                        var text = _text.Replace("鬱", "\u2028");

                        textObject.Text = text;

                        textObject.Visibility = Visibility.Visible;

                        this.scenarioCount += 1;
                        this.ScenarioPlay();

                        break;

                    case "next":

                        this.ItemNextPageButton.Visibility = Visibility.Visible;
                        this.ItemBackPageButton.Visibility = Visibility.Visible;
                        this.isClickable = true;

                        break;

                    case "item_hide":

                        var hideTarget = this.scenarios[this.scenarioCount][1];

                        switch (hideTarget)
                        {
                            case "image":

                                this.position = this.scenarios[this.scenarioCount][2];
                                this.itemImages[this.position].Visibility = Visibility.Hidden;

                                this.scenarioCount += 1;
                                this.ScenarioPlay();

                                break;

                            case "text":

                                this.position = this.scenarios[this.scenarioCount][2];
                                this.itemTextBlocks[this.position].Visibility = Visibility.Hidden;
                                this.itemTextBlocks[this.position].Text = "";

                                this.scenarioCount += 1;
                                this.ScenarioPlay();

                                break;
                        }
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
            else
            {
                this.scenarioCount += 1;
                this.ScenarioPlay();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.isClickable)
            {
                this.isClickable = false;

                this.scenarioCount += 1;
                this.ScenarioPlay();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.isClickable)
            {
                this.isClickable = false;

                // this.scenarioCount -= 1;
                // this.ScenarioPlay();
            }
            // 連続Backの実現にはもっと複雑な処理がいる
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            if (checkBox.IsChecked == true)
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

        private void HiddeAllScene()
        {
            this.MainGrid.Visibility = Visibility.Hidden;
            this.BoardGrid.Visibility = Visibility.Hidden;
            this.MangaGrid.Visibility = Visibility.Hidden;
            this.ItemGrid.Visibility = Visibility.Hidden;
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
