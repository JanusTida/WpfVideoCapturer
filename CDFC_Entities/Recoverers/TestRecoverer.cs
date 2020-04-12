using CDFC.Util;
using CDFCEntities.Files;
using CDFCEntities.Interfaces;
using CDFCEntities.Scanners;
using CDFCStatic.IOMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CDFCEntities.Recoverers {
    public class TestRecoverer : GenericStaticInstance<TestRecoverer> {
        private const string entryName = "cdfcproject2.dll";
        [DllImport(entryName, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool object_recover(IntPtr hDisk, SearchType eType, IntPtr stFile, IntPtr hFileOut, IntPtr nCurrSizeDW, IntPtr nError);
        
        public Video Video { get; private set; }

        public long CurProgressSize => throw new NotImplementedException();

        public int ErrorType => throw new NotImplementedException();


        public void Init(Video video) {
            this.Video = video;
        }

        public IntPtr ReadToBuffer() {
            throw new NotImplementedException();
        }

        [HandleProcessCorruptedStateExceptions]
        //public bool FileSave(IntPtr szFile, IntPtr hDisk, IntPtr saveFileHandle, IntPtr nCurrSizeDW, IntPtr nError) {
        public virtual bool SaveAs(string saveLocation) {
            //是否出现了错误:
            bool res = false;
            saveLocation = IOStaticMethods.GetValidPath(saveLocation);

            //保存文件的文件流;
            FileStream fs = new FileStream(saveLocation, FileMode.Create);

            #region 部署文件恢复方法出参;
            var progressPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(long)));
            var errorPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
            Marshal.WriteInt64(progressPtr, 0);
            Marshal.WriteInt32(errorPtr, 0);
            #endregion

            try {
                res = object_recover(TestScanner.StaticInstance.IObjectDevice.Handle,
                    SearchType.SearchType_FULL, Video.VideoPtr,
                    fs.SafeFileHandle.DangerousGetHandle(), progressPtr, errorPtr);
                #region 释放文件恢复出参;
                var curProgressSector = Marshal.ReadInt64(progressPtr);
                var errorType = Marshal.ReadInt32(errorPtr);
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
                EventLogger.Logger.WriteLine("DefaultObjectScanner->FileSave底层错误:" + ex.Message);
            }
            catch (Exception ex) {
                EventLogger.Logger.WriteLine("DefaultObjectScanner->FileSave未知错误:" + ex.Message);
            }
            finally {
                fs.Close();
            }

            return res;
        }
        public void SetPreview(ulong nSize) {
            throw new NotImplementedException();
        }
    }
}
