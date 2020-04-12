using CDFCMessageBoxes.MessageBoxes;
using CDFCUIContracts.Commands;
using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Commands;
using CDFCVideoExactor.Models;
using System;
using System.Text;
using System.Windows;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCVideoExactor.ViewModels {
    //高级扫描设定视图模型;
    public partial class SeniorObjectScanSettingViewModel:ViewModelBase {
        public ObjectScanSetting ObjectScanSetting { get; private set; }
        public SeniorObjectScanSettingViewModel(ObjectScanSetting objectScanSetting) {
            if(objectScanSetting == null) {
                EventLogger.Logger.WriteLine("SeniorObjectScanSettingViewModel出错,objectScanSetting参数为空!");
                throw new NullReferenceException("objectScanSetting can't be null!");
            }
            this.ObjectScanSetting = objectScanSetting;

            #region 复制一份高级设定;
            this.maxSector = objectScanSetting.IObjectDevice.Size / (objectScanSetting.IObjectDevice.SectorSize != 0?
                (long)objectScanSetting.IObjectDevice.SectorSize:512);
            this.EndSector = objectScanSetting.EndSector;
            this.IniSector = objectScanSetting.IniSector;
            this.ClusterSize = objectScanSetting.ClusterSize;
            this.LbaPos = objectScanSetting.LbaPos;
            this.TimePos = objectScanSetting.TimePos;
            this.SectorSize = objectScanSetting.SectorSize;
            #endregion
        }

    }
    //高级扫描设定视图模型的状态;
    public partial class SeniorObjectScanSettingViewModel {
        //控制窗体是否可用,用于关闭窗体;
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
        
        /// <summary>
        /// 初始扫描扇区号；
        /// </summary>
        private long iniSector;
        public long IniSector {
            get {
                return iniSector;
            }
            set {
                //if (value < endSector) {
                    iniSector = value;
                //}
                //else {
                   // iniSector = endSector;
                //}
                NotifyPropertyChanging(nameof(IniSector));
            }
        }

        /// <summary>
        /// 终止扫描扇区;
        /// </summary>
        private long endSector;
        //最大扇区号;
        private long maxSector;
        public long EndSector {
            get {
                return endSector;
            }
            set {
                //if (value > maxSector) {
                //    endSector = maxSector;
                //}
                //else if (value < iniSector) {
                //    endSector = iniSector;
                //}
                //else {
                    endSector = value;
                //}
                NotifyPropertyChanging(nameof(EndSector));
            }
        }

        /// <summary>
        /// 扇区大小设定;
        /// </summary>
        private int sectorSize;
        public int SectorSize {
            get {
                return sectorSize;
            }
            set {
                if(value > 2048) {
                    sectorSize = 2048;
                }
                else if(value < 256) {
                    sectorSize = 256;
                }
                else {
                    sectorSize = value;
                }
                NotifyPropertyChanging(nameof(SectorSize));
            }
        }

        /// <summary>
        /// 簇大小设置;
        /// </summary>
        private int clusterSize;
        public int ClusterSize {
            get {
                return clusterSize;
            }
            set {
                if(value > 1024) {
                    clusterSize = 1024;
                }
                else {
                    clusterSize = value;
                }
                
                NotifyPropertyChanging(nameof(ClusterSize));
            }
        }

        /// <summary>
        /// 时间偏移量;
        /// </summary>
        private long timePos = 0;
        public long TimePos {
            get {
                return timePos;
            }
            set {
                timePos = value;
            }
        }

        /// <summary>
        /// lba偏移量;
        /// </summary>
        private long lbaPos = 0;
        public long LbaPos {
            get {
                return lbaPos;
            }
            set {
                if(value > ObjectScanSetting.IObjectDevice.Size) {
                    lbaPos = ObjectScanSetting.IObjectDevice.Size;
                }
                else {
                    lbaPos = value;
                }
                NotifyPropertyChanging(nameof(LbaPos));
            }

        }
    }

    //高级扫描设定视图模型的命令绑定项;
    public partial class SeniorObjectScanSettingViewModel {
        #region 确定设定;
        /// <summary>
        /// 参数bool为确定是否保存当前设定;
        /// </summary>
        private DelegateCommand<bool> confirmCommand;
        public DelegateCommand<bool> ConfirmCommand {
            get {
                return confirmCommand ??
                    (confirmCommand = new DelegateCommand<bool>(ConfirmExecuted));
            }
        }
        /// <summary>
        /// 保存设定的动作;
        /// </summary>
        /// <param name="para">是否保存当前设定</param>
        private void ConfirmExecuted(bool para) {
            if (para) { 
                try {
                    StringBuilder sbWords = new StringBuilder();
                    
                    if((long)iniSector > endSector) {
                        sbWords.AppendLine(FindResourceString("PleaseCheckInput"));
                        sbWords.AppendLine(FindResourceString("StartSecLargerThanEndSec"));
                    }
                    if(endSector > maxSector) {
                        sbWords.AppendLine(FindResourceString("EndSecOutOfRange"));
                        EndSector = maxSector;
                    }
                    if(sbWords.Length > 0) {
                        CDFCMessageBox.Show(sbWords.ToString());
                        return;
                    }

                    ObjectScanSetting.IniSector = this.iniSector;
                    ObjectScanSetting.EndSector = this.endSector;
                    ObjectScanSetting.ClusterSize = this.clusterSize;
                    ObjectScanSetting.LbaPos = this.lbaPos;
                    ObjectScanSetting.TimePos = this.timePos;
                    ObjectScanSetting.SectorSize = this.sectorSize;
                    IsEnabled = false;
                }
                catch(Exception ex) {
                    EventLogger.Logger.WriteLine("SeniorObjectScanSettingViewModel->ConfirmCommandExecuted出错:" + ex.Message);
                    CDFCMessageBox.Show($"{FindResourceString("FailedToSaveSetting")}:{ex.Message}");
                }
                finally {
                    //IsEnabled = false;
                }
            }
            else {
                IsEnabled = false;
            }
        }

        /// <summary>
        /// 验证输入是否合法，若出现错误，则弹窗提示;
        /// </summary>
        /// <returns></returns>
        private bool ValidateSeniorScanSettingInput() {
            
            if((long)iniSector > endSector) {

            }
            return false;
        }
        #endregion
        
    }
}
