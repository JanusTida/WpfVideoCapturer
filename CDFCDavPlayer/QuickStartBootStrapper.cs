using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.ComponentModel.Composition.Hosting;
using CDFCDavPlayer.Windows;
using Prism.Mef;
using Prism.Ioc;

namespace CDFCDavPlayer {
    public class QuickStartBootStrapper : MefBootstrapper {
        protected override DependencyObject CreateShell() {
            return Container.GetExportedValue<Shell>();
        }

        protected override void InitializeShell() {
            base.InitializeShell();

            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
        }

        protected override void ConfigureAggregateCatalog() {
            base.ConfigureAggregateCatalog();

            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(QuickStartBootStrapper).Assembly));
        }

        protected override IContainerExtension CreateContainerExtension()
        {
            return null;
        }
    }
}
