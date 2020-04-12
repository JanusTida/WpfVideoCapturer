using CDFCLogger.Models;
using System;
using System.ComponentModel;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using CDFCMessageBoxes.MessageBoxes;
using System.IO;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCMessageBoxes.ViewModels {
    /// <summary>
    /// 创建案件窗体视图模型;
    /// </summary>
    public partial class CreateCaseWindowViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public CreateCaseWindowViewModel() {
            CaseName = "Case" + DateTime.Now.ToString().Replace(':','-').Replace('/','-');
            CaseTime = DateTime.Now.ToString();
            CasePath = AppDomain.CurrentDomain.BaseDirectory.Replace('\\','/') + "Cases";

        }

        private void NotifyPropertyChanged(string propName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }

    /// <summary>
    /// 创建案件窗体视图模型的状态；
    /// </summary>
    public partial class CreateCaseWindowViewModel {
        private string caseName;
        public string CaseName {
            get {
                return caseName;
            }
            set {
                caseName = value;
                NotifyPropertyChanged(nameof(CaseName));
            }
        }

        
        private string caseTime;
        public string CaseTime {
            get {
                return caseTime;
            }
            set {
                caseTime = value;
                NotifyPropertyChanged(nameof(CaseTime));
            }
        }

        private string caseType;
        public string CaseType {
            get {
                return caseType;
            }
            set {
                caseType = value;
                NotifyPropertyChanged(nameof(CaseType));
            }
        }

        private string caseNum;
        public string CaseNum {
            get {
                return caseNum;
            }
            set {
                caseNum = value;
                NotifyPropertyChanged(nameof(CaseNum));
            }
        }

        private string casePath;
        public string CasePath {
            get {
                return casePath;
            }
            set {
                casePath = value;
                NotifyPropertyChanged(nameof(CasePath));
            }
        }

        private string caseDes;
        public string CaseDes {
            get {
                return caseDes;
            }
            set {
                caseDes = value;
                NotifyPropertyChanged(nameof(CaseDes));
            }
        }

        private string caseInfo;
        public string CaseInfo {
            get {
                return caseInfo;
            }
            set {
                caseInfo = value;
                NotifyPropertyChanged(nameof(CaseInfo));
            }
        }

        private bool isEnabled = true;
        public bool IsEnabled {
            get {
                return isEnabled;
            }
            set {
                isEnabled = value;
                NotifyPropertyChanged(nameof(IsEnabled));
            }
        }
        public LoggerCase LoggerCase {
            get {
                return new LoggerCase {
                    CaseNum = CaseNum,
                    CaseType = CaseType,
                    CreateTime = CaseTime,
                    Description = CaseDes,
                    Info = CaseInfo,
                    Name = CaseName,
                    Path = CasePath
                };
            }
        }
    }

    /// <summary>
    /// 创建案件窗体视图模型的命令绑定项;
    /// </summary>
    public partial class CreateCaseWindowViewModel {
        private RelayCommand confirmCommand;
        public RelayCommand ConfirmCommand {
            get {
                return confirmCommand ??
                    (confirmCommand =
                    new RelayCommand(
                        () => {
                            if(string.IsNullOrEmpty(caseName)) {
                                CDFCMessageBox.Show(FindResourceString("CheckForNullCaseName"));
                                return;
                            }
                            if(caseName.IndexOfAny(new char[] { '\\', '/' }) != -1) {
                                CDFCMessageBox.Show(FindResourceString("IllegalCaseName"));
                                return;
                            }
                            IsEnabled = false;
                        }
                        )
                    );
            }
        }

        private RelayCommand queryPathCommand;
        public RelayCommand QueryPathCommand {
            get {
                return queryPathCommand ??
                    (queryPathCommand = new RelayCommand(
                        () => {
                            var dialog = new VistaFolderBrowserDialog();
                            
                            if (!Directory.Exists(CasePath)) {
                                Directory.CreateDirectory(CasePath);
                            }
                            dialog.SelectedPath = CasePath;
                            
                            //dialog.RootFolder = Environment.SpecialFolder.DesktopDirectory;
                            if (dialog.ShowDialog() == true) {
                                CasePath = dialog.SelectedPath.Replace('\\','/');

                            }
                        }
                    ));
            }
        }
    }

    public class RelayCommand : ICommand {

        #region Declarations

        readonly Func<Boolean> _canExecute;
        readonly Action _execute;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand<T>"/> class and the command can always be executed.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action execute)
            : this(execute, null) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand<T>"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action execute, Func<Boolean> canExecute) {

            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion

        #region ICommand Members

        public event EventHandler CanExecuteChanged {
            add {
                if (_canExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove {

                if (_canExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        public Boolean CanExecute(Object parameter) {
            return _canExecute == null ? true : _canExecute();
        }

        public void Execute(Object parameter) {
            _execute();
        }

        #endregion
    }
}
