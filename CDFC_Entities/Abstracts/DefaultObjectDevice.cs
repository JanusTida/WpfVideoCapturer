using CDFCEntities.Interfaces;
using System;
using System.Text;
using CDFCEntities.Enums;
using CDFCEntities.Files;
using System.Runtime.InteropServices;
using CDFCStatic.CMethods;
using CDFCEntities.DeviceObjects;
using CDFCEntities.Structs;
using EventLogger;

namespace CDFCEntities.Abstracts {
    public abstract class DefaultObjectDevice : IObjectDevice {
        public IntPtr Handle {get;set;}
        public int SectorSize {get;set;}
        public long Size {get; set;}
        public Enums.DriveType DriveType { get; set; }
        private DeviceTypeEnum deviceType;
        private ScanMethod scanMethod;
        private IScanner iObjectScanner;
        
        /// <summary>
        /// 获得当前扇区十六进制表示;
        /// </summary>
        /// <param name="lbaAddress"></param>
        /// <param name="SectorSize"></param>
        /// <param name="deviceType"></param>
        /// <param name="Handle"></param>
        /// <returns></returns>
        public string GetSectorHexString(long lbaAddress) {
            long lbaPos = lbaAddress;
            char[] charArray = new char[SectorSize];
            IntPtr ptrCharArray = Marshal.AllocHGlobal(SectorSize * Marshal.SizeOf(typeof(byte)));
            IntPtr ptrSize = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(uint)));
            bool res = false;
            Func<IntPtr, ulong, IntPtr, ulong, IntPtr,bool, bool> readFunc = CommonMethods.cdfc_common_read;
            res = readFunc(Handle, (ulong)lbaPos, ptrCharArray, (ulong)SectorSize, ptrSize,true);
            if (!res) {
                return null;
            }
            //得到获得的字符数量;
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
        
        /// <summary>
        /// 在对象中读取某一个位置;
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="index"></param>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        public virtual int Read(byte[] destination,int offset,int byteCount,long pos) {
            //获取能够被扇区数目整除的字节数;
            var byteCountForSector = byteCount % SectorSize == 0 ? byteCount : byteCount / SectorSize * SectorSize + SectorSize;
            var posForSector = pos % (long)SectorSize == 0 ? pos : pos / (long)SectorSize * (long)SectorSize;
            //两个位置的差;
            var subForPos = (int)(pos - posForSector);
            //临时缓冲区,以方便合法调用;
            byte[] bufferByteArray = null;

            if (posForSector + (long)byteCountForSector < (long)byteCount + pos) {
                byteCountForSector += SectorSize;
            }
            bufferByteArray = new byte[byteCountForSector];

            IntPtr ptrSize = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(uint)));
            IntPtr ptrBuffer = Marshal.AllocHGlobal(byteCountForSector * Marshal.SizeOf(typeof(byte)));

            int readSize = 0;
            bool res = false;

            Func<IntPtr, ulong, IntPtr, ulong, IntPtr, bool, bool> readFunc = CommonMethods.cdfc_common_read;
            res = readFunc(Handle, (ulong)posForSector, ptrBuffer, (ulong)byteCountForSector, ptrSize, true);

            if (res) {
                readSize = Marshal.ReadInt32(ptrSize);
                Marshal.Copy(ptrBuffer, bufferByteArray, 0, byteCountForSector);
                for (int i = subForPos; i < byteCount + subForPos; i++) {
                    destination[offset + i - subForPos] = bufferByteArray[i];
                }
            }

            Marshal.FreeHGlobal(ptrBuffer);
            Marshal.FreeHGlobal(ptrSize);

            return readSize;
        
        }

        //读取内容至缓冲区中;
        public bool Read(IntPtr buffer, long bufferSize,long offset,long count) {
            return false;
        }

        /// <summary>
        /// 获得自动识别的文件系统类型;
        /// </summary>
        private int fileSystemType = -1;
        public int FileSystemType {
            get {
                if(fileSystemType == -1) {
                    try {
                        //fileSystemType = 0;
                        fileSystemType = CommonMethods.cdfc_common_fstype(Handle);
                    }
                    catch(Exception ex) {
                        Logger.WriteLine($"{nameof(DefaultObjectDevice)}->{nameof(FileSystemType)}:{ex.Message}");
                        fileSystemType = 0;
                    }
                    
                    //return fileSystemType;
                    ////若为分区,直接返回零;
                    //if (DriveType == Enums.DriveType.Disk) {
                    //    fileSystemType = 0;
                    //}
                    ////若为物理设备,查看是否有分区;
                    //else if (DriveType == Enums.DriveType.PhysicalDevice) {
                    //    var device = this as Device;
                    //    if(device != null) {
                    //        if(device.Partitions.Count != 0) {
                    //            fileSystemType = 0;
                    //        }
                    //    }

                    //}
                    //else {
                    //    fileSystemType = CommonMethods.cdfc_common_fstype(Handle);
                    //}
                    
                }
                //   return 0;
                return fileSystemType;
            }
        }
        
        public bool IniLoggerParition(DeviceTypeEnum deviceType, ScanMethod scanMethod) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 构建文件分类实体；
        /// </summary>
        /// <param name="st">文件分类所依托的文件结构</param>
        /// <param name="scanMethod">扫描方式</param>
        /// <param name="deviceType">选择的扫描方式</param>
        /// <returns></returns>
        public DateCategory CreateDatecategory(DateCategoryStruct st, ScanMethod scanMethod,DeviceTypeEnum deviceType,bool isFromLogger = false) {
            DateCategory dateCategory = new DateCategory(st, scanMethod, this,deviceType,isFromLogger);
            return dateCategory;
        }

        /// <summary>
        /// 构建文件分类实体;(不构建文件分类实体)
        /// </summary>
        /// <param name="st">文件分类所依托的文件结构</param>
        /// <param name="scanMethod">扫描方式</param>
        /// <returns></returns>
        //public DateCategory CreateEmptyDatecategory(DateCategoryStruct st, ScanMethod scanMethod) {
        //    var category = DateCategory.CreateEmptyDatecategory(st, scanMethod, this);
        //    return category;
        //}
    }
}
