using CDFCUIContracts.Abstracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using CDFCEntities.DeviceObjects;
using VideoTester.Models;
using VideoTester.Commands;
using Ookii.Dialogs.Wpf;
using CDFCEntities.Scanners;
using System.Threading;
using System.Globalization;
using CDFC.Util;
using CDFCEntities.Files;
using System.ComponentModel;
using EventLogger;

namespace VideoTester {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }
    }
    
    public class MainWindowViewModel : BindableBaseTemp{
        public MainWindowViewModel() {
        }
        RelayCommand _doCommand;
        public RelayCommand DoCommand => _doCommand ??
            (_doCommand = new RelayCommand(
                () => {
                    var dialog = new VistaOpenFileDialog();
                    if(dialog.ShowDialog() == true) {
                        if(TestScanner.StaticInstance.IObjectDevice != null) {
                            TestScanner.StaticInstance.Exit();
                            (TestScanner.StaticInstance.IObjectDevice as ImgFile)?.Exit();
                            TestScanner.StaticInstance.IObjectDevice = null;
                            CurRows.Clear();
                        }

                        Path = dialog.FileName;
                        try {
                            var proDia = new ProgressDialog();
                            proDia.DoWork += (sender, e) => {
                                bool done = false;
                                TestScanner.StaticInstance.IObjectDevice = ImgFile.GetImgFile(dialog.FileName);

                                ThreadPool.QueueUserWorkItem(cb => {
                                    try {        
                                        TestScanner.StaticInstance.SearchStart();
                                    }
                                    catch(Exception ex) {
                                        EventLogger.Logger.WriteCallerLine(ex.Message);
                                    }
                                    finally {
                                        done = true;
                                    }
                                    
                                });


                                while (!done) {
                                    var data = TestScanner.StaticInstance.GetData();
                                    if(data != null) {
                                        var totalSec = TestScanner.StaticInstance.IObjectDevice.Size / TestScanner.StaticInstance.IObjectDevice.SectorSize;
                                        var curSec = data.Value.curSec;
                                        proDia.ReportProgress((int)(curSec * 100 / totalSec),
                                            $"当前扇区:{curSec}/{totalSec}",
                                            $"已找到文件:{data.Value.fileCount}");
                                    }
                                    if (proDia.CancellationPending) {
                                        TestScanner.StaticInstance.Stop();
                                    }
                                    Thread.Sleep(1000);
                                }
                                
                                
                            };
                            proDia.RunWorkerCompleted += (sender, e) => {
                                CurRows.Clear();
                                var gories = TestScanner.StaticInstance.GetCateGories();
                                if(gories != null) {
                                    foreach (var item in gories) {
                                        item.Videos?.ForEach(v => {
                                            CurRows.Add(new VideoRow(v));
                                        });
                                        
                                    }
                                }
                            };
                            proDia.ShowDialog();
                        }
                        catch {

                        }
                        
                    }
                }
            ));

        RelayCommand _recCommand;
        public RelayCommand RecoverCommand => _recCommand ??
            (_recCommand = new RelayCommand(
                () => {
                    if(SelectedRow == null) {
                        return;
                    }

                    RecoverFiles(new List<Video> { SelectedRow.Video });
                }
            ));

        /// <summary>
        /// 恢复文件的接口
        /// </summary>
        /// <param name="videos">所需要恢复的文件项</param>
        private void RecoverFiles(List<Video> videos) {
            //询问恢复位置;
            var dialog = new VistaSaveFileDialog();

            dialog.Title = "请选择一个位置";
            

            Logger.WriteCallerLine("Start Choosing Places...");
            //若未确定,则退出;
            if (dialog.ShowDialog() == false) {
                Logger.WriteCallerLine("ShowDialog returned false.");
                return;
            }
            Logger.WriteCallerLine("ShowDialog returned not false.");

            var RecoveringPath = dialog.FileName;
            try {
                
                //scanner.DefaultRecoverer.SetPreview(0);
            }
            catch (Exception ex) {
                EventLogger.Logger.WriteLine("RecoveringController->Start获得恢复器错误:" + ex.Message);
                return;
            }
            //部署后台恢复工作器;
            var worker = new ProgressDialog();

            worker.DoWork += (sender, e) => {
                StringBuilder sbFile = new StringBuilder();

                sbFile.AppendFormat(RecoveringPath);
                //获得文件的绝对存储路径长度，以多次重置;
                int relativeLength = sbFile.Length;


                videos.ForEach(p => {
                    int count = sbFile.Length - relativeLength;
                    sbFile.Remove(relativeLength, count);
                    
                    
                    try {
                        
                        TestScanner.StaticInstance.DefaultRecoverer.Init(p);
                        var res = TestScanner.StaticInstance.DefaultRecoverer.SaveAs(sbFile.ToString());
                    }
                    catch (AccessViolationException ex) {
                        EventLogger.Logger.WriteLine("RecoveringController->Start->Do_Work出错:" + ex.Message + ex.Source);
                    }
                });

                
            };

            worker.ShowDialog();
        }
        public ObservableCollection<VideoRow> CurRows { get; set; } = new ObservableCollection<VideoRow>();

        private VideoRow _selectedRow;
        public VideoRow SelectedRow {
            get => _selectedRow;
            set {
                
                _selectedRow = value;
            }
        }

        /// <summary>
        /// 进度值;
        /// </summary>
        private string _path;
        public string Path {
            get => _path;
            set => SetProperty(ref _path , value);
        }
    }

    public class DateNumConverter : GenericStaticInstance<DateNumConverter>,IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                var val = (uint)value;
                var dt = TestScanner.StaticInstance.DateConvert(val);
                return dt;
            }
            catch {
                
            }
            return DateTime.Now;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
