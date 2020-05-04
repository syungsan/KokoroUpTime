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

namespace KokoroUpTime
{
    /// <summary>
    /// OptionPage.xaml の相互作用ロジック
    /// </summary>
    public partial class OptionPage : Page
    {
        public OptionPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button.Name == "ReturnToTitleButton")
            {
                TitlePage nextPage = new TitlePage();

                this.NavigationService.Navigate(new Uri("TitlePage.xaml", UriKind.Relative));

                this.NavigationService.Navigate(nextPage);
            }
        }
    }
}
