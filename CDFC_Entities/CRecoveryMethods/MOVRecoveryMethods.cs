﻿using CDFCEntities.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace CDFCEntities.CRecoveryMethods {
    public partial class MOVRecoveryMethods : IRecoveryMethods {
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

        /// <summary>
        /// 设定预览大小接口;
        /// </summary>
        public Action<ulong> SetPreviewSizeAct {
            get {
                return cdfc_object_set_preview;
            }
        }
    }

    public partial class MOVRecoveryMethods   : IRecoveryMethods {
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_mov_filesave")]
        private extern static bool cdfc_object_filesave(IntPtr szFile, IntPtr hDisk, IntPtr saveFileHandle, IntPtr nCurrSizeDW, IntPtr nError);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_mov_filesave_f")]
        private extern static bool cdfc_object_filesave_f(IntPtr szFile, IntPtr hDisk, IntPtr szFile_2, IntPtr nCurrSizeDW, IntPtr nError);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_mov_set_preview")]
        private extern static void cdfc_object_set_preview(ulong nSize);


        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_mov_readbuffer")]
        private extern static bool cdfc_object_readbuffer(IntPtr szFile, IntPtr hDisk, IntPtr szBuffer, ulong nBuffSize);
    }

    public partial class MOVRecoveryMethods {
        private static CanonRecoveryMethods staticInstance;
        public static CanonRecoveryMethods StaticInstance {
            get {
                return staticInstance ??
                    (staticInstance = new CanonRecoveryMethods());
            }
        }
    }

}
