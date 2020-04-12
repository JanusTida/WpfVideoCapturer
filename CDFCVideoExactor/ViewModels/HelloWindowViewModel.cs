using CDFCVideoExactor.Abstracts;
using System;

namespace CDFCVideoExactor.ViewModels {
    public class HelloWindowViewModel:NotificationObject {
        public HelloWindowViewModel(Action<Action> updateInvoker) {
            this.UpdateInvoker = updateInvoker;
        }
        /// <summary>
        /// 窗体是否可用，用于控制窗体关闭动作;
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
        public Action<Action> UpdateInvoker { get; private set; }
    }
}
