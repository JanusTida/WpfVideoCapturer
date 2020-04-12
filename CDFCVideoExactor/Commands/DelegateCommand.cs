using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CDFCVideoExactor.Commands {
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

        public static implicit operator RelayCommand(DelegateCommand<MouseButtonEventArgs> v) {
            throw new NotImplementedException();
        }

        #endregion


    }
    public class DelegateCommand<T> : ICommand {
        private readonly Action<T> executeMethod = null;
        private readonly Func<T, bool> canExecuteMethod = null;

        public DelegateCommand(Action<T> executeMethod)
        : this(executeMethod, null) { }

        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod) {
            if (executeMethod == null)
                throw new ArgumentNullException("executeMetnod");
            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }

        #region ICommand 成员
        /// <summary>
             ///  Method to determine if the command can be executed
             /// </summary>
        public bool CanExecute(T parameter) {
            if (canExecuteMethod != null) {
                return canExecuteMethod(parameter);
            }
            return true;

        }

        /// <summary>
             ///  Execution of the command
             /// </summary>
        public void Execute(T parameter) {
            if (executeMethod != null) {
                executeMethod(parameter);
            }
        }

        #endregion


        event EventHandler ICommand.CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        #region ICommand 成员

        public bool CanExecute(object parameter) {
            if (parameter == null && typeof(T).IsValueType) {
                return (canExecuteMethod == null);

            }
            try {
                if (parameter.ToString() == "") {
                    return true;
                }
            }
            catch {
                //EventLogger.Logger.WriteLine("CanExecute出错:" + ex.Message + ex.Source + this.executeMethod.Method.Name);
                return false;
            }
            return CanExecute((T)parameter);
        }

        public void Execute(object parameter) {
            Execute((T)parameter);
        }

        #endregion
    }

}
