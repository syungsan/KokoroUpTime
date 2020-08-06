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

using System.Reflection;
using System.Diagnostics;
using CsvReadWrite;
using System.IO;
using System.Windows.Media.TextFormatting;
using SQLite;
using System.Linq;
using Microsoft.VisualBasic;
using SQLitePCL;

namespace KokoroUpTime
{
    /// <summary>
    /// TitlePage.xaml の相互作用ロジック
    /// </summary>
    public partial class TitlePage : Page
    {
        //全画面表示か
        private bool isMaximized = false;

        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();
        public DataProgress dataProgress = new DataProgress();

        private string[] dirPaths;

        private bool isFirstBootFlag = true;

        public TitlePage()
        {
            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            this.SelectUserListGrid.Visibility = Visibility.Hidden;
            this.SelectDataListGrid.Visibility = Visibility.Hidden;
            this.CoverLayerImage.Visibility = Visibility.Hidden;
            this.ExitBackGrid.Visibility = Visibility.Hidden;

            // デバッグ用表示 #################################################################
            Assembly asm = Assembly.GetExecutingAssembly(); // 実行中のアセンブリを取得する。

            // AssemblyNameから取得
            AssemblyName asmName = asm.GetName();

            string name = "AssemblyName.Name : " + asmName.Name + "\r\n";
            string version = "AssemblyName.Version : " + asmName.Version.ToString() + "\r\n";
            string fullname = "AssemblyName.FullName : " + asmName.FullName + "\r\n";
            string processor = "AssemblyName.ProcessorArchitecture : " + asmName.ProcessorArchitecture + "\r\n";
            string runtime = "Assembly.ImageRuntimeVersion : " + asm.ImageRuntimeVersion + "\r\n";

            this.VersionTextBlock.Text = name + version + fullname + processor + runtime + "\r\n";
            this.WindowTitle = asmName.Name + " Ver" + asmName.Version.ToString();
            // ################################################################################
        }

        public void SetNextPage(InitConfig _initConfig, DataOption _dataOption, DataItem _dataItem, DataProgress _dataProgress)
        {
            if (!this.isFirstBootFlag)
            {
                this.initConfig = _initConfig;
                this.dataOption = _dataOption;
                this.dataItem = _dataItem;
                this.dataProgress = _dataProgress;
            }
        }

        public void SetIsFirstBootFlag(bool flag)
        {
            this.isFirstBootFlag = flag;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.isFirstBootFlag)
            {
                this.LoadUser();
            }
            else
            {
                this.SetCurrentUserName();
                this.SetCurrentSceneInfo();
            }
        }

