using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Verde.Utility
{
    class ImageProcessing
    {
        public static void DrawImageToCenter(Canvas canvas, BitmapImage bmp)
        {
            Point pos = new Point((canvas.Width / 2) - (bmp.PixelWidth / 2), (canvas.Height / 2) - (bmp.PixelHeight / 2));
            ImageProcessing.DrawImage(canvas, pos, bmp);
        }

        public static void DrawImage(Canvas canvas, Point pos, BitmapImage bmp)
        {
            Image image = new Image();
            image.Source = bmp;
            image.Width = bmp.PixelWidth;
            image.Height = bmp.PixelHeight;

            Canvas.SetLeft(image, pos.X);
            Canvas.SetTop(image, pos.Y);
            canvas.Children.Add(image);
        }
    }
}
