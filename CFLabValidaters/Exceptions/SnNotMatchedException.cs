using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCValidaters.Exceptions {
    public class SnNotMatchedException:Exception {
        public SnNotMatchedException(string msg):base(msg) {
            
        }
    }
}
