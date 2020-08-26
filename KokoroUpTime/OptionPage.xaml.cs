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

using SQLite;

namespace KokoroUpTime
{
    /// <summary>
    /// OptionPage.xaml の相互作用ロジック
    /// </summary>
    public partial class OptionPage : Page
    {
        // メッセージスピード「遅い，ふつう，速い」
        private float[] MESSAGE_SPEEDS = { 20.0f, 30.0f, 300.0f };

        // ページ間参照変数橋渡し
        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();
        public DataProgress dataProgress = new DataProgress();

        public OptionPage()
        {
            InitializeComponent();
        }

        // ページ間参照関数橋渡し
        public void SetNextPage(InitConfig _initConfig, DataOption _dataOption, DataItem _dataItem, DataProgress _dataProgress)
        {
            this.initConfig = _initConfig;
            this.dataOption = _dataOption;
            this.dataItem = _dataItem;
            this.dataProgress = _dataProgress;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadOption();
        }

        // データベースから初期値を設定
        private void LoadOption()
        {
            if (this.dataOption.IsPlaySE == true)
            {
                this.SEOnButton.Foreground = new SolidColorBrush(Colors.Blue);
            }
            else if (this.dataOption.IsPlaySE == false)
            {
                this.SEOffButton.Foreground = new SolidColorBrush(Colors.Blue);
            }

            if (this.dataOption.IsPlayBGM == true)
            {
                this.BGMOnButton.Foreground = new SolidColorBrush(Colors.Blue);
            }
            else if (this.dataOption.IsPlayBGM == false)
            {
                this.BGMOffButton.Foreground = new SolidColorBrush(Colors.Blue);
            }

            if (this.dataOption.MessageSpeed == MESSAGE_SPEEDS[0])
            {
                this.MessageSpeedSlowButton.Foreground = new SolidColorBrush(Colors.Blue);
            }
            else if (this.dataOption.MessageSpeed == MESSAGE_SPEEDS[1])
            {
                this.MessageSpeedMiddleButton.Foreground = new SolidColorBrush(Colors.Blue);
            }
            else if (this.dataOption.MessageSpeed == MESSAGE_SPEEDS[2])
            {
                this.MessageSpeedFastButton.Foreground = new SolidColorBrush(Colors.Blue);
            }

            if (this.dataOption.IsAddRubi == true)
            {
                this.RubiOnButton.Foreground = new SolidColorBrush(Colors.Blue);
            }
            else if (this.dataOption.IsAddRubi == false)
            {
                this.RubiOffButton.Foreground = new SolidColorBrush(Colors.Blue);
            }

            if (this.dataOption.InputMethod == 0)
            {
                this.HandWritingButton.Foreground = new SolidColorBrush(Colors.Blue);
            }
            else if (this.dataOption.InputMethod == 1)
            {
                this.KeyboardButton.Foreground = new SolidColorBrush(Colors.Blue);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button.Name == "SEOnButton")
            {
                if (button.Foreground != new SolidColorBrush(Colors.Blue))
                {
                    button.Foreground = new SolidColorBrush(Colors.Blue);
                    this.SEOffButton.Foreground = new SolidColorBrush(Colors.White);

                    this.dataOption.IsPlaySE = true;
                }
            }

            if (button.Name == "SEOffButton")
            {
                if (button.Foreground != new SolidColorBrush(Colors.Blue))
                {
                    button.Foreground = new SolidColorBrush(Colors.Blue);
                    this.SEOnButton.Foreground = new SolidColorBrush(Colors.White);

                    this.dataOption.IsPlaySE = false;
                }
            }

            if (button.Name == "BGMOnButton")
            {
                if (button.Foreground != new SolidColorBrush(Colors.Blue))
                {
                    button.Foreground = new SolidColorBrush(Colors.Blue);
                    this.BGMOffButton.Foreground = new SolidColorBrush(Colors.White);

                    this.dataOption.IsPlayBGM = true;
                }
            }

            if (button.Name == "BGMOffButton")
            {
                if (button.Foreground != new SolidColorBrush(Colors.Blue))
                {
                    button.Foreground = new SolidColorBrush(Colors.Blue);
                    this.BGMOnButton.Foreground = new SolidColorBrush(Colors.White);

                    this.dataOption.IsPlayBGM = false;
                }
            }

            if (button.Name == "MessageSpeedFastButton")
            {
                if (button.Foreground != new SolidColorBrush(Colors.Blue))
                {
                    button.Foreground = new SolidColorBrush(Colors.Blue);
                    this.MessageSpeedMiddleButton.Foreground = new SolidColorBrush(Colors.White);
                    this.MessageSpeedSlowButton.Foreground = new SolidColorBrush(Colors.White);

                    this.dataOption.MessageSpeed = MESSAGE_SPEEDS[2];
                }
            }

            if (button.Name == "MessageSpeedMiddleButton")
            {
                if (button.Foreground != new SolidColorBrush(Colors.Blue))
                {
                    button.Foreground = new SolidColorBrush(Colors.Blue);
                    this.MessageSpeedFastButton.Foreground = new SolidColorBrush(Colors.White);
                    this.MessageSpeedSlowButton.Foreground = new SolidColorBrush(Colors.White);

                    this.dataOption.MessageSpeed = MESSAGE_SPEEDS[1];
                }
            }

            if (button.Name == "MessageSpeedSlowButton")
            {
                if (button.Foreground != new SolidColorBrush(Colors.Blue))
                {
                    button.Foreground = new SolidColorBrush(Colors.Blue);
                    this.MessageSpeedMiddleButton.Foreground = new SolidColorBrush(Colors.White);
                    this.MessageSpeedFastButton.Foreground = new SolidColorBrush(Colors.White);

                    this.dataOption.MessageSpeed = MESSAGE_SPEEDS[0];
                }
            }

            if (button.Name == "RubiOnButton")
            {
                if (button.Foreground != new SolidColorBrush(Colors.Blue))
                {
                    button.Foreground = new SolidColorBrush(Colors.Blue);
                    this.RubiOffButton.Foreground = new SolidColorBrush(Colors.White);

                    this.dataOption.IsAddRubi = true;
                }
            }

            if (button.Name == "RubiOffButton")
            {
                if (button.Foreground != new SolidColorBrush(Colors.Blue))
                {
                    button.Foreground = new SolidColorBrush(Colors.Blue);
                    this.RubiOnButton.Foreground = new SolidColorBrush(Colors.White);

                    this.dataOption.IsAddRubi = false;
                }
            }

            if (button.Name == "HandWritingButton")
            {
                if (button.Foreground != new SolidColorBrush(Colors.Blue))
                {
                    button.Foreground = new SolidColorBrush(Colors.Blue);
                    this.KeyboardButton.Foreground = new SolidColorBrush(Colors.White);

                    this.dataOption.InputMethod = 0;
                }
            }

            if (button.Name == "KeyboardButton")
            {
                if (button.Foreground != new SolidColorBrush(Colors.Blue))
                {
                    button.Foreground = new SolidColorBrush(Colors.Blue);
                    this.HandWritingButton.Foreground = new SolidColorBrush(Colors.White);

                    this.dataOption.InputMethod = 1;
                }
            }

            if (button.Name == "ReturnToTitleButton")
            {
                TitlePage titlePage = new TitlePage();

                // タイトルページのリロードなし
                titlePage.SetIsFirstBootFlag(false);

                titlePage.SetNextPage(this.initConfig, this.dataOption, this.dataItem, this.dataProgress);

                this.NavigationService.Navigate(titlePage);
            }
            else
            {
                // タイトルへ戻るボタン以外でオプションを常にデータベースへ記録
                using (var connection = new SQLiteConnection(this.initConfig.dbPath))
                {
                    connection.Execute($@"UPDATE DataOption SET IsPlaySE = '{Convert.ToInt32(this.dataOption.IsPlaySE)}', IsPlayBGM = '{Convert.ToInt32(this.dataOption.IsPlayBGM)}', MessageSpeed = '{this.dataOption.MessageSpeed}', IsAddRubi = '{Convert.ToInt32(this.dataOption.IsAddRubi)}', InputMethod = '{this.dataOption.InputMethod}' WHERE Id = 1;");
                }
            }
        }
    }
}
