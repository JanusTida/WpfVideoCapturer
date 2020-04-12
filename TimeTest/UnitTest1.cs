using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CDFCConverters.Converters;
using System.Runtime.InteropServices;
using System.IO;
using CDFC.Util.PInvoke;
using CDFCEntities.Files;
using CDFCEntities.Structs;

namespace TimeTest {
    [TestClass]
    public class UnitTest1 {
        #region
        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_filesave")]
        public extern static bool cdfc_object2_filesave(IntPtr szFile, IntPtr hDisk, IntPtr saveFileHandle, IntPtr nCurrSizeDW, IntPtr nError);

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_init")]
        private extern static bool cdfc_object2_init(ulong nStartSec, ulong nEndSec, int nSecSize,
            ulong nTimePos, ulong nLBAPos, IntPtr hDisk);

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_search_start")]
        private extern static IntPtr cdfc_object2_search_start(IntPtr handle, int type, IntPtr error);

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_search_start_f")]
        private extern static IntPtr cdfc_object2_search_start_f(IntPtr handle, int type, IntPtr error);

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_current_sector")]
        private extern static ulong cdfc_object2_current_sector();

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_filelist")]
        private extern static IntPtr cdfc_object2_filelist();

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_exit")]
        private extern static void cdfc_object2_exit();

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_stop")]
        private extern static void cdfc_object2_stop();

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_date_converter")]
        private extern static void cdfc_object2_date_converter(ulong date, IntPtr resDate);


        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_freetask")]
        private extern static void cdfc_object2_freetask(IntPtr stFile);

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_set_regionsize")]
        private extern static void cdfc_object2_set_regionsize(ulong regionSize);

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_search_start_free")]
        private extern static IntPtr cdfc_object2_search_start_free(IntPtr handle, int type, IntPtr error);

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_filelist_2")]
        private extern static IntPtr cdfc_object2_filelist_system();
#endregion
        [TestMethod]
        public void TestDHTime() {
            CDFCSetting.ScanSetting.DeviceType = CDFCEntities.Enums.DeviceTypeEnum.ZhongWei;
            var dt = DateNumToDateStringConverter.ConvertToNullableDate(3129956822);
        }

        [TestMethod]
        public void TestNVR() {
            var fs = File.OpenRead("G:/3g20171031(1)/3g20171031.dd");
            if(cdfc_object2_init(0,(ulong) fs.Length / 512, 512, 0, 0, fs.Handle)) {
                var res = cdfc_object2_search_start(fs.Handle, 2, IntPtr.Zero);
                var count = 0;
                while (res != IntPtr.Zero) {
                    var dt = res.GetStructure<DateCategoryStruct>();
                    var filePtr = dt.File;
                    while(filePtr != IntPtr.Zero) {
                        var vs = filePtr.GetStructure<VideoStruct>();
                        var cur = Marshal.AllocHGlobal(8);
                        using (var fs2 = File.Create($"E://Test/{count++}")) {

                            cdfc_object2_filesave(filePtr, fs.Handle, fs2.Handle,  cur, IntPtr.Zero);
                        }
                        filePtr = vs.Next;
                    }
                    res = dt.Next;
                }
            }
            
        }
    }
}
