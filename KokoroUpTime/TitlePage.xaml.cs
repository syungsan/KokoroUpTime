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
using CsvReadWrite;
using System.IO;
using SQLite;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using FileIOUtils;

namespace KokoroUpTime
{
    /// <summary>
    /// TitlePage.xaml の相互作用ロジック
    /// </summary>
    public partial class TitlePage : Page
    {
        private string BASE_USER_NAME = "名無しさん";

        //全画面表示か
        private bool isMaximized = true;

        // ページ間参照変数橋渡し
        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();
        public DataProgress dataProgress = new DataProgress();

        private string[] dirPaths;

        // 初回アクセスかどうかのフラグ
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
            this.CreditGrid.Visibility = Visibility.Hidden;

            // デバッグ用表示 #################################################################
            Assembly asm = Assembly.GetExecutingAssembly(); // 実行中のアセンブリを取得する。

            // AssemblyNameから取得
            AssemblyName asmName = asm.GetName();

            // string name = "AssemblyName.Name : " + asmName.Name + "\r\n";
            // string version = "AssemblyName.Version : " + asmName.Version.ToString() + "\r\n";
            // string fullname = "AssemblyName.FullName : " + asmName.FullName + "\r\n";
            // string processor = "AssemblyName.ProcessorArchitecture : " + asmName.ProcessorArchitecture + "\r\n";
            // string runtime = "Assembly.ImageRuntimeVersion : " + asm.ImageRuntimeVersion + "\r\n";

            // this.VersionTextBlock.Text = name + version + fullname + processor + runtime + "\r\n";
            this.VersionTextBlock.Text = "Version " + asmName.Version.ToString();

            this.WindowTitle = asmName.Name + " Ver" + asmName.Version.ToString();
            // ################################################################################
        }

        // ページ間参照橋渡し関数
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

        // 他ページから制御するフラグ
        public void SetIsFirstBootFlag(bool flag)
        {
            this.isFirstBootFlag = flag;
        }

