using CDllInvoker.StaticMethods;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CDllInvoker.Entities {
    public class Device:IObjectDevice {
        public static Device Create(PhysicsDeviceStruct st) {
            Device device = new Device();
            device.DeviceID = st.ObjectID;
            device.Lable = st.Lable;
            device.DevName = st.DevName;
            device.Size = st.DevSize;
            device.DevCHS = CHS.Create(st.DevCHS);
            device.DevMomd = st.DevMomd;
            device.DevType = st.DevType;
            device.SectorSize = st.SectorSize;
            device.Buffer = st.Buffer;
            device.Partition = st.Partiton;
            device.Arch = st.Arch;
            device.DevRW = st.DevRW;
            device.DevState = st.DevState;
            device.Partitions = new List<Partition>();
            device.Handle = st.Handle;
            return device;
        }
        private DeviceTypeEnum deviceType { get; set; }
        private ScanMethod scanMethod;
        
        public bool IniScanPartition(DeviceTypeEnum deviceType, ScanMethod scanMethod, ulong startSec, ulong endSec,int secSize, ulong timePos, ulong lbaPos) {
            this.scanMethod = scanMethod;
            this.deviceType = deviceType;
            if (Handle == IntPtr.Zero) {
                throw new Exception("No Handle For Partition!");
            }
            bool res = false;
            switch (deviceType) {
                case DeviceTypeEnum.DaHua:
                    res = DHRcoveryMethods.cdfc_dahua_init(startSec, endSec, secSize, timePos, lbaPos, Handle);
                    break;
                case DeviceTypeEnum.HaiKang: break;
                default: break;
            }
            return res;
        }
        public IntPtr ScanAsync(int type) {
            IntPtr ptr = IntPtr.Zero;
            switch (deviceType) {
                case DeviceTypeEnum.DaHua: {
                        switch (scanMethod) {
                            case ScanMethod.EntireDisk:
                                ptr = DHRcoveryMethods.cdfc_dahua_search_start(Handle, type);
                                break;
                            case ScanMethod.FileSystem:
                                ptr = DHRcoveryMethods.cdfc_dahua_search_start_f(Handle, type);
                                break;
                            default: break;
                        }
                        break;
                    }


                default: break;
            }
            return ptr;
        }
        
        public ulong GetCurSectorAsync() {
            switch (deviceType) {
                case DeviceTypeEnum.DaHua:
                    return DHRcoveryMethods.cdfc_dahua_current_sector();
                default: return 0;
            }

        }
        public List<DateCategory> GetCurDateCategories() {
            List<DateCategory> categories = new List<DateCategory>();
            IntPtr categoryPtr;
            IntPtr categoryNode;

            switch (deviceType) {
                case DeviceTypeEnum.DaHua:
                    categoryPtr = DHRcoveryMethods.cdfc_dahua_filelist();
                    break;
                default:
                    categoryPtr = IntPtr.Zero;
                    break;
            }
            categoryNode = categoryPtr;

            while (categoryNode != IntPtr.Zero) {
                var categoryStruct = categoryNode.GetStructure<DateCategoryStruct>();
                var category = DateCategory.CreateByDevice(categoryStruct, scanMethod,this);
                category.DeviceTypeEnum = this.deviceType;
                if (category != null) {
                    categories.Add(category);
                }

                categoryNode = categoryStruct.Next;
            }

            return categories;
        }
        public void StopScan() {
            switch (deviceType) {
                case DeviceTypeEnum.DaHua: DHRcoveryMethods.cdfc_dahua_stop(); break;
                default: break;
            }
        }
        public string GetSectorHexString(ulong lbaAddress) {
            ulong lbaPos = lbaAddress;
            char[] charArray = new char[SectorSize];
            IntPtr ptrCharArray = Marshal.AllocHGlobal(this.SectorSize * Marshal.SizeOf(typeof(byte)));
            IntPtr ptrSize = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(uint)));
            bool res = false;

            switch (deviceType) {
                case DeviceTypeEnum.DaHua:
                    res = DHRcoveryMethods.cdfc_dahua_read(this.Handle, lbaPos, ptrCharArray, (ulong)this.SectorSize, ptrSize);
                    break;
            }

            if (!res) {
                return null;
            }
            int gottenSize = Marshal.ReadInt32(ptrSize);
            Marshal.FreeHGlobal(ptrSize);
            
            IntPtr ptrNode = ptrCharArray;
            int index = 1;
            StringBuilder sb = new StringBuilder();

            while (ptrNode != IntPtr.Zero&&index < gottenSize+1) {
                var byChar = Marshal.ReadByte(ptrNode);
                sb.Append(byChar.ToString("X8").Substring(6,2));
                if (index % 16 == 0 ) {
                    sb.AppendLine();
                }
                else {
                    sb.Append(" ");
                }
                ptrNode += Marshal.SizeOf(typeof(byte));
                index++;
            }
            
            Marshal.FreeHGlobal(ptrCharArray);
            return sb.ToString();
        }

        public int DeviceID { get; set; }                     //设备标数(如果为16不以物理名称打开)
        public string Lable { get; set; }
        public string DevName { get; set; }                  //驱动名称

        public IntPtr Handle { get; set; }   /// <summary>
        /// 设备句柄
        /// </summary>
        public ulong Size { get; set; }                  //设备大小

        public CHS DevCHS { get; set; }                   //设备几何
        public ulong DevMomd { get; set; }                      //访问模式
        public uint DevType { get; set; }                   //设备类型

        public int SectorSize { get; set; }                //扇区字节
        public IntPtr Buffer { get; set; }                     //读写缓存
        public IntPtr Partition { get; set; }                    //分区结构（废弃）
        public IntPtr Arch { get; set; }                   //调用指针
        public IntPtr DevRW { get; set; }                      //设备读写
        public bool DevState { get; set; }

        public HddInfo HddInfo { get; set; }
        public List<Partition> Partitions { get; set; }

        public DriveType DriveType { get; set; }
        
    }
}
