using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CDFCDavPlayer.Helpers {
    public static class StartUpHelper {
        public static StartupEventArgs StartUpArgs { get; internal set; }
        
        /// <summary>
        /// 写入启动参数到共享内存中;
        /// </summary>
        /// <param name="args"></param>
        public static void WriteStartUpArgs(StartupEventArgs args) {
            if(args == null) {
                return;
            }
            
            try {
                var mmf = MemoryMappedFile.OpenExisting(PlayerMemory);
                var formatter = new BinaryFormatter();
                using (var stream = mmf.CreateViewStream()) {
                    formatter.Serialize(stream, args.Args);
                }
            }
            catch(Exception ex) {
                EventLogger.Logger.WriteCallerLine(ex.Message);
            }
            
        }

        //共享内存名称;用于控制唯一播放;
        public const string PlayerMemory = nameof(PlayerMemory);
        private static MemoryMappedFile _playerMappedFile;

        /// <summary>
        /// 创建共享内存;
        /// </summary>
        public static void CreateCommonMemory() {
            _playerMappedFile = MemoryMappedFile.CreateOrOpen(PlayerMemory, 65536, MemoryMappedFileAccess.ReadWrite);
        }

        public static string[] ReadCommonStartUpArgs() {
            try {
                using (var stream = _playerMappedFile.CreateViewStream()) {
                    var bts = new byte[2048];
                    stream.Read(bts, 0, bts.Length);
                    if(bts.Any(p => p != 0)) {
                        stream.Position = 0;
                        var formatter = new BinaryFormatter();
                        
                        var args = formatter.Deserialize(stream) as string[];

                        var prePos = stream.Position;
                        stream.Position = 0;
                        for (int i = 0; i < prePos; i++) {
                            stream.WriteByte(0);
                        }

                        return args;
                    }
                }
            }
            catch {

            }

            return null;
        }
    }
}
