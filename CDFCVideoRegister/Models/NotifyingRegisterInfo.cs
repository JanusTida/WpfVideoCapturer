using CDFCVideoRegister.Abstracts;

namespace CDFCVideoRegister.Models {
    /// <summary>
    /// 注册的信息;
    /// </summary>
    public class NotifyingRegisterInfo:NotificationObject{
        /// <summary>
        /// 姓名;
        /// </summary>
        private string name;
        public string Name {
            get {
                return name;
            }
            set {
                if (!string.IsNullOrEmpty(value)) {
                    name = value;
                    NotifyPropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// 联系方式;
        /// </summary>
        private string phone;
        public string Phone { get {
                return phone;
            }
            set {
                if (!string.IsNullOrEmpty(value)) {
                    phone = value;
                    NotifyPropertyChanged(nameof(Phone));
                }
            }
        }

        /// <summary>
        /// 公司;
        /// </summary>
        private string company;
        public string Company {
            get {
                return company;
            }
            set {
                if (!string.IsNullOrEmpty(value)) {
                    company = value;
                    NotifyPropertyChanged(nameof(Company));
                }
            }
        }

        /// <summary>
        /// 电子邮箱;
        /// </summary>
        private string email;
        public string Email {
            get {
                return email;
            }
            set {
                if (!string.IsNullOrEmpty(value)) {
                    email = value;
                    NotifyPropertyChanged(nameof(Email));
                }
            }
        }

        /// <summary>
        /// 硬件ID;
        /// </summary>
        private string hardId;
        private string HardId {
            get {
                return hardId;
            }
            set {
                if (string.IsNullOrEmpty(value)) {
                    hardId = value;
                    NotifyPropertyChanged(nameof(HardId));
                }
            }
        }
    }
}
