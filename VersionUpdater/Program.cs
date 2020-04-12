using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VersionUpdater {
    class Program {
        private static string outPutName;
        private static string assemblyName;
        private static string AssemblyName =>
            assemblyName ?? (assemblyName = typeof(Program).Assembly.GetName().Name + ".exe");
        
        static void Main(string[] args) {
            DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            outPutName = args.Count() != 0 ? args[0] : "Root.xml";
            XDocument xDoc = new XDocument();
            XElement root = new XElement(XName.Get("Root"));

            TraverseChild(di, root);
            xDoc.Add(root);
            xDoc.Save(outPutName);
        }
        
        private static void TraverseChild(DirectoryInfo di, XElement elem) {
            foreach (var item in di.GetDirectories()) {
                var diElem = new XElement(XName.Get("Folder"));
                diElem.SetAttributeValue(XName.Get("Name"), item.Name);
                TraverseChild(item, diElem);
                elem.Add(diElem);
            }

            foreach (var item in di.GetFiles()) {
                if (item.Name == outPutName || item.Name == AssemblyName) {
                    continue;
                }
                var fElem = new XElement(XName.Get("File"));
                MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
                var fs = File.OpenRead(item.FullName);
                var retVal = provider.ComputeHash(fs);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++) {
                    sb.Append(retVal[i].ToString("X2"));
                }
                var hash = sb.ToString().ToUpper();
                fs.Close();

                fElem.SetAttributeValue(XName.Get("Name"), item.Name);
                fElem.SetAttributeValue(XName.Get("Size"), item.Length);
                fElem.SetAttributeValue(XName.Get("MD5"), hash);

                elem.Add(fElem);
                Console.WriteLine($"{item.Name}\t{item.Length}\t{hash}");
            }
        }
    }
}
