using CDFCEntities.Interfaces;
using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using EventLogger;
using CDFCEntities.Files;
using CDFC.Util.PInvoke;
using CDFCEntities.Structs;

namespace CDFCEntities.CRecoveryMethods {
    /// <summary>
    /// 安联恢复文件所需方法;
    /// </summary>
    public class MP4RecoveryMethods : IRecoveryMethods {
        private static MP4RecoveryMethods staticInstance;
        public static MP4RecoveryMethods StaticInstance {
            get {
                return staticInstance ??
                    (staticInstance = new MP4RecoveryMethods());
            }
        }
        public Func<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, bool> FileSaveFFunc {
            get {
                return cdfc_object_filesave_f;
            }
        }
        public Func<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, bool> FileSaveFunc {
            get {
                return cdfc_object_filesave;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        public static bool SetCluster(int cluster) {
            try {
                cdfc_object_set_cluster(cluster);
                return true;
            }
            catch(AccessViolationException ex) {
                Logger.WriteLine($"{nameof(MP4RecoveryMethods)}->{nameof(SetCluster)}:{ex.Message}");
                return false;
            }
            catch(Exception ex) {
                Logger.WriteLine($"{nameof(MP4RecoveryMethods)}->{nameof(SetCluster)}:{ex.Message}");
                return false;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        public static int GetCluster(IntPtr handle) {
            try {
                return cdfc_get_object_cluster(handle);
            }
            catch(AccessViolationException ex) {
                Logger.WriteLine($"{nameof(MP4RecoveryMethods)}->{nameof(GetCluster)}：{ex.Message}");
                return 0;
            }
            catch(Exception ex) {
                Logger.WriteLine($"{nameof(MP4RecoveryMethods)}->{nameof(GetCluster)}：{ex.Message}");
                return 0;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        public static bool CreateFileExit() {
            try {
                cdfc_create_file_exit();
                return true;
            }
            catch(AccessViolationException ex) {
                Logger.WriteLine($"{nameof(MP4RecoveryMethods)}->{nameof(CreateFileExit)}:{ex.Message}");
                return false;
            }
            catch(Exception ex) {
                Logger.WriteLine($"{nameof(MP4RecoveryMethods)}->{nameof(CreateFileExit)}:{ex.Message}");
                return false;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        public static Video CreateFile(long ftypPos, long moovSize, long moovPos) {
            try {
                var videoPtr = cdfc_object_create_file((ulong)ftypPos, (ulong)moovSize, (ulong)moovPos);
                if(videoPtr != IntPtr.Zero) {
                    var videoStruct = videoPtr.GetStructure<VideoStruct>();
                    var video = new Video(videoPtr, videoStruct, null);
                    return video;
                }
                else {
                    return null;
                }
            }
            catch(AccessViolationException ex) {
                Logger.WriteLine($"{nameof(MP4RecoveryMethods)}->{nameof(CreateFile)}:{ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 设定预览大小接口;
        /// </summary>
        public Action<ulong> SetPreviewSizeAct {
            get {
                return cdfc_object_set_preview;
            }
        }

        public Func<IntPtr, IntPtr, IntPtr, ulong, bool> ReadToBuffer {
            get {
                return cdfc_object_readbuffer;
            }
        }

        public Func<IntPtr, IntPtr, IntPtr, ulong, bool> ReadToBuffer_F {
            get {
                throw new NotImplementedException();
            }
        }

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_mp4_set_preview")]
        private extern static void cdfc_object_set_preview(ulong nSize);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_mp4_filesave")]
        private extern static bool cdfc_object_filesave(IntPtr szFile, IntPtr hDisk, IntPtr saveFileHandle, IntPtr nCurrSizeDW, IntPtr nError);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_mp4_filesave_f")]
        private extern static bool cdfc_object_filesave_f(IntPtr szFile, IntPtr hDisk, IntPtr szFile_2, IntPtr nCurrSizeDW, IntPtr nError);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_mp4_create_file")]
        private extern static IntPtr cdfc_object_create_file(ulong ftypPos, ulong moovSize, ulong moovPos);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_mp4_cluster")]
        private extern static void cdfc_object_set_cluster(int clusterSize);    

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_get_mp4_cluster")]
        private extern static int cdfc_get_object_cluster(IntPtr hFile);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_get_mp4_cluster")]
        private extern static int cdfc_get_object_cluster();

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_create_file_exit")]
        private extern static void cdfc_create_file_exit();


        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_mp4_readbuffer")]
        private extern static bool cdfc_object_readbuffer(IntPtr szFile, IntPtr hDisk, IntPtr szBuffer, ulong nBuffSize);
    }
}
