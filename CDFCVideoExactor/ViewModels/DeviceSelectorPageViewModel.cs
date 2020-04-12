using CDFCEntities.DeviceObjects;
using CDFCEntities.Interfaces;
using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Commands;
using CDFCVideoExactor.Models;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System;
using CDFCMessageBoxes.MessageBoxes;
using CDFCUIContracts.Commands;
using static CDFCCultures.Managers.ManagerLocator;
using EventLogger;

namespace CDFCVideoExactor.ViewModels {
    public partial class DeviceSelectorPageViewModel:ViewModelBase {
        public readonly MainWindowViewModel MainWindowViewModel;
        //计算设备;
        private ComObject comObject;
        public ComObject ComObject {
            get {
                if(comObject == null) {
                    EventLogger.Logger.WriteLine("DeviceSelectorPage-> comObject对象为空!");
                }
                else {
                    if(comObject.Devices.Count == 0) {
                        EventLogger.Logger.WriteLine("DeviceSelectorPage-> comObject ->Devices对象为空!");
                    }
                }
                return comObject;
            }
        }

        #region 主页面所需的图标路径;
        private const string diskIconPath = "/CDFCVideoExactor;component/Images/DeviceSelectorPage/DiskIcon.png";
        private const string partitionIconPath = "/CDFCVideoExactor;component/Images/DeviceSelectorPage/PartitionIcon.png";
        #endregion

        #region 当前窗体的视图状态;
        /// <summary>
        /// 当前设备的文档;
        /// </summary>
        private XmlDocument deviceDoc;
        public XmlDocument DeviceDoc {
            get {
                // 若设备文档为空;
                if (deviceDoc == null) {
                    deviceDoc = new XmlDocument();
                    try {
                        LoadDocument();
                    }
                    //设备本体出现为空;
                    catch (Exception ex) {
                        Logger.WriteLine($"{nameof(DeviceSelectorPageViewModel)}->{nameof(DeviceDoc)}:{ex.Message}");
                    }

                }

                return deviceDoc;
                //return new XmlDocument();
            }
        }
        public bool IsLoaded => deviceDoc != null;

        /// <summary>
        /// 确定按钮是否可见;以控制确定操作;
        /// </summary>
        private bool selectCanExecute;
        public bool SelectCanExecute {
            get {
                return selectCanExecute;
            }
            set {
                selectCanExecute = value;
                NotifyPropertyChanging(nameof(SelectCanExecute));
            }
        }

        /// <summary>
        /// 选定设备的元素;
        /// </summary>
        private XmlElement selectedDeviceElem;
        public XmlElement SelectedDeviceElem {
            get {
                return selectedDeviceElem;
            }
            private set {
                selectedDeviceElem = value;
                if(selectedDeviceElem != null) {
                    SelectCanExecute = true;
                }
            }
        }
        #endregion
        
