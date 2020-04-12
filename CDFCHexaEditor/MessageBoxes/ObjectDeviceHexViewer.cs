using CDFCEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WPFHexaEditor.Control.Windows;

namespace WPFHexaEditor.Control.MessageBoxes {
    public class ObjectDeviceHexMessageBox {
        public ObjectDeviceHexMessageBox(IObjectDevice device,long startPos = 0) {
            this.ObjectDevice = device;
            window = new ObjectDeviceHexWindow(device, startPos);
            window.Closing += Window_Closing;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!closed) {
                e.Cancel = true;
                window.Hide();
                hidden = true;
            }
        }

        private bool hidden;
        private bool closed;
        public void Close() {
            closed = true;
            window.Close();
        }
        
        public void Show() {
            window.Show();
        }
        
        public void ShowDialog() {
            window.ShowDialog();
        }
        public void GetFocus() {
            if(hidden) {
                window.Show();
            }
            else {
                if (closed) {
                    return;
                }
            }
            window.Focus();
        }
        private ObjectDeviceHexWindow window;
        public IObjectDevice ObjectDevice { get; private set; }
        public void SetPosition(long position,long selectByte) {
            window.SetPosition(position, selectByte);
        }
    }
}
