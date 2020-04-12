using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCDavPlayer.Models {
    public interface IMainModule {
        string ModuleName { get; }
    }

    [Export(typeof(IMainModule))]
    public class MainModule:IMainModule {
        public string ModuleName { get; set; } = "FSuck";
    }

}
