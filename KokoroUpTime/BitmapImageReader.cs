using System;
using System.Windows.Media.Imaging;

namespace BitmapImageReader
{
    public static class BitmapImageReader
    {
        public static BitmapImage GifImageReader_Task(string _gifFile)
        {
            var gifImage = new BitmapImage();

            gifImage.BeginInit();
            gifImage.UriSource = new Uri($"/Images/{_gifFile}", UriKind.Relative);
            gifImage.EndInit();
            gifImage.Freeze();

            return gifImage;
        }
    }
}
