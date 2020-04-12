using CDFCVideoRegister.Abstracts;
using System;
using System.Windows.Threading;

namespace CDFCVideoRegister.ViewModels {
    /// <summary>
    /// 主窗体视图模型状态绑定.
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase {
        public MainWindowViewModel(Dispatcher updateInvoker) {
            if(updateInvoker == null) {
                EventLogger.RegisterLogger.WriteLine("MainWindowViewModel初始化错误:参数不得为空");
                throw new ArgumentNullException("MainWindow");
            }
            this.UpdateInvoker = updateInvoker;
        }
        /// <summary>
        /// 当前的页面视图模型;
        /// </summary>
        private ViewModelBase curPageViewModel;
        public ViewModelBase CurPageViewModel {
            get {
                
                return curPageViewModel ??
                    (curPageViewModel = RegisterInfoPageViewModel);
            }
            set {
                curPageViewModel = value;
                NotifyPropertyChanging(nameof(CurPageViewModel));
            }
        }

        /// <summary>
        /// UI线程调度器;
        /// </summary>
        public Dispatcher UpdateInvoker { get; private set; }
        
        /// <summary>
        /// 当前窗体是否可用，用于控制关闭;
        /// </summary>
        private bool isEnabled = true;
        public bool IsEnabled {
            get {
                return isEnabled;
            }
            set {
                isEnabled = value;
                NotifyPropertyChanging(nameof(IsEnabled));
            }
        }
    }
    /// <summary>
    /// 主窗体模型分部视图项;
    /// </summary>
    public partial class MainWindowViewModel :ViewModelBase {
    
        /// <summary>
        /// 注册信息页面模型;
        /// </summary>
        private RegisterInfoPageViewModel registerInfoPageViewModel;
        public RegisterInfoPageViewModel RegisterInfoPageViewModel {
            get {
                return registerInfoPageViewModel ??
                    (registerInfoPageViewModel = new RegisterInfoPageViewModel(this));
            }
        }

        private RegisterFinishedPageViewModel registerFinishedPageViewModel;
        public RegisterFinishedPageViewModel RegisterFinishedPageViewModel {
            get {
                return registerFinishedPageViewModel;
            }
            set {
                if(value != null) {
                    registerFinishedPageViewModel = value;
                }
                NotifyPropertyChanging(nameof(RegisterFinishedPageViewModel));
            }
        }
    }
}
