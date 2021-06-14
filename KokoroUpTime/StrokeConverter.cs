using FileIOUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KokoroUpTime
{
    class StrokeConverter
    {
        public void ConvertToBmpImage(InkCanvas canvas, StrokeCollection strokes, string nameBmp,string userName,int currentChapter)
        {
            // ストロークが描画されているCanvasのサイズを取得
            System.Windows.Rect rectBounds = new System.Windows.Rect(0, 0, canvas.ActualWidth, canvas.ActualHeight);

            // 描画先を作成
            DrawingVisual dv = new DrawingVisual();
            DrawingContext dc = dv.RenderOpen();

            // 描画エリアの位置補正（補正しないと黒い部分ができてしまう）
            dc.PushTransform(new TranslateTransform(-rectBounds.X, -rectBounds.Y));

            // 描画エリア(dc)に四角形を作成
            // 四角形の大きさはストロークが描画されている枠サイズとし、
            // 背景色はInkCanvasコントロールと同じにする
            dc.DrawRectangle(canvas.Background, null, rectBounds);

            // 上記で作成した描画エリア(dc)にInkCanvasのストロークを描画
            strokes.Draw(dc);
            dc.Close();

            // ビジュアルオブジェクトをビットマップに変換する
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)rectBounds.Width, (int)rectBounds.Height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);

            //仮置き
            string dirPath = $"./Log/{userName}/Chapter{currentChapter}";

            string nameBmpPath = System.IO.Path.Combine(dirPath, nameBmp);
            var startupPath = FileUtils.GetStartupPath();

            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(rtb));

            if (!Directory.Exists(dirPath))
            {
                DirectoryUtils.SafeCreateDirectory(dirPath);
            }

            if (File.Exists($@"{startupPath}/{nameBmpPath}"))
            {
                File.Delete($@"{startupPath}/{nameBmpPath}");
            }


            // ファイルのパスは仮
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
