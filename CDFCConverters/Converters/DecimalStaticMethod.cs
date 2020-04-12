using System;
using System.Text;

namespace CDFCConverters.Converters {
    public static class DecimalStaticMethod {
        public static string IntToBitsString(this int num, int bitsCount) {
            string numString = num.ToString();
            int preZeroCount = bitsCount - numString.Length;
            if(preZeroCount >= 0) {
                StringBuilder sb = new StringBuilder(bitsCount);
                for (int index = 0; index < preZeroCount;index++) {
                    sb.Append("0");
                }
                sb.Append(numString);
                return sb.ToString();
            }else {
                throw new OutOfMemoryException("Too less bit!");
            }
        }
    }
}
