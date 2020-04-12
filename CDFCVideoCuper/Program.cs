using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Threading;
using System.IO;

namespace CDFCVideoCuper {
    class Program {
        //1CE90000000467414D410000B18F0BFC;
        //61050000000970485973000012740000
        //自毁程序;
        static void Main(string[] args) {
            if(args.Length > 1) {
                Thread.Sleep(2048);
                if (args[0] == "tidatida") {
                    for (int i = 1; i < args.Length; i++) {
                        try {
                            File.Delete(args[i]);
                        }
                        catch {

                        }
                    }
                    
                }
            }

            
        }
    }
}
