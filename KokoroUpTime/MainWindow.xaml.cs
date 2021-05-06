using System.Windows.Input;
using System.Windows.Navigation;

namespace KokoroUpTime
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : NavigationWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            // 画面遷移ショートカットを無効にする
            // http://stackoverflow.com/questions/700094/disable-backspace-in-wpf
            NavigationCommands.BrowseBack.InputGestures.Clear();
            NavigationCommands.BrowseForward.InputGestures.Clear();
        }
    }
}