        //加载设备列表;
        private void LoadDocument() {
            Logger.WriteLine("开始加载节点");

            #region 初始化设备文档;
            int deviceID = 0;
            XElement rootElem = new XElement("FileSystem");
            rootElem.SetAttributeValue("xmlns", "");
            if (ComObject == null || comObject.Devices == null || comObject.Devices.Count == 0) {
                Logger.WriteLine("DeviceSelectorPageViewModel->DeviceDoc出错,comObject为空");
                //throw new NullReferenceException("The Comobject Is Null Or Empty!");
            }

            foreach (var p in ComObject.Devices) {
                Logger.WriteLine("开始加载设备:" + deviceID);
                if(p == null) {
                    Logger.WriteLine($"空引用设备:{deviceID++}");
                    continue;
                }

                var deviceElem = new XElement("Node");
                deviceElem.SetAttributeValue("Type", "Device");
                deviceElem.SetAttributeValue("IconPath", diskIconPath);
                deviceElem.SetAttributeValue("Level", 0);
                deviceElem.SetAttributeValue("TotalSpace", p.Size);
                deviceElem.SetAttributeValue("Name", p.SerialNumber); //"磁盘" + deviceID++);
                deviceElem.SetAttributeValue("DeviceID",$"{FindResourceString("HardDisk")}{p.DeviceID}:");
                //p.HddInfo.HddInfo2 != null ? p.HddInfo.HddInfo2.szModelNumber
                //: p.HddInfo.VendorID
                deviceElem.SetAttributeValue("Handle", p.Handle.ToString());
                deviceElem.SetAttributeValue("IsSelected", "False");
                if (p.Partitions != null) {
                    p.Partitions.ForEach(q => {
                        if(q == null) {
                            Logger.WriteLine($"{nameof(DeviceSelectorPageViewModel)}->{nameof(LoadDocument)}->LoadingPartitions:{nameof(p.SerialNumber)} null");
                            return;
                        }
                        Logger.WriteLine("开始加载分区:" + q.Sign);
                        try {
                            var partitionElem = new XElement("Node");
                            partitionElem.SetAttributeValue("IconPath", partitionIconPath);
                            partitionElem.SetAttributeValue("Name", q.Sign + ":");
                            partitionElem.SetAttributeValue("DeviceID", string.Empty);
                            partitionElem.SetAttributeValue("FileSystemType", q.Type.ToString());
                            partitionElem.SetAttributeValue("Sign", q.Sign);
                            partitionElem.SetAttributeValue("ElapsedSpace", q.ElapsedSpace);
                            partitionElem.SetAttributeValue("FreeSpace", q.FreeSpace);
                            partitionElem.SetAttributeValue("ElapsedValue", q.ElapsedSpace * 100 / q.Size);
                            partitionElem.SetAttributeValue("Type", "Partition");
                            partitionElem.SetAttributeValue("TotalSpace", q.Size);
                            partitionElem.SetAttributeValue("Level", 1);
                            partitionElem.SetAttributeValue("CHS", q.Device.DevCHS.Cylinder.ToString() + "/" + q.Device.DevCHS.HeadTrack.ToString() + "/" + q.Device.DevCHS.TrackSector);
                            partitionElem.SetAttributeValue("Handle", q.Handle.ToString());
                            partitionElem.SetAttributeValue("IsSelected", "False");
                            deviceElem.Add(partitionElem);
                        }
                        catch (Exception ex) {
                            EventLogger.Logger.WriteLine("加载" + q.Sign + "出错:" + ex.Message);
                        }
                    });
                }
                else {
                    Logger.WriteLine($"{nameof(DeviceSelectorPageViewModel)}->{nameof(LoadDocument)}:The partitions of Device {deviceID}({p.SerialNumber}) is null");
                }
                Logger.WriteLine("加载设备完毕:" + deviceID++);
                rootElem.Add(deviceElem);
            }
            XDocument xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
                rootElem);
            using (XmlReader reader = xDoc.CreateReader()) {
                MainWindowViewModel.UpdateInvoker(() => {
                    lock (deviceDoc) {
                        deviceDoc.RemoveAll();
                        deviceDoc.Load(reader);
                    }
                });
            }
            #endregion
        }

        //重新加载页面的委托;(由前台获得)
        public Action RegetAct { get; set; }
        //重新加载当前的设备列表;
        public void ReloadDocument() {
            if(deviceDoc == null) {
                deviceDoc = new XmlDocument();
            }
            else {
                
                //主窗体模型加载;
                MainWindowViewModel.ReloadInfo();
                comObject = MainWindowViewModel.ComObject;
                LoadDocument();
                //清空设备文档;
                MainWindowViewModel.UpdateInvoker(() => { RegetAct?.Invoke(); });
                
            }
            
        }
        
