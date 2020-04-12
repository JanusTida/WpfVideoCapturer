using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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

namespace CDFCDavPlayer.Windows {
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    [Export]
    public partial class TestWindow : Window {
        public TestWindow() {
            InitializeComponent();
        }

        private void Window_Drop(object sender, DragEventArgs e) {

        }
    }
}
