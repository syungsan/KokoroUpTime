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
    /// Page1.xaml の相互作用ロジック
    /// </summary>
    public partial class Page1 : Page
    {
        public Page1()
        {
            InitializeComponent();
        }
    }
    /*
     if (button.Name == "NameButton")
            {
                this.CanvasGrid.Visibility = Visibility.Visible;
                this.WritingGrid.Visibility = Visibility.Hidden;
            }
            if (button.Name == "PenButton")
            {
                WritingCanvas.EditingMode = InkCanvasEditingMode.Ink;
            }
            if (button.Name == "EraserButton")
            {
                WritingCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
            }
            if (button.Name == "AllEraseButton")
            {
                WritingCanvas.Strokes.Clear();
            }
            if (button.Name == "FinishButton")
            {
                if(WritingCanvas.Strokes != null)
                {
                    // ストロークが描画されている境界を取得
                    Rect rectBounds = WritingCanvas.Strokes.GetBounds();

// 描画先を作成
DrawingVisual dv = new DrawingVisual();
DrawingContext dc = dv.RenderOpen();
// 描画エリアの位置補正（補正しないと黒い部分ができてしまう）
dc.PushTransform(new TranslateTransform(-rectBounds.X, -rectBounds.Y));

                    // 描画エリア(dc)に四角形を作成
                    // 四角形の大きさはストロークが描画されている枠サイズとし、
                    // 背景色はInkCanvasコントロールと同じにする
                    dc.DrawRectangle(WritingCanvas.Background, null, rectBounds);

                    // 上記で作成した描画エリア(dc)にInkCanvasのストロークを描画
                    WritingCanvas.Strokes.Draw(dc);
                    dc.Close();

                    // ビジュアルオブジェクトをビットマップに変換する
                    RenderTargetBitmap rtb = new RenderTargetBitmap(
                        (int)rectBounds.Width, (int)rectBounds.Height,
                        96, 96,
                        PixelFormats.Default);
rtb.Render(dv);

                    // ビットマップエンコーダー変数の宣言
                    BitmapEncoder enc = null;
enc = new BmpBitmapEncoder();


                    if (enc != null)
                    {
                        // ビットマップフレームを作成してエンコーダーにフレームを追加する
                        enc.Frames.Add(BitmapFrame.Create(rtb));
                        // ファイルに書き込む
                        System.IO.Stream stream = System.IO.File.Create(@"C:\Users/maglab/Documents/Github/KoKoroUpTime/KoKoroUpTime/Log/Name.bmp");
enc.Save(stream);
                        stream.Close();
                    }
                    MessageBox.Show("start");
                    this.NameText.Source= new BitmapImage(new Uri("/Log/Name.bmp", UriKind.RelativeOrAbsolute));
                    MessageBox.Show("end");
}
                this.CanvasGrid.Visibility = Visibility.Hidden;
                this.WritingGrid.Visibility = Visibility.Visible;

            }
            if (button.Name =="DecisionButton")
            {
                this.WritingGrid.Visibility = Visibility.Hidden;

                this.scenarioCount += 1;
                this.ScenarioPlay();
            }

        }
    */
}
