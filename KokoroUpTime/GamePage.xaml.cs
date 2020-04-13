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


    
namespace KokoroUpTime
{
    /// <summary>
    /// GameWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class GamePage : Page
    {
        private int scenarioCount = 0;
        private List<List<string>> scenarios = null;

        private string charactor = "";

        public GamePage()
        {
            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            this.BigBubble.Visibility = Visibility.Hidden;
            this.BigSpeech.Visibility = Visibility.Hidden;

            this.NextMessageButton.Visibility = Visibility.Hidden;
            this.BackMessageButton.Visibility = Visibility.Hidden;

            this.scenarios = this.LoadScenario("Scenarios/chapter1.csv");
            this.ScenarioPlay();
        }

        // CSVから2次元配列へシナリオデータの収納（CsvReaderクラスを使用）
        List<List<string>> LoadScenario(string filePath)
        {
            using (var csv = new CsvReader(filePath))
            {
                this.scenarios = csv.ReadToEnd();
            }
            return scenarios;
        }

        // ゲーム進行の中核
        void ScenarioPlay()
        {
            var tag = this.scenarios[this.scenarioCount][0];

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

                    Storyboard sb = this.FindResource("Appearance") as Storyboard;
                    if (sb != null)
                    {
                        // アニメが終わってから次の処理
                        sb.Completed += (s, e) => {

                            this.scenarioCount += 1;
                            this.ScenarioPlay();
                        };
                        sb.Begin(this);
                    }
                    break;

                case "msg":

                    this.BigBubble.Visibility = Visibility.Visible;
                    this.BigSpeech.Visibility = Visibility.Visible;

                    this.NextMessageButton.Visibility = Visibility.Hidden;
                    this.BackMessageButton.Visibility = Visibility.Hidden;

                    // メッセージ表示関連
                    DispatcherTimer msgTimer;
                    string message = "";
                    int word_num = 0;

                    // 後々動的パラメータに
                    float messageSpeed = 30.0f;

                    this.charactor = this.scenarios[this.scenarioCount][1];
                    var _message = this.scenarios[this.scenarioCount][2];

                    // 苦悶の改行処理（文章中の「鬱」を疑似改行コードとする）
                    message = _message.Replace("鬱", "\u2028");

                    // メッセージ表示処理
                    msgTimer = new DispatcherTimer();
                    msgTimer.Tick += ViewMsg;
                    msgTimer.Interval = TimeSpan.FromSeconds(1.0f / messageSpeed);
                    msgTimer.Start();

                    // 一文字ずつメッセージ表示（Inner Func）
                    void ViewMsg(object sender, EventArgs e)
                    {
                        this.BigSpeech.Text = message.Substring(0, word_num);

                        if (word_num < message.Length)
                        {
                            word_num++;
                        }
                        else
                        {
                            msgTimer.Stop();
                            msgTimer = null;

                            this.scenarioCount += 1;
                            this.ScenarioPlay();
                        }
                    }
                    break;

                case "wait":

                    this.NextMessageButton.Visibility = Visibility.Visible;

                    if (this.scenarios[this.scenarioCount - 1] != null)
                    {
                        this.BackMessageButton.Visibility = Visibility.Visible;
                    }
                    break;
            }
        }

        private void NextMessageButton_Click(object sender, RoutedEventArgs e)
        {
            this.scenarioCount += 1;
            this.ScenarioPlay();
        }

        private void BackMessageButton_Click(object sender, RoutedEventArgs e)
        {
            this.scenarioCount -= 1;
            this.ScenarioPlay();
            // 連続Backの実現にはもっと複雑な処理がいる
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
