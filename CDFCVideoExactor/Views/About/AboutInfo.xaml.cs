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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CDFCVideoExactor.Views.About {
    /// <summary>
    /// Interaction logic for AboutInfo.xaml
    /// </summary>
    public partial class AboutInfo : UserControl {
        public AboutInfo() {
            InitializeComponent();
        }

        private static readonly string originSite = "http://www.cflab.net";
        private void OriSite_HyperLink_Click(object sender, RoutedEventArgs e) {
            Process.Start(originSite);
        }
    }
}
