using CsvReadWrite;
using Expansion;
using FileIOUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using WMPLib;

namespace KokoroUpTime
{
    /// <summary>
    /// EndingPage.xaml の相互作用ロジック
    /// </summary>
    public partial class EndingPage : Page
    {
        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();
        public DataProgress dataProgress = new DataProgress();

        private WindowsMediaPlayer mediaPlayer;

        private int contentsCount = 0;
        private List<List<string>> staffs = null;

        private double playTime = 0.0;
        private double margin = 0.0;

        private List<List<double>> illustMargins;

        private List<string> endingIllusts;

        private double changeIllustTimeSpan = 0.0;
        private int illustNumber = 0;

        private double lastTextUnderPosY = 0.0;

        private List<TextBlock> textBlocks = new List<TextBlock>();

        private List<double> scrollFirstPosYs = new List<double>();

        public EndingPage()
        {
            InitializeComponent();

            this.ShowsNavigationUI = false;

            this.mediaPlayer = new WindowsMediaPlayer();

            this.endingIllusts = new List<string>();

            this.illustMargins = new List<List<double>>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.lastTextUnderPosY = this.StaffRollGrid.ActualHeight;

            using (var csv = new CsvReader("./Datas/ending.csv"))
            {
                this.staffs = csv.ReadToEnd();
            }
            this.MakeContents();
        }

        public void SetNextPage(InitConfig _initConfig, DataOption _dataOption, DataItem _dataItem, DataProgress _dataProgress)
        {
            this.initConfig = _initConfig;
            this.dataOption = _dataOption;
            this.dataItem = _dataItem;
            this.dataProgress = _dataProgress;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button.Name == "SkipButton")
            {
                this.StopBGM();

                Storyboard sb = this.FindResource("ending_fade_out") as Storyboard;
                sb.Stop();

                this.EndingGrid.Visibility = Visibility.Hidden;
            }

            if (button.Name == "ReturnToTitleButton")
            {
                TitlePage titlePage = new TitlePage();

                titlePage.SetReloadPageFlag(false);

                titlePage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                this.NavigationService.Navigate(titlePage);
            }
        }

        private void SetBGM(string soundFile, int volume)
        {
            var startupPath = FileUtils.GetStartupPath();

            this.mediaPlayer.settings.setMode("loop", false);

            this.mediaPlayer.URL = $@"{startupPath}/Sounds/{soundFile}";

            this.mediaPlayer.settings.volume = volume;
        }

        private void PlayBGM()
        {
            this.mediaPlayer.controls.play();
        }

        private void StopBGM()
        {
            this.mediaPlayer.controls.stop();
        }

        private void MakeContents()
        {
            Debug.Print((this.contentsCount + 1).ToString());

            var tag = this.staffs[this.contentsCount][0];

            switch (tag)
            {
                case "time":

                    this.playTime = double.Parse(this.staffs[this.contentsCount][1]);

                    this.contentsCount += 1;
                    this.MakeContents();

                    break;


                case "bgm":

                    if (this.dataOption.IsPlayBGM)
                    {
                        var bgmStatus = this.staffs[this.contentsCount][1];

                        switch (bgmStatus)
                        {
                            case "set":

                                var bgmFile = this.staffs[this.contentsCount][2];
                                var bgmVolume = int.Parse(this.staffs[this.contentsCount][3]);
                                this.SetBGM(soundFile: bgmFile, volume: bgmVolume);

                                break;

                            case "play":

                                this.PlayBGM();
                                break;

                            case "stop":

                                this.StopBGM();
                                break;
                        }
                        this.contentsCount += 1;
                        this.MakeContents();
                    }
                    else
                    {
                        this.contentsCount += 1;
                        this.MakeContents();
                    }
                    break;

                case "illust":

                    this.endingIllusts.Add(this.staffs[this.contentsCount][1]);

                    List<double> margins = new List<double> { double.Parse(this.staffs[this.contentsCount][2]), double.Parse(this.staffs[this.contentsCount][3])};
                    this.illustMargins.Add(margins);

                    this.contentsCount += 1;
                    this.MakeContents();

                    break;

                case "margin":

                    this.margin = double.Parse(this.staffs[this.contentsCount][1]);

                    this.contentsCount += 1;
                    this.MakeContents();

                    break;

                case "text":

                    var text = this.staffs[this.contentsCount][1];
                    var fontSize = double.Parse(this.staffs[this.contentsCount][2]);
                    var fontHexColor = this.staffs[this.contentsCount][3];
                    var fontFamily = this.staffs[this.contentsCount][5];

                    var posY = this.lastTextUnderPosY + (this.margin * double.Parse(this.staffs[this.contentsCount][4]));
                    this.scrollFirstPosYs.Add(posY);

                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = text;
                    textBlock.FontSize = fontSize;
                    textBlock.TextAlignment = TextAlignment.Center;

                    Color fontColor = (Color)ColorConverter.ConvertFromString(fontHexColor);
                    textBlock.Foreground = new SolidColorBrush(fontColor);

                    textBlock.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), $"./Fonts/{fontFamily}");

