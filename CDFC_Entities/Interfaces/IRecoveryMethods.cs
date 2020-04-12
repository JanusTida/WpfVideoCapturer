using System;

namespace CDFCEntities.Interfaces {
    /// <summary>
    /// 文件恢复所需方法的接口;
    /// </summary>
    public interface IRecoveryMethods {
        //保存文件的委托;
        Func<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, bool> FileSaveFunc { get; }

        //保存文件的委托(文件系统);
        Func<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, bool> FileSaveFFunc { get; }

        //设置预览大小的接口;
        Action<ulong> SetPreviewSizeAct { get; }

        //将文件读取至缓冲区
        Func<IntPtr, IntPtr, IntPtr, ulong, bool> ReadToBuffer { get;}

        //将文件读取至缓冲区(文件系统)
        Func<IntPtr, IntPtr, IntPtr, ulong, bool> ReadToBuffer_F { get; }
    }
}
