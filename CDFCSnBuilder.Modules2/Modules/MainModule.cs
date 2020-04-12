using CDFCSnBuilder.Modules.Views;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCSnBuilder.Modules.Modules.Modules {
    [Export("CDFCSnBuilder.Modules.Modules.MainModule", typeof(MainModule))]
    public class MainModule : IModule {
        private readonly IRegionManager manager;

        [ImportingConstructor]
        public MainModule(IRegionManager manager) {
            this.manager = manager;
        }

        public void Initialize() {
            manager.AddToRegion("MainRegion", new MainView());
        }
    }
}
