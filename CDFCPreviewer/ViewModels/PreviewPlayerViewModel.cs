using CDFCEntities.Enums;
using CDFCUIContracts.Abstracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Integration;

namespace CDFCPreviewer.ViewModels {
    public class PreviewPlayerViewModel : BindableBaseTemp {
        public PreviewPlayerViewModel() {
            //FileStream fs = new FileStream("G://DHDavs//2017-4-20//大华监控机//大华DHFS/2016-10-10-11-16-37.dav", FileMode.Open);
            //var length = fs.Length < 104857600 ? fs.Length : 10485760;
            //streamPtr = Marshal.AllocHGlobal((int)length);
            //var bitArr = new byte[length];
            //fs.Read(bitArr, 0, (int)length);
            //fs.Close();
            //Marshal.Copy(bitArr, 0, streamPtr, (int)length);
            //PlayerBuffer = streamPtr;
            //DeviceType = DeviceTypeEnum.DaHua;
        }
        private IntPtr playerBuffer;
        public IntPtr PlayerBuffer {
            get {
                return playerBuffer;
            }
            set {
                SetProperty(ref playerBuffer, value);
                //playerBuffer = value;
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayerBuffer)));
            }
        }

        private long _bufferSize;
        public long BufferSize {
            get {
                return _bufferSize;
            }
            set {
                SetProperty(ref _bufferSize, value);
            }
        }

        private DeviceTypeEnum deviceType = DeviceTypeEnum.Unknown;
        public DeviceTypeEnum DeviceType {
            get {
                return deviceType;
            }
            set {
                SetProperty(ref deviceType, value);
            }
        }

        private bool bannerVisible = true;
        public bool BannerVisible {
            get {
                return bannerVisible;
            }
            set {
                SetProperty(ref bannerVisible, value);
            }
        }
        public void Close() {
            PlayerBuffer = IntPtr.Zero;
        }
    }
}
