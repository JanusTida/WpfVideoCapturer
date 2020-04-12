using System.ComponentModel;

namespace CDFCVideoRegister.Abstracts {
    public abstract class NotificationObject : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName) {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(propName));
        }
    }
}
