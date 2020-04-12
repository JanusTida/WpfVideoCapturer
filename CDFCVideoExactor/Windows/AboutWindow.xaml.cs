using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace CDFCVideoExactor.Windows {
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : MetroWindow {
        public AboutWindow() {
            InitializeComponent();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e) {
            var link = sender as Hyperlink;
            if(link != null) {
                Process.Start(link.NavigateUri.ToString());
            }
        }

        
    }
}