                    DropShadowEffect dropShadowEffect = new DropShadowEffect();

                    dropShadowEffect.Color = Colors.Gray;
                    dropShadowEffect.Direction = 320;
                    dropShadowEffect.ShadowDepth = 10;
                    dropShadowEffect.BlurRadius = 10;
                    dropShadowEffect.Opacity = 0.5;

                    textBlock.Effect = dropShadowEffect;

                    this.StaffRollGrid.Children.Add(textBlock);

                    textBlock.RenderTransform = new TranslateTransform(0, posY);

                    this.lastTextUnderPosY = posY + fontSize;

                    this.textBlocks.Add(textBlock);

                    this.contentsCount += 1;
                    this.MakeContents();

                    break;

                case "run":

                    this.Scroll();

                    this.changeIllustTimeSpan = this.playTime / this.endingIllusts.Count;

                    this.ChangeImage();

                    break;
            }
        }

        private void Scroll()
        {
            foreach (var (textBlock, index) in this.textBlocks.Indexed())
            {
                double currentPosY = this.scrollFirstPosYs[index];

                double newPosY = currentPosY - this.lastTextUnderPosY + (this.StaffRollGrid.ActualHeight * 0.5);

                TranslateTransform trans = new TranslateTransform();

                textBlock.RenderTransform = trans;

                DoubleAnimation anim = new DoubleAnimation(currentPosY, newPosY, TimeSpan.FromSeconds(this.playTime));

                if (index == this.textBlocks.Count - 1)
                {
                    anim.Completed += new EventHandler(this.EndProcess);
                }
                trans.BeginAnimation(TranslateTransform.YProperty, anim);
            }
        }

        private void EndProcess(object sender, EventArgs e)
        {
            Storyboard sb = this.FindResource("ending_fade_out") as Storyboard;

            if (sb != null)
            {
                sb.Completed += (s, e) =>
                {
                    this.StopBGM();
                    this.EndingGrid.Visibility = Visibility.Hidden;
                };
                sb.Begin(this);
            }
        }

        private void ChangeImage()
        {
            // Debug.Print(this.endingIllusts[this.illustNumber]);

            this.EndingImage.Source = new BitmapImage(new Uri($"./Images/{this.endingIllusts[this.illustNumber]}", UriKind.Relative));

            this.EndingImage.Margin = new Thickness(this.illustMargins[this.illustNumber][0], this.illustMargins[this.illustNumber][1], 0, 0);

            Storyboard sb = this.FindResource("image_fade_in_out") as Storyboard;

            foreach (Timeline timeline in sb.Children)
            {
                IKeyFrameAnimation keyframeTimeline = timeline as IKeyFrameAnimation;

                if (keyframeTimeline != null)
                {
                    int index = 0;

                    foreach (IKeyFrame keyframe in keyframeTimeline.KeyFrames)
                    {
                        //Do something with your keyframe, for example change its KeyTime to control  
                        //speed of animation

                        if (index == 2 && this.illustNumber < this.endingIllusts.Count - 1)
                        {
                            keyframe.KeyTime =TimeSpan.FromSeconds(this.changeIllustTimeSpan - 1.0);
                        }
                        else if (index == 2 && this.illustNumber >= this.endingIllusts.Count - 1)
                        {
                            // ending_fadeoutが8秒かかるのでその間は表示
                            keyframe.KeyTime = TimeSpan.FromSeconds(this.changeIllustTimeSpan + 8.0);
                        }

                        if (index == 3 && this.illustNumber < this.endingIllusts.Count - 1)
                        {
                            keyframe.KeyTime = TimeSpan.FromSeconds(this.changeIllustTimeSpan);
                        }
                        else if (index == 3 && this.illustNumber >= this.endingIllusts.Count - 1)
                        {
                            // ending_fadeoutが8秒かかるのでその間は表示
                            keyframe.KeyTime = TimeSpan.FromSeconds(this.changeIllustTimeSpan + 8.0);
                        }

                        index += 1;
                    }
                }
            }

            if (sb != null)
            {
                bool isDuplicate = false;

                sb.Completed += (s, e) =>
                {
                    if (!isDuplicate)
                    {
                        this.illustNumber += 1;

                        if (this.illustNumber < this.endingIllusts.Count)
                        {
                            this.ChangeImage();
                        }
                        isDuplicate = true;
                    }
                };
                sb.Begin(this);
            }
            sb = null;
        }
    }
}
