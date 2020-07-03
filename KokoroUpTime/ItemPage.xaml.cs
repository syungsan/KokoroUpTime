using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// ItemBook.xaml の相互作用ロジック
    /// </summary>
    public partial class ItemPage : Page
    {
        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();

        private string dbPath;

        private List<Button> itemMainButtons = new List<Button>();

        public ItemPage()
        {
            InitializeComponent();

            this.Item01MainButton.Visibility = Visibility.Hidden;
            this.Item02MainButton.Visibility = Visibility.Hidden;
            this.Item03MainButton.Visibility = Visibility.Hidden;
            this.Item04MainButton.Visibility = Visibility.Hidden;
            this.Item05MainButton.Visibility = Visibility.Hidden;
            this.Item06MainButton.Visibility = Visibility.Hidden;
            this.Item07MainButton.Visibility = Visibility.Hidden;
            this.Item08MainButton.Visibility = Visibility.Hidden;
            this.Item09MainButton.Visibility = Visibility.Hidden;
            this.Item10MainButton.Visibility = Visibility.Hidden;
            this.Item11MainButton.Visibility = Visibility.Hidden;

            this.itemMainButtons.Add(this.Item01MainButton);
            this.itemMainButtons.Add(this.Item02MainButton);
            this.itemMainButtons.Add(this.Item03MainButton);
            this.itemMainButtons.Add(this.Item04MainButton);
            this.itemMainButtons.Add(this.Item05MainButton);
            this.itemMainButtons.Add(this.Item06MainButton);
            this.itemMainButtons.Add(this.Item07MainButton);
            this.itemMainButtons.Add(this.Item08MainButton);
            this.itemMainButtons.Add(this.Item09MainButton);
            this.itemMainButtons.Add(this.Item10MainButton);
            this.itemMainButtons.Add(this.Item11MainButton);
        }

        public void SetInitConfig(InitConfig _initConfig)
        {
            this.initConfig = _initConfig;

            // データベース本体のファイルのパス設定
            string dbName = $"{initConfig.userName}.sqlite";
            string dirPath = $"./Log/{initConfig.userName}_{initConfig.userTitle}/";

            this.dbPath = System.IO.Path.Combine(dirPath, dbName);
        }

        public void SetDataOption(DataOption _dataOption)
        {
            this.dataOption = _dataOption;
        }

        public void SetDataItem(DataItem _dataItem)
        {
            this.dataItem = _dataItem;

            this.LoadItem();
        }

        void LoadItem()
        {
            Image[] itemNoneImages = { this.Item01NoneImage, this.Item02NoneImage, this.Item03NoneImage, this.Item04NoneImage, this.Item05NoneImage, this.Item06NoneImage, this.Item07NoneImage, this.Item08NoneImage, this.Item09NoneImage, this.Item10NoneImage, this.Item11NoneImage };

            bool[] hasGotItems = { this.dataItem.HasGotItem01, this.dataItem.HasGotItem02, this.dataItem.HasGotItem03, this.dataItem.HasGotItem04, this.dataItem.HasGotItem05, this.dataItem.HasGotItem06, this.dataItem.HasGotItem07, this.dataItem.HasGotItem08, this.dataItem.HasGotItem09, this.dataItem.HasGotItem10, this.dataItem.HasGotItem11 };
        
            for (int i=0; i < hasGotItems.Length; i++)
            {
                if (hasGotItems[i] == true)
                {
                    this.itemMainButtons[i].Visibility = Visibility.Visible;

                    itemNoneImages[i].Visibility = Visibility.Hidden;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button.Name == "ReturnToTitleButton")
            {
                TitlePage nextPage = new TitlePage();

                nextPage.SetIsFirstBootFlag(false);

                nextPage.SetInitConfig(this.initConfig);
                nextPage.SetDataOption(this.dataOption);
                nextPage.SetDataItem(this.dataItem);

                this.NavigationService.Navigate(nextPage);
            }
        }
    }
}
