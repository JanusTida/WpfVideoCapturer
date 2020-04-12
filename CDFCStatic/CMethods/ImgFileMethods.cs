using System;
using System.Runtime.InteropServices;

namespace CDFCStatic.CMethods {
    public class ImgFileMethods {
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public extern static ulong cdfc_common_imagefile_size(IntPtr handle);
    }
}
