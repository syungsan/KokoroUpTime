using FileIOUtils;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KokoroUpTime
{
    public class LogManager
    {
        string startupPath ;
        string dirPath ;
        string logDataPath;
        int logCount = 0;
        object gameScreen = null;
        Stopwatch stopwatch;
        InitConfig initConfig = null;
        DataProgress dataProgress = null;
        
        
        public void SaveLog(string objName, string tapPointX, string tapPointY,string isClickable)
        {
            try
            {
                using (StreamWriter file = new StreamWriter($@"{logDataPath}", true, Encoding.UTF8))
                {
                    file.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6}", this.dataProgress.CurrentScene, objName, tapPointX, tapPointY, (stopwatch.ElapsedMilliseconds / 100).ToString(), isClickable));
                }

                Debug.Print(string.Format("{0},{1},{2},{3},{4},{5},{6}", this.dataProgress.CurrentScene, objName, tapPointX, tapPointY, (stopwatch.ElapsedMilliseconds / 100).ToString(), isClickable));
                this.CaptureGameScreen();

                logCount++;
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }

        }

        public void StartLog(InitConfig _initConfig, DataProgress _dataProgress,object _gameScreen)
        {
            this.initConfig = _initConfig;
            this.dataProgress = _dataProgress;
            this.gameScreen = _gameScreen;
            this.logCount = 0;

            this.startupPath = FileUtils.GetStartupPath();
            this.dirPath = $"./Log/{this.initConfig.userName}/Chapter{this.dataProgress.CurrentChapter.ToString()}/{DateTime.Now.ToString("yyyy-MMdd-HHmmss")}";
            this.logDataPath = Path.Combine(startupPath, $"{dirPath}/Chapter{this.dataProgress.CurrentChapter.ToString()}Log_{DateTime.Now.ToString("yyyy-MMdd-HHmmss ")}.csv");

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
                        file.WriteLine($"PlayScene,TapObjectName,TapPointX,TapPointY,TapTime[100ms],isClickable");
                    }
                }

                stopwatch.Start();
                
            }
            catch
            {
                MessageBox.Show("Logファイルの作成に失敗しました。");
            }
            
        }

        private void CaptureGameScreen()
        {
            var visual = gameScreen;
            var bounds = VisualTreeHelper.GetDescendantBounds((Visual)visual);

            var bitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, 96.0, 96.0, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                var vb = new VisualBrush((Visual)visual);
                dc.DrawRectangle(vb, null, bounds);
            }
            bitmap.Render(dv);
           
            string folderPath = $"{this.dirPath}/Capture";
            string nameBmp = $"capture_screen_{logCount}.png";
            string nameBmpPath = System.IO.Path.Combine(folderPath, nameBmp);
            var startupPath = FileUtils.GetStartupPath();

            if (!Directory.Exists(folderPath))
            {
                DirectoryUtils.SafeCreateDirectory(folderPath);
            }
            if (File.Exists($@"{startupPath}/{nameBmpPath}"))
            {
                File.Delete($@"{startupPath}/{nameBmpPath}");
            }

            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(bitmap));

            // ファイルのパスは仮(ISF形式で保存)
            using (var stream = File.Create($@"{startupPath}/{nameBmpPath}"))
            {
                png.Save(stream);
            }

            var pngmap = new BitmapImage();
            pngmap.BeginInit();
            pngmap.CacheOption = BitmapCacheOption.OnLoad;    //ココ
            pngmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;  //ココ
            pngmap.UriSource = new Uri($@"{startupPath}/{nameBmpPath}", UriKind.Absolute);
            pngmap.EndInit();
            pngmap.Freeze();
        }
    }
}
