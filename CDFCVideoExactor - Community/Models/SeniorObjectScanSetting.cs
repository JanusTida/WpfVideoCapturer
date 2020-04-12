using CDFCEntities.Interfaces;
using CDFCVideoExactor.Abstracts;
using System;

namespace CDFCVideoExactor.Models {
    /// <summary>
    /// 进阶对象扫描设置;
    /// </summary>
    public class SeniorObjectScanSetting:NotificationObject {

        private readonly IObjectDevice iObjectDevice;
        //簇大小;
        private ushort clusterSize = 0;
        public ushort ClusterSize {
            get {
                return clusterSize;
            }
            set {
                if (value > 2048) {
                    clusterSize = 2048;
                }
                else {
                    clusterSize = value;
                }
                NotifyPropertyChanging(nameof(ClusterSize));
            }
        }

        /// <summary>
        /// 起始扇区;
        /// </summary>
        private ulong iniSector = 0;
        public ulong IniSector {
            get {
                return iniSector;
            }
            set {
                if (value > EndSector) {
                    iniSector = 0;
                }
                else if (value >= 0) {
                    iniSector = value;
                }
                NotifyPropertyChanging(nameof(IniSector));
            }
        }

        /// <summary>
        /// 终止扇区;
        /// </summary>
        private ulong endSector;
        public ulong EndSector {
            get {
                if (endSector == 0 && iObjectDevice != null) {
                    endSector = iObjectDevice.Size / 512;
                }
                return endSector;
            }
            set {
                if (value > iObjectDevice.Size / 512) {
                    endSector = iObjectDevice.Size / 512;
                }
                else if (value >= 0) {
                    endSector = value;
                }
                NotifyPropertyChanging(nameof(EndSector));
            }
        }

        /// <summary>
        /// 时间偏移;
        /// </summary>
        private ulong timePos = 0;
        public ulong TimePos {
            get {
                return timePos;
            }
            set {
                timePos = value;
            }
        }

        /// <summary>
        /// 文件位置偏移;
        /// </summary>
        private ulong lbaPos = 0;
        public ulong LbaPos {
            get {
                return lbaPos;
            }
            set {
                if (value > iObjectDevice.Size) {
                    lbaPos = iObjectDevice.Size;
                }
                lbaPos = value;
                NotifyPropertyChanging(nameof(LbaPos));
            }
        }

        /// <summary>
        /// 扇区大小;
        /// </summary>
        private uint sectorSize;
        public uint SectorSize {
            get {
                if (sectorSize == 0 && iObjectDevice != null) {
                    sectorSize = Convert.ToUInt32(iObjectDevice.SectorSize);
                }
                return sectorSize;
            }
            set {
                if (value > 1024) {
                    sectorSize = 1024;
                }
                else if (sectorSize > 0) {
                    sectorSize = value;
                }
                NotifyPropertyChanging(nameof(SectorSize));
            }
        }

    }
}
