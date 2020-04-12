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

using CsvReadWrite;
using System.Diagnostics;
using System.Windows.Media.Animation;


namespace KokoroUpTime
{
    /// <summary>
    /// GameWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class GamePage : Page
    {
        private int scenarioCount = 0;
        private List<List<string>> scenarios = null;

        public GamePage()
        {
            InitializeComponent();

            // Hide host's navigation UI
            this.ShowsNavigationUI = false;

            this.scenarios = this.LoadScenario("Scenarios/chapter1.csv");
            this.ScenarioPlay();
        }

        List<List<string>> LoadScenario(string filePath)
        {
            List<List<string>> scenarios = null;

            using (var csv = new CsvReader(filePath))
            {
                scenarios = csv.ReadToEnd();
            }
            return scenarios;
        }

        void ScenarioPlay()
        {
            var tag = this.scenarios[this.scenarioCount][0];

            switch (tag)
            {
                case "bg":

                    var fileName = scenarios[this.scenarioCount][1];
                    BG.Source = new BitmapImage(new Uri($"Images/{fileName}", UriKind.Relative));

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;

                case "chara":

                    Storyboard sb = this.FindResource("Appearance") as Storyboard;
                    if (sb != null)
                    {
                        BeginStoryboard(sb);
                    }

                    this.scenarioCount += 1;
                    this.ScenarioPlay();

                    break;
            }
        }
    }
}
