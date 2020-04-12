using CDFCEntities.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace CDFCEntities.CRecoveryMethods {
    public class PanasonicRecoveryMethods : IRecoveryMethods {
        private static PanasonicRecoveryMethods staticInstance;
        public static PanasonicRecoveryMethods StaticInstance {
            get {
                return staticInstance ??
                    (staticInstance = new PanasonicRecoveryMethods());
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
                throw new NotImplementedException();
            }
        }

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_panasonic_set_preview")]
        private extern static void cdfc_object_set_preview(ulong nSize);

        /// <summary>
        /// 索尼的保存入口点依据所选的nType而定;
        /// </summary>
        /// <param name="szFile"></param>
        /// <param name="hDisk"></param>
        /// <param name="szFile_2"></param>
        /// <param name="nCurrSizeDW"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private extern static bool cdfc_panasonic_filesave_mts(IntPtr szFile, IntPtr hDisk, IntPtr szFile_2, IntPtr nCurrSizeDW, IntPtr Error);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_panasonic_filesave")]
        private extern static bool cdfc_object_filesave(IntPtr szFile, IntPtr hDisk, IntPtr saveFileHandle, IntPtr nCurrSizeDW, IntPtr nError);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_panasonic_filesave_f")]
        private extern static bool cdfc_object_filesave_f(IntPtr szFile, IntPtr hDisk, IntPtr szFile_2, IntPtr nCurrSizeDW, IntPtr nError);


        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_panasonic_readbuffer")]
        private extern static bool cdfc_object_readbuffer(IntPtr szFile, IntPtr hDisk, IntPtr szBuffer, ulong nBuffSize);
    }
}
