using CDFCValueRanges.Models;
using System.Collections.Generic;
using System.Linq;

namespace CDFCValueRanges {
    /// <summary>
    /// 数值范围组;
    /// </summary>
    public class ValueRangeList {
        /// <summary>
        /// 数值范围组;
        /// </summary>
        public List<ValueRange> Ranges { get;private set; } = new List<ValueRange>();

        /// <summary>
        /// 连结范围节点;
        /// </summary>
        ///<param name="endValue">终止值</param>
        ///<param name="iniValue">初始值</param>
        /// <returns></returns>
        public void Concatenate(long iniValue,long endValue) {
            //获得初始值和尾值所在范围位置（若存在);
            var iniValueRange = Ranges.FirstOrDefault(p => p.IniValue <= iniValue && p.EndValue >= iniValue);
            var endValueRange = Ranges.FirstOrDefault(p => p.IniValue <= endValue && p.EndValue >= endValue);
            #region 若存在属于其中的范围，则移除碎片内文件范围;
            Ranges.RemoveAll(p => p.IniValue > iniValue && p.EndValue < endValue);
            #endregion
            
            //若初始值尾值所在范围位置均不存在;
            if (endValueRange == null && iniValueRange == null) {
                //添加新的范围;
                Ranges.Add(new ValueRange { IniValue = iniValue, EndValue = endValue });
            }
            //若初始值所在范围位置不存在;
            else if (iniValueRange == null) {
                endValueRange.IniValue = iniValue;
            }
            else if(endValueRange == null) {
                iniValueRange.EndValue = endValue;
            }
            else {
                if(endValueRange != iniValueRange) {
                    Ranges.Remove(endValueRange);
                    iniValueRange.EndValue = endValueRange.EndValue;
                }
                
            }
        }

        /// <summary>
        /// 得到某个小范围的与之相对位置;
        /// </summary>
        /// <param name="iniValue">其实值</param>
        /// <param name="endValue">终止值</param>
        /// <returns>true:此范围完全在描述范围内，false:此范围完全在描述范围外,且无交集，null:此范围与之有交集</returns>
        public bool? GetRangeValuePosition(long iniValue,long endValue) {
            var iniValueRange = Ranges.FirstOrDefault(p => p.IniValue <= iniValue && p.EndValue >= iniValue);
            var endValueRange = Ranges.FirstOrDefault(p => p.IniValue <= endValue && p.EndValue >= endValue);
            var innerRanges = Ranges.Where(p => p.IniValue >= iniValue && p.EndValue <= endValue);
            

            //若初始值尾值所在范围位置均不存在;
            if (endValueRange == null && iniValueRange == null) {
                if(innerRanges.Count() != 0) {
                    
                    return null;
                }
                else {
                    return false;
                }
            }
            //若初始值所在范围为同一个范围;
            else if (iniValueRange == endValueRange) {
                return true;
            }
            else {
                return null;
            }
        }
    }
}
