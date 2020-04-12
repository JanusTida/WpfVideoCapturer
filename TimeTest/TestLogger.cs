using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.SafeHandles;
using static CDFC.Util.PInvoke.Extensions;

namespace TimeTest {
    /// <summary>
    /// 文件本体的结构;
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct VideoStruct {
        public uint FrameNO;                    //帧号
        public uint ChannelNO;                  //通道号
        public uint StartDate;                  //文件开始时间
        public uint EndDate;                    //文件结束时间
        public ulong Size;             //文件大小
        public ulong SizeTrue;
        public ulong StartAddress;      //文件起始地址

        //已用空间扫描时用到的碎片链表
        public IntPtr stStAdd;

        //public IntPtr FtypList;
        //public IntPtr MoovList;

        public IntPtr Next;                    //下一个文件
    };
    /// <summary>
    /// 日期分类的结构;
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DateCategoryStruct {
        public uint Date;   //日期 年-月-日
        public IntPtr File;   //这个日期下的文件 时-分-秒的文件

        //public IntPtr FtypList;

        //public IntPtr MoovList;

        public IntPtr Next;  //下一个日期链表
    }

    [TestClass]
    public class TestLogger {
        const string _hisdll = "history.dll";

        [DllImport(_hisdll, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private extern static bool history_save(IntPtr szListHeader, SafeFileHandle hHisFile);

        [DllImport(_hisdll, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private extern static IntPtr history_load(SafeFileHandle hHisFile);

        [TestMethod]
        public void TestSaveAndLoad() {
            var video1 = new VideoStruct();
            video1.ChannelNO = 12;
            video1.FrameNO = 3213151;

            var video2 = new VideoStruct() {
                ChannelNO = 13,
                FrameNO = 46465
            };

            var vPtr2 = video2.GetPtrFromStructure();
            video1.Next = vPtr2;

            var vPtr1 = video1.GetPtrFromStructure();
            

            var cate = new DateCategoryStruct();
            for (int i = 0; i < 10000; i++) {
                cate.File = vPtr1;
                cate.Date = 312313;
                var catePtr2 = cate.GetPtrFromStructure();
                cate = new DateCategoryStruct();
                cate.Next = catePtr2;
            }


            var catePtr = cate.GetPtrFromStructure();

            using (var fs = File.Create("D://logger.bin")) {
                history_save(catePtr, fs.SafeFileHandle);
                Assert.IsTrue(fs.Length != 0);
            }

            using (var fs = File.OpenRead("D://logger.bin")) {
                var resPtr = history_load(fs.SafeFileHandle);

                Assert.IsTrue(resPtr != IntPtr.Zero);

                var cateRes = resPtr.GetStructure<DateCategoryStruct>();
                Assert.AreEqual(cateRes.Date, cate.Date);
                //Assert.AreNotEqual(cateRes.File,IntPtr.Zero);

                //var video1Res = cateRes.File.GetStructure<VideoStruct>();

                //Assert.AreEqual(video1Res.FrameNO, video1.FrameNO);
            }
        }
    }
}
