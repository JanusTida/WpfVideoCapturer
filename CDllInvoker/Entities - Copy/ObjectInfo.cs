using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDllInvoker.Entities {
    public class ObjectInfo {
        public DriveType DriveType { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }
        public string OSObject { get; set; }
        public string ObjectSize {
            get {
                return ((double)Size / 1024 / 1024 / 1024).ToString() + "GB";
            }
        }
        public string VenderID {
            get {
                return HddInfo.VendorID;
            }
        }
        public ulong Size { get; set; }
        public int SectorSize { get; set; }
        public string ObjectSectorSize {
            get {
                return SectorSize.ToString() + " Bytes";
            }
        }
        public CHS DevCHS { get; set; }
        public string Handle { get; set; }
        public string ModelNumber {
            get {
                return HddInfo.HddInfo2 == null ? HddInfo.VendorID : HddInfo.HddInfo2.szModelNumber;
            }
        }
        public string ProductID {
            get {
                return HddInfo.HddInfo2 == null ? HddInfo.ProductID : HddInfo.HddInfo2.szControllerNumber;
            }
        }
        public string ProductRevision {
            get {
                return HddInfo.ProductRevision;
            }
        }
        public string SerialNumber {
            get {
                return HddInfo.HddInfo2 == null ? HddInfo.SerialNumber : HddInfo.HddInfo2.szSerialNumber;
            }
        }

        public HddInfo HddInfo { get; set; }
        public ulong Offset { get; set; } = 0;
        public bool Boot { get; set; } = false;
        
    }
    public enum DriveType {
        PhysicalDevice,
        Disk,
        ImgFile
    }

}
