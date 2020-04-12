using System.Windows;
using Ookii.Dialogs.Wpf;
using CDFCEntities.DeviceObjects;
using WPFHexaEditor.Core.Bytes;
using System.Windows.Input;
using MahApps.Metro.Controls;
using WPFHexaEditor.Control.IO;

namespace CDFCVideoExactor {
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : MetroWindow {
        public TestWindow() {
            InitializeComponent();

        }
        ComObject co = ComObject.LocalObject;
        private void Button_Click(object sender, RoutedEventArgs e) {
            var part = co.Devices[0].Partitions[5];
            var img = ImgFile.GetImgFile("D://2.mp4");
            var stream = new ObjectDeviceStream(img,(long) img.Size / 5,512,0);
            Application.Current.MainWindow.Cursor = Cursors.Wait;
            HexC.Close();
            HexC.OpenHandleStream(stream);
            Application.Current.MainWindow.Cursor = null;
            //hexC.FileName = "D://2.mp4";
        }
    }
}
