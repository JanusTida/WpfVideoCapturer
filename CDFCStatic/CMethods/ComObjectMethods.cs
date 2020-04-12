using System;
using System.Runtime.InteropServices;

namespace CDFCStatic.CMethods {
    public static class ComObjectMethods {
        [DllImport("cdfc_device.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool cdfc_devices_init();
        [DllImport("cdfc_device.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool cdfc_devices_exit();
        [DllImport("cdfc_device.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr cdfc_devices_devicelist();
        [DllImport("cdfc_device.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr cdfc_devices_patitionlist();
        [DllImport("cdfc_device.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr get_hdd_vender();
        [DllImport("cdfc_device.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr exit_hdd_vender();

    }
}
