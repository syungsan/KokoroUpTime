using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace OutlineTextMaker
{
    /// <summary>
    /// 縁取り付きのテキストを提供します。
    /// </summary>
    /// <remarks>
    /// <para>Based on "WPF – 文字の縁取りをする"</para>
    /// <para>http://astel-labs.net/blog/diary/2012/05/06-1.html</para>
    /// </remarks>
    [ContentProperty("Text")]
    public class OutlineText : FrameworkElement
    {
        #region 定数

        /// <summary>
        /// 要素の最大幅を表します。
        /// </summary>
        private const double MAX_TEXT_WIDTH = 3579139.0D - 1.0D; // 処理途中で 0.5D を加算してもオーバーフローしないようにしておく

        /// <summary>
        /// 要素の最大高さを表します。
        /// </summary>
        private const double MAX_TEXT_HEIGHT = 3579139.0D - 1.0D; // 処理途中で 0.5D を加算してもオーバーフローしないようにしておく

        /// <summary>
        /// 既定の縁取りの幅を表します。
        /// </summary>
        private const double DEFAULT_OUTLINE_TICKNESS = 1.5D;

        /// <summary>
        /// 縁取りを右下方向にずらす値を設定します。
        /// </summary>
        /// <remarks>
        /// わずかに右下にずらしたほうが、小さいピクセルでの文字の見易さが向上するため。
        /// </remarks>
        private const double OUTLINE_OFFSET = 0.0D;

        #endregion

        #region フィールド

        /// <summary>
        /// <see cref="FormattedText"/> を保持します。
        /// </summary>
        protected FormattedText formattedText = null;

        /// <summary>
        /// <see cref="FormattedText"/> の <see cref="Geometry"/> を保持します。
        /// </summary>
        protected Geometry textGeometry = null;

        /// <summary>
        /// コンテンツ領域の背景を保持します。
        /// </summary>
        protected Rect backgroundRect = default(Rect);

        /// <summary>
        /// <see cref="Padding"/> のうち、正の値を保持します。
        /// </summary>
        protected Thickness positivePadding = default(Thickness);

        /// <summary>
        /// <see cref="Padding"/> のうち、負の値を保持します。
        /// </summary>
        protected Thickness negativePadding = default(Thickness);

        #endregion

        #region 依存関係プロパティ

        /// <summary>
        /// Background 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background", typeof(Brush), typeof(OutlineText),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Text 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(object), typeof(OutlineText),
            new FrameworkPropertyMetadata(null, FormattedTextInvalidated));

        /// <summary>
        /// TextAlignment 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
            "TextAlignment", typeof(TextAlignment), typeof(OutlineText),
            new FrameworkPropertyMetadata(TextAlignment.Left, FormattedTextUpdated));

        /// <summary>
        /// TextDecorations 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register(
            "TextDecorations", typeof(TextDecorationCollection), typeof(OutlineText),
            new FrameworkPropertyMetadata(null, FormattedTextUpdated));

        /// <summary>
        /// TextTrimming 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
            "TextTrimming", typeof(TextTrimming), typeof(OutlineText),
            new FrameworkPropertyMetadata(TextTrimming.None, FormattedTextUpdated));

        /// <summary>
        /// TextWrapping 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
            "TextWrapping", typeof(TextWrapping), typeof(OutlineText),
            new FrameworkPropertyMetadata(TextWrapping.NoWrap, FormattedTextUpdated));

        /// <summary>
        /// Foreground 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground", typeof(Brush), typeof(OutlineText),
            new FrameworkPropertyMetadata(Brushes.Black, ForegroundUpdated));

        /// <summary>
        /// Outline 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty OutlineProperty = DependencyProperty.Register(
            "Outline", typeof(Brush), typeof(OutlineText),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// OutlineThickness 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty OutlineThicknessProperty = DependencyProperty.Register(
            "OutlineThickness", typeof(double), typeof(OutlineText),
            new FrameworkPropertyMetadata(DEFAULT_OUTLINE_TICKNESS, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// OutlineVisibility 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty OutlineVisibilityProperty = DependencyProperty.Register(
            "OutlineVisibility", typeof(Visibility), typeof(OutlineText),
            new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// FontFamily 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(FormattedTextUpdated));

        /// <summary>
        /// FontSize 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(FormattedTextUpdated));

        /// <summary>
        /// FontStretch 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(FormattedTextUpdated));

        /// <summary>
        /// FontStyle 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(FormattedTextUpdated));

        /// <summary>
        /// FontWeight 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(
            typeof(OutlineText), new FrameworkPropertyMetadata(FormattedTextUpdated));

        /// <summary>
        /// Padding 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
            "Padding", typeof(Thickness), typeof(OutlineText),
            new FrameworkPropertyMetadata(PaddingUpdated));

        /// <summary>
        /// ClipBackgroundToText 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty ClipBackgroundToTextProperty = DependencyProperty.Register(
            "ClipBackgroundToText", typeof(bool), typeof(OutlineText),
            new FrameworkPropertyMetadata(false, FormattedTextUpdated));

        /// <summary>
        /// FormatString 依存関係プロパティを識別します。このフィールドは読み取り専用です。
        /// </summary>
        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register(
            "FormatString", typeof(string), typeof(OutlineText),
            new FrameworkPropertyMetadata(null, FormattedTextInvalidated));

        #endregion

        #region プロパティ

        /// <summary>
        /// コンテンツ領域の背景の塗りつぶしに使用する <see cref="Brush"/> を取得または設定します。
        /// </summary>
        /// <value>コンテンツ領域の背景の塗りつぶしに使用する <see cref="Brush"/>。既定値は、<c>null</c> です。</value>
        [Description("コンテンツ領域の背景の塗りつぶしに使用する Brush を取得または設定します。")]
        public Brush Background
        {
            get
            {
                return (Brush)GetValue(BackgroundProperty);
            }
            set
            {
                SetValue(BackgroundProperty, value);
            }
        }

        /// <summary>
        /// フォント ファミリを取得または設定します。
        /// </summary>
        [Category("Font")]
        [Description("フォント ファミリを取得または設定します。")]
        public FontFamily FontFamily
        {
            get
            {
                return (FontFamily)GetValue(FontFamilyProperty);
            }
            set
            {
                SetValue(FontFamilyProperty, value);
            }
        }

        /// <summary>
        /// フォント サイズを取得または設定します。
        /// </summary>
        [TypeConverter(typeof(FontSizeConverter))]
        [Category("Font")]
        [Description("フォント サイズを取得または設定します。")]
        public double FontSize
        {
            get
            {
                return (double)GetValue(FontSizeProperty);
            }
            set
            {
                SetValue(FontSizeProperty, value);
            }
        }

        /// <summary>
        /// フォント伸縮特性を取得または設定します。
        /// </summary>
        [Category("Font")]
        [Description("フォント伸縮特性を取得または設定します。")]
        public FontStretch FontStretch
        {
            get
            {
                return (FontStretch)GetValue(FontStretchProperty);
            }
            set
            {
                SetValue(FontStretchProperty, value);
            }
        }

        /// <summary>
        /// フォント スタイルを取得または設定します。
        /// </summary>
        [Category("Font")]
        [Description("フォント スタイルを取得または設定します。")]
        public FontStyle FontStyle
        {
            get
            {
                return (FontStyle)GetValue(FontStyleProperty);
            }
            set
            {
                SetValue(FontStyleProperty, value);
            }
        }

        /// <summary>
        /// フォントの太さを取得または設定します。
        /// </summary>
        [Category("Font")]
        [Description("フォントの太さを取得または設定します。")]
        public FontWeight FontWeight
        {
            get
            {
                return (FontWeight)GetValue(FontWeightProperty);
            }
            set
            {
                SetValue(FontWeightProperty, value);
            }
        }

        /// <summary>
        /// テキスト コンテンツに適用する <see cref="Brush"/> を取得または設定します。
        /// </summary>
        /// <value>テキスト コンテンツに適用するために使用するブラシ。既定値は、<see cref="Brushes.Black"/> です。</value>
        [Description("テキスト コンテンツに適用する Brush を取得または設定します。")]
        public Brush Foreground
        {
            get
            {
                return (Brush)GetValue(ForegroundProperty);
            }
            set
            {
                SetValue(ForegroundProperty, value);
            }
        }

        /// <summary>
        /// テキスト コンテンツの縁取りに適用する <see cref="Brush"/> を取得または設定します。
        /// </summary>
        /// <value>テキスト コンテンツの縁取りに適用するために使用するブラシ。既定値は、<c>null</c> です。</value>
        [Description("テキスト コンテンツの縁取りに適用する Brush を取得または設定します。")]
        public Brush Outline
        {
            get
            {
                return (Brush)GetValue(OutlineProperty);
            }
            set
            {
                SetValue(OutlineProperty, value);
            }
        }

        /// <summary>
        /// テキスト コンテンツの縁取りに適用する幅を取得または設定します。
        /// </summary>
        /// <value>テキスト コンテンツの縁取りに適用する幅。既定値は、<see cref="DEFAULT_OUTLINE_TICKNESS"/> です。</value>
        [Category("Appearance")]
        [Description("テキスト コンテンツの縁取りに適用する幅を取得または設定します。")]
        public double OutlineThickness
        {
            get
            {
                return (double)GetValue(OutlineThicknessProperty);
            }
            set
            {
                SetValue(OutlineThicknessProperty, value);
            }
        }

        /// <summary>
        /// テキスト コンテンツの縁取りに適用する <see cref="Visibility"/> を取得または設定します。
        /// </summary>
        /// <value>テキスト コンテンツの縁取りに適用する <see cref="Visibility"/>。既定値は、<see cref="Visibility.Visible"/> です。</value>
        [Category("Appearance")]
        [Description("テキスト コンテンツの縁取りに適用する Visibility を取得または設定します。")]
        public Visibility OutlineVisibility
        {
            get
            {
                return (Visibility)GetValue(OutlineVisibilityProperty);
            }
            set
            {
                SetValue(OutlineVisibilityProperty, value);
            }
        }

        /// <summary>
        /// テキスト コンテンツを取得または設定します。
        /// </summary>
        [Description("テキスト コンテンツを取得または設定します。")]
        public object Text
        {
            get
            {
                return GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        /// <summary>
        /// テキスト コンテンツの水平方向の配置を示す値を取得または設定します。
        /// </summary>
        /// <value>テキスト コンテンツの水平方向の配置を示す <see cref="TextAlignment"/>。既定値は、<see cref="TextAlignment.Left"/> です。</value>
        [Category("Text")]
        [Description("テキスト コンテンツの水平方向の配置を示す値を取得または設定します。")]
        public TextAlignment TextAlignment
        {
            get
            {
                return (TextAlignment)GetValue(TextAlignmentProperty);
            }
            set
            {
                SetValue(TextAlignmentProperty, value);
            }
        }

        /// <summary>
        /// テキストに適用する効果を取得または設定します。
        /// </summary>
        /// <value>テキスト コンテンツの水平方向の配置を示す <see cref="TextDecorationCollection"/>。既定値は、<c>null</c> (テキスト装飾は適用されません)。</value>
        [Category("Text")]
        [Description("テキストに適用する効果を取得または設定します。")]
        public TextDecorationCollection TextDecorations
        {
            get
            {
                return (TextDecorationCollection)GetValue(TextDecorationsProperty);
            }
            set
            {
                SetValue(TextDecorationsProperty, value);
            }
        }

        /// <summary>
        /// コンテンツ領域いっぱいになったときに使用するテキストのトリミング動作を取得または設定します。
        /// </summary>
        /// <value>コンテンツ領域いっぱいになったときに使用する <see cref="TextTrimming"/>。既定値は、<see cref="TextTrimming.None"/> です。</value>
        [Description("コンテンツ領域いっぱいになったときに使用するテキストのトリミング動作を取得または設定します。")]
        public TextTrimming TextTrimming
        {
            get
            {
                return (TextTrimming)GetValue(TextTrimmingProperty);
            }
            set
            {
                SetValue(TextTrimmingProperty, value);
            }
        }

        /// <summary>
        /// テキストをラップするかどうかを取得または設定します。
        /// </summary>
        /// <value>テキストをラップするかどうかを指定する <see cref="TextWrapping"/>。既定値は、<see cref="TextWrapping.NoWrap"/> です。</value>
        [Description("テキストをラップするかどうかを取得または設定します。")]
        public TextWrapping TextWrapping
        {
            get
            {
                return (TextWrapping)GetValue(TextWrappingProperty);
            }
            set
            {
                SetValue(TextWrappingProperty, value);
            }
        }

        /// <summary>
        /// コンテンツ領域の境界とコンテンツとの間に埋め込むスペースの幅を示す値を取得または設定します。
        /// </summary>
        /// <value>適用する埋め込みの量を指定する <see cref="Thickness"/> 構造体。デバイス非依存のピクセル単位で指定します。 既定値は、<c>0</c> です。</value>
        [Category("Layout")]
        [Description("コンテンツ領域の境界とコンテンツとの間に埋め込むスペースの幅を示す値を取得または設定します。")]
        public Thickness Padding
        {
            get
            {
                return (Thickness)GetValue(PaddingProperty);
            }
            set
            {
                SetValue(PaddingProperty, value);
            }
        }

        /// <summary>
        /// 背景をテキスト領域に合わせるかどうかを表す値を取得または設定します。
        /// </summary>
        /// <value>背景をテキスト領域に合わせるかどうかを表す値。既定値は、<c>false</c> です。</value>
        [Category("Appearance")]
        [Description("背景をテキスト領域に合わせるかどうかを表す値を取得または設定します。")]
        public bool ClipBackgroundToText
        {
            get
            {
                return (bool)GetValue(ClipBackgroundToTextProperty);
            }
            set
            {
                SetValue(ClipBackgroundToTextProperty, value);
            }
        }

        /// <summary>
        /// テキスト コンテンツに適用する書式文字列を取得または設定します。 
        /// </summary>
        /// <value>テキスト コンテンツに適用する書式文字列。</value>
        [Description("テキスト コンテンツに適用する書式文字列を取得または設定します。")]
        public string FormatString
        {
            get
            {
                return (string)GetValue(FormatStringProperty);
            }
            set
            {
                SetValue(FormatStringProperty, value);
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>
        /// <see cref="OutlineText"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public OutlineText()
        {
            TextDecorations = new TextDecorationCollection();
        }

        #endregion

        #region メソッド

        /// <summary>
        /// <see cref="OutlineText"/> の内容を描画します。
        /// </summary>
        /// <param name="drawingContext">特定の要素の描画の手順です。 このコンテキストは、レイアウト システムで提供されています。</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            // 背景の描画
            if (Background != null)
            {
                drawingContext.DrawRectangle(Background, null, backgroundRect);
            }

            if (string.IsNullOrEmpty(GetFormattedString()) == true)
            {
                return;
            }

            // 描画があふれないようにクリップする
            drawingContext.PushClip(new RectangleGeometry(
                new Rect(
                    -negativePadding.Left,
                    -negativePadding.Top,
                    ActualWidth + negativePadding.Left + negativePadding.Right,
                    ActualHeight + negativePadding.Top + negativePadding.Bottom)));

            EnsureFormattedText();

            double offsetX = 0.0D;

            // これを行うとグリッドに配置した TextAlignment.Right の部品の挙動が
            // セル幅を縮めていくと良くないのでコメント。
#if false
            // TextAlignment.Right であふれている際は、描画起点座標を修正
            if ((formattedText.TextAlignment == TextAlignment.Right) &&
                ((ActualWidth - positivePadding.Left - positivePadding.Right) < formattedText.MaxTextWidth))
            {
                offsetX = (ActualWidth - positivePadding.Left - positivePadding.Right) - formattedText.MaxTextWidth;
            }
#endif

            // Center, Right の描画座標に、末尾のスペースを考慮させる
            if (formattedText.TextAlignment == TextAlignment.Center)
            {
                // 末尾スペース考慮との差分の 50% 分、描画起点座標を左にずらす
                offsetX = offsetX - (formattedText.WidthIncludingTrailingWhitespace - formattedText.Width) / 2;
            }
            if (formattedText.TextAlignment == TextAlignment.Right)
            {
                // 末尾スペース考慮との差分の 100% 分、描画起点座標を左にずらす
                offsetX = offsetX - (formattedText.WidthIncludingTrailingWhitespace - formattedText.Width);
            }

            EnsureGeometry(offsetX);

            // 縁取りの描画
            if ((OutlineVisibility == Visibility.Visible) && (Outline != Brushes.Transparent) && (Outline != null) && (OutlineThickness > 0))
            {
                // DrawGeometry はパスの中心から OutlineThickness の太さで描画するので、
                // 外側の太さとしては、2 倍にして描画させる

                Pen pen = new Pen(Outline, OutlineThickness * 2)
                {
                    LineJoin = PenLineJoin.Round
                };

                drawingContext.DrawGeometry(Outline, pen, textGeometry);
            }

            // DrawGeometry は ClearType が効かないので、改めて文字を描画する
            // MEMO: Window の AllowsTransparency="True" とした場合、グレースケールになってしまう。
            //       この場合、Window の各要素の CrearType は
            //       RenderOptions.ClearTypeHint="Enabled" によって改善する部品もあるが、
            //       OnRender の場合、ClearType に強制的に設定する方法が現状存在しない。
            drawingContext.DrawText(formattedText, new Point(positivePadding.Left + offsetX, positivePadding.Top));
        }

        /// <summary>
        /// 子要素に必要なレイアウトのサイズを測定し、<see cref="OutlineText"/> のサイズを決定します。
        /// </summary>
        /// <param name="availableSize">
        /// この要素が子要素に提供できる使用可能なサイズ。
        /// あらゆるコンテンツに要素がサイズを合わせることを示す値として、無限大を指定できます。
        /// </param>
        /// <returns>子要素のサイズの計算に基づいて、この要素が判断したレイアウト時に必要なサイズ。</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            EnsureFormattedText();

            Size measuredSize = default(Size);

            // 文字が指定されていなかった場合
            if (string.IsNullOrEmpty(GetFormattedString()) == true)
            {
                measuredSize = availableSize;

                if (double.IsInfinity(availableSize.Width) == true)
                {
                    measuredSize.Width = positivePadding.Left + positivePadding.Right;
                }
                if (double.IsInfinity(availableSize.Height) == true)
                {
                    measuredSize.Height = positivePadding.Top + positivePadding.Bottom;
                }

                return measuredSize;
            }

            // 基本的には、与えられたサイズを使うが、
            // もし、それより少ないサイズでよければ、小さいサイズを使う、という考え方になる。

            if (TextWrapping == TextWrapping.NoWrap)
            {
                // 幅と高さを計算し、もし、利用可能サイズより小さかったら、小さいサイズを返す
                formattedText.MaxTextWidth = MAX_TEXT_WIDTH;
                formattedText.MaxTextHeight = MAX_TEXT_HEIGHT;

                // 計測された幅を最大幅とする
                formattedText.MaxTextWidth = formattedText.WidthIncludingTrailingWhitespace;

                if (double.IsInfinity(availableSize.Width) == true)
                {
                    measuredSize.Width = formattedText.WidthIncludingTrailingWhitespace + positivePadding.Left + positivePadding.Right;
                }
                else if ((formattedText.WidthIncludingTrailingWhitespace + positivePadding.Left + positivePadding.Right) > availableSize.Width)
                {
                    measuredSize.Width = availableSize.Width;
                }
                else
                {
                    measuredSize.Width = formattedText.WidthIncludingTrailingWhitespace + positivePadding.Left + positivePadding.Right;
                }

                if (double.IsInfinity(availableSize.Height) == true)
                {
                    measuredSize.Height = formattedText.Height + positivePadding.Top + positivePadding.Bottom;
                }
                else if ((formattedText.Height + positivePadding.Top + positivePadding.Bottom) > availableSize.Height)
                {
                    measuredSize.Height = availableSize.Height;
                }
                else
                {
                    measuredSize.Height = formattedText.Height + positivePadding.Top + positivePadding.Bottom;
                }
            }
            else
            {
                // 高さを計算し、もし、利用可能サイズより小さかったら、小さいサイズを返す
                if ((double.IsInfinity(availableSize.Width) == true) || (availableSize.Width > MAX_TEXT_WIDTH))
                {
                    formattedText.MaxTextWidth = MAX_TEXT_WIDTH;
                }
                else
                {
                    formattedText.MaxTextWidth = availableSize.Width - positivePadding.Left - positivePadding.Right + 0.5D; // LayoutRounding の影響で整数に丸められると描画できなくなることがあるので、丸め分大きくする
                }
                formattedText.MaxTextHeight = MAX_TEXT_HEIGHT;

                measuredSize.Width = formattedText.WidthIncludingTrailingWhitespace + positivePadding.Left + positivePadding.Right;

                if (double.IsInfinity(availableSize.Height) == true)
                {
                    measuredSize.Height = formattedText.Height + positivePadding.Top + positivePadding.Bottom;
                }
                else if ((formattedText.Height + positivePadding.Top + positivePadding.Bottom) > availableSize.Height)
                {
                    measuredSize.Height = availableSize.Height;
                }
                else
                {
                    measuredSize.Height = formattedText.Height + positivePadding.Top + positivePadding.Bottom;
                }
            }

            EnsureBackground(measuredSize);

            return measuredSize;
        }

        /// <summary>
        /// 子要素を配置し、<see cref="OutlineText"/> のサイズを決定します。
        /// </summary>
        /// <param name="finalSize">この要素が要素自体と子を配置するために使用する親の末尾の領域。</param>
        /// <returns>使用する実際のサイズ。</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            EnsureFormattedText();

            // 最終的なサイズを設定する
            if (formattedText != null)
            {
                if (finalSize.Width > (formattedText.WidthIncludingTrailingWhitespace + positivePadding.Left + positivePadding.Right))
                {
                    formattedText.MaxTextWidth = finalSize.Width - positivePadding.Left - positivePadding.Right + 0.5D; // LayoutRounding の影響で整数に丸められると描画できなくなることがあるので、丸め分大きくする
                }
                if (finalSize.Height > (formattedText.Height + positivePadding.Top + positivePadding.Bottom))
                {
                    formattedText.MaxTextHeight = finalSize.Height - positivePadding.Top - positivePadding.Bottom + 0.5D; // LayoutRounding の影響で整数に丸められると描画できなくなることがあるので、丸め分大きくする
                }
            }

            textGeometry = null;

            EnsureBackground(finalSize);

            return finalSize;
        }

        /// <summary>
        /// <see cref="Foreground"/> が更新された際に発生します。
        /// </summary>
        /// <param name="dependencyObject">プロパティの値が変更された <see cref="DependencyObject"/>。</param>
        /// <param name="e">このプロパティの有効値に対する変更を追跡するイベントによって発行されるイベント データ。</param>
        private static void ForegroundUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            OutlineText outlineText = (OutlineText)dependencyObject;

            outlineText.UpdateFormattedText();
            outlineText.InvalidateVisual();
        }

        /// <summary>
        /// <see cref="FormattedText"/> が無効になった際に発生します。
        /// </summary>
        /// <param name="dependencyObject">プロパティの値が変更された <see cref="DependencyObject"/>。</param>
        /// <param name="e">このプロパティの有効値に対する変更を追跡するイベントによって発行されるイベント データ。</param>
        private static void FormattedTextInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            OutlineText outlineText = (OutlineText)dependencyObject;

            outlineText.OnFormattedTextInvalidated(e);
        }

        /// <summary>
        /// <see cref="FormattedText"/> が無効になった際に発生します。
        /// </summary>
        /// <param name="e">このプロパティの有効値に対する変更を追跡するイベントによって発行されるイベント データ。</param>
        protected virtual void OnFormattedTextInvalidated(DependencyPropertyChangedEventArgs e)
        {
            formattedText = null;
            textGeometry = null;

            InvalidateMeasure();
            InvalidateVisual();
        }

        /// <summary>
        /// <see cref="FormattedText"/> が更新された際に発生します。
        /// </summary>
        /// <param name="dependencyObject">プロパティの値が変更された <see cref="DependencyObject"/>。</param>
        /// <param name="e">このプロパティの有効値に対する変更を追跡するイベントによって発行されるイベント データ。</param>
        private static void FormattedTextUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            OutlineText outlineText = (OutlineText)dependencyObject;

            outlineText.OnFormattedTextUpdated(e);
        }

        /// <summary>
        /// <see cref="FormattedText"/> が更新された際に発生します。
        /// </summary>
        /// <param name="e">このプロパティの有効値に対する変更を追跡するイベントによって発行されるイベント データ。</param>
        protected virtual void OnFormattedTextUpdated(DependencyPropertyChangedEventArgs e)
        {
            UpdateFormattedText();
            textGeometry = null;

            InvalidateMeasure();
            InvalidateVisual();
        }

        /// <summary>
        /// <see cref="Padding"/> が更新された際に発生します。
        /// </summary>
        /// <param name="dependencyObject">プロパティの値が変更された <see cref="DependencyObject"/>。</param>
        /// <param name="e">このプロパティの有効値に対する変更を追跡するイベントによって発行されるイベント データ。</param>
        private static void PaddingUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            OutlineText outlineText = (OutlineText)dependencyObject;

            outlineText.OnPaddingUpdated(e);
        }

        /// <summary>
        /// <see cref="Padding"/> が更新された際に発生します。
        /// </summary>
        /// <param name="e">このプロパティの有効値に対する変更を追跡するイベントによって発行されるイベント データ。</param>
        protected virtual void OnPaddingUpdated(DependencyPropertyChangedEventArgs e)
        {
            textGeometry = null;

            #region positivePadding の計算

            positivePadding = Padding;

            if (positivePadding.Left == double.NaN)
            {
                positivePadding.Left = 0;
            }
            else if (positivePadding.Left < 0)
            {
                positivePadding.Left = 0;
            }

            if (positivePadding.Top == double.NaN)
            {
                positivePadding.Top = 0;
            }
            else if (positivePadding.Top < 0)
            {
                positivePadding.Top = 0;
            }

            if (positivePadding.Right == double.NaN)
            {
                positivePadding.Right = 0;
            }
            else if (positivePadding.Right < 0)
            {
                positivePadding.Right = 0;
            }

            if (positivePadding.Bottom == double.NaN)
            {
                positivePadding.Bottom = 0;
            }
            else if (positivePadding.Bottom < 0)
            {
                positivePadding.Bottom = 0;
            }

            #endregion

            #region negativePadding の計算

            negativePadding = Padding;

            if (negativePadding.Left == double.NaN)
            {
                negativePadding.Left = 0;
            }
            else if (negativePadding.Left > 0)
            {
                negativePadding.Left = 0;
            }
            else
            {
                negativePadding.Left *= -1;
            }

            if (negativePadding.Top == double.NaN)
            {
                negativePadding.Top = 0;
            }
            else if (negativePadding.Top > 0)
            {
                negativePadding.Top = 0;
            }
            else
            {
                negativePadding.Top *= -1;
            }

            if (negativePadding.Right == double.NaN)
            {
                negativePadding.Right = 0;
            }
            else if (negativePadding.Right > 0)
            {
                negativePadding.Right = 0;
            }
            else
            {
                negativePadding.Right *= -1;
            }

            if (negativePadding.Bottom == double.NaN)
            {
                negativePadding.Bottom = 0;
            }
            else if (negativePadding.Bottom > 0)
            {
                negativePadding.Bottom = 0;
            }
            else
            {
                negativePadding.Bottom *= -1;
            }

            #endregion

            InvalidateMeasure();
            InvalidateVisual();
        }

        /// <summary>
        /// フォーマットを加味したテキスト コンテンツを返します。
        /// </summary>
        /// <returns>フォーマットを加味したテキスト コンテンツ。</returns>
        protected virtual string GetFormattedString()
        {
            string _text = null;

            if (Text == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(FormatString) != true)
            {
                _text = string.Format(FormatString, Text);
            }
            else
            {
                _text = Text.ToString();
            }

            return _text;
        }

        /// <summary>
        /// <see cref="formattedText"/> を再評価します。
        /// </summary>
        protected virtual void EnsureFormattedText()
        {
            if ((formattedText != null) || (string.IsNullOrEmpty(GetFormattedString()) == true))
            {
                return;
            }

            formattedText = new FormattedText(
                GetFormattedString(),
                CultureInfo.CurrentUICulture,
                FlowDirection,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                Brushes.Black,
                1.0D); // MEMO: High DPI や Per-Monitor DPI に対応する場合は、この部分に適切な値を設定する必要がある。

            UpdateFormattedText();
        }

        /// <summary>
        /// <see cref="formattedText"/> を更新します。
        /// </summary>
        protected virtual void UpdateFormattedText()
        {
            if (formattedText == null)
            {
                return;
            }

            if (TextWrapping == TextWrapping.NoWrap)
            {
                formattedText.MaxLineCount = 1;
            }
            else
            {
                formattedText.MaxLineCount = int.MaxValue;
            }

            formattedText.TextAlignment = TextAlignment;
            formattedText.Trimming = TextTrimming;

            formattedText.SetFontSize(FontSize);
            formattedText.SetFontStyle(FontStyle);
            formattedText.SetFontWeight(FontWeight);
            formattedText.SetFontFamily(FontFamily);
            formattedText.SetFontStretch(FontStretch);
            formattedText.SetTextDecorations(TextDecorations);
            formattedText.SetForegroundBrush(Foreground);
        }

        /// <summary>
        /// <see cref="textGeometry"/> を再評価します。
        /// </summary>
        /// <param name="offsetX">水平方向のオフセット値。</param>
        /// <param name="offsetY">垂直方向のオフセット値。</param>
        protected virtual void EnsureGeometry(double offsetX = 0, double offsetY = 0)
        {
            if (textGeometry == null)
            {
                EnsureFormattedText();

                if (string.IsNullOrEmpty(GetFormattedString()) != true)
                {
                    textGeometry = formattedText.BuildGeometry(new Point(positivePadding.Left + offsetX + OUTLINE_OFFSET, positivePadding.Top + offsetY + OUTLINE_OFFSET));
                }
            }
        }

        /// <summary>
        /// <see cref="backgroundRect"/> を再評価します。
        /// </summary>
        /// <param name="size">親のサイズ。</param>
        protected void EnsureBackground(Size size)
        {
            Size contentSize;

            if (formattedText == null)
            {
                contentSize = default(Size);
            }
            else
            {
                contentSize = new Size(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height);
            }

            if (ClipBackgroundToText == true)
            {
                switch (TextAlignment)
                {
                    case TextAlignment.Left:
                        backgroundRect = new Rect(
                            -negativePadding.Left,
                            -negativePadding.Top,
                            contentSize.Width + positivePadding.Left + positivePadding.Right + negativePadding.Left + negativePadding.Right,
                            contentSize.Height + positivePadding.Top + positivePadding.Bottom + negativePadding.Top + negativePadding.Bottom);
                        break;
                    case TextAlignment.Right:
                        backgroundRect = new Rect(
                            ActualWidth - contentSize.Width - positivePadding.Left - positivePadding.Right - negativePadding.Left,
                            -negativePadding.Top,
                            contentSize.Width + positivePadding.Left + positivePadding.Right + negativePadding.Left + negativePadding.Right,
                            contentSize.Height + positivePadding.Top + positivePadding.Bottom + negativePadding.Top + negativePadding.Bottom);
                        break;
                    case TextAlignment.Center:
                        backgroundRect = new Rect(
                            (ActualWidth - contentSize.Width) / 2.0D - positivePadding.Left - negativePadding.Left,
                            -negativePadding.Top,
                            contentSize.Width + positivePadding.Left + positivePadding.Right + negativePadding.Left + negativePadding.Right,
                            contentSize.Height + positivePadding.Top + positivePadding.Bottom + negativePadding.Top + negativePadding.Bottom);
                        break;
                    case TextAlignment.Justify:
                        backgroundRect = new Rect(
                            -negativePadding.Left,
                            -negativePadding.Top,
                            ActualWidth + negativePadding.Left + negativePadding.Right,
                            contentSize.Height + positivePadding.Top + positivePadding.Bottom + negativePadding.Top + negativePadding.Bottom);
                        break;
                    default:
                        // 通過することはない
                        // NOP
                        break;
                }
            }
            else
            {
                // Element の領域背景に色を付ける場合(TextBlock 互換)
                backgroundRect = new Rect(
                    -negativePadding.Left,
                    -negativePadding.Top,
                    size.Width + negativePadding.Left + negativePadding.Right,
                    size.Height + negativePadding.Top + negativePadding.Bottom);
            }
        }

        #endregion

    }
}
