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
using System.Windows.Media.Animation;

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

        public static BitmapDecoder CreateBitmapDecoderFromResource(string strImagePath)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var stmImage = assembly.GetManifestResourceStream(strImagePath);

            return BitmapDecoder.Create(stmImage, BitmapCreateOptions.None, BitmapCacheOption.Default);
        }

        public static Image GetStillImageFromResource(string strImagePath)
        {
            var frameImage = ImageProcessing.CreateBitmapDecoderFromResource(strImagePath).Frames.First();

            Image image = new Image();
            image.Source = frameImage;
            image.Width = frameImage.PixelWidth;
            image.Height = frameImage.PixelHeight;

            return image;
        }

        public static GifBitmapDecoder CreateGifBitmapDecoderFromResource(string strImagePath, out Stream stmImage)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            stmImage = assembly.GetManifestResourceStream(strImagePath);

            return new GifBitmapDecoder(stmImage, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        }

        public static void FadeOut(Image image, double nFadeTime, EventHandler OnCompleted)
        {
            DoubleAnimation animFader = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(nFadeTime)), FillBehavior.HoldEnd);
            if (OnCompleted != null) {
                animFader.Completed += new EventHandler(OnCompleted);
            }
            image.BeginAnimation(Rectangle.OpacityProperty, animFader);
        }
    }

    class AnimationGif
    {
        private DispatcherTimer timerInterval = null;
        private GifBitmapDecoder decAnimationGif;
        private int idxFrame = 0;
        private int[] arrDelays;
        private int nLoop;
        private int nLoopRemain;

        private Image imgFrame = null;
        public Image FrameImage { get { return this.imgFrame; } }

        public AnimationGif(string strImagePath)
        {
            Stream stmImage;
            this.decAnimationGif = ImageProcessing.CreateGifBitmapDecoderFromResource(strImagePath, out stmImage);

            stmImage.Seek(0, SeekOrigin.Begin);
            var tmpImage = new System.Drawing.Bitmap(stmImage);
            var piFrameDelay = tmpImage.GetPropertyItem(0x5100);
            this.arrDelays = new int[this.decAnimationGif.Frames.Count];
            for (int i = 0; i < this.arrDelays.Length; i++) {
                this.arrDelays[i] = BitConverter.ToInt32(piFrameDelay.Value, i * 4) * 10;
            }
            var piLoopCount = tmpImage.GetPropertyItem(0x5101);
            this.nLoop = BitConverter.ToInt16(piLoopCount.Value, 0);
            this.nLoopRemain = this.nLoop;

            BitmapFrame frame = this.decAnimationGif.Frames.First();
            this.imgFrame = new Image();
            this.imgFrame.Source = frame;
            this.imgFrame.Stretch = Stretch.None;
            this.imgFrame.Width = frame.PixelWidth;
            this.imgFrame.Height = frame.PixelHeight;

            this.timerInterval = new DispatcherTimer();
            this.timerInterval.Interval = new TimeSpan(0, 0, 0, 0, this.arrDelays[0]);
            this.timerInterval.Tick += new EventHandler(AnimationHandler);
            this.timerInterval.Start();
        }

        private void AnimationHandler(object sender, EventArgs e)
        {
            if (++this.idxFrame >= this.decAnimationGif.Frames.Count) {
                this.idxFrame = 0;
                if (this.nLoop > 0) {
                    if (--this.nLoopRemain < 0) {
                        this.timerInterval.Stop();
                    }
                }
            }

            this.FrameImage.Source = this.decAnimationGif.Frames[this.idxFrame];
            this.timerInterval.Interval = new TimeSpan(0, 0, 0, 0, this.arrDelays[this.idxFrame]);
        }
    }
}
