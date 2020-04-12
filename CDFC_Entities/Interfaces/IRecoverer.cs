using CDFCEntities.Files;
using System;

namespace CDFCEntities.Interfaces {
    /// <summary>
    /// 文件恢复器的接口;
    /// </summary>
    public interface IRecoverer {
        /// <summary>
        /// 当前恢复的对象所在的设备;
        /// </summary>
        IObjectDevice IObjectDevice { get; }
        /// <summary>
        /// 当前所恢复的文件;
        /// </summary>
        Video Video { get; }
        /// <summary>
        /// 当前恢复的大小;
        /// </summary>
        long CurProgressSize { get; }
        /// <summary>
        /// 错误类型;
        /// </summary>
        int ErrorType { get; }

        /// <summary>
        /// 初始化接口;
        /// </summary>
        /// <param name="video">需恢复的对象</param>
        void Init(Video video);

        /// <summary>
        /// 设置预览尺寸;
        /// </summary>
        /// <param name="nSize"></param>
        void SetPreview(ulong nSize);

        /// <summary>
        /// 文件保存接口;
        /// </summary>
        /// <param name="szFile"></param>
        /// <param name="saveFilePath"></param>
        /// <returns></returns>
        bool SaveAs(string saveFilePath);

        IntPtr ReadToBuffer();
    }
}