        private void LoadUser()
        {
            if (File.Exists("./Log/system.conf"))
            {
                using (var csv = new CsvReader("./Log/system.conf"))
                {
                    var csvs = csv.ReadToEnd();

                    this.CurrentUserTextBlock.Text = $"{csvs[0][0]}{csvs[0][1]}";

                    this.initConfig.userName = csvs[0][0];
                    this.initConfig.userTitle = csvs[0][1];
                }

                string dbName = $"{this.initConfig.userName}.sqlite";
                string userDirPath = $"./Log/{this.initConfig.userName}_{this.initConfig.userTitle}/";

                this.initConfig.dbPath = System.IO.Path.Combine(userDirPath, dbName);

                using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                {
                    var option = connection.Query<DataOption>("SELECT * FROM DataOption WHERE Id = 1;");

                    foreach (var row in option)
                    {
                        this.dataOption.InputMethod = row.InputMethod;

                        this.dataOption.IsPlaySE = row.IsPlaySE;

                        this.dataOption.IsPlayBGM = row.IsPlayBGM;

                        this.dataOption.MessageSpeed = row.MessageSpeed;

                        this.dataOption.IsAddRubi = row.IsAddRubi;
                    }

                    var item = connection.Query<DataItem>("SELECT * FROM DataItem WHERE Id = 1;");

                    foreach (var row in item)
                    {
                        this.dataItem.HasGotItem01 = row.HasGotItem01;

                        this.dataItem.HasGotItem02 = row.HasGotItem02;

                        this.dataItem.HasGotItem03 = row.HasGotItem03;

                        this.dataItem.HasGotItem04 = row.HasGotItem04;

                        this.dataItem.HasGotItem05 = row.HasGotItem05;

                        this.dataItem.HasGotItem06 = row.HasGotItem06;

                        this.dataItem.HasGotItem07 = row.HasGotItem07;

                        this.dataItem.HasGotItem08 = row.HasGotItem08;

                        this.dataItem.HasGotItem09 = row.HasGotItem09;

                        this.dataItem.HasGotItem10 = row.HasGotItem10;

                        this.dataItem.HasGotItem11 = row.HasGotItem11;
                    }

                    var progress = connection.Query<DataProgress>("SELECT * FROM DataProgress WHERE Id = 1;");

                    foreach (var row in progress)
                    {
                        this.dataProgress.CurrentCapter = row.CurrentCapter;

                        this.dataProgress.CurrentScene = row.CurrentScene;

                        this.dataProgress.LatestChapter1Scene = row.LatestChapter1Scene;

                        this.dataProgress.HasCompletedChapter1 = row.HasCompletedChapter1;

                        this.dataProgress.LatestChapter2Scene = row.LatestChapter2Scene;

                        this.dataProgress.HasCompletedChapter2 = row.HasCompletedChapter2;

                        this.dataProgress.LatestChapter3Scene = row.LatestChapter3Scene;

                        this.dataProgress.HasCompletedChapter3 = row.HasCompletedChapter3;

                        this.dataProgress.LatestChapter4Scene = row.LatestChapter4Scene;

                        this.dataProgress.HasCompletedChapter4 = row.HasCompletedChapter4;

                        this.dataProgress.LatestChapter5Scene = row.LatestChapter5Scene;

                        this.dataProgress.HasCompletedChapter5 = row.HasCompletedChapter5;

                        this.dataProgress.LatestChapter6Scene = row.LatestChapter6Scene;

                        this.dataProgress.HasCompletedChapter6 = row.HasCompletedChapter6;

                        this.dataProgress.LatestChapter7Scene = row.LatestChapter7Scene;

                        this.dataProgress.HasCompletedChapter7 = row.HasCompletedChapter7;

                        this.dataProgress.LatestChapter8Scene = row.LatestChapter8Scene;

                        this.dataProgress.HasCompletedChapter8 = row.HasCompletedChapter8;

                        this.dataProgress.LatestChapter9Scene = row.LatestChapter9Scene;

                        this.dataProgress.HasCompletedChapter9 = row.HasCompletedChapter9;

                        this.dataProgress.LatestChapter10Scene = row.LatestChapter10Scene;

                        this.dataProgress.HasCompletedChapter10 = row.HasCompletedChapter10;

                        this.dataProgress.LatestChapter11Scene = row.LatestChapter11Scene;

                        this.dataProgress.HasCompletedChapter11 = row.HasCompletedChapter11;

                        this.dataProgress.LatestChapter12Scene = row.LatestChapter12Scene;

                        this.dataProgress.HasCompletedChapter12 = row.HasCompletedChapter12;
                    }
                }
                this.SetCurrentSceneInfo();
                this.SetCurrentUserName();
            }
            else
            {
                this.CurrentUserTextBlock.Text = "名無しさん";
            }

            foreach (var confPath in new string[2] { $"./Log/{this.initConfig.userName}_{this.initConfig.userTitle}/user.conf", "./Log/system.conf" })
            {
                // if (File.Exists(confPath))
                // {
                    var accessTime = DateTime.Now.ToString();

                    var initConfigs = new List<List<string>>();

                    var initConfig = new List<string>() { this.initConfig.userName, this.initConfig.userTitle, accessTime };

                    initConfigs.Add(initConfig);

                    using (var csv = new CsvWriter(confPath))
                    {
                        csv.Write(initConfigs);
                    }
                // }
            }
        }

