﻿using CDFC.Util;
using CDFCEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CDFCEntities.CRecoveryMethods {
    public partial class XiaoMiRecoveryMethods: GenericStaticInstance<XiaoMiRecoveryMethods>, IRecoveryMethods {
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

        public Action<ulong> SetPreviewSizeAct => cdfc_object_set_preview;
    }

    public partial class XiaoMiRecoveryMethods {
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_xiaomi_filesave")]
        private extern static bool cdfc_object_filesave(IntPtr szFile, IntPtr hDisk, IntPtr saveFileHandle, IntPtr nCurrSizeDW, IntPtr nError);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_xiaomi_filesave_f")]
        private extern static bool cdfc_object_filesave_f(IntPtr szFile, IntPtr hDisk, IntPtr szFile_2, IntPtr nCurrSizeDW, IntPtr nError);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_xiaomi_set_preview")]
        private extern static void cdfc_object_set_preview(ulong nSize);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_xiaomi_readbuffer")]
        private extern static bool cdfc_object_readbuffer(IntPtr szFile, IntPtr hDisk, IntPtr szBuffer, ulong nBuffSize);
    }
}
