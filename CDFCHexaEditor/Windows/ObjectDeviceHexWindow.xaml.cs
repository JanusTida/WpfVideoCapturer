using CDFCEntities.Interfaces;
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
using System.Windows.Shapes;
using WPFHexaEditor.Control;
using EventLogger;
using MahApps.Metro.Controls;
using WPFHexaEditor.Control.IO;

namespace WPFHexaEditor.Control.Windows {
    /// <summary>
    /// Interaction logic for ObjectDeviceHexWindow.xaml
    /// </summary>
    public partial class ObjectDeviceHexWindow : MetroWindow {
        public ObjectDeviceHexWindow(IObjectDevice objectDevice,long pos = 0) {
            InitializeComponent();
            this.ObjectDevice = objectDevice;
            Stream = new ObjectDeviceStream(objectDevice, (long)objectDevice.Size, objectDevice.SectorSize);
            hexEditor.OpenHandleStream(Stream);   
            //if(pos != 0) {
            //    hexEditor.SetPosition(pos);
            //}
        }
        
        public IObjectDevice ObjectDevice { get; private set; }
        public ObjectDeviceStream Stream { get; private set; }

        private long position;
        public long Position {
            get {
                return position;
            }
            set {
                if(value >= (long) ObjectDevice.Size) {
                    Logger.WriteLine($"{nameof(ObjectDeviceHexWindow)}->Set_{nameof(Position)}:Position Out Of Range,Value:{value}.");
                    return;
                }
                position = value;
                SetPosition(position,1);
            }
        }

        public void SetPosition(long position,long byteLength) {
            hexEditor.SetPosition(position, byteLength);
        }
    }
}
