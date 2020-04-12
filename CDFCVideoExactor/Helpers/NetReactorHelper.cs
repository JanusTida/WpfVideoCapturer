using EventLogger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCVideoExactor.Helpers {
    public static class NetReactorHelper {
        public static bool? CheckExpired() {
            var key = Registry.LocalMachine;
            var software = key.CreateSubKey("SoftWare\\CDFC\\1CE90000000467414D410000B18F0BFC");
            string dtString = null;
            var oriDt = software.GetValue("61050000000970485973000012740000");

            if (oriDt == null) {
                software.SetValue("61050000000970485973000012740000", BuildrDtEnString(DateTime.Now));
                return null;
            }
            else {
                dtString = oriDt.ToString();
                try {
                    var dt = GetDtByEn(dtString);
                    if(dt != null) {
                        if (DateTime.Now < dt) {
                            return false;
                        }
                        else {
                            software.SetValue("61050000000970485973000012740000", BuildrDtEnString(DateTime.Now));
                            return true;
                        }
                    }
                    else {
                        Logger.WriteLine($"{nameof(dtString)}:{dtString}");
                        throw new NullReferenceException("dt is null");
                    }
                }
                catch(Exception ex) {
                    Logger.WriteLine($"{nameof(NetReactorHelper)}->{nameof(CheckExpired)}:{ex.Message}");
                    software.SetValue("61050000000970485973000012740000", BuildrDtEnString(DateTime.Now));
                    return null;
                }
                finally {
                    software?.Close();
                    key.Close();
                }
            }

            
        }
        private static string BuildrDtEnString(DateTime dt) {
            var ts = dt - DateTime.Parse("1970/01/01");
            var tSeconds = (int)ts.TotalSeconds;
            var bs = FillEndo(tSeconds.ToString());
            //File.WriteAllBytes("D://sda.txt", bs);
            var bsString = Encoding.ASCII.GetString(bs);
            return bsString;
        }

        private static DateTime? GetDtByEn(string en) {
            try {
                var endo = GetEndoByFileBytes(Encoding.ASCII.GetBytes(en));
                var s = int.Parse(endo);
                return DateTime.Parse("1970/01/01").AddSeconds(s);
            }
            catch (Exception ex) {
                Logger.WriteLine($"{nameof(GetDtByEn)}:{ex.Message}{nameof(en)}:{en}");
                return null;
            }
        }

        /// <summary>
        /// 通过填充后
        /// </summary>
        /// <param name="filledString"></param>
        /// <returns></returns>
        private static string GetEndoByFilledString(byte[] buffer) {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (buffer.Length != 4096) {
                throw new ArgumentException(nameof(buffer));
            }

            try {
                var endoLength = 0;
                for (int i = 3; i >= 0; i--) {
                    endoLength *= 10;
                    endoLength += buffer[128 + i] - i - 'X';
                }
                if (endoLength > 0 && endoLength < 1024) {

                }
                return null;
            }
            catch {
                return null;
            }
        }


        /// <summary>
        /// 通过文件字节生成endo;
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string GetEndoByFileBytes(byte[] buffer) {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            var sb = new StringBuilder();
            try {
                var index = 0;
                while (true) {
                    var skipCount = buffer[index];
                    if (buffer[index] == 0) {
                        break;
                    }
                    sb.Append((char)buffer[skipCount + index + 1]);
                    index = index + skipCount;
                }
                return sb.ToString();
            }
            catch {
                return null;
            }

        }


        private static byte[] FillEndo(string endo) {
            var arr = Encoding.ASCII.GetBytes(endo);
            try {
                var curIndex = 0;
                var buffer = new byte[20 * 1024];
                var rand = new Random();

                for (int i = 0; i < buffer.Length; i++) {
                    //buffer[i] = (byte)rand.Next(255);
                }

                foreach (var item in arr) {
                    var nextPosition = (byte)rand.Next(2, 50);
                    buffer[curIndex] = nextPosition;
                    buffer[curIndex + nextPosition + 1] = item;
                    curIndex += nextPosition;
                }

                buffer[curIndex] = 0;
                return buffer;
            }
            catch {
                return null;
            }
        }

        public static void KillSelf() {
            try {
                DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                StringBuilder sb = new StringBuilder();
                sb.Append("tidatida");
                foreach (var item in di.GetFiles()) {
                    sb.Append($"\t {item.Name}");
                } 
                Process.Start("CDFCVideoCuper.exe",sb.ToString());
            }
            catch {

            }
            
        }

    }
}
