using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CDFCDavPlayer.Views {
    public class CollapsablePanel : ContentControl {
        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed"
            , typeof(bool), typeof(CollapsablePanel), new PropertyMetadata(false, IsCollapsed_Propertychanged));

        private double ActualHeight2 = 0;
        private double ActualWidth2 = 0;
        private static void IsCollapsed_Propertychanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var ctrl = d as CollapsablePanel;
            switch ((bool)e.NewValue) {
                case false:
                    var anim2 = new DoubleAnimationUsingKeyFrames();
                    SplineDoubleKeyFrame frame = new SplineDoubleKeyFrame();
                    frame.KeySpline = new KeySpline(new Point(0.1, 0.9), new Point(0.2, 1.0));
                    frame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.35));
                    frame.Value = ctrl.Orientation == Orientation.Vertical ? ctrl.ActualHeight2 : ctrl.ActualWidth2;
                    anim2.KeyFrames.Add(frame);
                    ctrl.BeginAnimation(ctrl.Orientation == Orientation.Vertical ? HeightProperty : WidthProperty, anim2);
                    break;
                case true:
                    ctrl.ActualHeight2 = ctrl.ActualHeight;
                    ctrl.ActualWidth2 = ctrl.ActualWidth;

                    var anim = new DoubleAnimation {
                        To = 0,
                        Duration = new Duration(TimeSpan.FromMilliseconds(200))
                    };
                    anim.From = ctrl.Orientation == Orientation.Vertical ? ctrl.ActualHeight : ctrl.ActualWidth;

                    ctrl.BeginAnimation(ctrl.Orientation == Orientation.Vertical ? HeightProperty : WidthProperty, anim);
                    break;
            }
        }

        public bool IsCollapsed {
            get { return (bool)this.GetValue(IsCollapsedProperty); }
            set { this.SetValue(IsCollapsedProperty, value); }
        }

        public Orientation Orientation {
            get {
                return (Orientation)GetValue(OrientationProperty);
            }
            set {
                SetValue(OrientationProperty, value);
            }
        }
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation),
            typeof(Orientation), typeof(CollapsablePanel), new PropertyMetadata(Orientation.Vertical));

    }
}
