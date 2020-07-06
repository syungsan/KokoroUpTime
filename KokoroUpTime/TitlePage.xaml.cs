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

        private string[] dirPaths;

        private bool isFirstBootFlag = true;

        public TitlePage()
        {
            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            this.SelectUserListGrid.Visibility = Visibility.Hidden;
            this.CoverLayerImage.Visibility = Visibility.Hidden;

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

        public void SetInitConfig(InitConfig _initConfig)
        {
            if (!this.isFirstBootFlag)
            {
                this.initConfig = _initConfig;
            }
        }

        public void SetDataOption(DataOption _dataOption)
        {
            if (!this.isFirstBootFlag)
            {
                this.dataOption = _dataOption;
            }  
        }

        public void SetDataItem(DataItem _dataItem)
        {
            if (!this.isFirstBootFlag)
            {
                this.dataItem = _dataItem;
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
                this.setCurrentUserName();
            }
        }

        void LoadUser()
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
                        this.dataOption.IsWordRecognition = row.IsWordRecognition;

                        this.dataOption.IsPlaySE = row.IsPlaySE;

                        this.dataOption.IsPlayBGM = row.IsPlayBGM;

                        this.dataOption.MessageSpeed = row.MessageSpeed;

                        this.dataOption.IsAddRubi = row.IsAddRubi;

                        this.dataOption.CreatedAt = row.CreatedAt;
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
                }
                this.setCurrentUserName();
            }
            else
            {
                this.CurrentUserTextBlock.Text = "名無しさん";
            }

            foreach (var confPath in new string[2] { $"./Log/{this.initConfig.userName}_{this.initConfig.userTitle}/user.conf", "./Log/system.conf" })
            {
                if (File.Exists(confPath))
                {
                    var accessTime = DateTime.Now.ToString();

                    var initConfigs = new List<List<string>>();

                    var initConfig = new List<string>() { this.initConfig.userName, this.initConfig.userTitle, accessTime };

                    initConfigs.Add(initConfig);

                    using (var csv = new CsvWriter(confPath))
                    {
                        csv.Write(initConfigs);
                    }
                }
            }
        }

        void setCurrentUserName()
        {
            string userDirPath = $"./Log/{this.initConfig.userName}_{this.initConfig.userTitle}/";

            if (!this.dataOption.IsWordRecognition)
            {
                // 実行ファイルの場所を絶対パスで取得
                var startupPath = FileUtils.GetStartupPath();

                this.CurrentNameImage.Source = new BitmapImage(new Uri($@"{startupPath}/{userDirPath}/Name.bmp", UriKind.Absolute));
                this.CurrentUserTextBlock.Text = this.initConfig.userTitle;
            }
            else
            {
                this.CurrentNameImage.Source = null;
                this.CurrentUserTextBlock.Text = $"{this.initConfig.userName}{this.initConfig.userTitle}";
            }
        }

        void LoadUsers()
        {
            List<UserInfoItem> items = new List<UserInfoItem>();

            this.dirPaths = Directory.GetDirectories("./Log/");

            foreach (var dirPath in this.dirPaths)
            {
                if (File.Exists($"{dirPath}/user.conf"))
                {
                    string[] userInfo;

                    bool isWordRecognition = false;

                    using (var csv = new CsvReader($"{dirPath}/user.conf"))
                    {
                        var csvs = csv.ReadToEnd();
                        userInfo = new string[3] { csvs[0][0], csvs[0][1], csvs[0][2] };
                    }

                    string dbName = $"{userInfo[0]}.sqlite";
                    string dbDirPath = $"./Log/{userInfo[0]}_{userInfo[1]}/";

                    var individualDbPath = System.IO.Path.Combine(dbDirPath, dbName);

                    using (var connection = new SQLiteConnection(individualDbPath))
                    {
                        var option = connection.Query<DataOption>("SELECT IsWordRecognition FROM DataOption WHERE Id = 1;");

                        foreach (var row in option)
                        {
                            isWordRecognition = row.IsWordRecognition;
                        }
                    }
 
                    if (!isWordRecognition)
                    {
                        var startupPath = FileUtils.GetStartupPath();
                        items.Add(new UserInfoItem() { NameBmpPath = $@"{startupPath}/{dirPath}/Name.bmp", UserInfo = $"{userInfo[1]}, {userInfo[2]}" });
                    }
                    else
                    {
                        items.Add(new UserInfoItem() { NameBmpPath = null, UserInfo = $"{userInfo[0]}{userInfo[1]}, {userInfo[2]}" });
                    }
                }
                else
                {
                    // error;
                }
            }
            this.SelectUserListBox.ItemsSource = items;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button.Content.ToString() == "Full/Win")
            {
                Window _mainWindow = Application.Current.MainWindow;

                this.Maximize(mainWindow: _mainWindow);
            }
            else if (button.Content.ToString() == "アイテム図鑑")
            {
                ItemPage nextPage = new ItemPage();

                nextPage.SetInitConfig(this.initConfig);
                nextPage.SetDataOption(this.dataOption);
                nextPage.SetDataItem(this.dataItem);

                this.NavigationService.Navigate(nextPage);
            }
            else if (button.Content.ToString() == "オプション")
            {
                OptionPage nextPage = new OptionPage();

                nextPage.SetInitConfig(this.initConfig);
                nextPage.SetDataOption(this.dataOption);
                nextPage.SetDataItem(this.dataItem);

                this.NavigationService.Navigate(nextPage);
            }
            else if (button.Content.ToString() == "名前入力")
            {

            }
            else if (button.Content.ToString() == "なまえ選択")
            {
                if (this.initConfig.userName == null)
                {
                    MessageBox.Show("なまえがひとつも登録されていません。\nまずは名前の入力から始めてください。");
                }
                else
                {
                    this.LoadUsers();

                    this.CoverLayerImage.Visibility = Visibility.Visible;
                    this.SelectUserListGrid.Visibility = Visibility.Visible;
                }
            }
            else if (button.Content.ToString() == "OK")
            {
                // 名前を何でもよいから選択しないと落ちる
                // 後々デフォルトで先頭が選択された状態で始める
                var newUserNamePath = this.dirPaths[this.SelectUserListBox.SelectedIndex];

                File.Copy($@"{newUserNamePath}/user.conf", @"./Log/system.conf", true);

                this.LoadUser();

                this.SelectUserListGrid.Visibility = Visibility.Hidden;
                this.CoverLayerImage.Visibility = Visibility.Hidden;
            }
            else
            {
                // Pageインスタンスを渡して遷移
                switch (button.Content.ToString())
                {
                    case "第1回":

                        Chapter1 nextPage1 = new Chapter1();

                        nextPage1.SetInitConfig(this.initConfig);
                        nextPage1.SetDataOption(this.dataOption);
                        nextPage1.SetDataItem(this.dataItem);

                        nextPage1.SetScenario("./Scenarios/chapter1.csv");
                        
                        this.NavigationService.Navigate(nextPage1);

                        break;

                    case "第2回":

                        Chapter2 nextPage2 = new Chapter2();
                        nextPage2.SetScenario("./Scenarios/chapter2.csv");

                        this.NavigationService.Navigate(nextPage2);

                        break;
                }
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
