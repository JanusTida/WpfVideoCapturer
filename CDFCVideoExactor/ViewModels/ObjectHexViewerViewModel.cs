using CDFCEntities.Interfaces;
using CDFCUIContracts.Commands;
using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Commands;
using System;
using System.Globalization;
using System.Text;

namespace CDFCVideoExactor.ViewModels {
    /// <summary>
    /// 十六进制的表示视图模型;
    /// </summary>
    public partial class ObjectHexViewerViewModel:ViewModelBase {
        /// <summary>
        /// 十六进制表示的字符串;
        /// </summary>
        private string stokenString;
        public string StokenString {
            get {
                return stokenString;
            }
            set {
                stokenString = value;                
                NotifyPropertyChanging(nameof(StokenString));
                NotifyPropertyChanging(nameof(WordsString));
            }

        }
        
        /// <summary>
        /// 经转化后的字符串;
        /// </summary>
        public string WordsString {
            get {
                if (stokenString == null) {
                    return null;
                }
                StringBuilder sb = new StringBuilder();
                string valString = stokenString;
                int nowIndex = 0;
                int newLineIndex;
                string newLine = Environment.NewLine;
                while ((newLineIndex = valString.IndexOf(newLine, nowIndex)) != -1) {
                    string lineString = valString.Substring(nowIndex, newLineIndex - nowIndex);
                    string[] stokens = lineString.Split(' ');
                    foreach (var p in stokens) {
                        if (!string.IsNullOrWhiteSpace(p)) {
                            if (string.IsNullOrWhiteSpace(p)) {
                                if (p != " ") {
                                    sb.AppendLine();
                                }
                            }
                            else {
                                char ch = System.Convert.ToChar(int.Parse(p, NumberStyles.HexNumber));
                                if (!char.IsControl(ch)) {
                                    sb.Append(ch.ToString());
                                }
                                else {
                                    sb.Append('_');
                                }
                            }
                        }
                    }
                    nowIndex = newLineIndex + 2;

                    sb.AppendLine();
                }
                return sb.ToString();
            }
        }

        private IObjectDevice iObjectDevice;
        //最大扇区地址;
        private long maximumSector;
        //当前扇区号;
        private long nowSector;
        public long NowSector {
            get {
                return nowSector;
            }
            set {
                if (value > maximumSector) {
                    nowSector = maximumSector;
                }
                else if (value <= 0) {
                    nowSector = 0;
                }
                else {
                    nowSector = value;
                }
                
                NotifyPropertyChanging(nameof(NowSector));
            }
        }
        
        /// <summary>
        /// 获得显示对象;
        /// </summary>
        /// <param name="iObjectDevice">将要显示的对象</param>
        public void GetIObjectDevice(IObjectDevice iObjectDevice) {
            this.iObjectDevice = iObjectDevice;
            this.maximumSector = iObjectDevice.Size / iObjectDevice.SectorSize - 1;
            this.StokenString = iObjectDevice.GetSectorHexString(0);
        }

        /// <summary>
        /// 设定hex的显示范围;
        /// </summary>
        /// <param name="iniLba">初始扇区号;</param>
        /// <param name="size">扇区显示的范围;</param>
        public void SetSectorRange(long iniLba, long size) {
            this.iniLba = iniLba;
            maximumSector = size / iObjectDevice.SectorSize;
            NowSector = 0;
            EscapePage();
        }

        //退出接口;
        public void Exit() {
            iniLba = 0;
            maximumSector = 0;
            nowSector = 0;
            stokenString = null;
        }
    }

    /// <summary>
    /// 十六进制视图的命令绑定项;
    /// </summary>
    public partial class ObjectHexViewerViewModel {
        //初始LBA偏移，用于设定文件起始范围;
        private long iniLba;
        
        /// <summary>
        /// 跳转至当前页;
        /// </summary>
        private void EscapePage() {
            StokenString = iObjectDevice.GetSectorHexString(iniLba + (nowSector) * (long)iObjectDevice.SectorSize);
        }

        #region 返回上一页;
        /// <summary>
        /// 返回上一页的命令;
        /// </summary>
        private RelayCommand goPreviousPageCommand;
        public RelayCommand GoPreviousPageCommand {
            get {
                return goPreviousPageCommand ??
                    (goPreviousPageCommand = new RelayCommand(GoPreviousPageExecuted, GoPreviousPageCanExecute));
            }
        }
        /// <summary>
        /// 回退前一页的动作;
        /// </summary>
        private void GoPreviousPageExecuted() {
            NowSector--;
            EscapePage();
        }
        /// <summary>
        /// 是否可以执行回退一页的动作;
        /// </summary>
        /// <returns></returns>
        private bool GoPreviousPageCanExecute() {
            return NowSector > 0;
        }
        #endregion

        #region 移至下一页;
        /// <summary>
        /// 移至下一页的命令;
        /// </summary>
        private RelayCommand goNextPageCommand;
        public RelayCommand GoNextPageCommand {
            get {
                return goNextPageCommand ??
                    (goNextPageCommand = new RelayCommand(GoNextPageExecuted,GoNextPageCanExecute));
            }
        }
        private void GoNextPageExecuted() {
            NowSector++;
            EscapePage();
        }
        private bool GoNextPageCanExecute() {
            return NowSector + 1 < maximumSector;
        }
        #endregion

        #region 移至当前页;
        /// <summary>
        /// 移至当前选定页的命令;
        /// </summary>
        private RelayCommand goCurPageCommand;
        public RelayCommand GoCurPageCommand {
            get {
                return goCurPageCommand ??
                    (goCurPageCommand = new RelayCommand(GoCurPageExecuted));
            }
        }
        private void GoCurPageExecuted() {
            EscapePage();
        }
        #endregion 
    }
}
