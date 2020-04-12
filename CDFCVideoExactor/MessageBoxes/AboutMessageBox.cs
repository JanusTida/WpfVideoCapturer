using CDFCVideoExactor.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCVideoExactor.MessageBoxes {
    public class AboutMessageBox {
        public static void Show() {
            var window = new AboutWindow();
            window.ShowDialog();
        }
    }
}
