using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Verde.Utility
{
    class ImageProcessing
    {
        public static void SetImageToCenter(Canvas canvas, Image image)
        {
            Point pos = new Point((canvas.Width - image.Width) / 2, (canvas.Height - image.Height) / 2);
            ImageProcessing.SetImage(canvas, pos, image);
        }

        public static void SetImage(Canvas canvas, Point pos, Image image)
        {
            Canvas.SetLeft(image, pos.X);
            Canvas.SetTop(image, pos.Y);
            canvas.Children.Add(image);
        }

        public static BitmapDecoder GetMultiImageFromResource(string strImagePath)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var stmImage = assembly.GetManifestResourceStream(strImagePath);

            return BitmapDecoder.Create(stmImage, BitmapCreateOptions.None, BitmapCacheOption.Default);
        }

        public static Image GetStillImageFromResource(string strImagePath)
        {
            var frameImage = ImageProcessing.GetMultiImageFromResource(strImagePath).Frames.First();

            Image image = new Image();
            image.Source = frameImage;
            image.Width = frameImage.PixelWidth;
            image.Height = frameImage.PixelHeight;

            return image;
        }
    }

    class AnimationGif
    {
        private DispatcherTimer timerInterval = null;
        private BitmapDecoder decAnimationGif;
        private int idxFrame = 0;

        public Image FrameImage { set; get; }

        public AnimationGif(string strImagePath, int nInterval)
        {
            this.decAnimationGif = ImageProcessing.GetMultiImageFromResource(strImagePath);
            BitmapFrame frame = this.decAnimationGif.Frames.First();

            this.FrameImage = new Image();
            this.FrameImage.Width = frame.PixelWidth;
            this.FrameImage.Height = frame.PixelHeight;

            timerInterval = new DispatcherTimer();
            timerInterval.Interval = new TimeSpan(0, 0, 0, 0, nInterval);
            timerInterval.Tick += new EventHandler(AnimationHandler);
            timerInterval.Start();
        }

        private void AnimationHandler(object sender, EventArgs e)
        {
            if (++idxFrame >= this.decAnimationGif.Frames.Count) {
                idxFrame = 0;
            }

            this.FrameImage.Source = this.decAnimationGif.Frames[idxFrame];
        }
    }
}
