using CDFCEntities.Enums;
using CDFCVideoExactor.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CDFCCultures.Managers.LanguageHelper;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCVideoExactor.Helpers {
    public static class ConfigState {
        public static EntranceType EtrType { get; set; }
        public static DeviceTypeEnum SingleType { get; set; }
        public static EncryptWay EnWay { get; set; }

        public static bool IsSoftKeyApplied { get; set; }

        public const string VersionString = "V5.3";
        private static string softName;
        public static string SoftName {
            get {
                if(softName == null) {
                    switch (EtrType) {
                        case EntranceType.CapturerSingle:
                            switch (SingleType) {
                                case DeviceTypeEnum.AnLian:
                                    softName = $"{FindResourceString("AnlianSoftName")} {VersionString}";
                                    break;
                                case DeviceTypeEnum.DaHua:
                                    softName = $"{FindResourceString("DaHuaSoftName")} { VersionString}";
                                    break;
                                case DeviceTypeEnum.HaiKang:
                                    softName = $"{FindResourceString("HaiKangSoftName")} { VersionString}";
                                    break;
                                case DeviceTypeEnum.ChuangZe:
                                    softName = $"{FindResourceString("ChuangZeSoftName")} { VersionString}";
                                    break;
                                case DeviceTypeEnum.H264:
                                    softName = $"{FindResourceString("H264SoftName")} { VersionString}";
                                    break;
                                case DeviceTypeEnum.HaiShiTai:
                                    softName = $"{FindResourceString("HaiShiTaiSoftName")} { VersionString}";
                                    break;
                                case DeviceTypeEnum.HaiSi:
                                    softName = $"{FindResourceString("HaiSiSoftName")} { VersionString}";
                                    break;
                                case DeviceTypeEnum.HanBang:
                                    softName = $"{FindResourceString("HanBangSoftName")} { VersionString}";
                                    break;
                                case DeviceTypeEnum.WFS:
                                    softName = $"{FindResourceString("WFSSoftName")} { VersionString}";
                                    break;
                                case DeviceTypeEnum.RuiShi:
                                    softName = $"{FindResourceString("RuiShiSoftName")} { VersionString}";
                                    break;
                            }
                            break;
                        case EntranceType.Capturer:
                            softName = $"{FindResourceString("CpSoftName")} {VersionString}";
                            break;
                        case EntranceType.CPAndMultiMedia:
                            softName = $"{FindResourceString("CpAndMulSoftName")} {VersionString}";
                            break;
                        case EntranceType.MultiMedia:
                            softName = $"{FindResourceString("MulSoftName")} {VersionString}";
                            break;
                    }
                }
                return softName;
            }
        }
        public static int RequiredFeature =>
            EtrType == EntranceType.CPAndMultiMedia ?
            128 : FromDeviceTypeToFeatureID(SingleType);

        public static int FromDeviceTypeToFeatureID(DeviceTypeEnum dtype) {
            if (dtype == DeviceTypeEnum.HaiKang) {
                return 2;
            }
            return (int)SingleType;
        }

        public static bool IsDependencyInstalled => File.Exists("C://Windows/System32/msvcr100.dll");
    }
}
