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
        public enum Order {
            ORDER_BACKGROUND = 0,
            ORDER_FOREGROUND = 1,
            ORDER_FADING     = 2,
        };

        private static int nBackgroundZ = 0;
        private static int nForegroundZ = 1;
        private static int nFadingZ     = 2;
        private Dictionary<string, Canvas> dicCanvas = null;
        private KeyValuePair<string, Canvas> canvasFading;

        public CanvasManager()
        {
            dicCanvas = new Dictionary<string, Canvas>();
        }

        public void Add(string keyword, Canvas canvas, Order order)
        {
            dicCanvas.Add(keyword, canvas);
            Canvas.SetZIndex(canvas, (int)order);
        }

        public void Raise(string keyword)
        {
            foreach (var item in this.dicCanvas) {
                if (item.Key.Equals(keyword)) {
                    //if (Canvas.GetZIndex(item.Value) == CanvasManager.nForegroundZ) {
                    //    return; // no processing
                    //}
                    //Canvas.SetZIndex(item.Value, CanvasManager.nForegroundZ);
                    DoubleAnimation animFader = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(100)), FillBehavior.HoldEnd);
                    item.Value.BeginAnimation(Rectangle.OpacityProperty, animFader);
                    Canvas.SetZIndex(item.Value, CanvasManager.nForegroundZ);
                    //item.Value.Opacity = 1.0;
                } else {
                    if (Canvas.GetZIndex(item.Value) == CanvasManager.nForegroundZ) {
                        DoubleAnimation animFader = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(500)), FillBehavior.HoldEnd);
                        animFader.Completed += new EventHandler(OnCompletedFader);
                        Canvas.SetZIndex(item.Value, CanvasManager.nFadingZ);
                        item.Value.BeginAnimation(Rectangle.OpacityProperty, animFader);
                        this.canvasFading = item;
                    }
                }
            }
        }

        private void OnCompletedFader(object sender, EventArgs e)
        {
            //Canvas.SetZIndex(this.canvasFading.Value, CanvasManager.nBackgroundZ);
            //this.canvasFading.Value.Opacity = 1.0;
            //this.canvasFading = null;
        }
    }
}
