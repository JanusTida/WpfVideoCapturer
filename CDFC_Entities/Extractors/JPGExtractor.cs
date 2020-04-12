using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CDFCEntities.Extractors {
    //JPG抽帧;
    public class JPGExtractor {
                /*
        功能：传入文件结构体szFile,对文件中所有JPG进行抽JPG图像
        szFile：扫描的文件结构体
        hDisk：磁盘句柄
        szFile_2：无效，传个NULL即可
        返回值：返回这个文件的JPG图片链表(stJPG*)
        */
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr common_jpg_recovery(IntPtr szFile, IntPtr hDisk, IntPtr szFile_2);


        /*
        功能：保存JPG图片
        szFile：前面返回的JPG链表
        hDisk：磁盘句柄
        szFile_2：JPG图片句柄(外面 生成)
        返回值：0/1
        */
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool common_jpg_recovery_filesave(IntPtr szFile, IntPtr hDisk, SafeFileHandle szFile_2);

        /*
        返回值：相对于这个文件的当前扇区，可以跟文件的大小做百分比计算
        */
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong common_jpg_recovery_current_sector();

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void common_jpg_recovery_stop();

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void common_jpg_recovery_exit();
    }
}   
