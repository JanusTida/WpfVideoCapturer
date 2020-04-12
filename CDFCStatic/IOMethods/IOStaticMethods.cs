using System;
using System.IO;

namespace CDFCStatic.IOMethods {
    public static class IOStaticMethods {
        public static string GetValidPath(string desLocation) {
            string fileName;
            int dotIndex;
            string extensionName;
            if (File.Exists(desLocation)) {
                int index = 1;
                dotIndex = desLocation.LastIndexOf(".");
                if (dotIndex != -1) {
                    fileName = desLocation.Substring(0, dotIndex);
                    extensionName = desLocation.Substring(dotIndex);
                }
                else {
                    fileName = desLocation;
                    extensionName = string.Empty;
                }
                while (File.Exists(string.Format("{0}-{1}{2}", fileName,index,extensionName))) {
                    index++;
                }
                return string.Format("{0}-{1}{2}", fileName, index, extensionName);
            }
            else {
                return desLocation;
            }
        }

        //验证路径是否存在，若不存在，则创建该路径;
        public static void ValidateDirectory(string desDirectory) {
            if (!Directory.Exists(desDirectory)) {
                try { 
                    Directory.CreateDirectory(desDirectory);
                }
                catch(Exception ex) {
                    EventLogger.Logger.WriteLine("IOStaticMethods->ValidateDirectory错误:" + ex.Message+"triedName:"+desDirectory);
                }
            }
        }
    }
}
