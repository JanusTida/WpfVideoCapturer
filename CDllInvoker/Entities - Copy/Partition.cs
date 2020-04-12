using System;
using CDllInvoker.StaticMethods;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CDllInvoker.Entities {
    public class Partition:IObjectDevice {
        public Device Device { get; set; }
        public int SectorSize { get; set; }

        private DeviceTypeEnum deviceType;
        private ScanMethod scanMethod;
        
        public bool IniScanPartition(DeviceTypeEnum deviceType,ScanMethod scanMethod,ulong startSec, ulong endSec,int secSize,ulong timePos,ulong lbaPos) {
            this.scanMethod = scanMethod;
            this.deviceType = deviceType;
            if(Handle == IntPtr.Zero) {
                throw new Exception("No Handle For Partition!");
            }
            bool res = false;
            switch (deviceType) {
                case DeviceTypeEnum.DaHua: 
                            res = DHRcoveryMethods.cdfc_dahua_init(startSec, endSec, secSize, timePos, lbaPos, Handle); 
                            break;
                case DeviceTypeEnum.HaiKang:  break;
                default:break;
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
                            default:break;
                        }
                        break;
                    }


                default:break;
            }
            return ptr;
        }
        public ulong GetCurSectorAsync() {
            switch (deviceType) {
                case DeviceTypeEnum.DaHua: 
                        return DHRcoveryMethods.cdfc_dahua_current_sector();
                default:return 0;
            }
            
        }
        public List<DateCategory>  GetCurDateCategories() {
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

            while(categoryNode != IntPtr.Zero) {
                var categoryStruct = categoryNode.GetStructure<DateCategoryStruct>();
                var category = DateCategory.CreateByPartition(categoryStruct,scanMethod,this);
                if(category != null) {
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

        public static Partition Create(PartitonStruct st) {
            Partition partition = new Partition();
            partition.LoGo = st.m_LoGo;
            partition.VolumeName = st.VolumeName;
            partition.FileSystem = st.FileSystem;
            partition.Name = st.m_Name;
            partition.Size = st.m_Size;
            partition.Type = (PartitionType)st.m_Type;
            partition.Offset = st.m_Offset;
            partition.Boot = st.m_Boot;
            partition.pDev = st.m_pDev;
            partition.Sign = Convert.ToChar(st.m_Sign);
            partition.pDBR = st.pDBR;
            return partition;
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

            while (ptrNode != IntPtr.Zero && index < gottenSize + 1) {
                var byChar = Marshal.ReadByte(ptrNode);
                sb.Append(byChar.ToString("X8").Substring(6, 2));
                if (index % 16 == 0) {
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

        public int LoGo { get; set; }   //为那个物理设备的分区
        public string VolumeName { get; set; }  //卷标名称
        public string FileSystem { get; set; }     //文件系统
        public IntPtr Handle { get; set; }
        public IntPtr Name { get; set; }              //分区名称
        public ulong Size { get; set; }                 //分区大小
        public PartitionType Type { get; set; }                    //分区类型
        public ulong Offset { get; set; }                  //MBR的偏移
        public bool Boot { get; set; }                      //是否引导
        public IntPtr pDev { get; set; }                 //指向设备
        public char Sign { get; set; }                      //分区盘符
        public IntPtr pDBR { get; set; }
        public HddInfo HddInfo { get; set; }

        public DriveType DriveType { get; set; }
    }
}
