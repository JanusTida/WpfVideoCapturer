using System.ComponentModel;

namespace CDFCVideoRegister.Abstracts {
    public abstract class ViewModelBase : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanging(string propName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        /// <summary>
        /// 默认的构造函数;页面为最初级;
        /// </summary>
        public ViewModelBase()
        : this(0) { }

        /// <summary>
        /// ViewModelBase的构造方法;
        /// </summary>
        /// <param name="pageLevel"></param>
        public ViewModelBase(byte pageLevel) {
            this.PageLevel = pageLevel;
        }
        public readonly byte PageLevel;
    }
}
