using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CDllInvoker.Entities {
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct VideoStruct {
        public uint FrameNO;                    //帧号
        public uint ChannelNO;                  //通道号
        public uint StartDate;                  //文件开始时间
        public uint EndDate;                    //文件结束时间
        public ulong Size;             //文件大小
        public ulong SizeTrue;
        public ulong StartAddress;		//文件起始地址
	    public IntPtr stStAdd;                //已用空间扫描时用到的碎片链表(全盘扫描不用)
        public IntPtr Next;                    //下一个文件
    };
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DateCategoryStruct {
        public uint Date;   //日期 年-月-日
        public IntPtr File;   //这个日期下的文件 时-分-秒的文件
        public IntPtr Next;  //下一个日期链表
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct FileFragmentStruct {
        public ulong StartAddress;
	    public ulong StartAddress1;
	    public ulong StartAddress2;
	    public ulong Size;
	    public int ChannelNO;
        public int StartDate;                  //文件开始时间
        public int EndDate;                    //文件结束时间
        public IntPtr Next;
    };
    
}
