using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        // ページ間参照橋渡し変数
        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();
        public DataProgress dataProgress = new DataProgress();

        private List<Button> itemMainButtons = new List<Button>();

        private bool[] hasGotItems;

        // アイテム所持是非のフラグ
        private Image[] itemDetailImages;

        private int currentItemNo;

        public ItemPage()
        {
            InitializeComponent();

            this.ResetMainVisible();
            this.ResetDetailVisible();

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

        // xamlの処理が終わってWindowが表示されてからの処理
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadItem();
        }

        // ページ間参照変数の橋渡し
        public void SetNextPage(InitConfig _initConfig, DataOption _dataOption, DataItem _dataItem, DataProgress _dataProgress)
        {
            this.initConfig = _initConfig;
            this.dataOption = _dataOption;
            this.dataItem = _dataItem;
            this.dataProgress = _dataProgress;
        }

        private void ResetMainVisible()
        {
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
        }

        private void ResetDetailVisible()
        { 
            this.Item01DetailImage.Visibility = Visibility.Hidden;
            this.Item02DetailImage.Visibility = Visibility.Hidden;
            this.Item03DetailImage.Visibility = Visibility.Hidden;
            this.Item04DetailImage.Visibility = Visibility.Hidden;
            this.Item05DetailImage.Visibility = Visibility.Hidden;
            this.Item06DetailImage.Visibility = Visibility.Hidden;
            this.Item07DetailImage.Visibility = Visibility.Hidden;
            this.Item08DetailImage.Visibility = Visibility.Hidden;
            this.Item09DetailImage.Visibility = Visibility.Hidden;
            this.Item10DetailImage.Visibility = Visibility.Hidden;
            this.Item11DetailImage.Visibility = Visibility.Hidden;
        }

        private void LoadItem()
        {
            Image[] itemNoneImages = { this.Item01NoneImage, this.Item02NoneImage, this.Item03NoneImage, this.Item04NoneImage, this.Item05NoneImage, this.Item06NoneImage, this.Item07NoneImage, this.Item08NoneImage, this.Item09NoneImage, this.Item10NoneImage, this.Item11NoneImage };

            // アイテムを持っているかどうかのフラグ
            this.hasGotItems = new bool[] { this.dataItem.HasGotItem01, this.dataItem.HasGotItem02, this.dataItem.HasGotItem03, this.dataItem.HasGotItem04, this.dataItem.HasGotItem05, this.dataItem.HasGotItem06, this.dataItem.HasGotItem07, this.dataItem.HasGotItem08, this.dataItem.HasGotItem09, this.dataItem.HasGotItem10, this.dataItem.HasGotItem11 };

            this.itemDetailImages = new Image[] { this.Item01DetailImage, this.Item02DetailImage, this.Item03DetailImage, this.Item04DetailImage, this.Item05DetailImage, this.Item06DetailImage, this.Item07DetailImage, this.Item08DetailImage, this.Item09DetailImage, this.Item10DetailImage, this.Item11DetailImage };

            for (int i=0; i < hasGotItems.Length; i++)
            {
                if (hasGotItems[i] == true)
                {
                    this.itemMainButtons[i].Visibility = Visibility.Visible;

                    itemNoneImages[i].Visibility = Visibility.Hidden;
                }
            }
            this.NextPageButton.Visibility = Visibility.Hidden;
            this.ReturnToItemButton.Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button.Name == "ReturnToTitleButton")
            {
                TitlePage titlePage = new TitlePage();

                // タイトルページのリロードなし
                titlePage.SetIsFirstBootFlag(false);

                titlePage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                this.NavigationService.Navigate(titlePage);
            }

            for (int i=0; i < this.itemMainButtons.Count; i++)
            {
                var numStr = String.Format("{0:D2}", i + 1);

                if (button.Name == $"Item{numStr}MainButton")
                {
                    this.ItemMainGrid.Visibility = Visibility.Hidden;
                    this.ItemNoneGrid.Visibility = Visibility.Hidden;

                    this.ReturnToTitleButton.Visibility = Visibility.Hidden;

                    this.ReturnToItemButton.Visibility = Visibility.Visible;

                    // もっとアイテムを持っているのであれば次を表示するボタン
                    if (this.hasGotItems.Where(c => c).Count() > 1)
                    {
                        this.NextPageButton.Visibility = Visibility.Visible;
                    }
                    this.TitleTextBlock.Visibility = Visibility.Hidden;

                    this.itemDetailImages[i].Visibility = Visibility.Visible;

                    this.currentItemNo = i;
                }
            }

            if (button.Name == "ReturnToItemButton")
            {
                this.ResetDetailVisible();

                this.ReturnToItemButton.Visibility = Visibility.Hidden;

                if (this.hasGotItems.Where(c => c).Count() > 1)
                {
                    this.NextPageButton.Visibility = Visibility.Hidden;
                }
                this.TitleTextBlock.Visibility = Visibility.Visible;

                this.ItemMainGrid.Visibility = Visibility.Visible;
                this.ItemNoneGrid.Visibility = Visibility.Visible;

                this.ReturnToTitleButton.Visibility = Visibility.Visible;
            }

            if (button.Name == "NextPageButton")
            {
                // アイテム詳細を循環させる
                if (this.currentItemNo >= this.itemDetailImages.Length - 1)
                {
                    this.currentItemNo = 0;
                }
                else
                {
                    this.currentItemNo += 1;
                }

                while (!this.hasGotItems[this.currentItemNo])
                {
                    this.currentItemNo += 1;
                }
                this.ResetDetailVisible();

                this.itemDetailImages[this.currentItemNo].Visibility = Visibility.Visible;
            }
        }
    }
}
