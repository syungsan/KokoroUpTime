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

namespace KokoroUpTime
{
    /// <summary>
    /// RuleBoardPage.xaml の相互作用ロジック
    /// </summary>
    public partial class RuleBoardPage : Page
    {
        // ゲームを進行させるシナリオ
        private int scenarioCount = 0;
        private List<List<string>> scenarios = null;

        // 各種コントロールの名前を収める変数
        private string position = "";

        // マウスクリックを可能にするかどうかのフラグ
        private bool isClickable = false;

        // 画面を何回タップしたか
        private int tapCount = 0;

        // メッセージ表示関連
        private DispatcherTimer msgTimer;
        private int word_num;

        // 各種コントロールを任意の文字列で呼び出すための辞書
        private Dictionary<string, Image> imageObjects = null;
        private Dictionary<string, TextBlock> textBlockObjects = null;
        private Dictionary<string, Grid> gridObjects = null;

        // 黒板のチェックボックス
        private CheckBox[] checkBoxs;

        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();
        public DataProgress dataProgress = new DataProgress();

        public RuleBoardPage()
        {
            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            this.InitControls();
        }

        private void InitControls()
        {
            // 各種コントロールに任意の文字列をあてがう

            this.imageObjects = new Dictionary<string, Image>
            {
                ["bg_image"] = this.BackgroundImage,
                ["shiroji_small_right_down_image"] = this.ShirojiSmallRightDownImage,
            };

            this.textBlockObjects = new Dictionary<string, TextBlock>
            {
                ["rule_board_title_msg"] = this.RuleBoardTitleTextBlock,
                ["rule_board_check1_msg"] = this.RuleBoardCheck1TextBlock,
                ["rule_board_check2_msg"] = this.RuleBoardCheck2TextBlock,
                ["rule_board_check3_msg"] = this.RuleBoardCheck3TextBlock,
                ["thin_msg"] = this.ThinMessageTextBlock,
            };

            this.gridObjects = new Dictionary<string, Grid>
            {
                ["rule_board_grid"] = this.RuleBoardGrid,
                ["thin_msg_grid"] = this.ThinMessageGrid,
            };
        }

        private void ResetControls()
        {
            // 各種コントロールを隠すことでフルリセット

            this.BackgroundImage.Visibility = Visibility.Hidden;
            this.RuleBoardTitleTextBlock.Visibility = Visibility.Hidden;
            this.RuleBoardCheck1TextBlock.Visibility = Visibility.Hidden;
            this.RuleBoardCheck2TextBlock.Visibility = Visibility.Hidden;
            this.RuleBoardCheck3TextBlock.Visibility = Visibility.Hidden;
            this.RuleBoardCheck1Box.Visibility = Visibility.Hidden;
            this.RuleBoardCheck2Box.Visibility = Visibility.Hidden;
            this.RuleBoardCheck3Box.Visibility = Visibility.Hidden;
            this.ShirojiSmallRightDownImage.Visibility = Visibility.Hidden;
            this.RuleBoardGrid.Visibility = Visibility.Hidden;
            this.ThinMessageGrid.Visibility = Visibility.Hidden;
            this.ThinMessageTextBlock.Visibility = Visibility.Hidden;
            this.RuleBoardTitleTextBlock.Text = "";
            this.RuleBoardCheck1TextBlock.Text = "";
            this.RuleBoardCheck2TextBlock.Text = "";
            this.RuleBoardCheck3TextBlock.Text = "";
            this.ThinMessageTextBlock.Text = "";
            this.RuleBoardCheck1Box.IsEnabled = false;
            this.RuleBoardCheck2Box.IsEnabled = false;
            this.RuleBoardCheck3Box.IsEnabled = false;
            this.ReturnToTopButton.Visibility = Visibility.Hidden;
        }

        public void SetNextPage(InitConfig _initConfig, DataOption _dataOption, DataItem _dataItem, DataProgress _dataProgress)
        {
            this.initConfig = _initConfig;
            this.dataOption = _dataOption;
            this.dataItem = _dataItem;
            this.dataProgress = _dataProgress;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var csv = new CsvReader("./Scenarios/rule_board.csv"))
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
                // フルリセット
                case "reset":

                    this.ResetControls();

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                // グリッドに対しての処理
                case "grid":

                    // グリッドコントロールを任意の名前により取得
                    this.position = this.scenarios[this.scenarioCount][1];

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

                        // ストーリーボードの名前にコントロールの名前を付け足す
                        gridStoryBoard += $"_{this.position}";

                        this.ShowAnime(storyBoard: gridStoryBoard, isSync: gridAnimeIsSync);
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

                    string imageFile;

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        imageFile = this.scenarios[this.scenarioCount][2];

                        // フォルダの画像でなくリソース内の画像を表示することでスピードアップ
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

                // 流れる文字をTextBlockで表現するための処理
                case "msg":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var _textObject = this.textBlockObjects[this.position];

                    _textObject.Visibility = Visibility.Hidden;

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var _message = this.scenarios[this.scenarioCount][2];

                        this.ShowMessage(textObject: _textObject, message: _message);
                    }
                    else
                    {
                        // xamlに直接書いたStaticな文章を表示する場合
                        this.ShowMessage(textObject: _textObject, message: _textObject.Text);
                    }
                    break;

                // メッセージに対する待ち（メッセージボタンの表示切り替え）
                case "wait":

                    this.isClickable = true;

                    break;

                case "time_span":

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
                    break;

                case "return":

                    this.ReturnToTopButton.Visibility = Visibility.Visible;

                    this.isClickable = true;

                    break;

                // 教室のルール黒板の処理
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

                case "check":

                    var checkObjName = this.scenarios[this.scenarioCount][1];

                    switch (checkObjName)
                    {
                        case "check_box":

                            var checkObjNum = this.scenarios[this.scenarioCount][2];

                            this.checkBoxs = new CheckBox[] { this.RuleBoardCheck1Box, this.RuleBoardCheck2Box, this.RuleBoardCheck3Box };

                            this.CheckObj(checkBoxs[int.Parse(checkObjNum)]);

                            break;
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;
            }
        }

        void ShowMessage(TextBlock textObject, string message, object obj = null)
        {
            this.word_num = 0;

            // メッセージ表示処理
            this.msgTimer = new DispatcherTimer();
            this.msgTimer.Tick += ViewMsg;
            this.msgTimer.Interval = TimeSpan.FromSeconds(1.0f / this.dataOption.MessageSpeed);
            this.msgTimer.Start();

            // 一文字ずつメッセージ表示（Inner Func）
            void ViewMsg(object sender, EventArgs e)
            {
                textObject.Text = message.Substring(0, this.word_num);

                if (this.word_num == 0)
                {
                    textObject.Visibility = Visibility.Visible;
                }

                if (this.word_num < message.Length)
                {
                    this.word_num++;
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

        // アニメーション（ストーリーボード）の処理
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

        // 黒板ルール処理のためだけの追加
        private void MessageCallBack(object obj)
        {
            switch (obj)
            {
                case CheckBox checkBox:

                    checkBox.Visibility = Visibility.Visible;

                    break;
            }
        }

        private void CheckObj(object obj)
        {
            switch (obj)
            {
                case CheckBox checkBox:

                    checkBox.IsChecked = true;

                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 各種ボタンが押されたときの処理

            Button button = sender as Button;

            if (button.Name == "ReturnToTopButton")
            {
                TitlePage titlePage = new TitlePage();

                titlePage.SetIsFirstBootFlag(false);

                titlePage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                this.NavigationService.Navigate(titlePage);
            }
        }
    }
}
