using CDFCEntities.Abstracts;
using CDFCEntities.Enums;
using CDFCEntities.Interfaces;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CDFCEntities.Recoverers {
    /// <summary>
    /// 安联恢复器;
    /// </summary>
    public partial class SonyRecoverer : DefaultObjectRecoverer {
        public SonyRecoverer(ScanMethod scanMethod, int versionType, IObjectDevice iObjectDevice) : base(DeviceTypeEnum.Sony, scanMethod, iObjectDevice) {
            this.versionType = versionType;
        }
        private int versionType;
    }
    public partial class SonyRecoverer : DefaultObjectRecoverer {
        /// <summary>
        /// 索尼的保存入口点依据所选的nType而定;
        /// </summary>
        /// <param name="szFile"></param>
        /// <param name="hDisk"></param>
        /// <param name="szFile_2"></param>
        /// <param name="nCurrSizeDW"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl,EntryPoint = "cdfc_sony_filesave_mts")]
        private extern static bool cdfc_object_filesave_mts(IntPtr szFile, IntPtr hDisk, IntPtr szFile_2, IntPtr nCurrSizeDW, IntPtr Error);

    }
    public partial class SonyRecoverer : DefaultObjectRecoverer {
        /// 索尼的保存接口根据基本型号不同有所差异;
        public override bool SaveAs(string desLocation) {
            if (versionType == 2) {
                //是否正常:
                bool res = false;
                desLocation  = CDFCStatic.IOMethods.IOStaticMethods.GetValidPath(desLocation);
                if (objectRecoveryMethods.FileSaveFunc == null) {
                    EventLogger.Logger.WriteLine("DefaultObjectScanner->FileSave错误:接口未注册");
                    res = false;
                }
                else {
                    //保存文件的文件流;
                    FileStream fs = new FileStream(desLocation, FileMode.Create);

                    #region 部署文件恢复方法出参;
                    progressPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(long)));
                    errorPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
                    Marshal.WriteInt64(progressPtr, 0);
                    Marshal.WriteInt32(errorPtr, 0);
                    #endregion

                    try {
                        res = cdfc_object_filesave_mts(Video.VideoPtr, IObjectDevice.Handle, fs.SafeFileHandle.DangerousGetHandle(), progressPtr, errorPtr);

                        #region 释放文件恢复出参;
                        curProgressSector = Marshal.ReadInt64(progressPtr);
                        errorType = Marshal.ReadInt32(errorPtr);
                        //进行指针保存，并释放地址;
                        var proPtr = progressPtr;
                        var errPtr = errorPtr;
                        //首先将其指向零;
                        progressPtr = IntPtr.Zero;
                        errorPtr = IntPtr.Zero;
                        //再进行释放;
                        Marshal.FreeHGlobal(proPtr);
                        Marshal.FreeHGlobal(errPtr);
                        #endregion
                    }

                    catch (AccessViolationException ex) {
                        EventLogger.Logger.WriteLine("DefaultObjectScanner->FileSave底层错误:" + ex.Message + ex.Source);
                    }
                    catch (Exception ex) {
                        EventLogger.Logger.WriteLine("DefaultObjectScanner->FileSave未知错误:" + ex.Message + ex.Source);
                    }
                    finally {
                        fs.Close();
                    }
                }
                return res;
            }
            else {
                return base.SaveAs(desLocation);
            }

        }
    }

}