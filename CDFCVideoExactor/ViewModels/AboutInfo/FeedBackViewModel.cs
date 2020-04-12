using CDFCUIContracts.Abstracts;
using CDFCUIContracts.Commands;
using CDFCVideoExactor.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCVideoExactor.ViewModels.AboutInfo {
    public partial class FeedBackViewModel:BindableBaseTemp {
        public FeedBackViewModel() {

        }

        public string ContactString { get; set; }
        public string FeedBackWord { get; set; }

        //是否正在提交;
        private bool isSubmiting;
        public bool IsSubmiting {
            get {
                return isSubmiting;
            }
            set {
                SetProperty(ref isSubmiting, value);
            }
        }

        private bool canSubmit = true;
        public bool CanSubmit {
            get {
                return canSubmit;
            }
            set {
                SetProperty(ref canSubmit, value);
            }
        }

        private string reportWord;
        public string ReportWord {
            get {
                return reportWord;
            }
            set {
                SetProperty(ref reportWord, value);
            }
        }

    }
    public partial class FeedBackViewModel {
        private RelayCommand submitCommand;
        public RelayCommand SubmitCommand =>
            submitCommand ??
            (submitCommand = new RelayCommand(() => {
                ThreadPool.QueueUserWorkItem(callBack => {
                    if (CheckInput()) {
                        IsSubmiting = true;
                        CanSubmit = false;
                        Thread.Sleep(1000);
                        IsSubmiting = false;
                        ReportWord = FindResourceString("ThanksForFeedBack");
                        Thread.Sleep(2000);
                        ReportWord = string.Empty;
                        CanSubmit = true;
                    }
                });
                
            }));

        private bool CheckInput() {
            Action<string> tellError = e => {
                CanSubmit = false;
                ReportWord = e;
                Thread.Sleep(2000);
                ReportWord = string.Empty;
                CanSubmit = true;
            };
            if (string.IsNullOrEmpty(ContactString)) {
                tellError(FindResourceString("PleaseInputContactInfo"));
                return false;
            }
            else if(string.IsNullOrEmpty(FeedBackWord)) {
                tellError(FindResourceString("PleaseInputFeedBackInfo"));
                return false;
            }
            else {
                return true;
            }
        }
    }
}