        public DeviceSelectorPageViewModel(MainWindowViewModel mainWindowViewModel):base(3){
            comObject = mainWindowViewModel.ComObject;
            this.MainWindowViewModel = mainWindowViewModel;
        }
        
    }

    /// <summary>
    /// 设定设备选中页的命令绑定;
    /// </summary>
    public partial class DeviceSelectorPageViewModel {
        #region 设备选定的命令绑定
        private DelegateCommand<XmlElement> selectedDeviceChangedCommand;
        public DelegateCommand<XmlElement> SelectedDeviceChangedCommand {
            get {
                if (selectedDeviceChangedCommand == null) {
                    selectedDeviceChangedCommand = new DelegateCommand<XmlElement>(SelectedDeviceChangedExecuted);
                }
                return selectedDeviceChangedCommand;
            }
        }
        /// <summary>
        /// 执行选定变化处理的命令;
        /// </summary>
        /// <param name="selectedValue"></param>
        private void SelectedDeviceChangedExecuted(XmlElement selectedValue) {
            if (selectedValue != null) {
                SelectedDeviceElem = selectedValue;
                SelectCanExecute = true;
            }
        }
        #endregion

        #region 确定设备单位的命令;

        private RelayCommand sureDoCommand;
        public RelayCommand SureDoCommand {
            get {
                if(sureDoCommand == null) {
                    sureDoCommand = new RelayCommand(SureDoExecuted);
                }
                return sureDoCommand;
            }
        }
        /// <summary>
        /// 确定执行选定对象单元的任务;
        /// </summary>
        private void SureDoExecuted() { 
            if(selectedDeviceElem != null) {
                string handle = null;
                IObjectDevice iObjectDevice = null;
                ObjectScanSetting objectScanSetting = null;

                try { 
                    handle = selectedDeviceElem.GetAttribute("Handle");
                }
                catch {
                    EventLogger.Logger.WriteLine(@"DeviceSelectorPageViewModel->SureDoCommand出错,属性为空; 
                        selectedDeviceElem.InnerXml:" + selectedDeviceElem.InnerXml);
                    CDFCMessageBox.Show(FindResourceString("ErroacquiringDiskInfo"), FindResourceString("Error"),
                        MessageBoxButton.OK);
                    return;
                }


                MainWindowViewModel.IsLoading = true;

                //部署后台工作器;
                BackgroundWorker worker = new BackgroundWorker();
                //后台轮询器确定对象;
                worker.DoWork += (sender, e) => {
                    #region 尝试获取正确的媒介对象;
                    iObjectDevice = comObject.Devices.FirstOrDefault(p => p.Handle.ToString() == handle);
                    if (iObjectDevice == null) {
                        foreach (var device in comObject.Devices) {
                            iObjectDevice = device.Partitions.FirstOrDefault(p => p.Handle.ToString() == handle);
                            if (iObjectDevice != null) {
                                break;
                            }
                        }
                        if (iObjectDevice == null) {
                            EventLogger.Logger.WriteLine(@"DeviceSelectorPageViewModel->SureDoCommand出错,未获取到对象; 
                        selectedDeviceElem.InnerXml:" + selectedDeviceElem.InnerXml);
                            MainWindowViewModel.UpdateInvoker.Invoke(() => {
                                CDFCMessageBox.Show(FindResourceString("FailedToAcquireObject"),
                                    FindResourceString("Error"), MessageBoxButton.OK);
                            });
                            return;
                        }
                    }
                    objectScanSetting = new ObjectScanSetting(iObjectDevice, MainWindowViewModel.SelectedEntranceType,MainWindowViewModel.SingleType);
                    #endregion
                };
                //工作完成后处理;
                worker.RunWorkerCompleted += (sender, e) => {
                    if(objectScanSetting != null) {
                        PrimaryObjectScanSettingPageViewModel targetPageViewModel = null;
                        switch (MainWindowViewModel.SelectedEntranceType) {
                            case Enums.EntranceType.MultiMedia:
                                targetPageViewModel = MainWindowViewModel.MultiMediaPrimaryObjectScanSettingPageViewModel;
                                break;
                            case Enums.EntranceType.CPAndMultiMedia:
                                targetPageViewModel = MainWindowViewModel.CPAndMPrimarySettingViewModel;
                                break;
                            default:
                                targetPageViewModel = MainWindowViewModel.PrimaryObjectScanSettingPageViewModel;
                                break;
                        }
                        if(targetPageViewModel != null) {
                            targetPageViewModel.ObjectScanSetting = objectScanSetting;
                        }

                        MainWindowViewModel.CurPageViewModel = targetPageViewModel;
                    }
                    MainWindowViewModel.IsLoading = false;
                };

                worker.RunWorkerAsync();
                
                
            }
        }
        #endregion
    }

}
