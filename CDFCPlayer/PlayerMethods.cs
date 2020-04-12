using System;
using System.Diagnostics;
using System.IO;

namespace CDFCPlayer {
    public static class PlayerMethods {
        /// <summary>
        /// 删除临时存放预览文件的目录;
        /// </summary>
        public static void DisposeTemp() { 
         //删除临时目录;
         
            string targetPath = AppDomain.CurrentDomain.BaseDirectory + "/temp";
            if (!Directory.Exists(targetPath)) {
                return;
            }
            while (true) {
                try {
                    if (ClearPath(targetPath)) {
                        break;
                    }
                }
                catch {
                    //关闭占用文件的程序;
                    Process[] processes = Process.GetProcessesByName("cdfcplayer");
                    foreach (var process in processes) {
                        process.Close();
                    }
                }
            }
        }
        /// <summary>
        /// 删除临时存放预览帧的目录;
        /// </summary>
        public static void DisposeFrames() {
            string framePath = AppDomain.CurrentDomain.BaseDirectory + "/previewFrames";
            
            if (!Directory.Exists(framePath)) {
                return;
            }
            while (true) {
                try {
                    if (ClearPath(framePath)) {
                        break;
                    }
                }
                catch {

                }
            }
        }
        private static bool ClearPath(string clearedPath) {
            try {
                DirectoryInfo rootDInfo = new DirectoryInfo(clearedPath);
                foreach (var file in rootDInfo.GetFiles()) {
                    try { 
                        file.Delete();
                    }
                    catch {
                        
                    }
                }
                //删除文件夹
                Directory.Delete(clearedPath);
            }
            catch {
                //BinaryReader binReader = new BinaryReader(File.Open(strPath, FileMode.Open));
                //FileInfo fileInfo = new FileInfo(strPath);
                //byte[] bytes = binReader.ReadBytes((int)fileInfo.Length);
                //binReader.Close();
            }
            return true;
        }
    }
}
