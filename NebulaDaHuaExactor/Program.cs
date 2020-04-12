using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NebulaDaHuaExactor {
    class Program {
       
        static void Main(string[] args) {
            Process.Start("CDFCVideoExactor.exe", "CapturerSingle DaHua SoftKey".Trim());
        }
    }
}
