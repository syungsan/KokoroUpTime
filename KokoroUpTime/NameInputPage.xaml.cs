using CsvReadWrite;
using Expansion;
using FileIOUtils;
using Osklib;
using Osklib.Wpf;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace KokoroUpTime
{
    /// <summary>
    /// GameWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class NameInputPage : Page
    {
        private float INIT_MESSAGE_SPEED = 30.0f;

        private bool IS_3_SECOND_RULE = true;

        private float THREE_SECOND_RULE_TIME = 3.0f;

        private string[] EDIT_BUTTON = { "えんぴつ", "けしごむ", "すべてけす", "かんせい" };

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

        private Dictionary<string, List<Run>> runs = new Dictionary<string, List<Run>>();
        private Dictionary<string, List<InlineUIContainer>> imageInlines = new Dictionary<string, List<InlineUIContainer>>();

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

            #if DEBUG
                //Debug時の設定
                this.INIT_MESSAGE_SPEED = 100000.0f;
                this.IS_3_SECOND_RULE = false;
            #endif


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

                        var gridObjectName = gridObject.Name;

                        this.ShowAnime(storyBoard: gridStoryBoard,objectName:gridObjectName, isSync: gridAnimeIsSync);
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

                        var imageObjectName = imageObject.Name;

                        this.ShowAnime(storyBoard: imageStoryBoard,objectName:imageObjectName, isSync: imageAnimeIsSync);
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

                        this.ShowAnime(storyBoard: buttonStoryBoard,objectName:buttonObjectName, isSync: buttonAnimeIsSync);
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

                        var _messages = this.SequenceCheck(_message);

                        this.ShowSentence(textObject: _textObject, sentences: _messages, mode: "msg");
                    }
                    else
                    {
                        var _messages = this.SequenceCheck(_textObject.Text);

                        // xamlに直接書いたStaticな文章を表示する場合
                        this.ShowSentence(textObject: _textObject, sentences: _messages, mode: "msg");
                    }
                    break;

                // 流れない文字に対するTextBlock処理
                case "text":

                    this.position = this.scenarios[this.scenarioCount][1];

                    var __textObject = this.textBlockObjects[this.position];

                    __textObject.Visibility = Visibility.Visible;

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        var _text = this.scenarios[this.scenarioCount][2];

                        var _texts = this.SequenceCheck(_text);

                        this.ShowSentence(textObject: __textObject, sentences: _texts, mode: "text");
                    }
                    else
                    {
                        var _texts = this.SequenceCheck(__textObject.Text);

                        // xamlに直接書いたStaticな文章を表示する場合
                        this.ShowSentence(textObject: __textObject, sentences: _texts, mode: "text");
                    }

                    string textAnimeIsSync = "sync";

                    // テキストに対するアニメも一応用意
                    if (this.scenarios[this.scenarioCount].Count > 5 && this.scenarios[this.scenarioCount][5] != "")
                    {
                        textAnimeIsSync = this.scenarios[this.scenarioCount][5];
                    }

                    if (this.scenarios[this.scenarioCount].Count > 4 && this.scenarios[this.scenarioCount][4] != "")
                    {
                        var textStoryBoard = this.scenarios[this.scenarioCount][4];

                        var textObjectName = __textObject.Name;

                        this.ShowAnime(storyBoard: textStoryBoard,objectName:textObjectName, isSync: textAnimeIsSync);
                    }
                    else
                    {
                        this.scenarioCount += 1;
                        this.ScenarioPlay();
                    }
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

                    if (this.scenarios[this.scenarioCount].Count > 2 && this.scenarios[this.scenarioCount][2] != "")
                    {
                        if (this.scenarios[this.scenarioCount][2] == "disable_click")
                        {
                            this.isClickable = false;
                        }
                    }
                    else
                    {
                        this.isClickable = true;
                    }
                    break;

                // ボタン押下待ち
                case "click":

                    string clickMethod = "";

                    if (this.scenarios[this.scenarioCount].Count > 1 && this.scenarios[this.scenarioCount][1] != "")
                    {
                        clickMethod = this.scenarios[this.scenarioCount][1];
                    }

                    if (IS_3_SECOND_RULE)
                    {
                        DispatcherTimer waitTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(THREE_SECOND_RULE_TIME) };

                        if (clickMethod == "next_only")
                        {
                            waitTimer.Start();

                            waitTimer.Tick += (s, args) =>
                            {
                                waitTimer.Stop();
                                waitTimer = null;

                                this.NextMessageButton.Visibility = Visibility.Visible;
                            };
                        }
                        else
                        {
                            waitTimer.Start();

                            waitTimer.Tick += (s, args) =>
                            {
                                waitTimer.Stop();
                                waitTimer = null;

                                this.NextMessageButton.Visibility = Visibility.Visible;
                                this.BackMessageButton.Visibility = Visibility.Visible;
                            };
                        }
                    }
                    else
                    {
                        if (clickMethod == "next_only")
                        {
                            this.NextMessageButton.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this.NextMessageButton.Visibility = Visibility.Visible;
                            this.BackMessageButton.Visibility = Visibility.Visible;
                        }
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

                case "sub":

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
            }
        }

        private List<List<string>> SequenceCheck(string text)
        {
            Dictionary<string, string> imageOrTextDic = new Dictionary<string, string>()
            {
                {"name", this.newUserName},
                {"dumy", "dumyText"}
            };

            text = text.Replace("【くん／ちゃん／さん】", this.selectUserTitle);

            // 苦悶の改行処理（文章中の「鬱」を疑似改行コードとする）
            text = text.Replace("鬱", "\u2028");

            if (this.selectInputMethod == 1)
            {
                MatchCollection imageOrTextTags = null;

                foreach (string imageOrTextKey in imageOrTextDic.Keys)
                {
                    switch (imageOrTextKey)
                    {
                        case "name":

                            imageOrTextTags = new Regex(@"\<image=name\>(.*?)\<\/image\>").Matches(text);

                            break;

                        default:
                            break;
                    }

                    if (imageOrTextTags != null)
                    {
                        foreach (Match imageOrTextTag in imageOrTextTags)
                        {
                            text = text.Replace(imageOrTextTag.Value, imageOrTextDic[imageOrTextKey]);
                        }
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

        private void ShowSentence(TextBlock textObject, List<List<string>> sentences, string mode)
        {
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
                this.msgTimer.Interval = TimeSpan.FromSeconds(1.0f / INIT_MESSAGE_SPEED);
                this.msgTimer.Start();
            }

            textObject.Inlines.Clear();


            // 画像インラインと文字インラインの合体
            foreach (var stns in sentences)
            {
                string namePngPath = $"./temp/temp_name.png";

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
                            case "yellow": { textDecoration.Pen = new Pen(Brushes.Yellow, 1); break; };
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

                    string namePngPath = $"./temp/temp_name.png";

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

                    string namePngPath = $"./temp/temp_name.png";

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
        private void ShowAnime(string storyBoard,string objectName, string isSync)
        {
            Storyboard sb = this.FindResource(storyBoard) as Storyboard;

            foreach (var child in sb.Children)
                Storyboard.SetTargetName(child, objectName);

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
        private void GoTo(string tag)
        {
            foreach (var (scenario, index) in this.scenarios.Indexed())
            {
                if (scenario[0] == "sub" && scenario[1] == tag)
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
                this.isClickable = false;

                if (button.Name == "BackMessageButton")
                {
                    this.BackMessageButton.Visibility = Visibility.Hidden;
                    this.NextMessageButton.Visibility = Visibility.Hidden;

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

                if (button.Name == "NextMessageButton")
                {
                    this.BackMessageButton.Visibility = Visibility.Hidden;
                    this.NextMessageButton.Visibility = Visibility.Hidden;

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }

                if (button.Name == "NameButton")
                {
                    if (this.selectInputMethod == 0)
                    {
                        this.NameInputGrid.Visibility = Visibility.Hidden;
                        this.CanvasGrid.Visibility = Visibility.Visible;
                    }
                }

                if (button.Name == "DecisionButton")
                {

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

                        string[] NG_WORD = { "\\","/", ":", "*","?","<",">","|" };

                        foreach (string word in NG_WORD)
                        {
                            if (this.newUserName.Contains(word))
                            {
                                MessageBox.Show("つぎの文字は名前には使えません。\n　\\ / : * ? < > |", "情報");

                                this.isClickable = true;

                                this.NameTextBox.SelectAll();

                                return;
                            }

                        }
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

                        this.CloseOSK();
                    }

                    this.NameInputGrid.Visibility = Visibility.Hidden;

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }

                if (button.Name == "SelectInputMethodOKButton")
                {
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
                        this.NameTextBox.Text = BASE_USER_NAME;
                        this.NameTextBox.Visibility = Visibility.Visible;
                    }
                    this.SelectInputMethodGrid.Visibility = Visibility.Hidden;

                    this.scenarioCount += 1;
                    this.ScenarioPlay();
                }

                if (button.Name == "ReturnToTitleButton")
                {
                    if (Directory.Exists("./temp"))
                    {
                        Directory.Delete("./temp", true);
                    }

                    TitlePage titlePage = new TitlePage();

                    titlePage.SetReloadPageFlag(true);

                    this.NavigationService.Navigate(titlePage);
                }

                if (button.Name == "SelectUserTitleOKButton")
                {
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

                    GoTo("complete");
                }

                if (button.Name == "MakeUserNameCompleteNOButton")
                {
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

                    GoTo("make_name");
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

        // TextBoxをクリックしたときに起動
        private void TextBoxMouseDown(object sender, RoutedEventArgs e)
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

        private void TriggerKeyboard(object sender, RoutedEventArgs e)
        {

        }
    }
}
