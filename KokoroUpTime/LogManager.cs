using FileIOUtils;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace KokoroUpTime
{
    public class LogManager
    {
        string startupPath ;
        string dirPath ;
        string logDataPath;

        Stopwatch stopwatch;
        
        
        public void SaveLog(InitConfig initConfig, DataProgress dataProgress, string objName, string tapPointX, string tapPointY,string isClickable)
        {
            try 
            {
                using (StreamWriter file = new StreamWriter($@"{logDataPath}", true, Encoding.UTF8))
                {
                    file.WriteLine(string.Format("{0},{1},{2},{3},{4},{5}", dataProgress.CurrentScene, objName, tapPointX, tapPointY, stopwatch.ElapsedMilliseconds.ToString(), isClickable));
                }

                Debug.Print(string.Format("{0},{1},{2},{3},{4},{5}", dataProgress.CurrentScene, objName, tapPointX, tapPointY, stopwatch.ElapsedMilliseconds.ToString(), isClickable));
            }
            catch (Exception )
            {
                Debug.Print("書き込み失敗");
            }
            
        }

        public void StartLog(InitConfig initConfig, DataProgress dataProgress)
        {
            this.startupPath = FileUtils.GetStartupPath();
            this.dirPath = $"./Log/{initConfig.userName}/Chapter{dataProgress.CurrentChapter.ToString()}";
            this.logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter{dataProgress.CurrentChapter.ToString()}Log {DateTime.Now.ToString().Replace("/", "").Replace(":", "")}.csv");

            this.stopwatch = new Stopwatch();
            try
            {
                if (!Directory.Exists(dirPath))
                {
                    DirectoryUtils.SafeCreateDirectory(dirPath);
                }
                if (!File.Exists(logDataPath))
                {
                    File.Create($@"{logDataPath}").Close();

                    using (StreamWriter file = new StreamWriter($@"{logDataPath}", true, Encoding.UTF8))
                    {
                        file.WriteLine($"RecordingDate,{DateTime.Now.ToString()}");
                        file.WriteLine($"WindowSize,{Application.Current.MainWindow.ActualWidth.ToString()}×{Application.Current.MainWindow.ActualHeight.ToString()}");
                        file.WriteLine("");
                        file.WriteLine($"PlayScene,TapObjectName,TapPointX,TapPointY,TapTime[ms],isClickable");
                    }
                }

                stopwatch.Start();
                
            }
            catch
            {
                MessageBox.Show("Logファイルの作成に失敗しました。");
            }
            
        }
    }
}
