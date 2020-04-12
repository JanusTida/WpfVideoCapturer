using System;
using System.Runtime.InteropServices;

namespace CDFCStatic.CMethods {
    /// <summary>
    /// 公共静态方法;
    /// </summary>
    public static class CommonMethods {
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public extern static ulong cdfc_common_imagefile_size(IntPtr handle);
        /// <summary>
        /// //读取字节流;
        /// </summary>
        /// <param name="hDisk">对象</param>
        /// <param name="nPos">位置</param>
        /// <param name="szBuffer">缓冲区</param>
        /// <param name="nSize">需读取的大小</param>
        /// <param name="nDwSize">实际读取的大小</param>
        /// <param name="pos"></param>
        /// <returns></returns>
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl,EntryPoint = "cdfc_common_read")]
        public extern static bool cdfc_common_read(IntPtr hDisk, ulong nPos, IntPtr szBuffer, ulong nSize, IntPtr nDwSize,bool pos);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public extern static int cdfc_common_fstype(IntPtr handle);
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public extern static void cdfc_get_diskinfo();

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_common_write")]
        public extern static bool cdfc_common_write(IntPtr hDisk, ulong nPos, IntPtr szBuffer, ulong nSize, IntPtr nDwSize, bool bPos);
        
    }
}
