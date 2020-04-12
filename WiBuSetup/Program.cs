using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiBuSetup {
    class Program {
        static void Main(string[] args) {
            try {
                var pro = Process.Start("CodeMeterRuntime.exe");
                pro.WaitForExit();
            }
            catch {

            }
        }
    }
}
