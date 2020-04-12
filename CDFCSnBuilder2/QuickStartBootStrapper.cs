using Prism.Mef;
using System.ComponentModel.Composition.Hosting;
using System.Windows;

namespace CDFCSnBuilder {
    public class QuickStartBootStrapper : MefBootstrapper {
        protected override System.Windows.DependencyObject CreateShell() {
            return new Shell();
        }

        protected override void InitializeShell() {
            Application.Current.MainWindow = (Shell)this.Shell;
            Application.Current.MainWindow.Show();
        }
        //protected override IModuleCatalog CreateModuleCatalog() {
        //    return new DirectoryModuleCatalog() { ModulePath = "../" };
        //}
        protected override void ConfigureAggregateCatalog() {
            this.AggregateCatalog.Catalogs.Add(new DirectoryCatalog("Modules"));
        }
    }
}
