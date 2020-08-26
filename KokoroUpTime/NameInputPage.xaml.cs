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
using System.IO;
using System.Linq;
using SQLite;
using Expansion;
using FileIOUtils;
using Osklib;
using Osklib.Wpf;

namespace KokoroUpTime
{
    /// <summary>
    /// GameWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class NameInputPage : Page
    {
        private float INIT_MESSAGE_SPEED = 30.0f;

        private string[] EDIT_BUTTON = { "えんぴつ", "けしごむ", "すべてけす", "かんせい" };

        private string[] IMAGE_TEXTS = { "name" };

        private string BASE_USER_NAME = "名無し";

        // ゲームを進行させるシナリオ
        private int scenarioCount = 0;
        private List<List<string>> scenarios = null;

        // 各種コントロールの名前を収める変数
        private string position = "";

        // ボタンのマウスクリックを可能にするかどうかのフラグ
        private bool isClickable = false;

        // メッセージ表示関連
        private DispatcherTimer msgTimer;
        private int word_num;

        private int inlineCount;
        private int imageInlineCount;

        private List<Run> runs = new List<Run>();
        private List<InlineUIContainer> imageInlines = new List<InlineUIContainer>();

        // 各種コントロールを任意の文字列で呼び出すための辞書
        private Dictionary<string, Image> imageObjects = null;
        private Dictionary<string, TextBlock> textBlockObjects = null;
        private Dictionary<string, Button> buttonObjects = null;
        private Dictionary<string, Grid> gridObjects = null;

        // 入力方法 [0: 手書き, 1: キーボード]
        private int selectInputMethod = 0;

        // 新しいユーザネーム
        private string newUserName = "";
        private string selectUserTitle = "";

        public NameInputPage()
        {
            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            // ページ遷移履歴のクリア
            // これをしないとタッチキーボードのバックスペースでタイトルに戻ってしまう
            NavigationCommands.BrowseBack.InputGestures.Clear();
            NavigationCommands.BrowseForward.InputGestures.Clear();

            this.EditingModeItemsControl.ItemsSource = EDIT_BUTTON;

            this.NameImage.Source = null;

            this.InitControls();

            this.NameTextBox.Text = BASE_USER_NAME;

            OnScreenKeyboardSettings.EnableForTextBoxes = true;

            // 最初に一時フォルダを削除しておく
            if (Directory.Exists("./temp"))
            {
                Directory.Delete("./temp", true);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var csv = new CsvReader("./Scenarios/name_input.csv"))
            {
                this.scenarios = csv.ReadToEnd();
            }
            this.ScenarioPlay();
        }

        private void InitControls()
        {
            // 各種コントロールに任意の文字列をあてがう

            this.imageObjects = new Dictionary<string, Image>
            {
                ["bg_image"] = this.BackgroundImage,
                ["shiroji_right_image"] = this.ShirojiRightImage,
                ["main_msg_bubble_image"] = this.MainMessageBubbleImage,
                ["cover_layer_image"] = this.CoverLayerImage,
            };

            this.textBlockObjects = new Dictionary<string, TextBlock>
            {
                ["main_msg"] = this.MainMessageTextBlock,
            };

            this.buttonObjects = new Dictionary<string, Button>
            {
                ["next_msg_button"] = this.NextMessageButton,
                ["back_msg_button"] = this.BackMessageButton,
                ["return_to_title_button"] = this.ReturnToTitleButton,
            };

            this.gridObjects = new Dictionary<string, Grid>
            {
                ["main_msg_grid"] = this.MainMessageGrid,
                ["name_input_grid"] = this.NameInputGrid,
                ["select_input_method_grid"] = this.SelectInputMethodGrid,
                ["select_user_title_grid"] = this.SelectUserTitleGrid,
                ["make_user_name_complete_grid"] = this.MakeUserNameCompleteGrid,
            };
        }

        private void ResetControls()
        {
            // 各種コントロールを隠すことでフルリセット

            this.NameInputGrid.Visibility = Visibility.Hidden;
            this.CanvasGrid.Visibility = Visibility.Hidden;
            this.MainMessageGrid.Visibility = Visibility.Hidden;
            this.BackgroundImage.Visibility = Visibility.Hidden;
            this.ShirojiRightImage.Visibility = Visibility.Hidden;
            this.MainMessageTextBlock.Visibility = Visibility.Hidden;
            this.NextMessageButton.Visibility = Visibility.Hidden;
            this.BackMessageButton.Visibility = Visibility.Hidden;
            this.ReturnToTitleButton.Visibility = Visibility.Hidden;
            this.SelectInputMethodGrid.Visibility = Visibility.Hidden;
            this.CoverLayerImage.Visibility = Visibility.Hidden;
            this.SelectUserTitleGrid.Visibility = Visibility.Hidden;
            this.MakeUserNameCompleteGrid.Visibility = Visibility.Hidden;
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

                        buttonStoryBoard += $"_{this.position}";

                        this.ShowAnime(storyBoard: buttonStoryBoard, isSync: buttonAnimeIsSync);
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

                        this.ShowMessage(textObject: _textObject, messages: _messages);
                    }
                    else
                    {
                        var _messages = this.SequenceCheck(_textObject.Text);

                        // xamlに直接書いたStaticな文章を表示する場合
                        this.ShowMessage(textObject: _textObject, messages: _messages);
                    }
                    break;

                // メッセージに対する待ち（メッセージボタンの表示切り替え）
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

                        case "grid":

                            this.NextMessageButton.Visibility = Visibility.Hidden;
                            this.BackMessageButton.Visibility = Visibility.Hidden;

                            this.position = this.scenarios[this.scenarioCount][2];
                            this.gridObjects[this.position].Visibility = Visibility.Hidden;

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

                case "jump":

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;
            }
        }

        private List<List<string>> SequenceCheck(string text)
        {
            // 苦悶の改行処理（文章中の「鬱」を疑似改行コードとする）
            text = text.Replace("鬱", "\u2028");

            if (this.selectInputMethod == 1)
            {
                text = text.Replace("【name】", this.newUserName);
            }

            text = text.Replace("【くん／ちゃん／さん】", this.selectUserTitle);

            List<List<string>> text2ds = new List<List<string>>();

            // まずは画像文字から
            text = text.Replace("】", "【");

            var texts = text.Split("【");

            foreach (var imageText in IMAGE_TEXTS)
            {
                int[] matchIndexs = { };

                foreach (var (txt, index) in texts.Indexed())
                {
                    if (txt == imageText)
                    {
                        matchIndexs.Append(index);
                    }
                }

                foreach (var tex in texts)
                {
                    List<string> tex1ds = new List<string> { tex };
                    text2ds.Add(tex1ds);
                }

                foreach (var matchIndex in matchIndexs)
                {
                    text2ds[matchIndex].Add(imageText);
                }
            }

            /*
            // そしてテキストタグを
            var matchTexts = new Regex(@"<@=(.+?)>(.+?)</@>").Matches(text);

            for (int i = 0; i < matchTexts.Count; i++)
            {
                var sequence = matchTexts[i].Value;

                var matchTags = new Regex(@"<@=(.+?)>").Matches(sequence);

                var trimTag = matchTags[0].Value.Replace("<@=", "").Replace(">", "");

                var tags = trimTag.Split(",");

                text = text.Replace(matchTags[0].Value, "<@").Replace("</@>", "<@");

                var texts = text.Split("<@");

                foreach (var tex in texts)
                {
                    List<string> tex1ds = new List<string> { tex };
                    text2ds.Add(tex1ds);
                } 
                var trimSeq = sequence.Replace(matchTags[0].Value, "").Replace("</@>", "");

                foreach (var tag in tags)
                {
                    text2ds[Array.IndexOf(texts, trimSeq)].Add(tag);
                }
            }
            */
            return text2ds;
        }

        private void ShowMessage(TextBlock textObject, List<List<string>> messages)
        {
            textObject.Text = "";
            textObject.Visibility = Visibility.Visible;

            this.word_num = 0;

            // メッセージ表示処理
            this.msgTimer = new DispatcherTimer();
            this.msgTimer.Tick += ViewMsg;
            this.msgTimer.Interval = TimeSpan.FromSeconds(1.0f / INIT_MESSAGE_SPEED);
            this.msgTimer.Start();

            this.inlineCount = 0;
            this.imageInlineCount = 0;

            foreach (var run in this.runs)
            {
                run.Text = "";
            }
            this.runs.Clear();

            this.imageInlines.Clear();

            textObject.Inlines.Clear();

            // 画像インラインと文字インラインの合体
            foreach (var msgs in messages)
            {
                string namePngPath = "./temp/temp_name.png";

                if (msgs[0] == "name" && File.Exists(namePngPath))
                {
                    var imageInline = new InlineUIContainer { Child = new Image { Source = null, Height = 48 } };

                    textObject.Inlines.Add(imageInline);

                    this.imageInlines.Add(imageInline);
                }
                var run = new Run { Text = "", Foreground = new SolidColorBrush(Colors.Black) };

                textObject.Inlines.Add(run);

                this.runs.Add(run);
            }

            // 一文字ずつメッセージ表示（Inner Func）
            void ViewMsg(object sender, EventArgs e)
            {
                if (this.inlineCount < messages.Count)
                {
                    var msgs = messages[this.inlineCount];

                    string namePngPath = "./temp/temp_name.png";

                    if (msgs[0] == "name" && File.Exists(namePngPath))
                    {
                        // msgs[0].Replace("name", "");

                        // 実行ファイルの場所を絶対パスで取得
                        var startupPath = FileUtils.GetStartupPath();

                        var image = new BitmapImage();

                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        image.UriSource = new Uri($@"{startupPath}/{namePngPath}", UriKind.Absolute);
                        image.EndInit();

                        image.Freeze();

                        (this.imageInlines[imageInlineCount].Child as Image).Source = image;

                        this.imageInlineCount++;

                        this.inlineCount++;
                        this.word_num = 0;

                        return;
                    }
                    this.runs[inlineCount].Text = msgs[0].Substring(0, this.word_num);

                    if (this.word_num < msgs[0].Length)
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

        // 登録済みの画像名前ユーザの添え字の最大値を返す
        private int GetLatestImageUserNumber()
        {
            List<int> imageUserNumbers = new List<int>();

            var dirPaths = Directory.GetDirectories("./Log/");

            foreach (var dirPath in dirPaths)
            {
                var splitDirName = dirPath.Split("_");

                if (splitDirName[0] == "./Log/画像Name")
                {
                    imageUserNumbers.Add(Int32.Parse(splitDirName[1]));
                }
            }
            int imageUserMaxNumber;

            if (imageUserNumbers.Count > 0)
            {
                imageUserMaxNumber = imageUserNumbers.Max() + 1;
            }
            else
            {
                imageUserMaxNumber = 1;
            }
            return imageUserMaxNumber;
        }

        // 文字名前ユーザのリストを返す
        private List<string> GetWordUserList()
        {
            List<string> wordUserList = new List<string>();

            var dirPaths = Directory.GetDirectories("./Log/");

            foreach (var dirPath in dirPaths)
            {
                var splitDirName = dirPath.Split("_");

                if (splitDirName[0] != "./Log/画像Name")
                {
                    var wordUserName = splitDirName[0].Split("/")[2];
                    
                    wordUserList.Add(wordUserName);
                }
            }
            return wordUserList;
        }

        // シナリオ選択肢処理
        private void JumpTo(string tag)
        {
            foreach (var (scenario, index) in this.scenarios.Indexed())
            {
                if (scenario[0] == "jump" && scenario[1] == tag)
                {
                    this.scenarioCount = index + 1;
                    this.ScenarioPlay();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 各種ボタンが押されたときの処理

            Button button = sender as Button;

            if (this.isClickable)
            {
                if (button.Name == "NextMessageButton")
                {
                    this.isClickable = false;

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }

                if (button.Name == "NameButton")
                {
                    this.isClickable = false;

                    if (this.selectInputMethod == 0)
                    {
                        this.NameInputGrid.Visibility = Visibility.Hidden;
                        this.CanvasGrid.Visibility = Visibility.Visible;
                    }
                }

                if (button.Name == "DecisionButton")
                {
                    this.isClickable = false;

                    DirectoryUtils.SafeCreateDirectory("./Log");

                    Image[] handWritingNameImages = new Image[] { this.KunHandWritingNameImage, this.ChanHandWritingNameImage, this.SanHandWritingNameImage, this.NoneHandWritingNameImage };
                    TextBlock[] nameTextBlocks = new TextBlock[] { this.KunTextBlock, this.ChanTextBlock, this.SanTextBlock, this.NoneTextBlock };
                    string[] titles = new string[] { " くん", " ちゃん", " さん", "（なし）" };

                    if (this.selectInputMethod == 0)
                    {
                        // 実行ファイルの場所を絶対パスで取得
                        var startupPath = FileUtils.GetStartupPath();

                        if (!File.Exists($@"{startupPath}/temp/temp_name.png"))
                        {
                            MessageBox.Show("空の名前は入力できません。", "情報");

                            this.isClickable = true;

                            return;
                        }

                        // 全ての宛名に画像の登録
                        foreach (var (handWritingNameImage, index) in handWritingNameImages.Indexed())
                        {
                            handWritingNameImage.Source = new BitmapImage(new Uri($@"{startupPath}/temp/temp_name.png", UriKind.Absolute));

                            nameTextBlocks[index].Text = titles[index];
                        }
                    }
                    else if (this.selectInputMethod == 1)
                    {
                        // キーボード
                        this.newUserName = this.NameTextBox.Text;

                        var wordUserNames = this.GetWordUserList();

                        if (this.newUserName == "")
                        {
                            MessageBox.Show("空の名前は入力できません。", "情報");

                            this.isClickable = true;

                            return;
                        }
                        else if (!wordUserNames.Contains(this.newUserName))
                        {                          
                            foreach (var (handWritingNameImage, index) in handWritingNameImages.Indexed())
                            {
                                handWritingNameImage.Source = null;

                                nameTextBlocks[index].Text = this.newUserName + titles[index];
                            }
                        }
                        else
                        {
                            MessageBox.Show("すでに同じ名前の人がいます。\n別の名前を入力してください。", "情報");

                            this.isClickable = true;

                            return;
                        }
                    }
                    this.NameInputGrid.Visibility = Visibility.Hidden;

                    // OSKを落とすと管理者権限出ないと再表示できなくなる
                    // this.CloseOSK();

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }

                if (button.Name == "SelectInputMethodOKButton")
                {
                    this.isClickable = false;

                    RadioButton[] inputMethodRadioButtons = new RadioButton[] { this.HandWritingRadioButton, this.KeyboardRadioButton };

                    foreach (var (method, index) in inputMethodRadioButtons.Indexed())
                    {
                        if (method.IsChecked == true)
                        {
                            this.selectInputMethod = index;
                        }
                    }

                    // 入力方法を選択した場合の準備
                    if (this.selectInputMethod == 0)
                    {
                        this.NameTextBox.Visibility = Visibility.Hidden;
                    }
                    else if (this.selectInputMethod == 1)
                    {
                        
                    }
                    this.SelectInputMethodGrid.Visibility = Visibility.Hidden;

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }

                if (button.Name == "ReturnToTitleButton")
                {
                    this.isClickable = false;

                    if (Directory.Exists("./temp"))
                    {
                        Directory.Delete("./temp", true);
                    }

                    TitlePage titlePage = new TitlePage();

                    titlePage.SetIsFirstBootFlag(true);

                    this.NavigationService.Navigate(titlePage);
                }

                if (button.Name == "SelectUserTitleOKButton")
                {
                    this.isClickable = false;

                    RadioButton[] userTiitleRadioButtons = new RadioButton[] { this.KunRadioButton, this.ChanRadioButton, this.SanRadioButton, this.NoneRadioButton };
                    string[] titles = new string[] { "くん", "ちゃん", "さん", "" };

                    foreach (var (userTiitleRadioButton, index) in userTiitleRadioButtons.Indexed())
                    {
                        if (userTiitleRadioButton.IsChecked == true)
                        {
                            this.selectUserTitle = titles[index];
                        }
                    }
                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }

                if (button.Name == "MakeUserNameCompleteOKButton")
                {
                    this.isClickable = false;

                    // 実行ファイルの場所を絶対パスで取得
                    var startupPath = FileUtils.GetStartupPath();

                    var newUserNameDirPath = "";

                    if (this.selectInputMethod == 0)
                    {
                        var tempNameImagePath = $@"{startupPath}/temp/temp_name.png";

                        this.newUserName = $@"画像Name_{this.GetLatestImageUserNumber()}";

                        newUserNameDirPath = $@"{startupPath}/Log/{this.newUserName}";

                        var distNameImagePath = $@"{newUserNameDirPath}/name.png";

                        DirectoryUtils.SafeCreateDirectory(newUserNameDirPath);

                        File.Copy(tempNameImagePath, distNameImagePath);
                    }
                    else if (this.selectInputMethod == 1)
                    {
                        // キーボード
                        newUserNameDirPath = $@"{startupPath}/Log/{this.newUserName}";

                        DirectoryUtils.SafeCreateDirectory(newUserNameDirPath);
                    }

                    // 新入力ユーザをカレントユーザにする操作
                    this.InitConfigFile();
                    this.InitDatabaseFile();

                    File.Copy($@"{newUserNameDirPath}/user.conf", @"./Log/system.conf", true);

                    JumpTo("complete");
                }

                if (button.Name == "MakeUserNameCompleteNOButton")
                {
                    this.isClickable = false;

                    this.NameImage.Source = null;

                    var startupPath = FileUtils.GetStartupPath();

                    if (File.Exists($@"{startupPath}/temp/temp_name.png"))
                    {
                        File.Delete($@"{startupPath}/temp/temp_name.png");
                    }
                    this.NameCanvas.Strokes.Clear();

                    this.NameTextBox.Text = BASE_USER_NAME;

                    this.HandWritingRadioButton.IsChecked = true;

                    this.SanRadioButton.IsChecked = true;

                    JumpTo("make_name");
                }
            }

            if (button.Content.ToString() == "えんぴつ")
            {
                this.NameCanvas.EditingMode = InkCanvasEditingMode.Ink;
            }

            if (button.Content.ToString() == "けしごむ")
            {
                this.NameCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
            }

            if (button.Content.ToString() == "すべてけす")
            {
                this.NameCanvas.Strokes.Clear();
            }

            if (button.Content.ToString() == "かんせい")
            {
                DirectoryUtils.SafeCreateDirectory("./temp");

                // ストロークが描画されている境界を取得
                System.Windows.Rect rectBounds = this.NameCanvas.Strokes.GetBounds();

                // 描画先を作成
                DrawingVisual dv = new DrawingVisual();
                DrawingContext dc = dv.RenderOpen();

                // 描画エリアの位置補正（補正しないと黒い部分ができてしまう）
                dc.PushTransform(new TranslateTransform(-rectBounds.X, -rectBounds.Y));

                // 描画エリア(dc)に四角形を作成
                // 四角形の大きさはストロークが描画されている枠サイズとし、
                // 背景色はInkCanvasコントロールと同じにする
                dc.DrawRectangle(this.NameCanvas.Background, null, rectBounds);

                // 上記で作成した描画エリア(dc)にInkCanvasのストロークを描画
                this.NameCanvas.Strokes.Draw(dc);
                dc.Close();

                if ((int)rectBounds.Width < 0 || (int)rectBounds.Height < 0)
                {
                    MessageBox.Show("何か描いて、名前を入力してください。", "情報");

                    return;
                }

                // ビジュアルオブジェクトをビットマップに変換する
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)rectBounds.Width, (int)rectBounds.Height, 96, 96, PixelFormats.Pbgra32);

                rtb.Render(dv);

                //仮置き
                string nameBmp = "temp_name.png";
                string dirPath = $"./temp";

                string nameBmpPath = System.IO.Path.Combine(dirPath, nameBmp);
                var startupPath = FileUtils.GetStartupPath();

                PngBitmapEncoder png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(rtb));

                // ファイルのパスは仮
                using (var stream = File.Create($@"{startupPath}/{nameBmpPath}"))
                {
                    png.Save(stream);
                }

                var pngmap = new BitmapImage();

                pngmap.BeginInit();
                pngmap.CacheOption = BitmapCacheOption.OnLoad;    //ココ
                pngmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;  //ココ
                pngmap.UriSource = new Uri($@"{startupPath}/{nameBmpPath}", UriKind.Absolute);
                pngmap.EndInit();

                pngmap.Freeze();                                  //ココ

                this.NameImage.Source = null;
                this.NameImage.Source = pngmap;

                this.isClickable = true;

                this.NameInputGrid.Visibility = Visibility.Visible;
                this.CanvasGrid.Visibility = Visibility.Hidden;
            }
        }

        // このユーザのコンフィギュレーションファイルを作成
        private void InitConfigFile()
        {
            var startupPath = FileUtils.GetStartupPath();

            var newUserNameDirPath = $@"{startupPath}/Log/{this.newUserName}";

            var confPath = $@"{newUserNameDirPath}/user.conf";

            var accessTime = DateTime.Now.ToString();

            var initConfigs = new List<List<string>>();

            var initConfig = new List<string>() { this.newUserName, this.selectUserTitle, accessTime };

            initConfigs.Add(initConfig);

            using (var csv = new CsvWriter(confPath))
            {
                csv.Write(initConfigs);
            }
        }

        // デフォルトのデータベースをユーザのものへとコピー
        private void InitDatabaseFile()
        {
            var startupPath = FileUtils.GetStartupPath();

            var newUserNameDirPath = $@"{startupPath}/Log/{this.newUserName}";

            var srcDBPath = $@"{startupPath}/Datas/default.sqlite";

            var distDBPath = $@"{newUserNameDirPath}/{this.newUserName}.sqlite";

            File.Copy(srcDBPath, distDBPath);

            using (var connection = new SQLiteConnection(distDBPath))
            {
                connection.Execute($@"UPDATE DataOption SET InputMethod = '{this.selectInputMethod}' WHERE Id = 1;");
            }
        }

        // TextBoxにフォーカスが当たったときに起動
        private void TriggerKeyboard(object sender, EventArgs e)
        {
            try
            {
                OnScreenKeyboard.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // TextBoxをクリックしたときに起動
        private void TextBoxMouseDown(object sender, RoutedEventArgs e)
        {
            if (!OnScreenKeyboard.IsOpened())
            {
                try
                {
                    OnScreenKeyboard.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /*
        // OSKを完全に切ってしまう
        private void CloseOSK()
        {
            if (OnScreenKeyboard.IsOpened())
            {
                try
                {
                    OnScreenKeyboard.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        */
    }
}
