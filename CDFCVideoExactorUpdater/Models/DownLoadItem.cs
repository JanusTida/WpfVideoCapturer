using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCVideoExactorUpdater.Models {
    public class DownLoadItem {
        //远程绝对地址;
        public string RemotePreAbPath { get; set; }

        //远程相对地址;
        public string RemoteRlaPath { get; set; }

        //本地相对地址;
        public string LocalRlaPath { get; set; }

        public string Uri {
            get {
                return $"{RemotePreAbPath}{RemoteRlaPath}{Name}";
            }
        }

        public long Size { get; set; }

        public string MD5 { get; set; }

        public string Name { get; set; }
    }
}
