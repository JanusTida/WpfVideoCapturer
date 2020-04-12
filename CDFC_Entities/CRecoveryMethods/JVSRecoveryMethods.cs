﻿using CDFCEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CDFCEntities.CRecoveryMethods {
    public partial class JVSRecoveryMethods : IRecoveryMethods {
        public Func<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, bool> FileSaveFFunc => cdfc_object_filesave_f;
        public Func<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, bool> FileSaveFunc => cdfc_object_filesave;

        public Func<IntPtr, IntPtr, IntPtr, ulong, bool> ReadToBuffer => cdfc_object_readbuffer;

        public Func<IntPtr, IntPtr, IntPtr, ulong, bool> ReadToBuffer_F {
            get {
                throw new NotImplementedException();
            }
        }

        public Action<ulong> SetPreviewSizeAct {
            get {
                return cdfc_object_set_preview;
            }
        }
    }

    public partial class JVSRecoveryMethods {
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_jvs_filesave")]
        private extern static bool cdfc_object_filesave(IntPtr szFile, IntPtr hDisk, IntPtr saveFileHandle, IntPtr nCurrSizeDW, IntPtr nError);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_jvs_filesave_f")]
        private extern static bool cdfc_object_filesave_f(IntPtr szFile, IntPtr hDisk, IntPtr szFile_2, IntPtr nCurrSizeDW, IntPtr nError);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_jvs_set_preview")]
        private extern static void cdfc_object_set_preview(ulong nSize);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_jvs_readbuffer")]
        private extern static bool cdfc_object_readbuffer(IntPtr szFile, IntPtr hDisk, IntPtr szBuffer, ulong nBuffSize);
    }
    public partial class JVSRecoveryMethods {
        private static JVSRecoveryMethods staticInstance;
        public static JVSRecoveryMethods StaticInstance {
            get {
                return staticInstance ??
                    (staticInstance = new JVSRecoveryMethods());
            }
        }
    }
}
