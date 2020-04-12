using CDFCEntities.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace CDFCEntities.CRecoveryMethods {
    /// <summary>
    /// 大华恢复文件所需方法;
    /// </summary>
    public class DHRecoveryMethods : IRecoveryMethods {
        private static DHRecoveryMethods staticInstance;
        public static DHRecoveryMethods StaticInstance {
            get {
                return staticInstance ??
                    (staticInstance = new DHRecoveryMethods());
            }
        }
        public Func<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, bool> FileSaveFFunc {
            get {
                return cdfc_object_filesave_f;
            }
        }
        public Func<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, bool> FileSaveFunc {
            get {
                return cdfc_object_filesave;
            }
        }

        /// <summary>
        /// 设定预览大小接口;
        /// </summary>
        public Action<ulong> SetPreviewSizeAct {
            get {
                return cdfc_object_set_preview;
            }
        }

        public Func<IntPtr, IntPtr, IntPtr, ulong, bool> ReadToBuffer {
            get {
                return cdfc_object_readbuffer;
            }
        }

        public Func<IntPtr, IntPtr, IntPtr, ulong, bool> ReadToBuffer_F {
            get {
                return cdfc_object_readbuffer_f;
            }
        }

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dahua_set_preview")]
        private extern static void cdfc_object_set_preview(ulong nSize);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dahua_filesave")]
        public extern static bool cdfc_object_filesave(IntPtr szFile, IntPtr hDisk, IntPtr saveFileHandle, IntPtr nCurrSizeDW, IntPtr nError);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dahua_filesave_f")]
        private extern static bool cdfc_object_filesave_f(IntPtr szFile, IntPtr hDisk, IntPtr szFile_2, IntPtr nCurrSizeDW, IntPtr nError);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dahua_readbuffer")]
        private extern static bool cdfc_object_readbuffer(IntPtr szFile, IntPtr hDisk, IntPtr szBuffer, ulong nBuffSize);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dahua_readbuffer_f")]
        private extern static bool cdfc_object_readbuffer_f(IntPtr szFile, IntPtr hDisk, IntPtr szBuffer, ulong nBuffSize);
        
    }
}
