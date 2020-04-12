using Prism.Modularity;
using System;
using Prism.Mef.Modularity;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using System.ComponentModel.Composition;
using CDFCSnBuilder.Modules.Views;
using CDFCSnBuilder.Modules.ViewModels;

namespace CDFCSnBuilder.Modules.Modules {
    [ModuleExport("CDFCSnBuilder.Modules.Modules.MainModule",typeof(MainModule))]
    public class MainModule : IModule {
        private readonly IRegionManager manager;
        [ImportingConstructor]
        public MainModule(IRegionManager manager) {
            this.manager = manager;
        }

        public void Initialize() {
            var mainView = new MainView();
            mainView.DataContext = new MainViewModel();
            this.manager.AddToRegion("MainRegion", mainView);
        }
    }
}
