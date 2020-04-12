using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CDFCDavPlayer.Views {
    /// <summary>
    /// Interaction logic for PlayerControlPanel.xaml
    /// </summary>
    public partial class PlayerControlPanel : UserControl {
        public PlayerControlPanel() {
            InitializeComponent();
        }
        
        public bool IsProcessing {
            get {
                return (bool)GetValue(IsProcessingProperty);
            }
            set {
                SetValue(IsProcessingProperty, value);
            }
        }
        public static readonly DependencyProperty IsProcessingProperty = DependencyProperty.Register(nameof(IsProcessing), typeof(bool),
            typeof(PlayerControlPanel), new FrameworkPropertyMetadata( false, 
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private void Pro_Slider_MouseDown(object sender, MouseButtonEventArgs e) {
            IsProcessing = true;
        }

        private void Pro_Slider_MouseLeave(object sender, MouseEventArgs e) {
            IsProcessing = false;
        }

        private void Pro_Slider_MouseUp(object sender, MouseButtonEventArgs e) {
            IsProcessing = false;
        }
    }
}
