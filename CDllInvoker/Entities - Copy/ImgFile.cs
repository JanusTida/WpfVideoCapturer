using CDllInvoker.StaticMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CDllInvoker.Entities {
    public class ImgFile : IObjectDevice {
        private DeviceTypeEnum deviceType;
        public IntPtr Handle { get; set; }
        private ScanMethod scanMethod;
        public DriveType DriveType { get; set; }

        public ulong Size {get;set;}
        public int SectorSize { get; set; }

        public string Name { get; set; }
        public ulong GetCurSectorAsync() {
            switch (deviceType) {
                case DeviceTypeEnum.DaHua:
                    return DHRcoveryMethods.cdfc_dahua_current_sector();
                default: return 0;
            }
        }
        private FileStream fs;
        
        public bool Exit() {
            try { 
                fs.Close();
            }
            catch {
                return false;
            }
            return true;
        }
        public static ImgFile GetImgFile(string path) {
            ImgFile imgFile = new ImgFile();
            imgFile.SectorSize = 512;
            try { 
                imgFile.fs = new FileStream(path, FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
                int dotIndex = path.LastIndexOf(@"\");

                imgFile.DriveType = DriveType.ImgFile;
                imgFile.Handle = imgFile.fs.SafeFileHandle.DangerousGetHandle();
                imgFile.Size = ImgFileMethods.cdfc_common_imagefile_size(imgFile.Handle);
                imgFile.Name = path.Substring(dotIndex+1);
            }
            catch(Exception ex) {
                throw new Exception("The File Is In Use!", ex);
            }
            return imgFile;
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

        public bool IniScanPartition(DeviceTypeEnum deviceType, ScanMethod scanMethod, ulong startSec, ulong endSec, int secSize, ulong timePos, ulong lbaPos) {
            this.scanMethod = scanMethod;
            this.deviceType = deviceType;
            if (Handle == IntPtr.Zero) {
                throw new Exception("No Handle For ImgFile!");
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

        public void StopScan() {
            switch (deviceType) {
                case DeviceTypeEnum.DaHua: DHRcoveryMethods.cdfc_dahua_stop(); break;
                default: break;
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
                var category = DateCategory.CreateByImgFile(categoryStruct ,scanMethod,this);
                category.DeviceTypeEnum = this.deviceType;
                if (category != null) {
                    categories.Add(category);
                }
                categoryNode = categoryStruct.Next;
            }

            return categories;
        }
    }
}
