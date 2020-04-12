using CDFCDavPlayer.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for DavPlayerPanel.xaml
    /// </summary>
    public partial class DavPlayerPanel : UserControl {
        public DavPlayerPanel() {
            InitializeComponent();
            this.Loaded += (sender, e) => {
                ThreadPool.QueueUserWorkItem(cb => {
                    Thread.Sleep(1000);
                    this.Dispatcher.Invoke(() => {
                        this.Refresh();
                    });
                });
            };
        }

        public IPlayer Player {
            get {
                return (IPlayer)GetValue(PlayerProperty);
            }
            set {
                SetValue(PlayerProperty,value);
            }
        }
        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register(nameof(Player), typeof(IPlayer), typeof(DavPlayerPanel),
            new PropertyMetadata(null, Player_PropertyChanged));

        private static void Player_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var panel = d as DavPlayerPanel;
            if(panel != null){
                if(e.NewValue is DHPlayer dhPlayer) {
                    dhPlayer.PlayerHandle = panel.pbLayer.Handle;
                }
                else if(e.NewValue == null) {
                    panel.pbLayer.Refresh();
                }
            }
        }

        private void pbLayer_Resize(object sender, EventArgs e) {
            Refresh();
        }

        public void Refresh() {
            pbLayer.Refresh();
        }

        public event EventHandler Click;
        private void pbLayer_Click(object sender, EventArgs e) {
            Click?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler DoubleClick;
        private void pbLayer_DoubleClick(object sender, EventArgs e) {
            DoubleClick?.Invoke(this, EventArgs.Empty);
        }
    }
}
