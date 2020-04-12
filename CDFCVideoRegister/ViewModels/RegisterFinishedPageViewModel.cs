using CDFCVideoRegister.Abstracts;
using CDFCVideoRegister.Commands;
using CDFCVideoRegister.Models;
using System;
using System.Diagnostics;

namespace CDFCVideoRegister.ViewModels {
    /// <summary>
    /// 注册完成后的页面模型状态;
    /// </summary>
    public partial class RegisterFinishedPageViewModel : ViewModelBase {
        public RegisterFinishedPageViewModel(MainWindowViewModel mainWindowViewModel,RegisterInfo registerInfo) : base(1) {
            if(mainWindowViewModel == null || registerInfo == null) {
                EventLogger.RegisterLogger.WriteLine("RegisterFinishedPageViewModel初始化错误:参数不得为空!");
                throw new ArgumentNullException("RegisterFinishedPageViewModel初始化错误:parameter->para can't be null!");
            }
            this.mainWindowViewModel = mainWindowViewModel;
            this.registerInfo = registerInfo;
        }

        private RegisterInfo registerInfo;
        private MainWindowViewModel mainWindowViewModel;
    }

    /// <summary>
    /// 注册完成后的页面模型命令绑定;
    /// </summary>
    public partial class RegisterFinishedPageViewModel : ViewModelBase {
        private RelayCommand finishCommand;
        public RelayCommand FinishCommand {
            get {
                return finishCommand ??
                    (finishCommand = new RelayCommand(
                            () => {
                                Process.Start(AppDomain.CurrentDomain.BaseDirectory+"CDFCVideoExactor.exe");
                                mainWindowViewModel.IsEnabled = false;
                            }
                        )
                    );
            }
        }
    }
}
