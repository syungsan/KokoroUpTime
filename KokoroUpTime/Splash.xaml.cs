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

using System.Windows.Media.Animation;

namespace KokoroUpTime
{
    /// <summary>
    /// Splash.xaml の相互作用ロジック
    /// </summary>
    /// 
    /// このスプラッシュスクリーンはアプリ立ち上げ時の
    /// 表示の不安定さを隠ぺいするためのもの
    public partial class Splash : Page
    {
        public Splash()
        {
            Window _mainWindow = Application.Current.MainWindow;

            // メインウィンドウの最大化
            this.Maximize(mainWindow: _mainWindow);

            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            this.ShowAnime("word_lotation");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        // アニメーション（ストーリーボード）の処理
        private void ShowAnime(string storyBoard)
        {
            Storyboard sb = this.FindResource(storyBoard) as Storyboard;

            if (sb != null)
            {
                sb.Completed += (s, e) =>
                {
                    // アニメ終了後にタイトル画面へ遷移
                    TitlePage titlePage = new TitlePage();

                    this.NavigationService.Navigate(titlePage);
                };
                sb.Begin(this);
            }
        }

        //全画面表示にする
        public void Maximize(Window mainWindow)
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
        }
    }
}
