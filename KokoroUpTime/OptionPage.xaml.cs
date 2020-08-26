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
        private float[] MESSAGE_SPEEDS = { 20.0f, 30.0f, 300.0f };

        public InitConfig initConfig = new InitConfig();
        public DataOption dataOption = new DataOption();
        public DataItem dataItem = new DataItem();

        private string dbPath;

        public OptionPage()
        {
            InitializeComponent();
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

            this.LoadOption();
        }

        public void SetDataItem(DataItem _dataItem)
        {
            this.dataItem = _dataItem;
        }

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

            if (false)//this.dataOption.IsWordRecognition == true)
            {
                this.WordRecognitionOnButton.Foreground = new SolidColorBrush(Colors.Blue);
            }
            else if (false)//this.dataOption.IsWordRecognition == false)
            {
                this.WordRecognitionOffButton.Foreground = new SolidColorBrush(Colors.Blue);
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

            if (button.Name == "WordRecognitionOnButton")
            {
                if (button.Foreground != new SolidColorBrush(Colors.Blue))
                {
                    button.Foreground = new SolidColorBrush(Colors.Blue);
                    this.WordRecognitionOffButton.Foreground = new SolidColorBrush(Colors.White);

                   // this.dataOption.IsWordRecognition = true;
                }
            }

            if (button.Name == "WordRecognitionOffButton")
            {
                if (button.Foreground != new SolidColorBrush(Colors.Blue))
                {
                    button.Foreground = new SolidColorBrush(Colors.Blue);
                    this.WordRecognitionOnButton.Foreground = new SolidColorBrush(Colors.White);

                   // this.dataOption.IsWordRecognition = false;
                }
            }

            if (button.Name == "ReturnToTitleButton")
            {
                TitlePage nextPage = new TitlePage();

                nextPage.SetIsFirstBootFlag(false);

                nextPage.SetInitConfig(this.initConfig);
                nextPage.SetDataOption(this.dataOption);
                nextPage.SetDataItem(this.dataItem);

                this.NavigationService.Navigate(nextPage);
            }
            else
            {
                //this.dataOption.CreatedAt = DateTime.Now.ToString();

                using (var connection = new SQLiteConnection(this.dbPath))
                {
                   // connection.Execute($@"UPDATE DataOption SET IsPlaySE = '{Convert.ToInt32(this.dataOption.IsPlaySE)}', IsPlayBGM = '{Convert.ToInt32(this.dataOption.IsPlayBGM)}', MessageSpeed = '{this.dataOption.MessageSpeed}', IsAddRubi = '{Convert.ToInt32(this.dataOption.IsAddRubi)}', IsWordRecognition = '{Convert.ToInt32(this.dataOption.IsWordRecognition)}', CreatedAt = '{this.dataOption.CreatedAt}' WHERE Id = 1;");
                }
            }
        }
    }
}
