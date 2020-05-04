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

namespace KokoroUpTime
{
    /// <summary>
    /// TitlePage.xaml の相互作用ロジック
    /// </summary>
    public partial class TitlePage : Page
    {
        //全画面表示か
        private bool isMaximized = false;

        public TitlePage()
        {
            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            Assembly asm = Assembly.GetExecutingAssembly(); // 実行中のアセンブリを取得する。

            // AssemblyNameから取得
            AssemblyName asmName = asm.GetName();
            string name = "AssemblyName.Name : " + asmName.Name + "\r\n";
            string version = "AssemblyName.Version : " + asmName.Version.ToString() + "\r\n";
            string fullname = "AssemblyName.FullName : " + asmName.FullName + "\r\n";
            string processor = "AssemblyName.ProcessorArchitecture : " + asmName.ProcessorArchitecture + "\r\n";
            string runtime = "Assembly.ImageRuntimeVersion : " + asm.ImageRuntimeVersion + "\r\n";

            this.VersionText.Text = name + version + fullname + processor + runtime + "\r\n";

            this.WindowTitle = asmName.Name + " Ver" + asmName.Version.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            for (int i = 1; i <= 12; ++i)
            {
                // Pageインスタンスを渡して遷移
                GamePage nextPage = new GamePage();

                string scenario;

                if (button.Content.ToString() == $"第{i}回")
                {
                    scenario = $"Scenarios/chapter{i}.csv";

                    nextPage.SetScenario(scenario);

                    this.NavigationService.Navigate(new Uri("GamePage.xaml", UriKind.Relative));

                    this.NavigationService.Navigate(nextPage);
                }
            }

            if (button.Content.ToString() == "Full/Win")
            {
                Window _mainWindow = Application.Current.MainWindow;

                this.Maximize(mainWindow: _mainWindow);
            }

            if (button.Content.ToString() == "オプション")
            {
                OptionPage nextPage = new OptionPage();

                this.NavigationService.Navigate(new Uri("OptionPage.xaml", UriKind.Relative));

                this.NavigationService.Navigate(nextPage);
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
}
