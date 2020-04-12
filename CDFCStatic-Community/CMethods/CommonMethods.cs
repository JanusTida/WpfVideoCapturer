using System;
using System.Runtime.InteropServices;

namespace CDFCStatic.CMethods {
    /// <summary>
    /// 公共静态方法;
    /// </summary>
    public static class CommonMethods {
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public extern static ulong cdfc_common_imagefile_size(IntPtr handle);
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl,EntryPoint = "cdfc_common_read")]
        public extern static bool cdfc_common_read(IntPtr hDisk, ulong nPos, IntPtr szBuffer, ulong nSize, IntPtr nDwSize,bool nPos2);
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public extern static int cdfc_common_fstype(IntPtr handle);
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public extern static void cdfc_get_diskinfo();
    }
}
