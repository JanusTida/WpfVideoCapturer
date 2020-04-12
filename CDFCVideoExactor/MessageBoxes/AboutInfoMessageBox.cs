using CDFCVideoExactor.ViewModels.AboutInfo;
using CDFCVideoExactor.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCVideoExactor.MessageBoxes {
    public class AboutInfoMessageBox {
        public static void Show() {
            var window = new AboutInfoWindow();
            window.DataContext = new AboutInfoWindowViewModel();
            window.ShowDialog();
        }
    }
}
