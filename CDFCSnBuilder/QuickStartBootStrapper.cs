using Prism.Mef;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CDFCSnBuilder {
    class QuickStartBootStrapper:MefBootstrapper {
        protected override System.Windows.DependencyObject CreateShell() {
            return new Shell();
        }

        protected override void InitializeShell() {
            //base.InitializeShell();
            Application.Current.MainWindow = (Shell)this.Shell;
            Application.Current.MainWindow.Show();
        }
        //protected override IModuleCatalog CreateModuleCatalog() {
        //    //return new DirectoryModuleCatalog() { ModulePath = "Modules" };
        //}
        protected override void ConfigureModuleCatalog() {
            //base.ConfigureModuleCatalog();
            //this.ModuleCatalog.AddModule(")
        }
        protected override void ConfigureAggregateCatalog() {
            //ModuleCatalog moduleCatalog = (ModuleCatalog)this.ModuleCatalog;
            //moduleCatalog.AddModule(typeof(microPrismModule.testModule));
            //this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(QuickStartBootstrapper).Assembly));
            this.AggregateCatalog.Catalogs.Add(new DirectoryCatalog("./"));
            // this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(FirstMainModule).Assembly));
        }
    }
}
