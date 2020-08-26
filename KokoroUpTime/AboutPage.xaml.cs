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
        // データモデル
        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();
        public DataProgress dataProgress = new DataProgress();

        public AboutPage()
        {
            InitializeComponent();
        }

        // 追加のイニシャライザ
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        // データモデルの橋渡し
        public void SetNextPage(InitConfig _initConfig, DataOption _dataOption, DataItem _dataItem, DataProgress _dataProgress)
        {
            this.initConfig = _initConfig;
            this.dataOption = _dataOption;
            this.dataItem = _dataItem;
            this.dataProgress = _dataProgress;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button.Name == "ReturnToTopButton")
            {
                TitlePage titlePage = new TitlePage();

                // タイトルページのリロードなし
                titlePage.SetIsFirstBootFlag(false);

                // データモデルの橋渡し
                titlePage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                // ページ遷移
                this.NavigationService.Navigate(titlePage);
            }
        }
    }
}