        // xamlをロードしきったら発火
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists("./Log/system.conf"))
            {
                if (this.isFirstBootFlag)
                {
                    this.LoadCurrentUser();
                }
                else
                {
                    this.SetCurrentUserName();
                    this.SetCurrentSceneInfo();
                }
            }
            else
            {
                this.CurrentUserTextBlock.Text = BASE_USER_NAME;
                this.SetCurrentSceneInfo();
            }
        }

        // 現在のユーザが選択されたら情報をロード
        private void LoadCurrentUser()
        {
            using (var csv = new CsvReader("./Log/system.conf"))
            {
                var csvs = csv.ReadToEnd();

                this.initConfig.userName = csvs[0][0];
                this.initConfig.userTitle = csvs[0][1];
                this.initConfig.accessDateTime = csvs[0][2];

                // this.CurrentUserTextBlock.Text = $"{this.initConfig.userName}{this.initConfig.userTitle}";
            }

            string dbName = $"{this.initConfig.userName}.sqlite";
            string userDirPath = $"./Log/{this.initConfig.userName}/";

            this.initConfig.dbPath = System.IO.Path.Combine(userDirPath, dbName);

            // データベースよりロード
            using (var connection = new SQLiteConnection(this.initConfig.dbPath))
            {
                // オプション関係
                var option = connection.Query<DataOption>("SELECT * FROM DataOption WHERE Id = 1;");

                foreach (var row in option)
                {
                    this.dataOption.InputMethod = row.InputMethod;

                    this.dataOption.IsPlaySE = row.IsPlaySE;

                    this.dataOption.IsPlayBGM = row.IsPlayBGM;

                    this.dataOption.MessageSpeed = row.MessageSpeed;

                    this.dataOption.IsAddRubi = row.IsAddRubi;
                }

                // アイテム関係
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

                // 進捗情報関係
                var progress = connection.Query<DataProgress>("SELECT * FROM DataProgress WHERE Id = 1;");

                foreach (var row in progress)
                {
                    this.dataProgress.CurrentChapter = row.CurrentChapter;

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
            this.SetCurrentUserName();
            this.SetCurrentSceneInfo();
            
            // アプリを立ち上げたときに常に選択ユーザのアクセスタイムを記録するのか
            foreach (var confPath in new string[2] { $"./Log/{this.initConfig.userName}/user.conf", "./Log/system.conf" })
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

        // タイトルの現在のユーザを表示
        private void SetCurrentUserName()
        {
            string userDirPath = $"./Log/{this.initConfig.userName}/";

            // 実行ファイルの場所を絶対パスで取得
            var startupPath = FileUtils.GetStartupPath();

            if (File.Exists($@"{startupPath}/{userDirPath}/name.png"))
            {
                var bitmap = new BitmapImage();

                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;    //ココ
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;  //ココ
                bitmap.UriSource = new Uri($@"{startupPath}/{userDirPath}/name.png", UriKind.Absolute);
                bitmap.EndInit();

                bitmap.Freeze();                                  //ココ

                this.CurrentNameImage.Source = null;
                this.CurrentNameImage.Source = bitmap;

                this.CurrentUserTextBlock.Text = this.initConfig.userTitle;
            }
            else
            {
                this.CurrentNameImage.Source = null;
                this.CurrentUserTextBlock.Text = $"{this.initConfig.userName}{this.initConfig.userTitle}";
            }
        }

        // タイトルの現在のユーザの最近のプレイシーンを表示
        private void SetCurrentSceneInfo()
        {
            if (this.dataProgress.CurrentChapter == 0 && this.dataProgress.CurrentScene == null)
            {
                this.CurrentSceneTextBlock.Text = "まだ何もプレイしてません…。";
            }
            else
            {
                this.CurrentSceneTextBlock.Text = $"第{this.dataProgress.CurrentChapter}回の「{this.dataProgress.CurrentScene}」をプレイ中…。";
            }  
        }

        // 登録中のすべてのユーザ情報をロード
        private List<UserInfoItem> LoadAnyUsers()
        {
            List<UserInfoItem> items = new List<UserInfoItem>();

            this.dirPaths = Directory.GetDirectories("./Log/");

            foreach (var dirPath in this.dirPaths)
            {
                if (File.Exists($"{dirPath}/user.conf"))
                {
                    string[] userInfos;

                    // int inputMethod = 0;

                    using (var csv = new CsvReader($"{dirPath}/user.conf"))
                    {
                        var csvs = csv.ReadToEnd();
                        userInfos = new string[3] { csvs[0][0], csvs[0][1], csvs[0][2] };
                    }

                    /*
                    string dbName = $"{userInfos[0]}.sqlite";
                    string dbDirPath = $"./Log/{userInfos[0]}/";

                    var individualDbPath = System.IO.Path.Combine(dbDirPath, dbName);

                    using (var connection = new SQLiteConnection(individualDbPath))
                    {
                        var option = connection.Query<DataOption>("SELECT InputMethod FROM DataOption WHERE Id = 1;");

                        foreach (var row in option)
                        {
                            inputMethod = row.InputMethod;
                        }
                    }
                    */

                    var startupPath = FileUtils.GetStartupPath();

                    if (File.Exists($@"{startupPath}/{dirPath}/name.png"))
                    {
                        items.Add(new UserInfoItem() { NameBmpPath = $@"{startupPath}/{dirPath}/name.png", UserInfo = $"{userInfos[1]}, {userInfos[2]}", UserName = $"{userInfos[0]}{userInfos[1]}", UserDir = userInfos[0] });
                    }
                    else
                    {
                        items.Add(new UserInfoItem() { NameBmpPath = null, UserInfo = $"{userInfos[0]}{userInfos[1]}, {userInfos[2]}", UserName = $"{userInfos[0]}{userInfos[1]}", UserDir = userInfos[0] });
                    }
                }
                else
                {
                    // error;
                    // Logディレクトリに異物（ディレクトリ）が入っている
                }
            }
            return items;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            switch (button.Name)
            {
                case "ChangeScreenButton":

                    Window _mainWindow = Application.Current.MainWindow;

                    this.Maximize(mainWindow: _mainWindow);

                    break;

                case "ItemsButtonButton":

                    if (this.initConfig.userName == null)
                    {
                        MessageBox.Show("まずは名前の入力から始めてください。", "情報");
                    }
                    else
                    {
                        ItemPage itemPage = new ItemPage();

                        itemPage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                        this.NavigationService.Navigate(itemPage);
                    }
                    break;

                case "OptionButton":

                    if (this.initConfig.userName == null)
                    {
                        MessageBox.Show("まずは名前の入力から始めてください。", "情報");
                    }
                    else
                    {
                        OptionPage optionPage = new OptionPage();

                        optionPage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                        this.NavigationService.Navigate(optionPage);
                    }
                    break;

                case "NameEntryButton":

                    NameInputPage nameInputPage = new NameInputPage();

                    // nameInputPage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                    this.NavigationService.Navigate(nameInputPage);

                    break;

                case "SelectDataButton":

                    if (this.initConfig.userName == null)
                    {
                        MessageBox.Show("データが一つもできていません。\nまずは名前を入力して、\nどれかプレイしてください。", "情報");
                    }
                    else
                    {
                        this.SelectDataListBox.ItemsSource = this.LoadAnyUsers();

                        this.CoverLayerImage.Visibility = Visibility.Visible;
                        this.SelectDataListGrid.Visibility = Visibility.Visible;
                    }
                    break;

                case "SelectDataListExportButton":

                    if (this.SelectDataListBox.SelectedItems.Count > 0)
                    {
                        var result = FileOpenDialog.Dialogs.DialogResult.None;

                        var browser = new FileOpenDialog.Dialogs.FolderBrowserDialog();

                        browser.Title = "フォルダーを選択してください";

                        // ウィンドウが取得できるときは設定する
                        var obj = sender as DependencyObject;

                        if (obj != null)
                        {
                            var window = Window.GetWindow(obj);

                            if (window != null) result = browser.ShowDialog(window);
                        }
                        else
                        {
                            result = browser.ShowDialog(IntPtr.Zero);
                        }

                        if (result == FileOpenDialog.Dialogs.DialogResult.OK)
                        {
                            DirectoryUtils.SafeCreateDirectory("./temp");

                            // 複数選択可能
                            foreach (var item in this.SelectDataListBox.SelectedItems)
                            {
                                var sourceDirName = this.dirPaths[this.SelectDataListBox.Items.IndexOf(item)];

                                var destDirName = $@"./temp/{ (item as UserInfoItem).UserDir}";

                                DirectoryUtils.CopyDirectory(sourceDirName, destDirName);

                                string[] userInfos;

                                using (var csv = new CsvReader($"{destDirName}/user.conf"))
                                {
                                    var csvs = csv.ReadToEnd();
                                    userInfos = new string[3] { csvs[0][0], csvs[0][1], csvs[0][2] };
                                }
                                File.Delete($"{destDirName}/user.conf");

                                var finalDir = $"{browser.SelectedPath}/{userInfos[0]}";

                                var dbPath = $"{destDirName}/{userInfos[0]}.sqlite";

                                if (File.Exists($"{destDirName}/{userInfos[0]}.xlsx"))
                                {
                                    File.Delete($"{destDirName}/{userInfos[0]}.xlsx");
                                }
                                File.Copy("./Datas/default.xlsx", $"{destDirName}/{userInfos[0]}.xlsx");

                                DB2Excel.WriteDB2Excel(dbPath, $"{destDirName}/{userInfos[0]}.xlsx", userInfos);

                                File.Delete($"{destDirName}/{userInfos[0]}.sqlite");

                                try
                                {
                                    Directory.Move(destDirName, finalDir);
                                }
                                catch (Exception error)
                                {
                                    MessageBox.Show(error.Message, "失敗！");
                                }

                                if (Directory.Exists("./temp"))
                                {
                                    Directory.Delete("./temp", true);
                                }
                            }

                            // 処理が重くなったら後々プログレスバーをつける
                            MessageBox.Show("全てのデータの出力が完成しました。", "情報");
                        }
                    }
                    else
                    {
                        MessageBox.Show("データを一つ以上選択してください。", "情報");
                    }
                    break;

                case "SelectDataListReturnButton":

                    this.SelectDataListGrid.Visibility = Visibility.Hidden;
                    this.CoverLayerImage.Visibility = Visibility.Hidden;

                    break;

                case "SelectDataListDeleteButton":

                    if (this.SelectDataListBox.SelectedItems.Count > 0)
                    {
                        if (MessageBox.Show("選択したデータを完全に削除します。\nよろしいですか？", "注意！", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
                        {
                            return;
                        }
                        else
                        {
                            // 複数選択可能
                            foreach (var item in this.SelectDataListBox.SelectedItems)
                            {
                                var selectedDataPath = this.dirPaths[this.SelectDataListBox.Items.IndexOf(item)];

                                // (this.SelectDataListBox.ItemsSource as List<UserInfoItem>).Remove(item as UserInfoItem);

                                if (Directory.Exists(selectedDataPath))
                                {
                                    Directory.Delete(selectedDataPath, true);
                                }
                            }
                            // this.SelectDataListBox.Items.Refresh();

                            this.SelectDataListBox.ItemsSource = this.LoadAnyUsers();

                            if (this.SelectDataListBox.Items.Count <= 0)
                            {
                                if (this.CurrentNameImage.Source != null)
                                {
                                    this.CurrentNameImage.Source = null;
                                }
                                this.CurrentUserTextBlock.Text = BASE_USER_NAME;

                                this.dataProgress.CurrentChapter = 0;
                                this.dataProgress.CurrentScene = null;

                                this.SetCurrentSceneInfo();

                                this.initConfig.userName = null;

                                if (File.Exists("./Log/system.conf"))
                                {
                                    File.Delete("./Log/system.conf");
                                }
                            }
                            else
                            {
                                bool nameExist = false;

                                foreach (var item in this.SelectDataListBox.Items)
                                {
                                    if ((item as UserInfoItem).UserName == $"{this.initConfig.userName}{this.initConfig.userTitle}")
                                    {
                                        nameExist = true;
                                    }
                                }

                                // 削除候補に現在のユーザが含まれていた時の処理（残りのユーザの先頭をカレントユーザにする）
                                if (!nameExist)
                                {
                                    var startupPath = FileUtils.GetStartupPath();

                                    var userDirPath = $@"{startupPath}/Log/{(this.SelectDataListBox.Items[0] as UserInfoItem).UserDir}";

                                    File.Copy($@"{userDirPath}/user.conf", @"./Log/system.conf", true);
                                }
                                this.LoadCurrentUser();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("データを一つ以上選択してください。", "情報");
                    }
                    break;

                case "SelectUserButton":

                    if (this.initConfig.userName == null)
                    {
                        MessageBox.Show("なまえがひとつも登録されていません。\nまずは名前の入力から始めてください。", "情報");
                    }
                    else
                    {
                        this.SelectUserListBox.ItemsSource = this.LoadAnyUsers();

                        this.CoverLayerImage.Visibility = Visibility.Visible;
                        this.SelectUserListGrid.Visibility = Visibility.Visible;

                        // カレントユーザを最初から選択された状態にする
                        foreach (var item in this.SelectUserListBox.Items)
                        {
                            if ((item as UserInfoItem).UserName == $"{this.initConfig.userName}{this.initConfig.userTitle}")
                            {
                                this.SelectUserListBox.SelectedItem = item;
                            }
                        }
                    }
                    break;

                // ユーザの切り替え処理
                case "SelectUserListOKButton":

                    var selectedUserNamePath = this.dirPaths[this.SelectUserListBox.SelectedIndex];

                    File.Copy($@"{selectedUserNamePath}/user.conf", @"./Log/system.conf", true);

                    this.LoadCurrentUser();

                    this.SelectUserListGrid.Visibility = Visibility.Hidden;
                    this.CoverLayerImage.Visibility = Visibility.Hidden;

                    break;

                case "AboutButton":

                    AboutPage aboutPage = new AboutPage();

                    aboutPage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                    this.NavigationService.Navigate(aboutPage);

                    break;

                case "RuleButton":

                    RuleBoardPage ruleBoardPage = new RuleBoardPage();

                    ruleBoardPage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                    this.NavigationService.Navigate(ruleBoardPage);

                    break;

                case "EndButton":

                    this.CoverLayerImage.Visibility = Visibility.Visible;
                    this.ExitBackGrid.Visibility = Visibility.Visible;

                    break;

                case "ExitBackYesButton":

                    Application.Current.Shutdown();

                    break;


                case "ExitBackNoButton":

                    this.ExitBackGrid.Visibility = Visibility.Hidden;
                    this.CoverLayerImage.Visibility = Visibility.Hidden;

                    break;

                case "CreditButton":

                    Storyboard sbStart = this.FindResource("appear_credit") as Storyboard;

                    if (sbStart != null)
                    {
                        sbStart.Begin(this);
                    }
                    this.CoverLayerImage.Visibility = Visibility.Visible;
                    this.CreditGrid.Visibility = Visibility.Visible;

                    break;

                case "CreditReturnButton":

                    Storyboard sbEnd = this.FindResource("disappear_credit") as Storyboard;

                    if (sbEnd != null)
                    {
                        sbEnd.Begin(this);
                    }
 
                    // 1秒後に処理を実行
                    DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                    timer.Start();
                    timer.Tick += (s, args) =>
                    {
                        // タイマーの停止
                        timer.Stop();

                        // 以下に待機後の処理を書く
                        this.CoverLayerImage.Visibility = Visibility.Hidden;
                        this.CreditGrid.Visibility = Visibility.Hidden;
                    };
                    break;
            }

            switch (button.Content.ToString())
            {

                case "第1回":

                    if (this.initConfig.userName == null)
                    {
                        MessageBox.Show("まずは名前の入力から始めてください。", "情報");
                    }
                    else
                    {
                        Chapter1 chapter1 = new Chapter1();

                        chapter1.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                        this.NavigationService.Navigate(chapter1);
                    }
                    break;

                case "第2回":

                    if (this.initConfig.userName == null)
                    {
                        MessageBox.Show("まずは名前の入力から始めてください。", "情報");
                    }
                    else
                    {
                        Chapter2 chapter2 = new Chapter2();

                        chapter2.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                        this.NavigationService.Navigate(chapter2);
                    }
                    break;

                case "第3回":

                    if (this.initConfig.userName == null)
                    {
                        MessageBox.Show("まずは名前の入力から始めてください。", "情報");
                    }
                    else
                    {
                        Chapter3 chapter3 = new Chapter3();

                        chapter3.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                        this.NavigationService.Navigate(chapter3);
                    }
                    break;
            }
        }

        //全画面表示にする
        public void Maximize(Window mainWindow)
        {
            if (!this.isMaximized)
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

                this.isMaximized = true;
            }
            else if (this.isMaximized)
            {
                mainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                mainWindow.ResizeMode = ResizeMode.CanResize;
                mainWindow.Topmost = false;

                this.isMaximized = false;
            }
        }
    }

    public class UserInfoItem
    {
        public string UserInfo { get; set; }
        public string NameBmpPath { get; set; }
        public string UserName { get; set; }
        public string UserDir { get; set; }
    }
}
