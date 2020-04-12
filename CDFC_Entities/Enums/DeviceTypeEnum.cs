namespace CDFCEntities.Enums {
    public enum DeviceTypeEnum {
        HaiKang,
        AnLian,
        DaHua,
        HaiSi,
        HanBang,
        WFS,
        ZhongWei,
        XingKang,
        H264,
        HaiShiTai,
        RuiShi,
        ChuangZe,
        JVS,
        YinTan,
        QiDun,
        XiaoMi,

        Sony,
        Panasonic,
        Canon ,
        MOV,
        GoPro,
        LingDu,
        XingJi,
        JingYi,
        ShanLing,
        UnknownCar,
        WJCL,

        Unknown
    }

    public enum DeviceCategory
    {
        Capturer,
        MultiMedia,
        Unknown
    }

    public static class DeviceTypeEnumStatic
    {
        public static DeviceCategory GetDeviceCategory(this DeviceTypeEnum deviceType)
        {
            if ((int)deviceType < 13)
            {
                return Enums.DeviceCategory.Capturer;
            }
            else if ((int)deviceType < 23)
            {
                return Enums.DeviceCategory.MultiMedia;
            }
            else
            {
                return Enums.DeviceCategory.Unknown;
            }
        }

        public static string GetNameString(this DeviceTypeEnum deviceType) {
            switch (deviceType) {
                case DeviceTypeEnum.DaHua:
                    return "大华";
                case DeviceTypeEnum.HaiKang:
                    return "海康";
                case DeviceTypeEnum.AnLian:
                    return "安联";
                case DeviceTypeEnum.Canon:
                    return "佳能";
                case DeviceTypeEnum.ChuangZe:
                    return "创泽";
                case DeviceTypeEnum.GoPro:
                    return "GoPro";
                case DeviceTypeEnum.H264:
                    return "H264";
                case DeviceTypeEnum.HaiShiTai:
                    return "海视泰";
                case DeviceTypeEnum.HaiSi:
                    return "海思";
                case DeviceTypeEnum.HanBang:
                    return "汉邦";
                case DeviceTypeEnum.JingYi:
                    return "警翼";
                case DeviceTypeEnum.LingDu:
                    return "凌度";
                case DeviceTypeEnum.MOV:
                    return "佳能";
                case DeviceTypeEnum.Panasonic:
                    return "索尼";
                case DeviceTypeEnum.UnknownCar:
                    return "未知品牌1";
                case DeviceTypeEnum.WFS:
                    return "WFS";
                case DeviceTypeEnum.XingJi:
                    return "星际";
                case DeviceTypeEnum.XingKang:
                    return "兴康";
                case DeviceTypeEnum.ZhongWei:
                    return "中维";
            }
            return "未知";
        }
    }
}
