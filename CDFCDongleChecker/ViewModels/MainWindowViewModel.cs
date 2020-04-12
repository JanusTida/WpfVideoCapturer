using Aladdin.HASP;
using CDFCDongleChecker.Models;
using CDFCUIContracts.Abstracts;
using CDFCUIContracts.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CDFCDongleChecker.ViewModels {
    public partial class MainWindowViewModel : NotificationObject {
        public ObservableCollection<LicenseItemBase> Licenses { get; private set; } = new ObservableCollection<LicenseItemBase>();
        private string keyId;
        public string KeyID {
            get {
                return keyId;
            }
            set {
                keyId = value;
                NotifyPropertyChanged(nameof(KeyID));
            }
        }
    }
    public partial class MainWindowViewModel  {
        private RelayCommand refreshCommand;
        public RelayCommand RefreshCommand {
            get {
                return refreshCommand ??
                    (refreshCommand = new RelayCommand(
                        () => {
                            Licenses.Clear();
                            KeyID = null;

                            var scope = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?> <haspscope/> ";
                            string vendorCode = "U6Y92pZ9VdC9OIn+Nne+v44l/1WBEnwWQO6QdLnSV4yQjfyVe4jyCf9KAjQDBBGlE4s0bWxYoh/tIhT3Tr0t9IVvOnfftRSkMsZaUmJmeqMkM6j0yagdnedRqF07uaK4Kw1LUBqY638AeHTU0U+JOQLSrZh2CHusHERuWil1pl/Eb/Z4h8XfcbHHTsQ/6hNz+zZEISPT0R4eOOt/mliCn+CyarXtiB1A7b87y26c8bYbenaL0VM2XyYTYRUKGbgWhwyE5FOKjL4jQIAl654UbmLqBqNEt6L+sDEkDYZka00+zqI882Tfs6dFAoah3F3S4PdFrlb2YVLSw2Pej253stWwISYiasrs1LXJdq+hoRlu3+zXpyAMMhxxFuSIya4v0U1kuMu5gfIHV+F25LzOtA8YTsOkPCQmyqgB9n63ncFRk9D04zTC2Dpa7ndI9kyKVECptXdSzV6B6JtkSoKYpBFGOuvy6Lou7LoQe199rchzya7haakLjrR0iZhRw0jtfalEu9CsQ7ywyApXCYCyzjz7lcUBOEGSc7LAZajaVQXEvAix44k9t3UJABvfXDugvEvkW4msw+5L7g9ryat+qaZZ/wWI3Lo13e8IN4e3ktKGmBKfZ+UtPD+nS1UqkodnO9Z4c7hds+s5xWRHK7Zz89+CWfzhoe45TtOsYLEyRqK/GVSL/HqxgKfP4Sh50fsCiBtu7ztjjtoxIpabWMgbtnkq33hBNjdX5sjh4ZT5lyKNC0j7oZLhly/20IgDJdyW8Y2jjLG1WB7e22DtLHUANK+j9QuCn1F8G5VmDQo3tJt7aAVrq8yDRwvgZzOi8Fkjf9fqFyrOT64SsFVHgFbgaVGXiFeu2XSNAIDqr5O+6rPI+bZsvM2NW/PSz6KWmBQNFRCtR3wfomJEd/Rjlc8sCEul+AoJtcNO/Qit3EYDgeE=";
                            string info = null;

                            #region 用于获取加密狗序列号
                            var idFormat = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
                                "<haspformat root=\"hasp\">" +
                                "<hasp>" +
                                "<element name=\"id\"/>" +
                                "</hasp>" +
                               "</haspformat>";
                            var status = Hasp.GetInfo(scope, idFormat, vendorCode, ref info);

                            if (status == HaspStatus.StatusOk) {
                                XDocument idDoc = XDocument.Parse(info);
                                var infoElem = idDoc.Root;
                                var haspElem = infoElem.Element(XName.Get("hasp"));
                                var idElem = haspElem.Element(XName.Get("id"));
                                var id = idElem.Value;
                                KeyID = id;
                            }
                            #endregion

                            #region 登录一次确保，start_time存在;
                            HaspFeature hf = HaspFeature.FromFeature(25346);
                            Hasp hs = new Hasp(hf);
                            var s = hs.Login(vendorCode);
                            hs.Logout();
                            hf = HaspFeature.FromFeature(25324);
                            hs = new Hasp(hf);
                            s = hs.Login(vendorCode);
                            //var file = hs.GetFile(HaspFileId.License);
                            //var ds = file.FileId;
                            //int size = 0;
                            //file.FileSize(ref size);
                            //string retString = null;
                            //file.Read(ref retString);
                            ////hs.Feature.SetOptions
                            //var b = hs.IsValid();
                            var s2 = hs.Logout();
                            #endregion

                            #region 获得模块信息;
                            var featureFormat = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
                                                                      "<haspformat root=\"hasp_info\">" +
                                                                        " <feature>" +
                                                                        "  <attribute name=\"id\" />" +
                                                                        "  <element name=\"license\" />" +
                                                                        " </feature>" +
                                                                        "</haspformat>";
                            status = Hasp.GetInfo(scope, featureFormat, vendorCode, ref info);
                            if (status == HaspStatus.StatusOk) {
                                XDocument featureDoc = XDocument.Parse(info);
                                var infoElem = featureDoc.Root;
                                var featureElems = infoElem.Elements();
                                foreach (var elem in featureElems) {
                                    var id = elem.Attribute(XName.Get("id"));
                                    var idVal = id.Value;
                                    var licenseElem = elem.Element(XName.Get("license"));
                                    var licenseTypeElem = licenseElem.Element("license_type");
                                    LicenseItemBase licenseItem = null;
                                    if (licenseTypeElem.Value == "trial") {
                                        var timestartElem = licenseElem.Element("time_start");
                                        var totaltimeElem = licenseElem.Element("total_time");
                                        DateTime dtIni = DateTime.Parse("1970/01/01 00:00:00");
                                        
                                        int startVal = 0;
                                        if(Int32.TryParse(timestartElem.Value,out startVal)){ 
                                            var dtStart = dtIni.AddSeconds(Convert.ToInt32(timestartElem.Value));
                                            var totalTime = new TimeSpan(0, 0, Convert.ToInt32(totaltimeElem.Value));
                                            licenseItem = new TrailLicenseItem {
                                                StartTime = dtStart,
                                                TotalTime = totalTime
                                            };
                                        }
                                        else {
                                            continue;
                                        }
                                    }
                                    else if (licenseTypeElem.Value == "perpetual") {
                                        licenseItem = new PerpetualLicenseItem();
                                    }
                                    switch (idVal) {
                                        case "0":
                                            licenseItem.ModuleName = "默认模块";
                                            break;
                                        case "15346":
                                            //licenseItem.ModuleName = "黑洞-监控模块";
                                            continue;
                                        case "25346":
                                            licenseItem.ModuleName = "黑洞-监控模块";
                                            break;
                                        case "15324":
                                            //licenseItem.ModuleName = "";
                                            continue;
                                        case "25324":
                                            licenseItem.ModuleName = "黑洞-多媒体模块";
                                            break;
                                        default:
                                            licenseItem.ModuleName = idVal;
                                            break;
                                    }
                                   
                                    Licenses.Add(licenseItem);
                                }
                            }
                            #endregion
                        }
                        ));
            }
        }
    }

}


