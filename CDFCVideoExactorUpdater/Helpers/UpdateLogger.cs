using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCVideoExactorUpdater.Helpers {
    public static class UpdateLogger {
        private static object locker = new object();
        public static void WriteLine(string line) {
            lock (locker) {
                var sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "/updateLog.txt", true);
                string record = string.Format("{0}-{1}\t{2}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString(), line);
                sw.WriteLine(record);
                sw.Close();
            }
        }
    }
}
