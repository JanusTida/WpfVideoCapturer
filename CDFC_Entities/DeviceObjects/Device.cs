using System;
using System.Collections.Generic;
using CDFCEntities.Structs;
using CDFCEntities.Enums;
using CDFCEntities.DeviceInfoes;
using CDFCEntities.Abstracts;

namespace CDFCEntities.DeviceObjects {
    public class Device : DefaultObjectDevice {
        private Device() { }
        public static Device Create(PhysicsDeviceStruct st) {
            Device device = new Device();
            device.DeviceID = st.ObjectID;
            device.Lable = st.Lable;
            device.DevName = st.DevName;
            device.Size = (long)st.DevSize;
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
            device.DriveType = DriveType.PhysicalDevice;
            return device;
        }
        
        public int DeviceID { get; set; }                     //设备标数(如果为16不以物理名称打开)
        public string Lable { get; set; }
        public string DevName { get; set; }                  //驱动名称
        
        public CHS DevCHS { get; set; }                   //设备几何
        public ulong DevMomd { get; set; }                      //访问模式
        public uint DevType { get; set; }                   //设备类型
        
        public IntPtr Buffer { get; set; }                     //读写缓存
        public IntPtr Partition { get; set; }                    //分区结构（废弃）
        public IntPtr Arch { get; set; }                   //调用指针
        public IntPtr DevRW { get; set; }                      //设备读写
        public bool DevState { get; set; }

        /// <summary>
        /// 硬盘标识号;
        /// </summary>
        public string SerialNumber {
            get {
                if(HddInfo?.HddInfo2 != null) {
                    return HddInfo.HddInfo2.szModelNumber;
                }
                return HddInfo?.VendorID??string.Empty;
            }
        }
        public HddInfo HddInfo { get; set; }
        public List<Partition> Partitions { get; set; }
        
    }
}
