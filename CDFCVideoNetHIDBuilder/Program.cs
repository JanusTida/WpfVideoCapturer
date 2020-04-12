using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CDFCVideoNetHIDBuilder {
    class Program {
        [STAThread]
        static void Main(string[] args) {
            var hid = License.Status.GetHardwareID(true, true, false, true);
            Console.WriteLine($"{hid}");
            Clipboard.SetDataObject(hid);
            var licName = "hardwareID.txt";
            var fs = new StreamWriter(licName);
            fs.WriteLine(hid);
            fs.Close();
            MessageBox.Show($"已生成硬件ID文件 {licName} ");
        }
    }

}
