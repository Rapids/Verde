using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows;

namespace Verde.Utility
{
    class CanvasManager
    {
        private static int nBackgroundZ = 0;
        private static int nForegroundZ = 1;
        private Dictionary<string, Canvas> dicCanvas = null;
        private Canvas canvasFading = null;

        public CanvasManager()
        {
            dicCanvas = new Dictionary<string, Canvas>();
        }

        public void Add(string keyword, Canvas canvas)
        {
            dicCanvas.Add(keyword, canvas);
        }

        public void Raise(string keyword)
        {
            foreach (var item in this.dicCanvas) {
                if (item.Key.Equals(keyword)) {
                    if (Canvas.GetZIndex(item.Value) == CanvasManager.nForegroundZ) {
                        return; // no processing
                    }
                    //Canvas.SetZIndex(canvas, CanvasManager.nForegroundZ);
                    //DoubleAnimation animFader = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(200)), FillBehavior.HoldEnd);
                    //canvas.BeginAnimation(Rectangle.OpacityProperty, animFader);
                    Canvas.SetZIndex(item.Value, CanvasManager.nForegroundZ);
                    item.Value.Opacity = 1.0;
                } else {
                    if (Canvas.GetZIndex(item.Value) == CanvasManager.nForegroundZ) {
                        DoubleAnimation animFader = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(500)), FillBehavior.HoldEnd);
                        animFader.Completed += new EventHandler(OnCompletedFader);
                        item.Value.BeginAnimation(Rectangle.OpacityProperty, animFader);
                        this.canvasFading = item.Value;
                    }
                }
            }
        }

        private void OnCompletedFader(object sender, EventArgs e)
        {
            Canvas.SetZIndex(this.canvasFading, CanvasManager.nBackgroundZ);
            this.canvasFading.Opacity = 1.0;
            this.canvasFading = null;
        }
    }
}
