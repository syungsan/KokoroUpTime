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

namespace KokoroUpTime
{
    /// <summary>
    /// TitlePage.xaml の相互作用ロジック
    /// </summary>
    public partial class TitlePage : Page
    {
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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Uriで遷移
            NavigationService.Navigate(new Uri("GamePage.xaml", UriKind.Relative));

            // Pageインスタンスを渡して遷移
            var nextPage = new GamePage();
            NavigationService.Navigate(nextPage);
        }
    }
}
