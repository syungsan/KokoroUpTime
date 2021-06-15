using System.Windows;
using System.Windows.Controls;

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

           

#if DEBUG

            //Debug用の画面サイズ
            mainWindow.Width = 800;
            mainWindow.Height = 600;
            mainWindow.ResizeMode = ResizeMode.CanResize;
            mainWindow.Left = 1130;
            mainWindow.Top = 490;

#else

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

#endif


        }
    }
}
