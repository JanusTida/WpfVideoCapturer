using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace VideoTester {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public App() {
            DispatcherUnhandledException += (sender, e) => {
                EventLogger.Logger.WriteLine("主线程错误:" + e.Exception.Message);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                EventLogger.Logger.WriteLine("工作线程错误:" + ((Exception)e.ExceptionObject).Message);
                EventLogger.Logger.WriteLine("工作线程错误:" + ((Exception)e.ExceptionObject).StackTrace);
                var ex = e.ExceptionObject as Exception;
                if (ex != null && ex.InnerException != null) {
                    EventLogger.Logger.WriteLine("工作线程错误:" + ex.InnerException.StackTrace);
                    EventLogger.Logger.WriteLine("工作线程错误: " + ex.InnerException.Message);
                }
                var nullex = e.ExceptionObject as NullReferenceException;
                if (nullex != null) {
                    EventLogger.Logger.WriteLine("Source:" + nullex.Source);

                    var enumrator = nullex.Data.GetEnumerator();
                    while (enumrator.MoveNext()) {
                        EventLogger.Logger.WriteLine("Object:" + enumrator.Current.ToString());
                    }
                }
            };
            
        }
    }
}
