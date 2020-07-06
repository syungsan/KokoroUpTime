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
    /// AboutPage.xaml の相互作用ロジック
    /// </summary>
    public partial class AboutPage : Page
    {
        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();

        public AboutPage()
        {
            InitializeComponent();
        }

        public void SetInitConfig(InitConfig _initConfig)
        {
            this.initConfig = _initConfig;
        }

        public void SetDataOption(DataOption _dataOption)
        {
            this.dataOption = _dataOption;
        }

        public void SetDataItem(DataItem _dataItem)
        {
            this.dataItem = _dataItem;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button.Name == "ReturnToTopButton")
            {
                TitlePage titlePage = new TitlePage();

                titlePage.SetIsFirstBootFlag(false);

                titlePage.SetInitConfig(this.initConfig);
                titlePage.SetDataOption(this.dataOption);
                titlePage.SetDataItem(this.dataItem);

                this.NavigationService.Navigate(titlePage);
            }
        }
    }
}
