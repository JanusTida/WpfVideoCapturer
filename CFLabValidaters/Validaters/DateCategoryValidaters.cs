using System;
using CDFCEntities.Files;
using CDFCConverters.Converters;

namespace CDFCValidaters.Validaters {
    public static class DateCategoryValidaters {
        //设定起始时间最小合法值;
        private static readonly DateTime dtMin = new DateTime(1970, 1, 1);
        /// <summary>
        /// 验证时间计次数是否合法;
        /// </summary>
        /// <param name="dateNum"></param>
        /// <returns></returns>
        public static bool ValidateDateNum(DateCategory category) {
            var dt = DateNumToDateStringConverter.ConvertToNullableDate(category.Date);
            if (dt.HasValue && dt > dtMin) {
                //若存在满足的文件项;
                if (category.Videos.Exists(p => p.ChannelNO < 200)) {
                    return true;
                }
            }
            return false;
        }
    }
}
