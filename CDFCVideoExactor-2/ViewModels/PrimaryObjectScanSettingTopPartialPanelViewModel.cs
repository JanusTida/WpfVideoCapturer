using CDFCMessageBoxes.MessageBoxes;
using CDFCUIContracts.Commands;
using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Commands;
using CDFCVideoExactor.Controllers;
using CDFCVideoExactor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CDFCVideoExactor.ViewModels {
    /// <summary>
    /// 设定页的参数;
    /// </summary>
    public partial class PrimaryObjectScanSettingTopPartialPanelViewModel : ViewModelBase {
        private MainWindowViewModel mainWindowViewModel;
        public PrimaryObjectScanSettingTopPartialPanelViewModel(MainWindowViewModel mainWindowViewModel) {
            if (mainWindowViewModel == null) {
                EventLogger.Logger.WriteLine("PrimaryObjectScanSettingPageTopParitialViewModel初始化出现错误:参数mainWindowViewModel不得为空!");
                throw new NullReferenceException("mainWindowViewModel can't be null!");
            }
            this.mainWindowViewModel = mainWindowViewModel;
        }
    }
    /// <summary>
    /// 命令绑定项;
    /// </summary>
    public partial class PrimaryObjectScanSettingTopPartialPanelViewModel : ViewModelBase {
        private RelayCommand openSeniorSettingCommand;
        public RelayCommand OpenSeniorSettingCommand {
            get {
                return openSeniorSettingCommand ??
                    (openSeniorSettingCommand = new RelayCommand(
                        () => {
                            try {
                                ISeniorScanSettingController controller = new SeniorScanSettingController(this.mainWindowViewModel);
                                if (!controller.Start()) {
                                    CDFCMessageBox.Show("未知错误!");
                                }
                            }
                            catch (Exception ex) {
                                CDFCMessageBox.Show("抱歉，发生未知错误!" + ex.Message);
                            }
                        }
                    ));
            }
        }
    }
}
