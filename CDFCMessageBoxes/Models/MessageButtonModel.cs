using CDFCMessageBoxes.ViewModels;
using static CDFCCultures.Managers.ManagerLocator;
namespace CDFCMessageBoxes.Models {
    public partial class MessageButtonModel {
        private MessageButtonModel(string btnWord) {
            this.BtnWord = btnWord;
        }
        public static MessageButtonModel OK {
            get {
                return new MessageButtonModel(FindResourceString("OK"));
            }
        }
        public static MessageButtonModel YES {
            get {
                return new MessageButtonModel(FindResourceString("Yes"));
            }
        }
        public static MessageButtonModel NO {
            get {
                return new MessageButtonModel(FindResourceString("No"));
            }
        }
        public static MessageButtonModel Cancel {
            get {
                return new MessageButtonModel(FindResourceString("Cancel"));
            }
        }
    }
    public partial class MessageButtonModel  {
        public RelayCommand Command {
            get;
            set;
        }
        public string BtnWord { get;set; }
    }
    
}