        private void SetCurrentUserName()
        {
            string userDirPath = $"./Log/{this.initConfig.userName}_{this.initConfig.userTitle}/";

            if (this.dataOption.InputMethod == 0)
            {
                // 実行ファイルの場所を絶対パスで取得
                var startupPath = FileUtils.GetStartupPath();

                this.CurrentNameImage.Source = new BitmapImage(new Uri($@"{startupPath}/{userDirPath}/name.png", UriKind.Absolute));
                this.CurrentUserTextBlock.Text = this.initConfig.userTitle;
            }
            else if (this.dataOption.InputMethod == 1 || this.dataOption.InputMethod == 2)
            {
                this.CurrentNameImage.Source = null;
                this.CurrentUserTextBlock.Text = $"{this.initConfig.userName}{this.initConfig.userTitle}";
            }
        }

        private void SetCurrentSceneInfo()
        {
            this.CurrentSceneTextBlock.Text = $"第{this.dataProgress.CurrentCapter}回の「{this.dataProgress.CurrentScene}」をプレイ中…。";
        }

        private List<UserInfoItem> LoadUsers()
        {
            List<UserInfoItem> items = new List<UserInfoItem>();

            this.dirPaths = Directory.GetDirectories("./Log/");

            foreach (var dirPath in this.dirPaths)
            {
                if (File.Exists($"{dirPath}/user.conf"))
                {
                    string[] userInfos;

                    int inputMethod = 0;

                    using (var csv = new CsvReader($"{dirPath}/user.conf"))
                    {
                        var csvs = csv.ReadToEnd();
                        userInfos = new string[3] { csvs[0][0], csvs[0][1], csvs[0][2] };
                    }

                    string dbName = $"{userInfos[0]}.sqlite";
                    string dbDirPath = $"./Log/{userInfos[0]}_{userInfos[1]}/";

                    var individualDbPath = System.IO.Path.Combine(dbDirPath, dbName);

                    using (var connection = new SQLiteConnection(individualDbPath))
                    {
                        var option = connection.Query<DataOption>("SELECT InputMethod FROM DataOption WHERE Id = 1;");

                        foreach (var row in option)
                        {
                            inputMethod = row.InputMethod;
                        }
                    }
 
                    if (inputMethod == 0)
                    {
                        var startupPath = FileUtils.GetStartupPath();
                        items.Add(new UserInfoItem() { NameBmpPath = $@"{startupPath}/{dirPath}/name.png", UserInfo = $"{userInfos[1]}, {userInfos[2]}" });
                    }
                    else if (inputMethod == 1 || inputMethod == 2)
                    {
                        items.Add(new UserInfoItem() { NameBmpPath = null, UserInfo = $"{userInfos[0]}{userInfos[1]}, {userInfos[2]}" });
                    }
                }
                else
                {
                    // error;
                }
            }
            return items;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            switch (button.Content.ToString())
            {
                case "Full/Win":

                    Window _mainWindow = Application.Current.MainWindow;

                    this.Maximize(mainWindow: _mainWindow);

                    break;

                case "アイテム図鑑":

                    ItemPage itemPage = new ItemPage();

                    itemPage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                    this.NavigationService.Navigate(itemPage);

                    break;

                case "オプション":

                    OptionPage optionPage = new OptionPage();

                    optionPage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                    this.NavigationService.Navigate(optionPage);

                    break;


                case "なまえ入力":

                    NameInputPage nameInputPage = new NameInputPage();

                    // nameInputPage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                    this.NavigationService.Navigate(nameInputPage);

                    break;

                case "データ出力":

                    if (this.initConfig.userName == null)
                    {
                        MessageBox.Show("データが一つもできていません。\nまずはどれかプレイしてください。");
                    }
                    else
                    {
                        this.SelectDataListBox.ItemsSource = this.LoadUsers();

                        this.CoverLayerImage.Visibility = Visibility.Visible;
                        this.SelectDataListGrid.Visibility = Visibility.Visible;
                    }
                    break;

                case "出力":

                    // 複数選択可能
                    foreach (var item in this.SelectDataListBox.SelectedItems)
                    {
                        var selectedUserDataPath = this.dirPaths[this.SelectDataListBox.Items.IndexOf(item)];
                        // ここからだよん
                    }

                    this.SelectDataListGrid.Visibility = Visibility.Hidden;
                    this.CoverLayerImage.Visibility = Visibility.Hidden;

                    break;

                case "キャンセル":

                    this.SelectDataListGrid.Visibility = Visibility.Hidden;
                    this.CoverLayerImage.Visibility = Visibility.Hidden;

                    break;

                case "なまえ選択":

                    if (this.initConfig.userName == null)
                    {
                        MessageBox.Show("なまえがひとつも登録されていません。\nまずは名前の入力から始めてください。");
                    }
                    else
                    {
                        this.SelectUserListBox.ItemsSource = this.LoadUsers();

                        this.CoverLayerImage.Visibility = Visibility.Visible;
                        this.SelectUserListGrid.Visibility = Visibility.Visible;
                    }
                    break;

                case "OK":

                    // 名前を何でもよいから選択しないと落ちる
                    // 後々デフォルトで先頭が選択された状態で始める
                    var selectedUserNamePath = this.dirPaths[this.SelectUserListBox.SelectedIndex];

                    File.Copy($@"{selectedUserNamePath}/user.conf", @"./Log/system.conf", true);

                    this.LoadUser();

                    this.SelectUserListGrid.Visibility = Visibility.Hidden;
                    this.CoverLayerImage.Visibility = Visibility.Hidden;

                    break;

                case "こころアップタイムとは":

                    AboutPage aboutPage = new AboutPage();

                    aboutPage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                    this.NavigationService.Navigate(aboutPage);

                    break;

                case "終了":

                    this.CoverLayerImage.Visibility = Visibility.Visible;
                    this.ExitBackGrid.Visibility = Visibility.Visible;

                    break;

                case "第1回":

                    Chapter1 chapter1 = new Chapter1();

                    chapter1.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                    this.NavigationService.Navigate(chapter1);

                    break;

                case "第2回":

                    Chapter2 chapter2 = new Chapter2();

                    // chapter2.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                    this.NavigationService.Navigate(chapter2);

                    break;

                case "第3回":

                    Chapter3 chapter3 = new Chapter3();

                    chapter3.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                    this.NavigationService.Navigate(chapter3);

                    break;
            }

            if (button.Name == "ExitBackYesButton")
            {
                Application.Current.Shutdown();
            }

            if (button.Name == "ExitBackNoButton")
            {
                this.ExitBackGrid.Visibility = Visibility.Hidden;
                this.CoverLayerImage.Visibility = Visibility.Hidden;
            }
        }

        //全画面表示にする
        public void Maximize(Window mainWindow)
        {
            if (!isMaximized)
            {
                mainWindow.ShowActivated = true;
                mainWindow.Topmost = true;
                mainWindow.ShowInTaskbar = false;
                mainWindow.WindowStyle = WindowStyle.None;
                mainWindow.ResizeMode = ResizeMode.NoResize;
                mainWindow.Left = 0;
                mainWindow.Top = 0;
                mainWindow.Width = SystemParameters.VirtualScreenWidth;
                mainWindow.Height = SystemParameters.VirtualScreenHeight;
                // mainWindow.Cursor = Cursors.None;
                mainWindow.WindowState = WindowState.Maximized;

                isMaximized = true;
            }
            else if (isMaximized)
            {
                mainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                mainWindow.ResizeMode = ResizeMode.CanResize;
                mainWindow.Topmost = false;

                isMaximized = false;
            }
        }
    }

    public class UserInfoItem
    {
        public string UserInfo { get; set; }
        public string NameBmpPath { get; set; }
    }
}
