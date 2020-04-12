using CDFCDavPlayer.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CDFCDavPlayer {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            var curProcess = Process.GetCurrentProcess();
            var preProcess = Process.GetProcessesByName(curProcess.ProcessName).Where(p => p.Id != curProcess.Id);
            if(preProcess != null && preProcess.Count() != 0) {
                StartUpHelper.WriteStartUpArgs(e);
                Environment.Exit(0);
                
            }
            else {
                StartUpHelper.StartUpArgs = e;

                StartUpHelper.CreateCommonMemory();
                (new QuickStartBootStrapper()).Run();
            }
            
        }
    }
}
